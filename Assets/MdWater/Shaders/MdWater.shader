Shader "MdWater/MdWater"
{
	Properties
	{
		[Header(MdWater Mtl Properties)]
		[Space]
		[Space(10)]

		[Toggle(WIREFRAME)] _Wireframe    ("Wireframe?"				   , Float)     = 0
		[KeywordEnum(High, Low)] _WProfile("WProfile"			       , Float)		= 0


		// 注意float最大默认值为999999
		// 标注 * 的是修改过单位，从厘米 -> 米
		_MainTex                          ("Base (RGB)"                , 2D)        = "white" {}			// todo.ksh: 删除
		[HideInInspector] _ReflectionTex  ("Reflect Tex"               , 2D)        = "white" {}			// reflect tex
		[HideInInspector] _RefractionTex  ("Refract Tex"               , 2D)        = "white" {}			// refract tex
		[HideInInspector] _VertTex        ("Vertex Modify"             , 2D)        = "" {}					// noise tex
		gw_sNormal0						  ("Normal 0"                  , 2D)		= "white" {}			// normal 0
		gw_sNormal1						  ("Normal 1"                  , 2D)		= "white" {}			// normal 1
		gw_sCaustics				      ("Caustics"                  , 2D)		= "white" {}			// caustics
		
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
		gw_fNormalNoise                   ("gw_fNormalNoise"           , float)     = 0.05				    // normal扰动强度
		gw_fNoNoiseScreen                 ("gw_fNoNoiseScreen"         , float)     = 0.03				    // normal扰动在屏幕边缘变弱

		// fresnel
		gw_fRefractRadius                 ("gw_fRefractRadius"         , float)     = 50				    // *  折射圈: 离镜头很近，折射很强；离镜头远，海水颜色很强
		gw_fRefractMinAlpha               ("gw_fRefractMinAlpha"       , float)     = 0.82			        // 折射最小alpha
		gw_fFresnelBias                   ("gw_fFresnelBias"           , float)     = 0.1
		gw_fFresnelScale                  ("gw_fFresnelScale"          , float)     = 1.3
		gw_fFresnelPower                  ("gw_fFresnelPower"          , float)     = 0.3
		gw_fFresnelMode                   ("gw_fFresnelMode"           , float)     = 1
		gw_bRefract                       ("gw_bRefract"               , float)     = 1					    // 是否折射

		// caustics
		gw_fCausticsUVScale               ("gw_fCausticsUVScale"       , float)     = 20				    // 刻蚀图uv密度
		gw_fCausticsDepth                 ("gw_fCausticsDepth"         , float)     = 2.6					//  *  多深的水才没有刻蚀图
		gw_fWorldSideLengthX              ("gw_fWorldSideLengthX"      , float)     = 18432					//  *  整个大世界的宽
		gw_fWorldSideLengthY              ("gw_fWorldSideLengthY"      , float)     = 18432					//  *  整个大世界的高
		gw_fCaustics                      ("gw_fCaustics"              , float)     = 1						// 如果是0.0f（通常是因为heightmap初始化失败了），就没有刻蚀图

		// sun
		gw_SunLightDir                    ("gw_SunLightDir"            , Vector)    = (-0.5, 0, -0.1, 0)	// 这里有点奇怪，xy是光的方向的负数，z是光的方向
		gw_SunColor                       ("gw_SunColor"               , Color)     = (1.2, 0.4, 0.1, 1)	// 太阳光颜色
		gw_fSunFactor                     ("gw_fSunFactor"             , float)     = 1.5					// 太阳高光强度
		gw_fSunPower                      ("gw_fSunPower"              , float)     = 250					// how shiny we want the sun specular term on the water to be.
		gw_fSunNormalSpacing              ("gw_fSunNormalSpacing"      , float)     = 0.007					// 
		gw_fSunNormalRatio                ("gw_fSunNormalRatio"        , float)     = 0.86					//

		// fog
		gw_fFogEnable                     ("gw_fFogEnable"             , float)     = 1						// 是否启用雾
		gw_fFog                           ("gw_fFog"                   , Vector)    = (1, 1, 1, 0)			// xyz颜色 w深度
		gw_fFogMaxDistance                ("gw_fFogMaxDistance"        , float)     = 30000					// 这个距离之外fog强度都为1

		// noise from proj grid:
		gw_fNoiseDisplacementX            ("gw_fNoiseDisplacementX"    , float)     = 0						// proj grid noise 滚屏参数
		gw_fNoiseDisplacementY            ("gw_fNoiseDisplacementY"    , float)     = 0
		gw_fNoiseWaveHeightDiv            ("gw_fNoiseWaveHeightDiv"    , float)     = 2						//  *  高度/*倒数修正*/
		gw_np_size                        ("gw_np_size"                , float)     = 128					// 要和c++的宏一致
		gw_waterlv2                       ("gw_waterlv2"               , float)     = 65					// 要和c++的宏一致

		// low profile alpha
		gw_fAlphaRadius                   ("gw_fAlphaRadius"           , float)     = 2000					// 低配水20米外alpha=1
		gw_fAlphaMin                      ("gw_fAlphaMin"              , float)     = 0.88					// camera处alpha为0.88

		// grey
		gw_fGrey                          ("gw_fGrey"                  , float)     = 0						// grey out when dead
	}

	SubShader
	{
		Tags { "WaterMode" = "Refractive" "RenderType"="Opaque" } //"IgnoreProjector" = "True"
		LOD 100
 
		Pass {
			CGPROGRAM
			#pragma enable_d3d11_debug_symbols // 调试用
			#pragma target 3.0 // VTF
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			#pragma shader_feature WIREFRAME
			// low完全见不得人，最终去掉  todo.ksh
			#pragma multi_compile _WPROFILE_HIGH _WPROFILE_LOW
			#pragma multi_compile _WATER_REFRACTIVE _WATER_REFLECTIVE _WATER_SIMPLE

			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _VertTex;
			sampler2D _ReflectionTex;
			sampler2D _RefractionTex;
			sampler2D gw_sNormal0;
			sampler2D gw_sNormal1;
			sampler2D gw_sCaustics;

			float gw_np_size;
			float gw_waterlv2;
			float gw_fNoiseWaveHeightDiv;
			float gw_fNoiseDisplacementX;
			float gw_fNoiseDisplacementY;
			float4 gw_EyePos;
			float gw_fRefractRadius;
			float gw_fRefractMinAlpha;
			float gw_fNormalUVScale0;
			float gw_fNormalUVScale1;
			float4 gw_TexOffsets;
			float gw_fCausticsUVScale;
			float gw_fCausticsDepth;
			float gw_fCaustics;
			float gw_fWorldSideLengthX;
			float gw_fWorldSideLengthY;
			float gw_fNormalRatio;
			float gw_fNoNoiseScreen;
			float gw_fNormalNoise;
			float4 gw_WaterColor;
			float gw_fWaveVertexSpacing;
			float gw_bRefract;
			float gw_fWaveRatio;
			float gw_fFresnelBias;
			float gw_fFresnelScale;
			float gw_fFresnelPower;
			float gw_fSunNormalSpacing;
			float gw_fSunNormalRatio;
			float gw_fSunFactor;
			float4 gw_SunLightDir;
			float gw_fSunPower;
			float4 gw_SunColor;

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
				float4 pos			: SV_POSITION;
				float2 uv			: TEXCOORD0;
				float4 refl			: TEXCOORD1;
				float4 normal		: TEXCOORD2;
				float4 viewvec		: TEXCOORD3; // w = 顶点与镜头的距离 / gw_fRefractRadius
				float4 nmapUV		: TEXCOORD4; // xy是第一张uv，zw是第二张uv
				float4 causticsUV	: TEXCOORD5; // xy是刻蚀图uv，zw是地形高度图uv

				UNITY_FOG_COORDS(6) // 这里index要看前面已定义多少个TEXCOORD
			};
			
			v2f vert(appdata_full v)
			{
				v2f o;
				//o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv = CalcVertexTexcoord(float2(v.color.z, v.color.w));

				float vx = v.color.x;
				float vy = v.color.y;
				float fHeight = CalcVertexHeight(vx, vy);
				//v.vertex.y += fHeight; // 顶点先不动
				o.normal = CalcVertexNormal(vx, vy);
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.refl = ComputeScreenPos(o.pos);
				UNITY_TRANSFER_FOG(o, o.pos);

				float3 worldpos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.viewvec.xyz = worldpos - gw_EyePos.xyz; // z是高 -> y是高
				o.viewvec.w = min(gw_fRefractRadius, length(o.viewvec.xyz)); // 先算顶点到camera的距离
				o.viewvec.w = saturate(gw_fRefractMinAlpha + (1 - gw_fRefractMinAlpha) * o.viewvec.w / gw_fRefractRadius); // 再转成比例

				// Scroll normal maps and scale for tiling.
				o.nmapUV.xy = (o.uv * gw_fNormalUVScale0) + gw_TexOffsets.xy;
				o.nmapUV.zw = (o.uv * gw_fNormalUVScale1) + gw_TexOffsets.zw;

				// 刻蚀图（需要高度图）
				o.causticsUV.xy = o.uv * gw_fCausticsUVScale;
				o.causticsUV.zw = float2((o.pos.x + 4) / gw_fWorldSideLengthX, 1.0f - (o.pos.z - 4) / gw_fWorldSideLengthY); // y是高，所以用o.pos的xz

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 tex = tex2D(_MainTex, i.uv);

				// normal
				float3 n1 = tex2D(gw_sNormal0, i.nmapUV.xy).xzy;
				float3 n2 = tex2D(gw_sNormal1, i.nmapUV.zw).xzy;
				// Expand from [0, 1] compressed interval to true [-1, 1] interval.
				n1 = (n1 - 0.5f) * 2.0f;
				n2 = (n2 - 0.5f) * 2.0f;
				float3 normT = normalize(lerp(n1, n2, gw_fNormalRatio));

				// 屏幕边缘不要抖动，这里做一个线性弱化
				float2 SUV = i.refl.xy / i.refl.w; // SUV: Screen-space UV, 原来的 ProjTexcoord
				normT.x *= min(SUV.x, gw_fNoNoiseScreen) / gw_fNoNoiseScreen;
				normT.z *= min(SUV.y, gw_fNoNoiseScreen) / gw_fNoNoiseScreen; // 上边缘可以不用作此修正，可以省ps的一些寄存器slot
				normT.x *= min(1.0f - SUV.x, gw_fNoNoiseScreen) / gw_fNoNoiseScreen;
				normT.z *= min(1.0f - SUV.y, gw_fNoNoiseScreen) / gw_fNoNoiseScreen;

				// kuangsihao test: 水的深度
				float terrainHeight = -2.6;// tex2D(gw_sTerrainHeight, In.causticsUV.zw).x; // zw是高度图uv					// 修改这个来观察caustics
				float waterDepth = max(-terrainHeight, 0);
				float d = saturate(waterDepth / gw_fCausticsDepth);
				//d += (1 - d) * (1.0f - sign(gw_fCaustics));
				if (gw_fCaustics == 0.0f)
				{
					d = 1.0f;
				}

				//UNITY_PROJ_COORD：given a 4-component vector, return a texture coordinate suitable for projected texture reads.
				//On most platforms this returns the given value directly.
				float4 normD = float4(normT.xz * gw_fNormalNoise * i.refl.w, 0, 0);
				fixed4 ReflectionColor = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(i.refl + normD)); // tex2Dproj相当于tex2D(_ReflectionTex, i.refl.xy / i.refl.w);
				fixed4 RefractionColor = tex2Dproj(_RefractionTex, UNITY_PROJ_COORD(i.refl + normD));
				RefractionColor.rgb = lerp(RefractionColor.rgb, gw_WaterColor, (d + i.viewvec.w) / 2.0f); // viewvec.w的计算已移到vs
				fixed3 caustics = tex2D(gw_sCaustics, i.causticsUV.xy /*+ normT.xz * gw_fNormalNoise*/).xyz; // sample刻蚀图时就不用normal扰动了
				caustics *= saturate(1 - d);
				RefractionColor.rgb += (sign(gw_fCaustics)) * caustics;
				if (gw_bRefract == 0.0f)
				{
					RefractionColor.rgb = gw_WaterColor;
				}

				// 4个高度传入ps里算法线
				float4 noiseNormal = i.normal;

				// 计算此顶点normal和fresnel
				float3 posX0 = float3(-gw_fWaveVertexSpacing, noiseNormal.x, 0);				// todo.ksh: 这里4行：y才是高
				float3 posX1 = float3(+gw_fWaveVertexSpacing, noiseNormal.y, 0);
				float3 posY0 = float3(0, noiseNormal.z, -gw_fWaveVertexSpacing);
				float3 posY1 = float3(0, noiseNormal.w, +gw_fWaveVertexSpacing);
				float3 normWave = normalize(cross(posX1 - posX0, posY1 - posY0));
				float3 FNorm = lerp(normT, normWave, gw_fWaveRatio);
				FNorm = float3(0, 1, 0); // todo.ksh
				float FinalFresnel = dot(normalize(-i.viewvec.xyz), FNorm);
				//FinalFresnel = gw_fFresnelBias + gw_fFresnelScale * pow(abs(FinalFresnel), gw_fFresnelPower);
				//FinalFresnel = saturate(FinalFresnel);
				
				
				//FinalFresnel = 0.95; // todo.ksh
				float3 VVV = normalize(-i.viewvec.xyz);
				FinalFresnel = atan(abs(VVV.y) / sqrt(VVV.x * VVV.x + VVV.z * VVV.z));
				//FinalFresnel = VVV.x;
				//FinalFresnel = saturate(length(i.viewvec.xyz) / 100);
				fixed4 FFF = fixed4(FinalFresnel, 0, 0, 1);


				// ApplyWaterFresnel
				fixed4 finalColor = fixed4(lerp(ReflectionColor.rgb, RefractionColor.rgb, FinalFresnel), 1);

				// noise重新测试

				// 太阳高光
				float fSpacing = length(i.viewvec.xyz) * gw_fSunNormalSpacing; // 0.006
				posX0 = float3(-gw_fWaveVertexSpacing * fSpacing, noiseNormal.x, 0);			// todo.ksh: 这里4行：y才是高
				posX1 = float3(+gw_fWaveVertexSpacing * fSpacing, noiseNormal.y, 0);
				posY0 = float3(0, noiseNormal.z, -gw_fWaveVertexSpacing * fSpacing);
				posY1 = float3(0, noiseNormal.w, +gw_fWaveVertexSpacing * fSpacing);
				float3 SunNormWave = normalize(cross(posX1 - posX0, posY1 - posY0));
				float3 SunNorm = lerp(normT, SunNormWave, gw_fSunNormalRatio); // 0.84f
				float3 SR = normalize(reflect(-i.viewvec.xyz, SunNorm)); // todo.ksh: 这里SunNorm的y是高 reflect怎么写？
				float3 sunlight = gw_fSunFactor * pow(saturate(dot(SR, normalize(-gw_SunLightDir))), gw_fSunPower) * gw_SunColor;
				finalColor.rgb += sunlight;

				// fog
				UNITY_APPLY_FOG(i.fogCoord, finalColor); // UNITY_APPLY_FOG_COLOR(i.fogCoord, tex, fixed4(0, 0, 0, 0));
				

				//float2 R = i.refl.xy / i.refl.w; //test 可以看看反射、折射贴图的uv（ScreenSpace）
				//tex = fixed4(R.r, R.g, 0, 1);

				#if WIREFRAME
				tex = fixed4(1, 0, 0, 1);
				#endif
				#if _WPROFILE_LOW
				tex = fixed4(0, 0, 1, 1);
				#endif

				return FFF; // todo.ksh
				return finalColor; // tex *= ReflectionColor // tex = RefractionColor
			}

			ENDCG
	    }
	}
}
