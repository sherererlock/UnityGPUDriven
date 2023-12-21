using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderMeshIndirect : DrawMeshGPU
{
    GraphicsBuffer commandBuffer;
    GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
    RenderParams renderParams;
    const int commandCount = 1;

    public new void Init(Mesh mesh, Material material, int res, GraphFunction f)
    {
        base.Init(mesh, material, res, f);

        commandBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[commandCount];

        renderParams = new RenderParams(material);
        renderParams.worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one);
        renderParams.matProps = new MaterialPropertyBlock();

        commandData[0].indexCountPerInstance = instanceMesh.GetIndexCount(0);
        commandData[0].instanceCount = (uint)(resolution * resolution);
        commandBuffer.SetData(commandData);
    }

    public override void Draw()
    {
        instanceMaterial.SetBuffer("positionBuffer", l2wMatBuffer);
        Graphics.RenderMeshIndirect(renderParams, instanceMesh, commandBuffer, commandCount);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        commandBuffer?.Release();
        commandBuffer = null;
    }
}
