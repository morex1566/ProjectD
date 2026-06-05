Shader "Project/AlphaGradient"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)

        _LeftFade ("Left Fade", Range(0, 1)) = 0
        _RightFade ("Right Fade", Range(0, 1)) = 0
        _TopFade ("Top Fade", Range(0, 1)) = 0
        _BottomFade ("Bottom Fade", Range(0, 1)) = 0

        _FadePower ("Fade Power", Range(0.1, 4)) = 1

        // UI RectTransform 크기
        // 예: RectTransform Width 800, Height 120이면 (800, 120)
        _RectSize ("Rect Size", Vector) = (100, 100, 0, 0)

        // 보통 가운데 Pivot이면 (0.5, 0.5)
        _Pivot ("Pivot", Vector) = (0.5, 0.5, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "AlphaGradientSoft"

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;

                float _LeftFade;
                float _RightFade;
                float _TopFade;
                float _BottomFade;

                float _FadePower;

                float4 _RectSize;
                float4 _Pivot;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 texUV : TEXCOORD0;
                float2 localUV : TEXCOORD1;
                float4 color : COLOR;
            };

            float SmootherStep01(float t)
            {
                t = saturate(t);

                // smoothstep보다 더 부드러운 곡선
                return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.texUV = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;

                // UI 로컬 좌표를 0~1 범위로 변환
                // Pivot이 0.5, 0.5면 local x/y가 가운데 기준이어도 정상화됨
                float2 rectSize = max(_RectSize.xy, float2(0.0001, 0.0001));
                output.localUV = (input.positionOS.xy + rectSize * _Pivot.xy) / rectSize;

                return output;
            }

            float GetEdgeFade(float distanceToEdge, float fadeSize)
            {
                if (fadeSize <= 0.0001)
                    return 1.0;

                float t = distanceToEdge / fadeSize;

                // 끝부분은 완전히 0,
                // 안쪽으로 들어올수록 아주 부드럽게 1
                return SmootherStep01(t);
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texUV);
                col *= _Color * input.color;

                float2 uv = saturate(input.localUV);

                float leftAlpha = GetEdgeFade(uv.x, _LeftFade);
                float rightAlpha = GetEdgeFade(1.0 - uv.x, _RightFade);
                float bottomAlpha = GetEdgeFade(uv.y, _BottomFade);
                float topAlpha = GetEdgeFade(1.0 - uv.y, _TopFade);

                // 각 방향을 곱해서 적용
                // 끝부분은 확실히 0으로 사라짐
                float alphaGradient = leftAlpha * rightAlpha * bottomAlpha * topAlpha;

                alphaGradient = saturate(alphaGradient);
                alphaGradient = pow(alphaGradient, _FadePower);

                col.a *= alphaGradient;

                return col;
            }

            ENDHLSL
        }
    }
}