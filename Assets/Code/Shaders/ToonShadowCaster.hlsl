#pragma exclude_renderers gles gles3 glcore
#pragma target 4.5

#pragma vertex ShadowPassVertex
#pragma fragment ShadowPassFragment

float4 _BaseMap_ST;
float4 _BaseColor;
float _Cutoff;

// Material Keywords
#pragma shader_feature _ALPHATEST_ON
#pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A


// GPU Instancing
#pragma multi_compile_instancing
#pragma multi_compile _ DOTS_INSTANCING_ON

// Universal Pipeline Keywords
// (v11+) This is used during shadow map generation to differentiate between directional and punctual (point/spot) light shadows, as they use different formulas to apply Normal Bias
#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"