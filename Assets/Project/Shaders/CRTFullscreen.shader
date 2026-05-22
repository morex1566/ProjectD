Shader "Project/Fullscreen/CRT"
{
    Properties
    {
        _CurveStrength ("Curve Strength", Range(0, 0.3)) = 0.08
        _CurveFalloff ("Curve Falloff", Range(0.3, 4)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
        }

        Pass
        {
            Name "CRT Fullscreen"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            SAMPLER(sampler_BlitTexture);

            float _CurveStrength;
            float _CurveFalloff;

            float2 ApplyCurve(float2 uv)
            {
                // The center stays fixed, and distortion increases smoothly toward the edges.
                float2 centeredUv = uv * 2.0 - 1.0;
                float aspect = _ScreenParams.x / max(_ScreenParams.y, 1.0);
                float2 radialUv = float2(centeredUv.x * aspect, centeredUv.y);
                float maxRadius = length(float2(aspect, 1.0));
                float normalizedRadius = saturate(length(radialUv) / maxRadius);
                float curveAmount = pow(normalizedRadius, _CurveFalloff) * _CurveStrength;

                centeredUv *= 1.0 + curveAmount;
                centeredUv /= 1.0 + _CurveStrength;
                return centeredUv * 0.5 + 0.5;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 screenUv = input.texcoord.xy;
                float2 sampleUv = ApplyCurve(screenUv);
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, sampleUv);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
