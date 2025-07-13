Shader "Unlit/DepthTestShader"
{
     Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" "Queue" = "Geometry+100" }
			Pass
			{
				Tags
				{
					"LightMode" = "UniversalForward"
				}
				
				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

				struct Attributes
				{
					float4 vertex : POSITION;
					float4 uv : TEXCOORD0;

				};

				struct Varyings
				{
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float4 scrPos : TEXCOORD3;
				};


				CBUFFER_START(UnityPerMaterial)
				float4 _MainTex_ST;
				CBUFFER_END

				TEXTURE2D(_MainTex);
				SAMPLER(samper_MainTex);
				TEXTURE2D_X_FLOAT(_CameraDepthTexture);
				SAMPLER(sampler_CameraDepthTexture);

				Varyings vert(Attributes v)
				{
					Varyings o;
					VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
					o.pos = vertexInput.positionCS;

					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.scrPos = ComputeScreenPos(vertexInput.positionCS);

					return o;
				}

				float4 frag(Varyings i) : SV_Target
				{
					half2 screenPos = i.scrPos.xy / i.scrPos.w;
					float depth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, screenPos).r;
				    float depthValue = Linear01Depth(depth, _ZBufferParams);
					float3 finalColor = float3(depthValue, depthValue, depthValue);
					return float4(finalColor, 1.0f);
				}
				ENDHLSL
			}
		}
}
