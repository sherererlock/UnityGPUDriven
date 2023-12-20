using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class DrawMeshInstance
{
    ComputeBuffer l2wMatBuffer;
    ComputeBuffer cullResult;
    ComputeBuffer argsBuffer;

    List<Matrix4x4> l2wMatrixs = new List<Matrix4x4>();
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    Camera mainCamera;
    int kernel;
    Mesh instanceMesh;
    Material instanceMaterial;
    int resolution;
    int subMeshIndex = 0;
    GraphFunction graphf;

    ComputeShader cullingComputer;
    public void Init(Mesh mesh, Material material, ComputeShader compute, int res, GraphFunction f)
    {
        kernel = compute.FindKernel("CSMain");
        mainCamera = Camera.main;
        cullingComputer = compute;
        instanceMesh = mesh;
        instanceMaterial = material;
        resolution = res;
        graphf = f;

        cullResult = new ComputeBuffer(resolution * resolution, sizeof(float) * 16, ComputeBufferType.Append);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
    }

    private void UpdatePosition()
    {
        float step = 2f / resolution;
        Vector3 scale = Vector3.one * step;
        Vector3 position = Vector3.zero;
        float t = Time.time;
        for (int z = 0; z < resolution; z++)
        {
            position.z = (z + 0.5f) * step - 1f;
            for (int x = 0; x < resolution; x++)
            {
                position.x = (x + 0.5f) * step - 1f;
                position.y = graphf(position.x, position.z, t);
                l2wMatrixs.Add(Matrix4x4.TRS(position, Quaternion.identity, scale));
            }
        }
    }

    public void UpdateBuffers()
    {
        if (instanceMesh != null)
            subMeshIndex = Mathf.Clamp(subMeshIndex, 0, instanceMesh.subMeshCount - 1);

        l2wMatBuffer?.Release();

        l2wMatBuffer = new ComputeBuffer(resolution * resolution, 16 * sizeof(float));
        l2wMatrixs.Clear();

        UpdatePosition();

        l2wMatBuffer.SetData(l2wMatrixs);

        // Indirect args
        if (instanceMesh != null)
        {
            args[0] = (uint)instanceMesh.GetIndexCount(subMeshIndex);
            args[1] = (uint)(resolution * resolution);
            args[2] = (uint)instanceMesh.GetIndexStart(subMeshIndex);
            args[3] = (uint)instanceMesh.GetBaseVertex(subMeshIndex);
        }
        else
        {
            args[0] = args[1] = args[2] = args[3] = 0;
        }
        argsBuffer.SetData(args);
    }

    public void Draw()
    {
        Profiler.BeginSample("DrawMeshInstance");

        instanceMaterial.SetBuffer("positionBuffer", l2wMatBuffer);

        Vector3 bounds = Vector3.one * 1000;
        Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, instanceMaterial, new Bounds(Vector3.zero, bounds), argsBuffer);

        Profiler.EndSample();
    }

    public void OnDisable()
    {
        l2wMatBuffer?.Release();
        l2wMatBuffer = null;

        cullResult?.Release();
        cullResult = null;

        argsBuffer?.Release();
        argsBuffer = null;
    }
}
