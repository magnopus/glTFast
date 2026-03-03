using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using GLTFast.Schema;

namespace GLTFast
{
  /// <summary>
  /// Utility component to inspect loaded glTF data in the Unity Editor. Mainly for EPIC_lightmap_textures Support.
  /// </summary>
  [AddComponentMenu("glTF/Gltf Inspector")]
  public class GltfInspector : MonoBehaviour
  {
    [Serializable]
    public struct NodeLightmapData
    {
      public string nodeName;
      public int nodeIndex;
      public EpicLightmap data;
    }

    [Header("Settings")]
    [Tooltip("Direct path to the .gltf or .glb file (URL or local path)")]
    public string gltfUrl;

    [Tooltip("If true, the file will be loaded automatically on Start")]
    public bool loadOnStart = true;

    [SerializeField]
    [Tooltip("If checked, url is treated as relative StreamingAssets path.")]
    bool streamingAsset;

    [Header("Loaded Data (Read Only)")]
    [SerializeField]
    int m_NodeCount;
    [SerializeField]
    int m_TextureCount;
    [SerializeField]
    int m_MaterialCount;

    [Header("Lightmap Data (EPIC_lightmap_textures)")]
    [SerializeField]
    List<NodeLightmapData> m_NodeLightmaps = new List<NodeLightmapData>();

    [Header("Textures")]
    [SerializeField]
    List<Texture2D> m_LoadedTextures = new List<Texture2D>();

    GltfImport m_GltfImport;
    public string FullUrl => streamingAsset
            ? Path.Combine(Application.streamingAssetsPath, gltfUrl)
            : gltfUrl;

    async void Start()
    {
      if (loadOnStart && !string.IsNullOrEmpty(gltfUrl))
      {
        await LoadGltf();
      }
    }

    /// <summary>
    /// Loads the glTF file specified in gltfUrl.
    /// </summary>
    public async Task LoadGltf()
    {
      m_GltfImport = new GltfImport();
      var url = FullUrl;
      bool success = await m_GltfImport.Load(url);

      if (success)
      {
        PopulateInspectorData();
        Debug.Log($"[GltfInspector] Successfully loaded: {url}");
      }
      else
      {
        Debug.LogError($"[GltfInspector] Failed to load: {url}");
      }
    }

    void PopulateInspectorData()
    {
      var root = m_GltfImport.GetSourceRoot();
      if (root == null) return;

      m_NodeCount = root.Nodes?.Count ?? 0;
      m_TextureCount = m_GltfImport.TextureCount;
      m_MaterialCount = m_GltfImport.MaterialCount;

      // Collect all textures
      m_LoadedTextures.Clear();
      for (int i = 0; i < m_TextureCount; i++)
      {
        m_LoadedTextures.Add(m_GltfImport.GetTexture(i));
      }

      // Collect nodes with lightmap data
      m_NodeLightmaps.Clear();
      for (int i = 0; i < m_NodeCount; i++)
      {
        var lightmap = m_GltfImport.GetNodeLightmap(i);
        if (lightmap != null)
        {
          m_NodeLightmaps.Add(new NodeLightmapData
          {
            nodeName = root.Nodes[i].name ?? $"Node {i}",
            nodeIndex = i,
            data = lightmap
          });
        }
      }
    }
  }
}
