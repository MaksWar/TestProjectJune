Shader "Custom/LetterTracing"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _FillColor ("Fill Color", Color) = (0.2, 0.8, 0.2, 1)
        _Progress ("Progress", Range(0, 1)) = 0
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
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _FillColor;
            float _Progress;
            float _EdgeSoftness;

            float4 _Waypoints[64];
            int _WaypointCount;

            struct appdata
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
                fixed4 color    : COLOR;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                fixed4 color    : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos   = UnityObjectToClipPos(v.vertex);
                o.uv    = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            // РџСЂРѕРµРєС†С–СЏ С‚РѕС‡РєРё P РЅР° РІС–РґСЂС–Р·РѕРє AB, РїРѕРІРµСЂС‚Р°С” t [0..1]
            float ProjectOnSegment(float2 P, float2 A, float2 B)
            {
                float2 AB   = B - A;
                float2 AP   = P - A;
                float lenSq = dot(AB, AB);
                if (lenSq < 0.00001) return 0;
                return saturate(dot(AP, AB) / lenSq);
            }

            // РџРѕРІРµСЂС‚Р°С” РіР»РѕР±Р°Р»СЊРЅРёР№ t [0..1] РІР·РґРѕРІР¶ С€Р»СЏС…Сѓ
            // РґР»СЏ РЅР°Р№Р±Р»РёР¶С‡РѕС— С‚РѕС‡РєРё РґРѕ P (UV РєРѕРѕСЂРґРёРЅР°С‚Рё)
            float GetPathT(float2 P)
            {
                float bestDist  = 1e9;
                float bestT     = 0;
                float totalLen  = 0;

                float segLengths[63];
                for (int i = 0; i < _WaypointCount - 1; i++)
                {
                    segLengths[i] = length(_Waypoints[i + 1].xy - _Waypoints[i].xy);
                    totalLen += segLengths[i];
                }

                if (totalLen < 0.00001) return 0;

                float accLen = 0;

                for (int j = 0; j < _WaypointCount - 1; j++)
                {
                    float2 A = _Waypoints[j].xy;
                    float2 B = _Waypoints[j + 1].xy;

                    float  localT   = ProjectOnSegment(P, A, B);
                    float2 closest  = lerp(A, B, localT);
                    float  dist     = length(P - closest);

                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestT    = (accLen + localT * segLengths[j]) / totalLen;
                    }

                    accLen += segLengths[j];
                }

                return bestT;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);

                // РџС–РєСЃРµР»С– РїРѕР·Р° Р»С–С‚РµСЂРѕСЋ вЂ” РІС–РґРєРёРґР°С”РјРѕ
                clip(tex.a - 0.1);

                if (_WaypointCount < 2)
                {
                    fixed4 color = _FillColor;
                    color.a = 0;
                    return color;
                }

                float pathT = GetPathT(i.uv);

                // Soft alpha reveal along the traced path. Unreached pixels stay transparent.
                float progress = saturate(_Progress);
                float softness = max(_EdgeSoftness, 0.00001);
                float reveal = 1.0 - smoothstep(
                    progress - softness,
                    progress + softness,
                    pathT
                );

                if (progress <= 0.00001)
                {
                    reveal = 0;
                }
                else if (progress >= 0.99999)
                {
                    reveal = 1;
                }

                fixed4 color = _FillColor;
                color.a = tex.a * i.color.a * _FillColor.a * reveal;
                return color;
            }

            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
