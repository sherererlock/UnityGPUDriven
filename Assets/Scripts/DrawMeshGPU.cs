using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class DrawMeshGPU
{
    protected ComputeBuffer l2wMatBuffer;
    List<Matrix4x4> l2wMatrixs = new();

    protected Mesh instanceMesh;
    protected Material instanceMaterial;
    protected int resolution;
    protected GraphFunction graphf;

    public void Init(Mesh mesh, Material material, int res, GraphFunction f)
    { 
        instanceMesh = mesh;
        instanceMaterial = material;
        resolution = res;
        graphf = f;
    }

    protected virtual void UpdatePosition()
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

    public virtual void UpdateBuffers()
    {
        l2wMatBuffer?.Release();

        l2wMatBuffer = new ComputeBuffer(resolution * resolution, 16 * sizeof(float));
        l2wMatrixs.Clear();

        UpdatePosition();

        l2wMatBuffer.SetData(l2wMatrixs);
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
