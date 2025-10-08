Shader "Custom/PixelGridBorderNumbers"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _IndexTex ("Index Texture", 2D) = "white" {}
        _NumberAtlas ("Number Atlas", 2D) = "white" {}
        _AtlasSize ("Atlas Size (cols, rows)", Vector) = (10, 10, 0, 0)

        _BorderColor ("Border Color", Color) = (0,0,0,1)
        _BorderGray ("Border Gray", Color) = (0.35,0.35,0.35,1)
        _BorderThickness ("Border Thickness", Range(0,0.5)) = 0.05
        _MinScale ("Min Scale", Float) = 7
        _MaxScale ("Max Scale", Float) = 12
        _CurScale ("Current Scale", Float) = 1
        _WhiteOnZoomStrength ("White on Zoom Strength", Range(0,1)) = 1

        [Header(Color Blinking)]

        _SelectedIndex ("Selected Color Index", Float) = -1
        _BlinkOn       ("Blink On", Float) = 0
        _BlinkSpeed    ("Blink Speed", Range(0.1, 10)) = 3
        _BlinkMin      ("Blink Min", Range(0.0, 1.0)) = 0.8   // xám đậm hơn
        _BlinkMax      ("Blink Max", Range(0.0, 2.0)) = 1.2   // xám nhạt hơn

        [Header(Sparkle Effect)]

        _SparkleOn ("Sparkle On", Float) = 0
        _SparkleColor ("Sparkle Color", Color) = (1, 1, 0.8, 1)
        _SparkleSpeed ("Sparkle Speed", Range(1, 20)) = 10
        _SparkleDensity ("Sparkle Density", Range(0.001, 0.2)) = 0.01
        _SparkleTex ("Sparkle Texture", 2D) = "white" {}
        _SparkleRotationSpeed ("Sparkle Rotation Speed", Range(-10, 10)) = 3
        _SparkleMinSize ("Sparkle Min Size", Range(0.1, 5)) = 0.6
        _SparkleMaxSize ("Sparkle Max Size", Range(0.1, 15)) = 2.0        
        _ArtworkMask ("Artwork Mask", 2D) = "white" {}
        _SparkleGlowIntensity ("Sparkle Glow Intensity", Range(0, 2)) = 0.5
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _IndexTex;
            sampler2D _NumberAtlas;

            float4 _MainTex_TexelSize;
            float4 _AtlasSize;

            float4 _BorderColor;
            float4 _BorderGray;
            float _BorderThickness;
            float _MinScale, _MaxScale, _CurScale;

            float _WhiteOnZoomStrength;

            float _SelectedIndex;
            float _BlinkOn;
            float _BlinkSpeed;
            float _BlinkMin;
            float _BlinkMax;

            float4 _TextureSize; // đã có trong shader hiện tại (bạn đang set từ C#):contentReference[oaicite:2]{index=2}

            float _SparkleOn;
            fixed4 _SparkleColor;
            float _SparkleSpeed;
            float _SparkleDensity;
            sampler2D _SparkleTex;
            float _SparkleRotationSpeed;
            float _SparkleMinSize;
            float _SparkleMaxSize;
            sampler2D _ArtworkMask;
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Kích thước 1 texel
                float2 texel = _MainTex_TexelSize.xy;

                // Láng giềng
                float leftAlpha   = tex2D(_MainTex, i.uv + float2(-texel.x, 0)).a;
                float rightAlpha  = tex2D(_MainTex, i.uv + float2( texel.x, 0)).a;
                float downAlpha   = tex2D(_MainTex, i.uv + float2(0, -texel.y)).a;
                float upAlpha     = tex2D(_MainTex, i.uv + float2(0,  texel.y)).a;

                // Giảm nửa nếu cạnh đó có hàng xóm alpha > 0.1
                float thickLeft   = (leftAlpha  > 0.1) ? _BorderThickness * 0.5 : _BorderThickness;
                float thickRight  = (rightAlpha > 0.1) ? _BorderThickness * 0.5 : _BorderThickness;
                float thickDown   = (downAlpha  > 0.1) ? _BorderThickness * 0.5 : _BorderThickness;
                float thickUp     = (upAlpha    > 0.1) ? _BorderThickness * 0.5 : _BorderThickness;

                float2 pixelCoord = i.uv / _MainTex_TexelSize.xy;
                float2 fracCoord  = frac(pixelCoord);

                bool isBorder =
                    fracCoord.x < thickLeft  || fracCoord.x > 1.0 - thickRight ||
                    fracCoord.y < thickDown  || fracCoord.y > 1.0 - thickUp;
                    
                fixed4 col = tex2D(_MainTex, i.uv);
                {
                    float t = saturate((_CurScale - _MinScale) / (_MaxScale - _MinScale));
                    t = smoothstep(0.0, 1.0, t);

                    if (col.a > 0.01)
                    {
                        float3 white = 1.0;
                        float blend = t * _WhiteOnZoomStrength;
                        col.rgb = lerp(col.rgb, white, blend);
                    }
                }
                // === Blink Selected Index ===
                if (_BlinkOn > 0.5)
                {
                // Lấy index tại UV hiện tại
                    float idx = tex2D(_IndexTex, i.uv).r * 255.0; // R8 0..255, bạn encode 0..99
                // Pixel chưa tô có alpha > ~0
                    if (idx > 0.5 && abs(idx - _SelectedIndex) < 0.5 && col.a > 0.01)
                    {
                    float wave = 0.5 + 0.5 * sin(_Time.y * _BlinkSpeed);
                    wave = pow(wave, 3.0); // giá trị > 1 làm sáng nhanh, tối lâu
                    float scale = lerp(_BlinkMin, _BlinkMax, wave);
                    col.rgb *= scale;
                    }
                }
                if (isBorder && col.a > 0.01)
                {
                    if (_CurScale >= _MinScale)
                    {
                        float t = saturate((_CurScale - _MinScale) / (_MaxScale - _MinScale));
                        fixed4 borderCol = lerp(_BorderGray, _BorderColor, t);
                        borderCol.a = t;
                        col.rgb = lerp(col.rgb, borderCol.rgb, borderCol.a);
                    }
                }

                // --- Number overlay ---
                if (_CurScale >= _MaxScale - 1.5)
                {
                    // Lấy số index từ IndexTex
                    float indexVal = tex2D(_IndexTex, i.uv).r * 255.0;
                    int numberIndex = (int)indexVal;

                    if (numberIndex > 0) // 0 = không vẽ số
                    {
                        // Tính cell trong atlas
                        int cols = (int)_AtlasSize.x;
                        int rows = (int)_AtlasSize.y;

                        int cx = numberIndex % cols;
                        int cy = rows - 1 - (numberIndex / cols);

                        float2 uvInPixel = frac(pixelCoord); // 0..1 trong pixel
            
                        float numberScale = 1;
                        uvInPixel = (uvInPixel - 0.5) * numberScale + 0.5;
            
                        float2 cellSize = 1.0 / _AtlasSize.xy;
                        float2 atlasUV = (float2(cx, cy) + uvInPixel) * cellSize;

                        fixed4 numCol = tex2D(_NumberAtlas, atlasUV);

                        // blend số lên ảnh gốc
                        col.rgb = lerp(col.rgb, numCol.rgb, numCol.a);
                    }
                }
                        // --- Sparkle Effect ---
            // Đọc giá trị từ "bản đồ" mask
            float mask = tex2D(_ArtworkMask, i.uv).r;

            // Thêm điều kiện "&& mask > 0.5" để đảm bảo chỉ lấp lánh trong vùng ảnh
            if (_SparkleOn > 0.5 && col.a < 0.01 && mask > 0.5)
            {
                float lifecycle_speed = _SparkleSpeed * 0.25;
    
                float grid_size = 30.0;
                float2 grid_uv = floor(i.uv * grid_size);
                float random_seed_for_cell = frac(sin(dot(grid_uv, float2(12.9898, 78.233))) * 43758.5453);
                float random_max_size = lerp(_SparkleMinSize, _SparkleMaxSize, random_seed_for_cell);


                float local_time = _Time.y * lifecycle_speed + random_seed_for_cell * 10.0;

                float lifecycle_id = floor(local_time);
                float progress_in_cycle = frac(local_time);

                float star_chance_noise = frac(sin(dot(grid_uv + lifecycle_id, float2(4.898, 7.23))) * 2568.5453);
    
                if (star_chance_noise > (1.0 - _SparkleDensity))
                {
                // Chu kỳ scale từ 0 -> 1 -> 0
                    float size = sin(progress_in_cycle * 3.14159);
        
                // Lấy tọa độ tương đối trong ô lưới (-0.5 đến 0.5)
                    float2 cell_uv = frac(i.uv * grid_size) - 0.5;

                // Tính toán góc xoay
                    float angle = _Time.y * _SparkleRotationSpeed;
                    float s = sin(angle);
                    float c = cos(angle);
                // Tạo ma trận xoay và áp dụng lên tọa độ của ngôi sao
                    float2x2 rotation_matrix = float2x2(c, -s, s, c);
                    float2 rotated_uv = mul(rotation_matrix, cell_uv);
        
                    float2 sparkle_tex_uv = rotated_uv / ((0.5 / random_max_size) * (0.01 + size)) + 0.5;

                // --- Bắt đầu logic mới ---
    // 1. Lấy alpha của hình ngôi sao sắc nét từ texture
            float star_alpha = tex2D(_SparkleTex, sparkle_tex_uv).a * size;

    // 2. Tính toán vầng sáng (glow) phía sau một cách thủ công
    // Tính khoảng cách từ tâm ngôi sao ra pixel hiện tại
            float distance_from_center = length(rotated_uv);
    // Tạo một vầng sáng tròn mềm mại, mờ dần khi ra xa tâm
            float glow_falloff = 1.0 - smoothstep(0.0, 0.4, distance_from_center); // 0.4 là bán kính của vầng sáng
    // Alpha của vầng sáng sẽ nhấp nháy theo 'size' và được điều khiển bởi Intensity
            float glow_alpha = pow(glow_falloff, 2) * size * _SparkleGlowIntensity;

    // 3. Kết hợp alpha của ngôi sao và vầng sáng lại với nhau
            float final_alpha = star_alpha + glow_alpha;

            if (final_alpha > 0.01)
            {
        // Trả về màu lấp lánh với độ trong suốt đã được kết hợp
                return fixed4(_SparkleColor.rgb * final_alpha, final_alpha);
            }
        }
            }
                return col;
            }
            ENDCG
        }
    }
}
