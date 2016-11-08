Shader "Custom/TestNoise"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" {}

    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        Pass
        {
            CGPROGRAM

			#pragma multi_compile CNOISE PNOISE SNOISE SNOISE_AGRAD SNOISE_NGRAD
			#pragma multi_compile _ THREED
			#pragma multi_compile _ FRACTAL

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			
			#include "perlin.cginc"
			#include "fbm.cginc"

			#if defined(SNOISE) || defined(SNOISE_NGRAD)
				#if defined(THREED)
				#include "SimplexNoise3D.cginc"
				#else
				#include "SimplexNoise2D.cginc"
				#endif
			#elif defined(SNOISE_AGRAD)
				#if defined(THREED)
				#include "SimplexNoiseGrad3D.cginc"
				#else
				#include "SimplexNoiseGrad2D.cginc"
				#endif
			#else
				#if defined(THREED)
				#include "ClassicNoise3D.cginc"
				#else
				#include "ClassicNoise2D.cginc"
				#endif
			#endif

            sampler2D _MainTex;
            float4 _MainTex_ST;


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

			float4 TestNoise(float2 inputUV)
			{
				const float epsilon = 0.0001;

				float2 uv = inputUV * 4.0 + float2(0.2, 1) * _Time.y;

				#if defined(SNOISE_AGRAD) || defined(SNOISE_NGRAD)
					#if defined(THREED)
						float3 o = 0.5;
					#else
						float2 o = 0.5;
					#endif
				#else
					float o = 0.5;
				#endif

				float s = 1.0;

				#if defined(SNOISE)
					float w = 0.25;
				#else
					float w = 0.5;
				#endif

				#ifdef FRACTAL
				for (int i = 0; i < 6; i++)
				#endif
				{
					#if defined(THREED)
						float3 coord = float3(uv * s, _Time.y);
						float3 period = float3(s, s, 1.0) * 2.0;
					#else
						float2 coord = uv * s;
						float2 period = s * 2.0;
					#endif

					#if defined(CNOISE)
						o += cnoise(coord) * w;
					#elif defined(PNOISE)
						o += pnoise(coord, period) * w;
					#elif defined(SNOISE)
						o += snoise(coord) * w;
					#elif defined(SNOISE_AGRAD)
						o += snoise_grad(coord) * w;
					#else // SNOISE_NGRAD
						#if defined(THREED)
							float v0 = snoise(coord);
							float vx = snoise(coord + float3(epsilon, 0, 0));
							float vy = snoise(coord + float3(0, epsilon, 0));
							float vz = snoise(coord + float3(0, 0, epsilon));
							o += w * float3(vx - v0, vy - v0, vz - v0) / epsilon;
						#else
							float v0 = snoise(coord);
							float vx = snoise(coord + float2(epsilon, 0));
							float vy = snoise(coord + float2(0, epsilon));
							o += w * float2(vx - v0, vy - v0) / epsilon;
						#endif
					#endif

					s *= 2.0;
					w *= 0.5;
				}

				#if defined(SNOISE_AGRAD) || defined(SNOISE_NGRAD)
					#if defined(THREED)
						return float4(o, 1);
					#else
						return float4(o, 1, 1);
					#endif
				#else
					return float4(o, o, o, 1);
				#endif
			}

			
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv.zw = float2(0, 0);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv.xy);
				float c = 1;
				
				// fbm
				float POWER = 1;
				float SCALE = 1;
				float BIAS = 0;
				float2 p = i.uv.xy;
				c = POWER*fbm( SCALE*p ) + BIAS;
				col = fixed4(c, c, c, 1);

				// perlin
				//c = perlin(i.uv.xy, 1, _Time.y);
				//col = fixed4(c, c, c, 1);

				// test noise
				col = TestNoise(i.uv.xy);

				
                return col;
            }


            ENDCG
        }
    }
}
