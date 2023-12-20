using UnityEngine;

using static UnityEngine.Mathf;

public delegate float GraphFunction(float x, float z, float t);

public static class FunctionLibrary
{
    public static float SinFunction(float x, float z, float t)
    {
        return Mathf.Sin(PI * (x + t));
    }

    public static float MultiSinFunction(float x, float z, float t)
    {
        float y = Mathf.Sin(PI * (x + t));
        y += Mathf.Sin(2 * PI * (x + 1f * t)) / 2f;
        y *= 2f / 3f;

        return y;
    }

    public static float Ripple(float x, float z, float t)
    {
        float d = Abs(x);
        float y = Sin(PI * (4f * d - t));

        return y / (1f + 10f * d);
    }

    public static float Sin2DFunction(float x, float z, float t)
    {
        return Mathf.Sin(PI * (x + z + t));
    }

    public static float MultiSin2DFunction(float x, float z, float t)
    {
        float y = Mathf.Sin(PI * (x + t));
        y += Mathf.Sin(PI * (z + t));
        y *= 0.5f;
        return y;
    }

    public static float MultiWave(float x, float z, float t)
    {
        float y = Mathf.Sin(PI * (x + 0.5f * t));
        y += 0.5f * Mathf.Sin(2f * PI * (z + t));
        return y * (2f / 3f);
    }

    public static float Ripple2D(float x, float z, float t)
    {
        float d = Sqrt(x * x + z * z);
        float y = Sin(PI * (4f * d - t));
        y /= 1f + 10f + d;

        return y;
    }
}