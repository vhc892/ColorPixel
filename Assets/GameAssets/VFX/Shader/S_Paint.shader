// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_Paint"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		_DrawTexture("DrawTexture", 2D) = "white" {}
		_TextureColor("TextureColor", Color) = (0,0,0,1)
		[Toggle]_IsDrawing("IsDrawing", Float) = 0
		_HighlightTexture("HighlightTexture", 2D) = "gray" {}
		[Toggle]_IsHighlight("IsHighlight", Float) = 0
		_Rotation("Rotation", Range( 0 , 2)) = 0.75
		_Tilling("Tilling", Vector) = (1,1,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}

	SubShader
	{
		LOD 0

		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		
		
		Pass
		{
		CGPROGRAM
			
			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"


			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				
			};
			
			uniform fixed4 _Color;
			uniform float _EnableExternalAlpha;
			uniform sampler2D _MainTex;
			uniform sampler2D _AlphaTex;
			uniform float _IsHighlight;
			uniform float _IsDrawing;
			uniform float4 _TextureColor;
			uniform float4 _MainTex_ST;
			uniform sampler2D _DrawTexture;
			uniform float4 _DrawTexture_ST;
			uniform sampler2D _HighlightTexture;
			uniform float2 _Tilling;
			uniform float _Rotation;

			
			v2f vert( appdata_t IN  )
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
				
				
				IN.vertex.xyz +=  float3(0,0,0) ; 
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

#if ETC1_EXTERNAL_ALPHA
				// get the color from an external texture (usecase: Alpha support for ETC1 on android)
				fixed4 alpha = tex2D (_AlphaTex, uv);
				color.a = lerp (color.a, alpha.r, _EnableExternalAlpha);
#endif //ETC1_EXTERNAL_ALPHA

				return color;
			}
			
			fixed4 frag(v2f IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float2 uv_MainTex = IN.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 tex2DNode2 = tex2D( _MainTex, uv_MainTex );
				float2 uv_DrawTexture = IN.texcoord.xy * _DrawTexture_ST.xy + _DrawTexture_ST.zw;
				float4 tex2DNode3 = tex2D( _DrawTexture, uv_DrawTexture );
				float4 appendResult7 = (float4(_TextureColor.rgb , ( tex2DNode2.a * tex2DNode3.a )));
				float4 appendResult10 = (float4(_TextureColor.rgb , tex2DNode2.a));
				float2 texCoord28 = IN.texcoord.xy * _Tilling + float2( 0,0 );
				float cos17 = cos( ( _Rotation * UNITY_PI ) );
				float sin17 = sin( ( _Rotation * UNITY_PI ) );
				float2 rotator17 = mul( texCoord28 - float2( 0.5,0.5 ) , float2x2( cos17 , -sin17 , sin17 , cos17 )) + float2( 0.5,0.5 );
				float4 tex2DNode11 = tex2D( _HighlightTexture, rotator17 );
				float temp_output_16_0 = ( tex2DNode11.a * tex2DNode2.a * abs( sin( ( _Time.y * 2.0 ) ) ) );
				float4 appendResult15 = (float4(tex2DNode11.rgb , temp_output_16_0));
				
				fixed4 c = (( _IsHighlight )?( appendResult15 ):( (( _IsDrawing )?( appendResult10 ):( appendResult7 )) ));
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19200
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-176.7328,-384.1725;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;17;-968.4214,-440.5119;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PiNode;18;-1218.421,-248.5119;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-1505.421,-241.5119;Inherit;False;Property;_Rotation;Rotation;5;0;Create;True;0;0;0;False;0;False;0.75;1.5;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;14;613.3685,-574.3691;Inherit;False;Property;_IsHighlight;IsHighlight;4;0;Create;True;0;0;0;False;0;False;0;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector2Node;29;-1421.97,-463.6827;Inherit;False;Property;_Tilling;Tilling;6;0;Create;True;0;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;28;-1279.97,-464.6827;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;3;-835.3961,585.5274;Inherit;True;Property;_DrawTexture;DrawTexture;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;8;-448.9385,666.3858;Inherit;False;2;0;FLOAT;0.001;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-210.3963,460.5274;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;5;-247.3963,233.5275;Inherit;False;Property;_TextureColor;TextureColor;1;0;Create;False;0;0;0;False;0;False;0,0,0,1;0,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;7;166.6037,336.5275;Inherit;False;COLOR;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;10;44.06159,83.38591;Inherit;False;COLOR;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ToggleSwitchNode;9;402.6398,87.24872;Inherit;False;Property;_IsDrawing;IsDrawing;2;0;Create;True;0;0;0;False;0;False;0;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;1;-906.2296,124.9883;Inherit;False;0;0;_MainTex;Shader;False;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-682.6315,204.8019;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;11;-569.8194,-555.7835;Inherit;True;Property;_HighlightTexture;HighlightTexture;3;0;Create;True;0;0;0;False;0;False;11;022063674484afe43aa16e1ba169f95b;022063674484afe43aa16e1ba169f95b;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;1516.319,-400.0697;Float;False;True;-1;2;ASEMaterialInspector;0;10;S_Paint;0f8ba0101102bb14ebf021ddadce9b49;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;False;True;3;1;False;;10;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;True;5;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;CanUseSpriteAtlas=True;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;0;;0;0;Standard;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.SinOpNode;30;-537.485,-140.1285;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;31;-964.785,-254.5285;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-721.7848,-134.5285;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;25;-402.9053,-309.2846;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;33;-346.8301,-762.8265;Inherit;False;Constant;_Color0;Color 0;7;0;Create;True;0;0;0;False;0;False;0.8301887,0.8301887,0.8301887,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;15;252.6027,-440.519;Inherit;False;COLOR;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;34;-68.63013,-228.5265;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
WireConnection;16;0;11;4
WireConnection;16;1;2;4
WireConnection;16;2;25;0
WireConnection;17;0;28;0
WireConnection;17;2;18;0
WireConnection;18;0;19;0
WireConnection;14;0;9;0
WireConnection;14;1;15;0
WireConnection;28;0;29;0
WireConnection;8;1;3;4
WireConnection;4;0;2;4
WireConnection;4;1;3;4
WireConnection;7;0;5;0
WireConnection;7;3;4;0
WireConnection;10;0;5;0
WireConnection;10;3;2;4
WireConnection;9;0;7;0
WireConnection;9;1;10;0
WireConnection;2;0;1;0
WireConnection;11;1;17;0
WireConnection;0;0;14;0
WireConnection;30;0;32;0
WireConnection;32;0;31;2
WireConnection;25;0;30;0
WireConnection;15;0;11;0
WireConnection;15;3;16;0
WireConnection;34;0;16;0
ASEEND*/
//CHKSM=7017C899128CD80858BF0DEFC0CF0DF63F3FFEC3