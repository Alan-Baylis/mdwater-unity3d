Shader "MdWater/MdWater"
{
	Properties
	{
		[Header(MdWater Mtl Properties)]
		[Space]
		[Space(10)]

		[Toggle(WIREFRAME)] _Wireframe    ("Wireframe?"				   , Float)     = 0
		[KeywordEnum(High, Low)] _Quality ("Overlay mode"			   , Float)		= 0


		// 注意float最大默认值为999999
		// 标注 * 的是修改过单位，从厘米 -> 米
		_MainTex                          ("Base (RGB)"                , 2D)        = "white" {}
		[HideInInspector] _ReflectionTex  (""                          , 2D)        = "white" {}
		[HideInInspector] _VertTex        ("Vertex Modify"             , 2D)        = "" {}
		
		// global
		gw_fFrameTime                     ("gw_fFrameTime"             , float)     = 0
		gw_EyePos                         ("gw_EyePos"                 , Vector)    = (1, 1, 1, 1)
		gw_WaterColor                     ("gw_WaterColor"             , Color)     = (0.12, 0.22, 0.29, 1.0)
		gw_fWaveVertexSpacing             ("gw_fWaveVertexSpacing"     , float)     = 10
		gw_fWaveRatio                     ("gw_fWaveRatio"             , float)     = 0.5

		// normal map
		gw_TexOffsets                     ("gw_TexOffsets"             , Vector)    = (0, 0, 0.2, 0.3)
		gw_fNormalUVScale0                ("gw_fNormalUVScale0"        , float)     = 2
		gw_fNormalUVScale1                ("gw_fNormalUVScale1"        , float)     = 2
		gw_fNormalRatio                   ("gw_fNormalRatio"           , float)     = 0.5					// 2张normal图之间的比例
		gw_fNormalNoise                   ("gw_fNormalNoise"           , float)     = 0.05				// normal扰动强度
		gw_fNoNoiseScreen                 ("gw_fNoNoiseScreen"         , float)     = 0.03				// normal扰动在屏幕边缘变弱

		// fresnel
		gw_fRefractRadius                 ("gw_fRefractRadius"         , float)     = 5000				// 折射圈: 离镜头很近，折射很强；离镜头远，海水颜色很强
		gw_fRefractMinAlpha               ("gw_fRefractMinAlpha"       , float)     = 0.82			    // 折射最小alpha
		gw_fFresnelBias                   ("gw_fFresnelBias"           , float)     = 0.1
		gw_fFresnelScale                  ("gw_fFresnelScale"          , float)     = 1.3
		gw_fFresnelPower                  ("gw_fFresnelPower"          , float)     = 0.3
		gw_fFresnelMode                   ("gw_fFresnelMode"           , float)     = 1
		gw_bRefract                       ("gw_bRefract"               , float)     = 1					// 是否折射

		// caustics
		gw_fCausticsUVScale               ("gw_fCausticsUVScale"       , float)     = 20				    // 刻蚀图uv密度
		gw_fCausticsDepth                 ("gw_fCausticsDepth"         , float)     = 260					// 多深的水才没有刻蚀图
		gw_fWorldSideLengthX              ("gw_fWorldSideLengthX"      , float)     = 18432		        //  *  整个大世界的宽
		gw_fWorldSideLengthY              ("gw_fWorldSideLengthY"      , float)     = 18432		        //  *  整个大世界的高
		gw_fCaustics                      ("gw_fCaustics"              , float)     = 1					// 如果是0.0f（通常是因为heightmap初始化失败了），就没有刻蚀图

		// sun
		gw_SunLightDir                    ("gw_SunLightDir"            , Vector)    = (-0.5, 0, -0.1)		// 这里有点奇怪，xy是光的方向的负数，z是光的方向
		gw_SunColor                       ("gw_SunColor"               , Color)     = (1.2, 0.4, 0.1, 1)	// 太阳光颜色
		gw_fSunFactor                     ("gw_fSunFactor"             , float)     = 1.5					// 太阳高光强度
		gw_fSunPower                      ("gw_fSunPower"              , float)     = 250					// how shiny we want the sun specular term on the water to be.
		gw_fSunNormalSpacing              ("gw_fSunNormalSpacing"      , float)     = 0.007			    // 
		gw_fSunNormalRatio                ("gw_fSunNormalRatio"        , float)     = 0.86				//

		// fog
		gw_fFogEnable                     ("gw_fFogEnable"             , float)     = 1					// 是否启用雾
		gw_fFog                           ("gw_fFog"                   , Vector)    = (1, 1, 1, 0)		// xyz颜色 w深度
		gw_fFogMaxDistance                ("gw_fFogMaxDistance"        , float)     = 30000				// 这个距离之外fog强度都为1

		// noise from proj grid:
		gw_fNoiseDisplacementX            ("gw_fNoiseDisplacementX"    , float)     = 0			        // proj grid noise 滚屏参数
		gw_fNoiseDisplacementY            ("gw_fNoiseDisplacementY"    , float)     = 0
		gw_fNoiseWaveHeightDiv            ("gw_fNoiseWaveHeightDiv"    , float)     = 2		            //  *  高度/*倒数修正*/
		gw_np_size                        ("gw_np_size"                , float)     = 128					// 要和c++的宏一致
		gw_waterlv2                       ("gw_waterlv2"               , float)     = 65					// 要和c++的宏一致

		// low profile alpha
		gw_fAlphaRadius                   ("gw_fAlphaRadius"           , float)     = 2000				// 低配水20米外alpha=1
		gw_fAlphaMin                      ("gw_fAlphaMin"              , float)     = 0.88				// camera处alpha为0.88

		// grey
		gw_fGrey                          ("gw_fGrey"                  , float)     = 0					// grey out when dead
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" } //"IgnoreProjector" = "True"
		LOD 100
 
		Pass {
			CGPROGRAM
			#pragma enable_d3d11_debug_symbols // 调试用
			#pragma target 3.0 // VTF
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			#pragma shader_feature WIREFRAME
			#pragma multi_compile _QUALITY_HIGH _QUALITY_LOW

			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			sampler2D _VertTex;
			sampler2D _ReflectionTex;
			float4 _MainTex_ST;

			float gw_np_size;
			float gw_waterlv2;
			float gw_fNoiseWaveHeightDiv;
			float gw_fNoiseDisplacementX;
			float gw_fNoiseDisplacementY;

			float CalcVertexHeight(float vx, float vy)
			{
				float nScale = 4; // 因为noise的每一跳距离是vspacing2，所以所有顶点计算displacement时都要乘以4； 换句话说gw_fNoiseDisplacementX每+1，noise图要跳4个点
				vx = vx + gw_fNoiseDisplacementX * nScale;
				vy = vy + gw_fNoiseDisplacementY * nScale;

				if (vx < 0)
					vx += gw_np_size;
				else if (vx >= gw_np_size)
					vx -= gw_np_size;
				if (vy < 0)
					vy += gw_np_size;
				else if (vy >= gw_np_size)
					vy -= gw_np_size;

				float fHeight = tex2Dlod(_VertTex, float4(vx / gw_np_size, vy / gw_np_size, 0, 0));
				fHeight = (fHeight - 0.5) * gw_fNoiseWaveHeightDiv * 2; // gw_fNoiseWaveHeightDiv原来是波浪高度倒数，这里含义变了；以后改名
				return fHeight;
			}

			float4 CalcVertexNormal(float vx, float vy)
			{
				return float4 (
					CalcVertexHeight(vx - 1, vy),
					CalcVertexHeight(vx + 1, vy),
					CalcVertexHeight(vx, vy - 1),
					CalcVertexHeight(vx, vy + 1)
					);
			}

			float2 CalcVertexTexcoord(float2 kInitialUV)
			{
				float fDeltaTc = 2.0f / (gw_waterlv2 - 1); // noise每一跳uv增量：ring2的uv总量是2，因为ring0有2*2个，uv从(4,4)到(6,6)；而noise每一跳是vspacing2
				float u = kInitialUV.x + gw_fNoiseDisplacementX * fDeltaTc;
				float v = kInitialUV.y + gw_fNoiseDisplacementY * fDeltaTc;
				return float2(u, v);
			}
			
			struct v2f
			{
				float4 pos		: SV_POSITION;
				float2 uv		: TEXCOORD0;
				float4 refl		: TEXCOORD1;
				float4 normal	: TEXCOORD2;
				float4 viewvec	: TEXCOORD3; // w = 顶点与镜头的距离 / gw_fRefractRadius
				
				UNITY_FOG_COORDS(4) // 这里index要看前面已定义多少个TEXCOORD
			};
			
			v2f vert(appdata_full v)
			{
				v2f o;
				//o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv = CalcVertexTexcoord(float2(v.color.z, v.color.w));

				float vx = v.color.x;
				float vy = v.color.y;
				float fHeight = CalcVertexHeight(vx, vy);
				v.vertex.y += fHeight;
				o.normal = CalcVertexNormal(vx, vy);
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);


				//o.viewvec.xyz = o.pos.xyz - gw_EyePos.xyz; // z是高
				//o.viewvec.w = min(gw_fRefractRadius, length(o.viewvec.xyz)); // 先算顶点到camera的距离
				//o.viewvec.w = saturate(gw_fRefractMinAlpha + (1 - gw_fRefractMinAlpha) * o.viewvec.w / gw_fRefractRadius); // 再转成比例


				o.refl = ComputeScreenPos(o.pos);
				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 tex = tex2D(_MainTex, i.uv);

				//UNITY_PROJ_COORD：given a 4-component vector, return a texture coordinate suitable for projected texture reads.
				//On most platforms this returns the given value directly.
				fixed4 refl = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(i.refl)); // 相当于tex2D(_ReflectionTex, i.refl.xy / i.refl.w);
				tex *= refl;

				UNITY_APPLY_FOG(i.fogCoord, tex); // UNITY_APPLY_FOG_COLOR(i.fogCoord, tex, fixed4(0, 0, 0, 0));

				#if WIREFRAME
				//tex = fixed4(1, 0, 0, 1);
				#endif
				#if _QUALITY_LOW
				//tex = fixed4(0, 0, 1, 1);
				#endif

				return tex;
			}

			ENDCG
	    }
	}
}
