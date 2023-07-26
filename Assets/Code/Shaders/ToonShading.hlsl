#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#pragma vertex Vertex
#pragma fragment Fragment

#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
#pragma multi_compile_fragment _ _SHADOWS_SOFT


#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
#pragma multi_compile _ SHADOWS_SHADOWMASK

#pragma shader_feature_local _DISABLE_BLENDING
#pragma shader_feature _ALPHATEST_ON

float4 _BaseColor;
float4 _AmbientColor;
float4 _SpecularColor;
float _Glossiness;
float4 _RimColor;
float _RimAmount;
float _RimThreshold;

float _ShadowSmoothness;
float _SpecularOpacity;
float _SpecularSmoothness;

float _SurfaceType;

TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
float4 _BaseMap_ST;


struct Attributes {
	float3 positionOS : POSITION; // Position in object space
	float3 normalOS : NORMAL;
	float2 uv : TEXCOORD0;
};

struct Interpolators {
	float4 positionCS : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 positionWS : TEXCOORD1;
	float3 normalWS : TEXCOORD2;
	float4 shadowCoord : TEXCOORD3;
};


Interpolators Vertex(Attributes input) {
	Interpolators output;

	VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS);
	VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

	// Pass position and orientation data to the fragment function
	output.positionCS = posInputs.positionCS;
	output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
	output.positionWS = posInputs.positionWS;

	output.normalWS = normInputs.normalWS;

	#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	output.shadowCoord = GetShadowCoord(posInputs);
	#else
	output.shadowCoord = float4(0, 0, 0, 0);
	#endif

	return output;
}

float4 Fragment(Interpolators input) : SV_TARGET{

	float2 uv = input.uv;
	float4 colorSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);


	//Lighting
	float3 normal = normalize(input.normalWS);

	//Shadow
	#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	input.shadowCoord = input.shadowCoord;
	#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
	input.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
	#else
	input.shadowCoord = float4(0, 0, 0, 0);
	#endif

	float shadow = 1;
	Light mainLightShadow = GetMainLight(input.shadowCoord);
	shadow = mainLightShadow.shadowAttenuation;

	float4 additionalLightsColor = float4(0, 0, 0, 0);

	//Additional Lights
	#ifdef _ADDITIONAL_LIGHTS
	// Shade additional cone and point lights. Functions in URP/ShaderLibrary/Lighting.hlsl
	uint numAdditionalLights = GetAdditionalLightsCount();

	float additionalLightDirDot;

	for (uint lightI = 0; lightI < numAdditionalLights; lightI++)
	{
		Light additionalLight = GetAdditionalLight(lightI, input.positionWS, 1);

		float3 radiance = additionalLight.color * (additionalLight.distanceAttenuation * additionalLight.shadowAttenuation);

		float additionalLightShadow = AdditionalLightRealtimeShadow(_ADDITIONAL_LIGHTS, input.shadowCoord);

		float AdditionalLightNdotL = dot(additionalLight.direction, normal * additionalLightShadow);

		float lightIntensity = smoothstep(0, _ShadowSmoothness, AdditionalLightNdotL);

		radiance *= lightIntensity;

		additionalLightsColor += float4(radiance, 0);

		additionalLightDirDot = AdditionalLightNdotL;
	}

	#endif

	//Shading
	float NdotL = dot(GetMainLight().direction, normal * shadow);

	float lightIntensity = smoothstep(0, _ShadowSmoothness, NdotL);

	float4 lightColor = float4(GetMainLight().color, 1) * lightIntensity;

	//Specular
	float3 viewDirWS = GetWorldSpaceViewDir(input.positionWS);
	float3 viewDirNormalized = normalize(viewDirWS);

	float3 halfVector = normalize(GetMainLight().direction + viewDirNormalized);
	float NdotH = dot(normal, halfVector);

	float specularIntensity = pow(max(0, NdotH) * smoothstep(0, _ShadowSmoothness / 2, NdotL), _Glossiness * _Glossiness);
	float specularIntensitySmooth = smoothstep(0.005, _SpecularSmoothness, specularIntensity);
	float4 specular = specularIntensitySmooth * _SpecularColor;
	specular *= _SpecularOpacity;

	//Rim

	float rimIntensity = 0;

	if (_RimAmount == 0)
	{
		_RimThreshold = 0;
		rimIntensity = 0;
	}
	else
	{
		float rimDot = 1 - dot(viewDirNormalized, normal);
		rimIntensity = rimDot * pow(max(0, NdotL), _RimThreshold);
		rimIntensity = smoothstep(_RimAmount, _RimAmount, rimIntensity);
	}

	float4 outColor = colorSample * _BaseColor;

	float4 ambientColorFinal = (float4(half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w), 0));


	outColor *= (ambientColorFinal + (lightColor + additionalLightsColor) + (rimIntensity * _RimColor));
	outColor += specular;


	outColor.a = _BaseColor.a;


	return outColor;
}
