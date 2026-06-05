Shader "Project/DiagonalDitherWipe"
{
    Properties
    {
        _Color ("Mask Color", Color) = (0, 0, 0, 1)
        _Progress ("Progress", Range(0, 1)) = 0
        _Softness ("Softness", Range(0.001, 0.5)) = 0.12
        _DitherScale ("Dither Scale", Float) = 4
        _Direction ("Direction", Vector) = (1, -1, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Diagonal Dither Wipe"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "UnityCG.cginc"

            float4 _Color;
            float _Progress;
            float _Softness;
            float _DitherScale;
            float4 _Direction;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 screenPos : TEXCOORD1;
            };

            v2f Vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            float Bayer4x4(int2 p)
            {
                int x = p.x & 3;
                int y = p.y & 3;
                int index = x + y * 4;

                float values[16] =
                {
                    0.0 / 16.0,  8.0 / 16.0,  2.0 / 16.0, 10.0 / 16.0,
                    12.0 / 16.0, 4.0 / 16.0, 14.0 / 16.0,  6.0 / 16.0,
                    3.0 / 16.0, 11.0 / 16.0,  1.0 / 16.0,  9.0 / 16.0,
                    15.0 / 16.0, 7.0 / 16.0, 13.0 / 16.0,  5.0 / 16.0
                };

                return values[index];
            }

            fixed4 Frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // 대각선 방향
                float2 dir = normalize(_Direction.xy);

                // uv 중앙 기준으로 대각선 위치 계산
                float diagonal = dot(uv - 0.5, dir);

                // 대략 0~1 범위로 보정
                diagonal = diagonal * 0.7071 + 0.5;

                // 화면 픽셀 기준 디더링 좌표
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float2 pixelPos = screenUV * _ScreenParams.xy / max(_DitherScale, 1.0);

                float dither = Bayer4x4((int2)pixelPos);

                // _Progress 0은 전체 가림, 1은 전체 노출입니다.
                float progress = saturate(_Progress);
                float threshold = lerp(1.0 + _Softness, -_Softness, progress);

                // 경계 부분에 디더링 노이즈 추가
                float ditheredDiagonal = diagonal + (dither - 0.5) * _Softness;

                // 1이면 검정 보임, 0이면 사라짐
                float mask = 1.0 - smoothstep(threshold - _Softness, threshold + _Softness, ditheredDiagonal);

                fixed4 col = _Color * i.color;
                col.a *= mask;

                return col;
            }
            ENDHLSL
        }
    }
}
