Shader "MMWater/MMWater"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_VertTex ("Vertex Modify", 2D) = "black" {}
		[HideInInspector] _ReflectionTex ("", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" } //"IgnoreProjector" = "True"
		LOD 100
 
		Pass {
			CGPROGRAM
			#pragma target 3.0 // VTF
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			sampler2D _VertTex;
			sampler2D _ReflectionTex;
			
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 refl : TEXCOORD1;
				float4 pos : SV_POSITION;
			};
			
			v2f vert(float4 pos : POSITION, float2 uv : TEXCOORD0)
			{
				v2f o;
				o.uv = TRANSFORM_TEX(uv, _MainTex);
				float4 tex = tex2Dlod(_VertTex, float4(o.uv, 0, 0));
				pos.y += tex.r;
				o.pos = mul (UNITY_MATRIX_MVP, pos);
				o.refl = ComputeScreenPos (o.pos);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 tex = tex2D(_MainTex, i.uv);
				fixed4 refl = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(i.refl));
				return tex * refl;
			}
			ENDCG
	    }
	}
}
