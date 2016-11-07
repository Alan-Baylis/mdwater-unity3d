Shader "Custom/test noise 1"
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

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "perlin.cginc"
			#include "fbm.cginc"

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
				
				float POWER = 1;
				float SCALE = 1;
				float BIAS = 0;
				float2 p = i.uv.xy;
				c = POWER*fbm( SCALE*p ) + BIAS;

				c = perlin(i.uv.xy, 1, _Time.y);



				col = fixed4(c, c, c, 1);
                return col;
            }


            ENDCG
        }
    }
}
