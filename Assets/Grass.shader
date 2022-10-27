Shader "Unlit/Grass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags{
            "Queue" = "Geometry+200"
            "IgnoreProjector" = "True"
            "DisableBatching" = "True"
        }
        Cull off

        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _GPUGRASS
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID

            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            StructuredBuffer<float3> posVisibleBuffer;

            v2f vert (appdata v,uint unity_InstanceID: SV_InstanceID)
            {
                v2f o;
            #ifdef _GPUGRASS
				float3 position =  posVisibleBuffer[unity_InstanceID] ;
				float rot =frac( sin(position.x)*100)*3.14*2;
				float crot, srot;
				sincos(rot, srot, crot);
				float4x4 o2w ;
				o2w._11_21_31_41 = float4(crot, 0, srot, 0);
				o2w._12_22_32_42 = float4(0, 1, 0, 0);
				o2w._13_23_33_43 = float4(-srot, 0, crot, 0);
				o2w._14_24_34_44 = float4(position.xyz,1);
                float4x4 w2o;
				w2o = o2w;
				w2o._14_24_34 *= -1;
				w2o._11_22_33 = 1.0f / w2o._11_22_33;
                o.vertex =  mul(UNITY_MATRIX_VP, mul(o2w, float4(v.vertex.xyz, 1.0)));
            #else                
                o.vertex = UnityObjectToClipPos(v.vertex);
            #endif
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                return fixed4(1,0,0,1);
		    #endif

                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
