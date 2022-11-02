﻿Shader "Unlit/HZBBuild"
{
    Properties
    {
        _DepthTexture("Depth Texture", 2D) = "black" {}
        _InvSize("Inverse Mipmap Size", Vector) = (0, 0, 0, 0) //x,y = (1/MipMapSize.x, 1/MipMapSize.y), zw = (0, 0)
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Name "HZBBuild"

        Pass
        {
            CGPROGRAM
            #pragma vertex HZBVert
            #pragma fragment HZBBuildFrag
			
            #include "UnityCG.cginc"
            
            sampler2D _DepthTexture;
 			float4 _InvSize;    // 1/w , 1/h 一个像素的uv长宽

            struct HZBAttributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct HZBVaryings
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            inline float HZBReduce(sampler2D  mainTex, float2 inUV, float2 invSize)
            {
                float4 depth;
                float2 uv0 = inUV + float2(-0.5f, -0.5f) * invSize;
                float2 uv1 = inUV + float2(0.5f, -0.5f) * invSize;
                float2 uv2 = inUV + float2(-0.5f, 0.5f) * invSize;
                float2 uv3 = inUV + float2(0.5f, 0.5f) * invSize;
                depth.x = tex2D(mainTex, uv0);
                depth.y = tex2D(mainTex, uv1);
                depth.z = tex2D(mainTex, uv2);
                depth.w = tex2D(mainTex, uv3);
                //接下来取最远的点的深度值
            #if defined(UNITY_REVERSED_Z)  
                return min(min(depth.x, depth.y), min(depth.z, depth.w));
            #else
                return max(max(depth.x, depth.y), max(depth.z, depth.w));
            #endif
            }

            HZBVaryings HZBVert(HZBAttributes v)
            {
                HZBVaryings o;
                o.vertex = UnityObjectToClipPos(v.vertex.xyz);
                o.uv = v.uv;

                return o;
            }

            float4 HZBBuildFrag(HZBVaryings input) : Color
			{	   
				float2 invSize = _InvSize.xy;
				float2 inUV = input.uv;

				float depth = HZBReduce(_DepthTexture, inUV, invSize);

				return float4(depth, 0, 0, 1.0f);
			}
            ENDCG
        }
    }
}
