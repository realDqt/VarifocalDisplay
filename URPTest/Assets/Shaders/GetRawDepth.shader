Shader "Unlit/GetRawDepth"
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
				float3 vPos : POSITION1;
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
				o.vPos = vertexInput.positionVS;

				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.scrPos = ComputeScreenPos(vertexInput.positionCS);

				return o;
			}

			float frag(Varyings i) : SV_Target
			{
				return i.vPos.z;
			}
			ENDHLSL
			}
		}
}
