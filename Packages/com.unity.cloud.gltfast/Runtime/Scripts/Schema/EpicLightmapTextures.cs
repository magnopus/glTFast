using System;

namespace GLTFast.Schema
{
  /// <summary>
  /// EPIC_lightmap_textures root extension (https://github.com/magnopus/olympus-specs/blob/main/gltf-extensions/gltf-extensions.md#baked-lighting)
  /// </summary>
  [Serializable]
    public class EpicLightmapTextures
    {
        /// <summary>
        /// Collection of lightmaps
        /// </summary>
        public EpicLightmap[] lightmaps;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            if (lightmaps != null)
            {
                writer.AddArray("lightmaps");
                foreach (var lightmap in lightmaps)
                {
                    lightmap.GltfSerialize(writer);
                }
                writer.CloseArray();
            }
            writer.Close();
        }

        /// <summary>
        /// Cleans up invalid parsing artifacts created by JsonUtility.
        /// </summary>
        /// <returns>True if element itself still holds value. False if it can be safely removed.</returns>
        public bool JsonUtilityCleanup()
        {
            return lightmaps != null && lightmaps.Length > 0;
        }
    }

    /// <summary>
    /// Individual lightmap data
    /// </summary>
    [Serializable]
    public class EpicLightmap
    {
        /// <summary>
        /// A name identifying the initial mesh associated with the lightmap when exported
        /// </summary>
        public string name;

        /// <summary>
        /// Texture reference
        /// </summary>
        public EpicLightmapTextureInfo texture;

        /// <summary>
        /// Used in the shader to remap the lightmap texture data to the correct range
        /// </summary>
        public float[] lightmapScale;

        /// <summary>
        /// Used in the shader to remap the lightmap texture data to the correct range
        /// </summary>
        public float[] lightmapAdd;

        /// <summary>
        /// Remaps the UV in the shader to sample from the appropriate texel(s)
        /// </summary>
        public float[] coordinateScaleBias;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            if (!string.IsNullOrEmpty(name))
            {
                writer.AddProperty("name", name);
            }
            if (texture != null)
            {
                writer.AddProperty("texture");
                texture.GltfSerialize(writer);
            }
            if (lightmapScale != null)
            {
                writer.AddArrayProperty("lightmapScale", lightmapScale);
            }
            if (lightmapAdd != null)
            {
                writer.AddArrayProperty("lightmapAdd", lightmapAdd);
            }
            if (coordinateScaleBias != null)
            {
                writer.AddArrayProperty("coordinateScaleBias", coordinateScaleBias);
            }
            writer.Close();
        }
    }

    /// <summary>
    /// Lightmap texture info
    /// </summary>
    [Serializable]
    public class EpicLightmapTextureInfo
    {
        /// <summary>
        /// The index of the lightmap texture within the array of textures in the glTF asset
        /// </summary>
        public int index = -1;

        /// <summary>
        /// The texture coordinate/UV set to be used to sample from the lightmap
        /// </summary>
        public int texCoord;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            if (index >= 0)
            {
                writer.AddProperty("index", index);
            }
            if (texCoord >= 0)
            {
                writer.AddProperty("texCoord", texCoord);
            }
            writer.Close();
        }
    }

    /// <summary>
    /// Node level EPIC_lightmap_textures extension
    /// </summary>
    [Serializable]
    public class EpicLightmapNode
    {
        /// <summary>
        /// Defines the index of the lightmap texture contained within the array of lightmaps to be used for this particular node.
        /// </summary>
        public int lightmap = -1;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            if (lightmap >= 0)
            {
                writer.AddProperty("lightmap", lightmap);
            }
            writer.Close();
        }
    }
}
