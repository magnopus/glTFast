using GLTFast.Schema;
using UnityEngine;

namespace GLTFast.Addons
{
    /// <summary>
    /// Import addon instance for the EPIC_lightmap_textures extension.
    /// Handles applying lightmap data to materials during instantiation.
    /// </summary>
    public class EpicLightmapAddonInstance : ImportAddonInstance
    {
        GltfImport m_Gltf;

        public override bool SupportsGltfExtension(string extensionName)
        {
            return extensionName == ExtensionName.EpicLightmapTextures;
        }

        public override void Inject(GltfImportBase gltfImport)
        {
            m_Gltf = gltfImport as GltfImport;
            if (m_Gltf != null)
            {
                m_Gltf.AddImportAddonInstance(this);
            }
        }

        public override void Inject(IInstantiator instantiator)
        {
            if (instantiator is GameObjectInstantiator goInstantiator)
            {
                goInstantiator.MeshAdded += OnMeshAdded;
            }
        }

        void OnMeshAdded(GameObject gameObject, uint nodeIndex, string meshName, MeshResult meshResult, uint[] joints, uint? rootJoint, float[] morphTargetWeights, int meshNumeration)
        {
            if (m_Gltf != null)
            {
                // Retrieve lightmap data for the specific node
                var lightmapData = m_Gltf.GetNodeLightmap((int)nodeIndex);
                if (lightmapData != null)
                {
                    var renderer = gameObject.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        ApplyEpicLightmap(renderer, lightmapData);
                    }
                }
            }
        }

        void ApplyEpicLightmap(Renderer renderer, EpicLightmap data)
        {
            if (data != null && data.texture != null)
            {
                // Retrieve the texture referenced by the lightmap data
                var lmTexture = m_Gltf.GetTexture(data.texture.index);
                int texCoordIndex = data.texture.texCoord;

                // Only proceed if a valid texture is found
                if (lmTexture != null && texCoordIndex >= 0)
                {
                    var mats = renderer.sharedMaterials;
                    if(mats == null || mats.Length <= 0)
                    {
                        return;
                    }

                    // Create new material instances based on the existing materials to apply the lightmap properties without affecting other objects that might share the same material asset
                    UnityEngine.Material[] newMats = new UnityEngine.Material[mats.Length];
                    for (int i = 0; i < mats.Length; ++i)
                    {
                        var matRef = mats[i];
                        if (matRef == null) continue;

                        // Need to create a new material instance to avoid modifying the original material asset, which could affect other objects using the same material
                        UnityEngine.Material newMat = new UnityEngine.Material(matRef);
                        newMats[i] = newMat;

                        // Enable the keyword so the shader knows to use EPIC lightmap data
                        newMat.EnableKeyword(EpicShaderPropertyIds.EpicLightmapKeyword);

                        // Set the texture
                        newMat.SetTexture(EpicShaderPropertyIds.EpicLightmapId, lmTexture);

                        newMat.SetFloat(EpicShaderPropertyIds.EpicLightmapTexcoordIntId, texCoordIndex);

                        // Set the Scale and Add properties
                        if (data.lightmapScale != null && data.lightmapScale.Length >= 4)
                        {
                            newMat.SetVector(EpicShaderPropertyIds.EpicLightmapScaleId, new Vector4(data.lightmapScale[0], data.lightmapScale[1], data.lightmapScale[2], data.lightmapScale[3]));
                        }

                        if (data.lightmapAdd != null && data.lightmapAdd.Length >= 4)
                        {
                            newMat.SetVector(EpicShaderPropertyIds.EpicLightmapAddId, new Vector4(data.lightmapAdd[0], data.lightmapAdd[1], data.lightmapAdd[2], data.lightmapAdd[3]));
                        }

                        // Set the UV Translation (Scale and Bias)
                        if (data.coordinateScaleBias != null && data.coordinateScaleBias.Length >= 4)
                        {
                            newMat.SetVector(EpicShaderPropertyIds.EpicLightmapSTId, new Vector4(data.coordinateScaleBias[0], data.coordinateScaleBias[1], data.coordinateScaleBias[2], data.coordinateScaleBias[3]));
                        }
                    }
                    // Re-assign materials to ensure changes are applied (especially if using property blocks, though here we modify sharedMaterials)
                    renderer.sharedMaterials = newMats;
                }
            }
        }

        public override void Dispose()
        {
        }
    }
}
