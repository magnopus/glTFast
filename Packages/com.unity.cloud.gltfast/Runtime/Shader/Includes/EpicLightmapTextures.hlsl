//UNITY_SHADER_NO_UPGRADE
#ifndef HLSL_INCLUDE_EPIC_LIGHTMAP_TEXTURES
#define HLSL_INCLUDE_EPIC_LIGHTMAP_TEXTURES

/// @RT: Support for EPIC_lightmap_textures (https://github.com/magnopus/olympus-specs/blob/main/gltf-extensions/gltf-extensions.md#baked-lighting)
void glTFast_UnpackLightmap_float(UnityTexture2D lightmapTexture, float2 uv
, float4 coordinateScaleBias, float4 lightmapScale, float4 lightmapAdd, out float3 outDiffuseLighting)
{
    float2 scale = coordinateScaleBias.xy;
    float2 bias = coordinateScaleBias.zw;

    // Unflip: Unity UV (V=0 bottom) → glTF UV (V=0 top)
    float2 unflippedUV = float2(uv.x, 1.0 - uv.y);

    // Apply scale/bias and split in glTF space
    float2 LightmapUV0 = (unflippedUV * scale + bias) * float2(1.0, 0.5);
    float2 LightmapUV1 = LightmapUV0 + float2(0.0, 0.5);

    // Reflip back to Unity texture space for sampling
    LightmapUV0 = float2(LightmapUV0.x, 1.0 - LightmapUV0.y);
    LightmapUV1 = float2(LightmapUV1.x, 1.0 - LightmapUV1.y);

    float4 Lightmap0 = SAMPLE_TEXTURE2D_LOD(lightmapTexture.tex,
        lightmapTexture.samplerstate, LightmapUV0, 0);
    float4 Lightmap1 = SAMPLE_TEXTURE2D_LOD(lightmapTexture.tex,
        lightmapTexture.samplerstate, LightmapUV1, 0);

    float LogL = Lightmap0.w;
    LogL += Lightmap1.w * (1.0 / 255.0) - (0.5 / 255.0);
    LogL = LogL * lightmapScale.w + lightmapAdd.w;

    float3 UVW = Lightmap0.rgb * Lightmap0.rgb * lightmapScale.rgb + lightmapAdd.rgb;

    const float LogBlackPoint = 0.01858136;
    float L = max(0.0, exp2(LogL) - LogBlackPoint);

    float Directionality = 0.6;
    float Luma = L * Directionality;

    outDiffuseLighting = Luma * max(float3(0, 0, 0), UVW);
}
#endif
