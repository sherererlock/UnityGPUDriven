using UnityEngine;

public static class CameraTools
{
    public static Vector4 GetPlane(Vector3 normal, Vector3 point)
    {
        return new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(normal, point));
    }

    public static Vector4 GetPlane(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 normal = Vector3.Normalize(Vector3.Cross(b - a, c - a));
        return GetPlane(normal, a);
    }

    private static Vector3[] GetFarPlanePoints()
    {
        Camera camera = Camera.main;
        Transform t = camera.transform;
        float distance = camera.farClipPlane;
        float halffov = Mathf.Deg2Rad * camera.fieldOfView * 0.5f;
        float uplen = Mathf.Tan(halffov) * distance;
        float rightlen = uplen * camera.aspect;
        Vector3 far = t.position + t.forward * distance;
        Vector3 up = t.up * uplen;
        Vector3 right = t.right * rightlen;

        Vector3 lt = far + up - right;
        Vector3 lb = far - up - right;
        Vector3 rt = far + up + right;
        Vector3 rb = far - up + right;

        return new Vector3[] { lb, rb, lt, rt };
    }

    public static Vector4[] GetFrustumPlanes()
    {
        Camera camera = Camera.main;
        Vector3 position = camera.transform.position;
        Vector3 forward = camera.transform.forward;
        Vector3[] farPlanePoints = GetFarPlanePoints(); // lt, rt, lb, rb

        Vector4 nearPlane = GetPlane(-forward, position + forward * camera.nearClipPlane);
        Vector4 farPlane = GetPlane(forward, position + forward * camera.farClipPlane);

        Vector4 leftPlane = GetPlane(position, farPlanePoints[0], farPlanePoints[2]);
        Vector4 rightPlane = GetPlane(position, farPlanePoints[3], farPlanePoints[1]);
        Vector4 topPlane = GetPlane(position, farPlanePoints[1], farPlanePoints[0]);
        Vector4 bottomPlane = GetPlane(position, farPlanePoints[2], farPlanePoints[3]);

        return new Vector4[] { nearPlane, farPlane, leftPlane, rightPlane, bottomPlane, topPlane };
    }
}
