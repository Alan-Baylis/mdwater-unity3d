Shader "MdWater/MdWater"
{
	Properties
	{
		_MainTex                        ("Base (RGB)"                , 2D)        = "white" {}
		[HideInInspector] _ReflectionTex(""                          , 2D)        = "white" {}
		[HideInInspector] _VertTex      ("Vertex Modify"             , 2D)        = "" {}

		
		// global
		gw_fFrameTime                   ("gw_fFrameTime"             , float)     = 0
		gw_EyePos                       ("gw_EyePos"                 , Vector)    = (1, 1, 1, 1)
		gw_WaterColor                   ("gw_WaterColor"             , Vector)    = (0.12, 0.22, 0.29)
		gw_fWaveVertexSpacing           ("gw_fWaveVertexSpacing"     , float)     = 10
		gw_fWaveRatio                   ("gw_fWaveRatio"             , float)     = 0.5

		// normal map
		gw_TexOffsets                   ("gw_TexOffsets"             , Vector)    = (0, 0, 0.2, 0.3)
		gw_fNormalUVScale0              ("gw_fNormalUVScale0"        , float)     = 2
		gw_fNormalUVScale1              ("gw_fNormalUVScale1"        , float)     = 2
		gw_fNormalRatio                 ("gw_fNormalRatio"           , float)     = 0.5					// 2张normal图之间的比例
		gw_fNormalNoise                 ("gw_fNormalNoise"           , float)     = 0.05				// normal扰动强度
		gw_fNoNoiseScreen               ("gw_fNoNoiseScreen"         , float)     = 0.03				// normal扰动在屏幕边缘变弱

		// fresnel
		gw_fRefractRadius               ("gw_fRefractRadius"         , float)     = 5000				// 折射圈: 离镜头很近，折射很强；离镜头远，海水颜色很强
		gw_fRefractMinAlpha             ("gw_fRefractMinAlpha"       , float)     = 0.82			    // 折射最小alpha
		gw_fFresnelBias                 ("gw_fFresnelBias"           , float)     = 0.1
		gw_fFresnelScale                ("gw_fFresnelScale"          , float)     = 1.3
		gw_fFresnelPower                ("gw_fFresnelPower"          , float)     = 0.3
		gw_fFresnelMode                 ("gw_fFresnelMode"           , float)     = 1
		gw_bRefract                     ("gw_bRefract"               , float)     = 1					// 是否折射

		// caustics
		gw_fCausticsUVScale             ("gw_fCausticsUVScale"       , float)     = 20				    // 刻蚀图uv密度
		gw_fCausticsDepth               ("gw_fCausticsDepth"         , float)     = 260					// 多深的水才没有刻蚀图
		gw_fWorldSideLengthX            ("gw_fWorldSideLengthX"      , float)     = 1843200		        // 整个大世界的宽
		gw_fWorldSideLengthY            ("gw_fWorldSideLengthY"      , float)     = 1843200		        // 整个大世界的高
		gw_fCaustics                    ("gw_fCaustics"              , float)     = 1					// 如果是0.0f（通常是因为heightmap初始化失败了），就没有刻蚀图

		// sun
		gw_SunLightDir                  ("gw_SunLightDir"            , Vector)    = (-0.5, 0, -0.1)		// 这里有点奇怪，xy是光的方向的负数，z是光的方向
		gw_SunColor                     ("gw_SunColor"               , Vector)    = (1.2, 0.4, 0.1)		// 太阳光颜色
		gw_fSunFactor                   ("gw_fSunFactor"             , float)     = 1.5					// 太阳高光强度
		gw_fSunPower                    ("gw_fSunPower"              , float)     = 250					// how shiny we want the sun specular term on the water to be.
		gw_fSunNormalSpacing            ("gw_fSunNormalSpacing"      , float)     = 0.007			    // 
		gw_fSunNormalRatio              ("gw_fSunNormalRatio"        , float)     = 0.86				//

		// fog
		gw_fFogEnable                   ("gw_fFogEnable"             , float)     = 1					// 是否启用雾
		gw_fFog                         ("gw_fFog"                   , Vector)    = (1, 1, 1, 0)		// xyz颜色 w深度
		gw_fFogMaxDistance              ("gw_fFogMaxDistance"        , float)     = 30000				// 这个距离之外fog强度都为1

		// noise from proj grid:
		gw_fNoiseDisplacementX          ("gw_fNoiseDisplacementX"    , float)     = 0			        // proj grid noise 滚屏参数
		gw_fNoiseDisplacementY          ("gw_fNoiseDisplacementY"    , float)     = 0
		gw_fNoiseWaveHeightDiv          ("gw_fNoiseWaveHeightDiv"    , float)     = 60		            // 高度倒数修正
		gw_np_size                      ("gw_np_size"                , float)     = 128					// 要和c++的宏一致
		gw_waterlv2                     ("gw_waterlv2"               , float)     = 65					// 要和c++的宏一致

		// low profile alpha
		gw_fAlphaRadius                 ("gw_fAlphaRadius"           , float)     = 2000				// 低配水20米外alpha=1
		gw_fAlphaMin                    ("gw_fAlphaMin"              , float)     = 0.88				// camera处alpha为0.88

		// grey
		gw_fGrey                        ("gw_fGrey"                  , float)     = 0					// grey out when dead
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
			float4 _MainTex_ST;
			
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
				pos.y += tex.r - 0.5;
				o.pos = mul (UNITY_MATRIX_MVP, pos);
				o.refl = ComputeScreenPos(o.pos);
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
