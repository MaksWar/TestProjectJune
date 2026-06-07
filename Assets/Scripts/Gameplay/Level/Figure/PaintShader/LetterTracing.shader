Shader "Custom/LetterTracing"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _FillColor ("Fill Color", Color) = (1,1,1,1)
        _EdgeSoftness ("Edge Softness", Range(0, 0.05)) = 0.01
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
            fixed4 _Color;
            fixed4 _FillColor;
            float _EdgeSoftness;

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

            void ProjectOnPart(float2 P, int startIndex, int pointCount, float totalLength, out float bestDistance, out float pathT)
            {
                bestDistance = 1e9;
                pathT = 0;

                if (pointCount < 2 || totalLength < 0.00001)
                {
                    return;
                }

                float accumulatedLength = 0;

                for (int i = 0; i < MAX_PATH_POINTS; i++)
                {
                    if (i >= pointCount - 1)
                    {
                        break;
                    }

                    int pointIndex = startIndex + i;
                    float2 A = _PathPoints[pointIndex].xy;
                    float2 B = _PathPoints[pointIndex + 1].xy;
                    float segmentLength = length(B - A);

                    if (segmentLength < 0.00001)
                    {
                        continue;
                    }

                    float localT = ProjectOnSegment(P, A, B);
                    float2 closest = lerp(A, B, localT);
                    float distance = length(P - closest);

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        pathT = (accumulatedLength + localT * segmentLength) / totalLength;
                    }

                    accumulatedLength += segmentLength;
                }
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                clip(tex.a - 0.1);

                if (_PartCount < 1)
                {
                    fixed4 emptyColor = _FillColor * i.color;
                    emptyColor.a = 0;
                    return emptyColor;
                }

                float nearestDistance = 1e9;
                float nearestPathT = 0;
                float nearestProgress = 0;

                for (int partIndex = 0; partIndex < MAX_PARTS; partIndex++)
                {
                    if (partIndex >= _PartCount)
                    {
                        break;
                    }

                    int startIndex = (int)_PartData[partIndex].x;
                    int pointCount = (int)_PartData[partIndex].y;
                    float totalLength = _PartData[partIndex].z;

                    float partDistance;
                    float partPathT;
                    ProjectOnPart(i.uv, startIndex, pointCount, totalLength, partDistance, partPathT);

                    if (partDistance < nearestDistance)
                    {
                        nearestDistance = partDistance;
                        nearestPathT = partPathT;
                        nearestProgress = saturate(_PartProgress[partIndex]);
                    }
                }

                float softness = max(_EdgeSoftness, 0.00001);
                float reveal = 1.0 - smoothstep(
                    nearestProgress - softness,
                    nearestProgress + softness,
                    nearestPathT
                );

                if (nearestProgress <= 0.00001)
                {
                    reveal = 0;
                }
                else if (nearestProgress >= 0.99999)
                {
                    reveal = 1;
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
