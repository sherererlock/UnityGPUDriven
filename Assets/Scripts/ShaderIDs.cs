using UnityEngine;

public static class ShaderIDs 
{
    public static readonly int _Count = Shader.PropertyToID("_Count");
    public static readonly int _ATime = Shader.PropertyToID("_ATime");
    public static readonly int _Resolution = Shader.PropertyToID("_Resolution");
    public static readonly int _Offset = Shader.PropertyToID("_Offset");
    public static readonly int _Planes = Shader.PropertyToID("_Planes");
    public static readonly int _l2wMats = Shader.PropertyToID("_l2wMats");
    public static readonly int _cullingResults = Shader.PropertyToID("_cullingResults");
    public static readonly int _positionBuffer = Shader.PropertyToID("positionBuffer");
}
