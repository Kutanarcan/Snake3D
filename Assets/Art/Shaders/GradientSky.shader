Shader "Snake/Gradient Sky"
{
    Properties
    {
        _TopColor ("Top Colour", Color) = (0.243, 0.776, 0.878, 1)
        _BottomColor ("Bottom Colour", Color) = (0.659, 0.929, 0.894, 1)
        _Falloff ("Falloff", Range(0.2, 4)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Background"
            "Queue" = "Background"
            "PreviewType" = "Skybox"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _TopColor;
                half4 _BottomColor;
                half _Falloff;
            CBUFFER_END

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);

                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                float height = GetNormalizedScreenSpaceUV(input.positionCS).y;
                half blend = pow(saturate(height), _Falloff);

                return lerp(_BottomColor, _TopColor, blend);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
