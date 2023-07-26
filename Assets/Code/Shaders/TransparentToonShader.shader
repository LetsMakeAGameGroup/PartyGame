Shader "MyShader/TransparentToonShader"
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

		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("SrcBlend", Float) = 5 //"One"
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("DestBlend", Float) = 11 //"Zero"


		[Header(Other)]
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2 //"Back"
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4 //"LessEqual"
		[Enum(Off,0,On,1)] _ZWrite("ZWrite", Float) = 1.0 //"On"
	}

	SubShader
	{
		Tags {
			"RenderPipeline" = "UniversalPipeline"
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
			"UniversalMaterialType" = "Lit"
			"IgnoreProjector" = "True"
		}

		Pass
		{
			Tags {"LightMode" = "UniversalForward"}
			LOD 300

			Blend[_SrcBlend][_DstBlend]
            BlendOp Add
			ZTest[_ZTest]
			ZWrite[_ZWrite]
			Cull[_Cull]


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
		}
	}
}