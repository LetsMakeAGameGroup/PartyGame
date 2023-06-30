Shader "MyShader/ToonShader"
{
	Properties
	{
		//Shader Properties

		[Header(Material Properties)]
		[MainColor] _BaseColor("BaseColor", Color) = (1,1,1,1)
		[MainTexture] _BaseMap("MainTexture", 2D) = "white" {}

		_AmbientColor("Ambient Color", Color) = (1,1,1,1)

		_SpecularOpacity("SpecularOpacity", Range(0, 1)) = 1
		_SpecularColor("Specular Color", Color) = (0,0,0,1)
		_Glossiness("Glossiness", Float) = 32

		_ShadowSmoothness("ShadowSmoothness", Range(0, 1)) = 0.01
		_SpecularSmoothness("_SpecularSmoothness", Range(0.005, 1)) = 0.01

		_RimColor("Rim Color", Color) = (1,1,1,1)
		_RimAmount("Rim Amount", Range(0, 1)) = 0.716
		_RimThreshold("Rim Threshold", Range(0, 1)) = 0.1
	}

	SubShader
	{
		Tags {
			"RenderPipeline" = "UniversalPipeline"
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
			"UniversalMaterialType" = "Lit"
			"IgnoreProjector" = "True"
		}

		Pass
		{
			Tags {"LightMode" = "UniversalForward"}

			
			HLSLPROGRAM

			#include "ToonShading.hlsl"

			ENDHLSL
		}
		Pass 
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On
			ZTest LEqual
			ColorMask 0
			Cull[_Cull]

			HLSLPROGRAM

			#include "ToonShadowCaster.hlsl"

			ENDHLSL
		}// DepthOnly, used for Camera Depth Texture (if cannot copy depth buffer instead, and the DepthNormals below isn't used)
		Pass {
			Name "DepthOnly"
			Tags { "LightMode" = "DepthOnly" }

			ColorMask 0
			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
			#pragma vertex DepthOnlyVertex
			#pragma fragment DepthOnlyFragment

			// Material Keywords
			#pragma shader_feature _ALPHATEST_ON
			#pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			// GPU Instancing
			#pragma multi_compile_instancing
			//#pragma multi_compile _ DOTS_INSTANCING_ON

			float4 _BaseMap_ST;
			float4 _BaseColor;
			float _Cutoff;


			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"

			// Note if we do any vertex displacement, we'll need to change the vertex function. e.g. :
			/*
			#pragma vertex DisplacedDepthOnlyVertex (instead of DepthOnlyVertex above)

			Varyings DisplacedDepthOnlyVertex(Attributes input) {
				Varyings output = (Varyings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				// Example Displacement
				input.positionOS += float4(0, _SinTime.y, 0, 0);

				output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
				output.positionCS = TransformObjectToHClip(input.position.xyz);
				return output;
			}
			*/

			ENDHLSL
		}
		// DepthNormals, used for SSAO & other custom renderer features that request it
		Pass {
			Name "DepthNormals"
			Tags { "LightMode" = "DepthNormals" }

			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
			#pragma vertex DepthNormalsVertex
			#pragma fragment DepthNormalsFragment

			// Material Keywords
			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature _ALPHATEST_ON
			#pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			// GPU Instancing
			#pragma multi_compile_instancing
			//#pragma multi_compile _ DOTS_INSTANCING_ON

			float4 _BaseMap_ST;
			float4 _BaseColor;
			float _Cutoff;

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/DepthNormalsPass.hlsl"

			// Note if we do any vertex displacement, we'll need to change the vertex function. e.g. :
			/*
			#pragma vertex DisplacedDepthNormalsVertex (instead of DepthNormalsVertex above)

			Varyings DisplacedDepthNormalsVertex(Attributes input) {
				Varyings output = (Varyings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				// Example Displacement
				input.positionOS += float4(0, _SinTime.y, 0, 0);

				output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
				output.positionCS = TransformObjectToHClip(input.position.xyz);
				VertexNormalInputs normalInput = GetVertexNormalInputs(input.normal, input.tangentOS);
				output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
				return output;
			}
			*/

			ENDHLSL
		}
	}
}