using System.Collections.Generic;
using UnityEngine;
using static FunctionLibrary;

public class Graph : MonoBehaviour
{
    public enum GraphFunctionName
    {
        Sine,
        MultiSine,
        Ripple,
        Sin2D,
        MultiSine2D,
        Wave2D,
        Ripple2D,
    }

    public enum DrawMeshInstanceWay
    {
        Draw,
        DrawMeshInstance,
        RenderMeshInstance
    }

    public Transform pointPrefab;
    public Material material;
    public ComputeShader cullingShader;
    public ComputeShader calcPostionShader;

    [Range(1, 100)]public int resolution = 50;
    [Range(1, 10)] public static float frequnecy = 1f;

    public GraphFunctionName functionName;
    public DrawMeshInstanceWay renderWay;

    GraphFunction[] functions = { SinFunction, MultiSinFunction, Ripple, Sin2DFunction, MultiSin2DFunction, MultiWave, Ripple2D };

    DrawMesh drawMesh;
    DrawMeshIndirect drawMeshIndirect;
    RenderMeshIndirect renderMeshIndirect;

    private void Awake()
    {
        GraphFunction f = functions[(int)functionName];

        switch(renderWay)
        {
            case DrawMeshInstanceWay.Draw:
                drawMesh = new DrawMesh();
                break;
            case DrawMeshInstanceWay.DrawMeshInstance:
                drawMeshIndirect = new DrawMeshIndirect();
                drawMeshIndirect.calcPositionShader = calcPostionShader;
                break;
            case DrawMeshInstanceWay.RenderMeshInstance:
                renderMeshIndirect = new();
                renderMeshIndirect.calcPositionShader = calcPostionShader;
                break;
            default:
                break;

        }

        drawMesh?.Init(resolution, transform, pointPrefab, f);

        Mesh mesh = pointPrefab.GetComponent<MeshFilter>().sharedMesh;
        drawMeshIndirect?.Init(mesh, material, cullingShader, resolution, f);
        renderMeshIndirect?.Init(mesh, material, resolution, f);
    }

    private void Update()
    {
        drawMesh?.Update();

        drawMeshIndirect?.UpdateBuffers(transform.position);
        drawMeshIndirect?.Draw();

        renderMeshIndirect?.UpdateBuffers(transform.position);
        renderMeshIndirect?.Draw();
    }

    void OnDisable()
    {
        drawMeshIndirect?.OnDestroy();
        renderMeshIndirect?.OnDestroy();
    }
}
