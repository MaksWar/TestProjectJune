Shader "Custom/LetterTracing"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HideInInspector] _SpriteUvRect ("Sprite UV Rect", Vector) = (0,0,1,1)
        [HideInInspector] _SpriteMetricScale ("Sprite Metric Scale", Vector) = (1,1,0,0)
        _Color ("Tint", Color) = (1,1,1,1)
        _FillColor ("Fill Color", Color) = (1,1,1,1)
        _EdgeSoftness ("Edge Softness", Range(0, 0.05)) = 0.02
        _BrushRadius ("Brush Radius", Range(0.001, 0.5)) = 0.14
        _BrushSoftness ("Brush Softness", Range(0.001, 0.2)) = 0.04
        _BrushNoiseStrength ("Brush Noise Strength", Range(0, 0.08)) = 0
        _BrushNoiseScale ("Brush Noise Scale", Range(1, 80)) = 28
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "CanUseSpriteAtlas" = "true"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            #define MAX_PARTS 32
            #define MAX_PATH_POINTS 256

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _SpriteUvRect;
            float2 _SpriteMetricScale;
            fixed4 _Color;
            fixed4 _FillColor;
            float _EdgeSoftness;
            float _BrushRadius;
            float _BrushSoftness;
            float _BrushNoiseStrength;
            float _BrushNoiseScale;

            float4 _PathPoints[MAX_PATH_POINTS];
            float4 _PartData[MAX_PARTS];
            float _PartProgress[MAX_PARTS];
            int _PartCount;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            float ProjectOnSegment(float2 P, float2 A, float2 B)
            {
                float2 AB = B - A;
                float2 AP = P - A;
                float lenSq = dot(AB, AB);
                if (lenSq < 0.00001)
                {
                    return 0;
                }

                return saturate(dot(AP, AB) / lenSq);
            }

            float2 ToSpriteUv(float2 textureUv)
            {
                float2 rectSize = max(_SpriteUvRect.zw - _SpriteUvRect.xy, float2(0.00001, 0.00001));
                return saturate((textureUv - _SpriteUvRect.xy) / rectSize);
            }

            float2 ToMetricUv(float2 spriteUv)
            {
                return spriteUv * _SpriteMetricScale;
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 cell = floor(p);
                float2 local = frac(p);
                float2 curve = local * local * (3.0 - 2.0 * local);

                float bottomLeft = Hash21(cell);
                float bottomRight = Hash21(cell + float2(1.0, 0.0));
                float topLeft = Hash21(cell + float2(0.0, 1.0));
                float topRight = Hash21(cell + float2(1.0, 1.0));

                float bottom = lerp(bottomLeft, bottomRight, curve.x);
                float top = lerp(topLeft, topRight, curve.x);
                return lerp(bottom, top, curve.y);
            }

            float GetBrushReveal(float2 paintUv, float pathT, float distance, float progress, float totalLength)
            {
                float progressLength = progress * totalLength;
                float pixelLength = pathT * totalLength;
                float aheadDistance = max(0, pixelLength - progressLength);

                float brushRadius = max(_BrushRadius, 0.00001);
                float noise = ValueNoise(paintUv * max(_BrushNoiseScale, 1.0));
                float noisyRadius = max(0.00001, brushRadius + (noise - 0.5) * _BrushNoiseStrength);

                float brushSoftness = max(_BrushSoftness + _EdgeSoftness, 0.00001);
                float sweptBrushDistance = length(float2(aheadDistance, distance)) - noisyRadius;
                return 1.0 - smoothstep(-brushSoftness, brushSoftness, sweptBrushDistance);
            }

            float GetPartReveal(float2 paintUv, int startIndex, int pointCount, float totalLength, float progress)
            {
                progress = saturate(progress);

                if (progress <= 0.00001 || pointCount < 2 || totalLength < 0.00001)
                {
                    return 0;
                }

                float accumulatedLength = 0;
                float reveal = 0;
                float2 metricP = ToMetricUv(paintUv);

                for (int i = 0; i < MAX_PATH_POINTS; i++)
                {
                    if (i >= pointCount - 1)
                    {
                        break;
                    }

                    int pointIndex = startIndex + i;
                    float2 A = ToMetricUv(_PathPoints[pointIndex].xy);
                    float2 B = ToMetricUv(_PathPoints[pointIndex + 1].xy);
                    float segmentLength = length(B - A);

                    if (segmentLength < 0.00001)
                    {
                        continue;
                    }

                    float localT = ProjectOnSegment(metricP, A, B);
                    float2 closest = lerp(A, B, localT);
                    float distance = length(metricP - closest);
                    float pathT = (accumulatedLength + localT * segmentLength) / totalLength;

                    reveal = max(reveal, GetBrushReveal(paintUv, pathT, distance, progress, totalLength));

                    accumulatedLength += segmentLength;
                }

                return reveal;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                if (tex.a <= 0.001)
                {
                    return fixed4(0, 0, 0, 0);
                }

                if (_PartCount < 1)
                {
                    fixed4 emptyColor = _FillColor * i.color;
                    emptyColor.a = 0;
                    return emptyColor;
                }

                float2 paintUv = ToSpriteUv(i.uv);
                float reveal = 0;

                for (int partIndex = 0; partIndex < MAX_PARTS; partIndex++)
                {
                    if (partIndex >= _PartCount)
                    {
                        break;
                    }

                    int startIndex = (int)_PartData[partIndex].x;
                    int pointCount = (int)_PartData[partIndex].y;
                    float totalLength = _PartData[partIndex].z;
                    float progress = _PartProgress[partIndex];

                    reveal = max(reveal, GetPartReveal(paintUv, startIndex, pointCount, totalLength, progress));
                }

                fixed4 color = _FillColor * i.color;
                color.a = tex.a * color.a * reveal;
                return color;
            }

            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
