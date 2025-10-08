Shader"Unlit/Pixelate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixelSize ("Pixel Size (texels)", Range(1,128)) = 8
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
//LOD100
        Cull
Off ZWrite
Off ZTest
Always
        Blend
SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "UnityCG.cginc"

sampler2D _MainTex;
float4 _MainTex_TexelSize; // (1/width,1/height,width,height)
float4 _MainTex_ST; // tiling/offset
float _PixelSize;

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};
struct v2f
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
};

v2f vert(appdata v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
                // dùng scale/offset từ material
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    return o;
}

fixed4 frag(v2f i) : SV_Target
{
    float2 size = _MainTex_TexelSize.zw;
    float2 uvPix = floor(i.uv * size / _PixelSize) * (_PixelSize / size);
    return tex2D(_MainTex, uvPix);
}
            ENDCG
        }
    }
}
