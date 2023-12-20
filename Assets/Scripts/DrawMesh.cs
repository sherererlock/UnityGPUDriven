using System.Collections.Generic;
using UnityEngine;

public class DrawMesh
{
    int resolution;
    Transform parent;
    Transform pointPrefab;
    GraphFunction graphFunction;

    List<Transform> points = new();

    public void Init(int res, Transform p, Transform prefab, GraphFunction f)
    {
        resolution = res;
        parent = p;
        pointPrefab = prefab;
        graphFunction = f;

        float step = 2f / resolution;
        Vector3 scale = Vector3.one * step;
        Vector3 position = Vector3.zero;

        for (int z = 0; z < resolution; z++)
        {
            position.z = (z + 0.5f) * step - 1f;
            for (int x = 0; x < resolution; x++)
            {
                Transform point = GameObject.Instantiate(pointPrefab);
                position.x = (x + 0.5f) * step - 1f;
                point.localPosition = position;
                point.localScale = scale;
                point.SetParent(parent, false);

                points.Add(point);
            }
        }
    }
    public void Update()
    {
        float t = Time.time;
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 position = points[i].localPosition;
            position.y = graphFunction(position.x, position.z, t);
            points[i].localPosition = position;
        }
    }

}
