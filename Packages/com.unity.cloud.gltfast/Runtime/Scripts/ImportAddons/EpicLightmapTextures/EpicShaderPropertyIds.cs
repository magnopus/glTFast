using UnityEngine;

namespace GLTFast
{
    /// <summary>
    /// Property IDs and constants for the EPIC_lightmap_textures extension.
    /// </summary>
    public static class EpicShaderPropertyIds
    {
        /// <summary>
        /// Property ID for the EPIC lightmap texture.
        /// </summary>
        public static readonly int EpicLightmapId = Shader.PropertyToID("_EpicLightmap");

        /// <summary>
        /// Property ID for the EPIC lightmap scale factor.
        /// </summary>
        public static readonly int EpicLightmapScaleId = Shader.PropertyToID("_EpicLightmapScale");

        /// <summary>
        /// Property ID for the EPIC lightmap add factor.
        /// </summary>
        public static readonly int EpicLightmapAddId = Shader.PropertyToID("_EpicLightmapAdd");

        /// <summary>
        /// Property ID for the EPIC lightmap texture coordinate scale and bias.
        /// </summary>
        public static readonly int EpicLightmapSTId = Shader.PropertyToID("_EpicLightmap_ST");

        /// <summary>
        /// Property ID for the EPIC lightmap texcoord index to use for UV.
        /// </summary>
        public static readonly int EpicLightmapTexcoordIntId = Shader.PropertyToID("_EpicLightmapTexcoordInt");

        /// <summary>
        /// Shader keyword to enable EPIC lightmap logic.
        /// </summary>
        public const string EpicLightmapKeyword = "EPIC_LIGHTMAP_ON";
    }
}