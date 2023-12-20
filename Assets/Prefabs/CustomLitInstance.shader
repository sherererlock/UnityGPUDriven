Shader "Custom/URPSimpleLitInstance"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseMap ("Base Texture", 2D) = "white" {}
        _SpecularColor("SpecularColor", Color) = (1,1,1,1)
        _Metallic("Metallic", Range(0,1)) = 0.0
        _Smoothness("Smoothness", Range(0,1)) = 0.0
        _Cutoff("Cutoff", float) = 0.5
        _Cull("Cull", float) = 0.5
    }

    SubShader
    {
        ZWrite On
        ZTest LEqual

        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _BaseColor;
            float4 _SpecularColor;
            float _Smoothness;
            float _Cutoff;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "URPSimpleLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM

            StructuredBuffer<float4x4> positionBuffer;

             #pragma target 2.0
             #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float3 normalWS : TEXCOORD3;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            Varings vert(Attributes IN, uint instanceID : SV_INSTANCEID)
            {
                Varings OUT;

                float4x4 datas = positionBuffer[instanceID];

                float4 data = float4(datas._14_24_34, datas._11);

                float3 localPosition = IN.positionOS.xyz * data.w;
                float3 worldPosition = data.xyz + localPosition;
                OUT.positionCS = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));

                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS.xyz);
                OUT.positionWS = worldPosition;
                OUT.viewDirWS = GetCameraPositionWS() - worldPosition;
                OUT.normalWS = normalInputs.normalWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            float4 frag(Varings IN) :SV_Target
            {

                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                //计算主光
                Light light = GetMainLight();
                half3 diffuse = LightingLambert(light.color, light.direction, IN.normalWS);
                half3 specular = LightingSpecular(light.color, light.direction, normalize(IN.normalWS), normalize(IN.viewDirWS), _SpecularColor, _Smoothness);
                //计算附加光照
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, IN.positionWS);
                    diffuse += LightingLambert(light.color, light.direction, IN.normalWS);
                    specular += LightingSpecular(light.color, light.direction, normalize(IN.normalWS), normalize(IN.viewDirWS), _SpecularColor, _Smoothness);
                }

                half3 color = baseMap.xyz * diffuse * _BaseColor.xyz * (IN.positionWS * 0.5 + 0.5);

                clip(baseMap.a - _Cutoff);
                return float4(color, 1);
            }

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _GLOSSINESS_FROM_BASE_ALPHA

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment


            //由于这段代码中声明了自己的CBUFFER，与我们需要的不一样，所以我们注释掉他
            //#include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
            //它还引入了下面2个hlsl文件
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}
