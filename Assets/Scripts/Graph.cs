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
    public Mesh instanceMesh;
    public Material material;
    public ComputeShader computeShader;

    [Range(10, 100)]public int resolution = 50;
    [Range(1, 10)] public static float frequnecy = 1f;

    public GraphFunctionName functionName;
    public DrawMeshInstanceWay renderWay;

    GraphFunction[] functions = { SinFunction, MultiSinFunction, Ripple, Sin2DFunction, MultiSin2DFunction, MultiWave, Ripple2D };
    const float pi = Mathf.PI;

    DrawMesh drawMesh;
    DrawMeshInstance drawMeshInstance;

    private void Awake()
    {
        GraphFunction f = functions[(int)functionName];

        switch(renderWay)
        {
            case DrawMeshInstanceWay.Draw:
                drawMesh = new DrawMesh();
                break;
            case DrawMeshInstanceWay.DrawMeshInstance:
                drawMeshInstance = new DrawMeshInstance();
                break;
            case DrawMeshInstanceWay.RenderMeshInstance:
                break;
            default:
                break;

        }

        drawMesh?.Init(resolution, transform, pointPrefab, f);

        Mesh mesh = pointPrefab.GetComponent<MeshFilter>().sharedMesh;
        instanceMesh = mesh;
        drawMeshInstance?.Init(instanceMesh, material, computeShader, resolution, f);
    }

    private void Update()
    {
        drawMesh?.Update();
        drawMeshInstance?.UpdateBuffers();
        drawMeshInstance?.Draw();
    }

    void OnDisable()
    {
        drawMeshInstance?.OnDisable();
    }
}
