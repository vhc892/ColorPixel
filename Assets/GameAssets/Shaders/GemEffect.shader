Shader "Custom/GlitterShimmer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlitterStrength ("Glitter Strength", Range(0,2)) = 1
        _GlitterSpeed ("Glitter Speed", Range(0,10)) = 3
        _GlitterScale ("Glitter Scale", Range(1,200)) = 60
        _HueRange ("Hue Range", Range(0,0.15)) = 0.08
        _ValueRange ("Value Range", Range(0,0.5)) = 0.3
        _WhiteSparkle ("White Sparkle", Range(0,1)) = 0.5
        _DarkAccentColor ("Dark Accent Color", Color) = (1,0.8,0.6,1) // vàng cam
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _GlitterStrength;
            float _GlitterSpeed;
            float _GlitterScale;
            float _HueRange;
            float _ValueRange;
            float _WhiteSparkle;
            float4 _DarkAccentColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 glitterUV : TEXCOORD1;
                float4 pos : SV_POSITION;
            };

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            // RGB <-> HSV
            float3 RGBtoHSV(float3 c) {
                float4 K = float4(0., -1./3., 2./3., -1.);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b,c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r,p.yzx), step(p.x,c.r));
                float d = q.x - min(q.w,q.y);
                float e = 1e-10;
                return float3(abs(q.z+(q.w-q.y)/(6.*d+e)), d/(q.x+e), q.x);
            }

            float3 HSVtoRGB(float3 c) {
                float3 rgb = clamp(abs(fmod(c.x*6.+float3(0,4,2),6.)-3.)-1.,0.,1.);
                return c.z * lerp(float3(1,1,1), rgb, c.y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.glitterUV = v.uv * _GlitterScale;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 baseCol = tex2D(_MainTex, i.uv);
                if (baseCol.a < 0.1) return baseCol;

                float2 cell = floor(i.glitterUV);
                float n = hash21(cell); // random cho từng hạt

                // Random offset & speed
                float randSpeed  = lerp(0.5, 1.5, hash21(cell + 12.34));
                float randOffset = hash21(cell + 56.78) * 6.2831;

                // Nhấp nháy riêng từng hạt
                float sparkle = sin(_Time.y * _GlitterSpeed * randSpeed + randOffset) * 0.5 + 0.5;
                sparkle = pow(sparkle, 6);

                // Màu pixel gốc sang HSV
                float3 hsv = RGBtoHSV(baseCol.rgb);

                // Glitter = màu biến thiên quanh pixel gốc
                float3 glitterHSV = hsv;
                glitterHSV.x += (n - 0.5) * _HueRange;               // lệch hue nhẹ
                glitterHSV.z += (sparkle - 0.5) * _ValueRange;       // sáng/tối theo sparkle
                glitterHSV.x = frac(glitterHSV.x);

                float3 glitterColor = HSVtoRGB(glitterHSV);

                // Accent cho vùng tối (nếu muốn vàng cam nhấn vào shadow)
                float brightness = hsv.z;
                glitterColor = lerp(glitterColor, _DarkAccentColor.rgb, (1.0 - brightness) * 0.6);

                // Pha trắng khi sparkle cao
                glitterColor = lerp(glitterColor, 1.0.xxx, sparkle * _WhiteSparkle);

                // Blend glitter vào base (giữ base nguyên, glitter chồng lên)
                baseCol.rgb = lerp(baseCol.rgb, glitterColor, sparkle * _GlitterStrength);

                return baseCol;
            }
            ENDCG
        }
    }
}
