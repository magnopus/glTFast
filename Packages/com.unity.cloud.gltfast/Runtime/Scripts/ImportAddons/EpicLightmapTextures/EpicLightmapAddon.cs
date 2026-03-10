using UnityEngine;

namespace GLTFast.Addons
{
    /// <summary>
    /// Import addon for the EPIC_lightmap_textures extension.
    /// </summary>
    public class EpicLightmapAddon : ImportAddon<EpicLightmapAddonInstance>
    {
        // Note: Disable for now as oko package currently rely on gltfutility shader with different shader property names for epic lightmap.
        ///// <summary>
        ///// Automatically registers the addon when the application loads.
        ///// </summary>
        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        //static void Register()
        //{
        //    ImportAddonRegistry.RegisterImportAddon(new EpicLightmapAddon());
        //}
    }
}
