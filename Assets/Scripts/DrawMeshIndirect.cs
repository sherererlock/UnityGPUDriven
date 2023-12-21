using UnityEngine;
using UnityEngine.Profiling;

public class DrawMeshIndirect : DrawMeshGPU
{
    ComputeBuffer cullResult;
    ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    Camera mainCamera;
    int kernel;
    int subMeshIndex = 0;

    ComputeShader cullingComputer;
    public void Init(Mesh mesh, Material material, ComputeShader compute, int res, GraphFunction f)
    {
        base.Init(mesh, material, res, f);
        kernel = compute.FindKernel("Culling");
        mainCamera = Camera.main;
        cullingComputer = compute;

        cullResult = new ComputeBuffer(resolution * resolution, sizeof(float) * 16, ComputeBufferType.Append);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
    }

    public override void UpdateBuffers(Vector3 offset) 
    {
        base.UpdateBuffers(offset);

        if (instanceMesh != null)
            subMeshIndex = Mathf.Clamp(subMeshIndex, 0, instanceMesh.subMeshCount - 1);

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

    public override void Draw()
    {
        Profiler.BeginSample("FrustumCullingWithComputeShader");
        int count = resolution * resolution;
        Vector4[] planes = CameraTools.GetFrustumPlanes();

        cullResult.SetCounterValue(0);
        cullingComputer.SetInt(ShaderIDs._Count, count);
        cullingComputer.SetVectorArray(ShaderIDs._Planes, planes);
        cullingComputer.SetBuffer(kernel, ShaderIDs._l2wMats, l2wMatBuffer);
        cullingComputer.SetBuffer(kernel, ShaderIDs._cullingResults, cullResult);

        cullingComputer.Dispatch(kernel, 1 + (count / 64), 1, 1);

        Profiler.EndSample();

        ComputeBuffer.CopyCount(cullResult, argsBuffer, sizeof(uint));
        instanceMaterial.SetBuffer(ShaderIDs._positionBuffer, cullResult);

        Vector3 bounds = Vector3.one * 1000;
        Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, instanceMaterial, new Bounds(Vector3.zero, bounds), argsBuffer);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        cullResult?.Release();
        cullResult = null;

        argsBuffer?.Release();
        argsBuffer = null;
    }
}
