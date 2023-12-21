using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class DrawMeshGPU
{
    protected ComputeBuffer l2wMatBuffer;
    Matrix4x4[] l2wMatrixs;

    protected Mesh instanceMesh;
    protected Material instanceMaterial;
    protected int resolution;
    protected GraphFunction graphf;

    int calcKernel;
    public ComputeShader calcPositionShader;

    public void Init(Mesh mesh, Material material, int res, GraphFunction f)
    { 
        instanceMesh = mesh;
        instanceMaterial = material;
        resolution = res;
        graphf = f;

        int count = resolution * resolution;
        l2wMatBuffer = new ComputeBuffer(count, 16 * sizeof(float));
        l2wMatrixs = new Matrix4x4[count];

        calcKernel = calcPositionShader.FindKernel("UpdatePosition");
    }

    protected virtual void UpdatePosition(Vector3 offset)
    {
        float step = 2f / resolution;
        Vector3 scale = Vector3.one * step;
        float t = Time.time;
        Vector3 position = Vector3.zero;
        int count = 0;
        for (int z = 0; z < resolution; z++)
        {
            position.z = (z + 0.5f) * step - 1f + offset.z;
            for (int x = 0; x < resolution; x++)
            {
                position.x = (x + 0.5f) * step - 1f + offset.x;
                position.y = graphf(position.x, position.z, t) + offset.y;
                Matrix4x4 mat = Matrix4x4.TRS(position, Quaternion.identity, scale);
                l2wMatrixs[count ++]= mat;
            }
        }
    }

    void DispatchComputeShader(Vector3 offset)
    {
        calcPositionShader.SetInt(ShaderIDs._Count, resolution * resolution);
        calcPositionShader.SetFloat(ShaderIDs._ATime, Time.time);
        calcPositionShader.SetInt(ShaderIDs._Resolution, resolution);
        calcPositionShader.SetVector(ShaderIDs._Offset, offset);
        calcPositionShader.SetBuffer(calcKernel, ShaderIDs._l2wMats, l2wMatBuffer);

        calcPositionShader.Dispatch(calcKernel, 1 + (resolution / 8), 1 + (resolution / 8), 1);
    }

    public virtual void UpdateBuffers(Vector3 offset)
    {
        Profiler.BeginSample("DrawMeshGPU.UpdateBuffers");

        //UpdatePosition(offset);
        //l2wMatBuffer.SetData(l2wMatrixs);
        DispatchComputeShader(offset);

        Profiler.EndSample();
    }

    public virtual void Draw()
    {
    }

    public virtual void OnDestroy()
    {
        l2wMatBuffer?.Release();
        l2wMatBuffer = null;
    }
}
