Shader"Custom/SparkleMenu"
{
    Properties
    {
        // --- UI (Stencil & batching) ---
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        _MainTex ("Sprite Texture (UI batching)", 2D) = "white" {}

        // --- Sparkle Effect ---
        [Header(Sparkle Effect)]
        _SparkleOn ("Sparkle On", Float) = 0
        _SparkleColor ("Sparkle Color", Color) = (1, 1, 0.8, 1)
        _SparkleSpeed ("Sparkle Speed", Range(1, 20)) = 10
        _SparkleDensity ("Sparkle Density", Range(0.001, 0.2)) = 0.01
        _SparkleTex ("Sparkle Texture", 2D) = "white" {}
        _SparkleRotationSpeed ("Sparkle Rotation Speed", Range(-10, 10)) = 3
        _SparkleSize ("Sparkle Size", Range(0.1, 15)) = 1.5
        _RandomOffset ("Random Offset", Float) = 0
        _SparkleGlowIntensity ("Sparkle Glow Intensity", Range(0, 2)) = 0.5

    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest[unity_GUIZTestMode]

        // --- UI Masking / Stencil support ---
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // UI batching inputs
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _UseUIAlphaClip;

                        // Sparkle Properties
            float _SparkleOn;
            fixed4 _SparkleColor;
            float _SparkleSpeed;
            float _SparkleDensity;
            sampler2D _SparkleTex;
            float _SparkleRotationSpeed;
            float _SparkleSize;
            float _RandomOffset;
            float _SparkleGlowIntensity;


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = fixed4(0, 0, 0, 0);

                // --- Sparkle Effect ---
            if (_SparkleOn > 0.5)
            {
                float lifecycle_speed = _SparkleSpeed * 0.25;
                float grid_size = 10.0;
                float2 grid_uv = floor(i.uv * grid_size);
                float random_seed_for_cell = frac(sin(dot(grid_uv, float2(12.9898, 78.233))) * 43758.5453);
                float local_time = (_Time.y * lifecycle_speed) + _RandomOffset + random_seed_for_cell * 10.0;
                float lifecycle_id = floor(local_time);
                float progress_in_cycle = frac(local_time);
                float star_chance_noise = frac(sin(dot(grid_uv + lifecycle_id, float2(4.898, 7.23))) * 2568.5453);
                if (star_chance_noise > (1.0 - _SparkleDensity))
                {
                    float size = sin(progress_in_cycle * 3.14159);
                    float2 cell_uv = frac(i.uv * grid_size) - 0.5;

                    float angle = _Time.y * _SparkleRotationSpeed;
                    float s = sin(angle);
                    float c = cos(angle);
                    float2x2 rotation_matrix = float2x2(c, -s, s, c);
                    float2 rotated_uv = mul(rotation_matrix, cell_uv);
                    float2 sparkle_tex_uv = rotated_uv / ((0.5 / _SparkleSize) * (0.01 + size)) + 0.5;
            
                    float star_alpha = tex2D(_SparkleTex, sparkle_tex_uv).a * size;

                    // (glow)
                    float distance_from_center = length(rotated_uv);
                    float glow_falloff = 1.0 - smoothstep(0.0, 0.4, distance_from_center);
                    float glow_alpha = pow(glow_falloff, 2) * size * _SparkleGlowIntensity;
            
                    float final_alpha = star_alpha + glow_alpha;

                    if (final_alpha > 0.01)
                    {
                        col = fixed4(_SparkleColor.rgb * final_alpha, final_alpha);
                    }
                    //if (star_alpha > 0.01)
                    //{
                    //    col = fixed4(_SparkleColor.rgb * star_alpha, star_alpha);
                    //}
                }
            }

                // Alpha clip theo UI nếu bật
    if (_UseUIAlphaClip > 0.5 && col.a < 0.001)
        discard;

    return col;
}
            ENDCG
        }
    }
}
