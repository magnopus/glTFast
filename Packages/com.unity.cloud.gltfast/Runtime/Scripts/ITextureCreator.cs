// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if KTX_IS_RECENT
#define KTX_IS_ENABLED
#elif KTX_IS_INSTALLED
#error You have to update the *KTX for Unity* package in package manager to enable support for KTX textures in *glTFast*.
#endif

using System.Threading;
using System.Threading.Tasks;
#if KTX_IS_ENABLED
using Unity.Collections;
#endif
using UnityEngine;

namespace GLTFast
{
    using Logging;
    using Schema;

    /// <summary>
    /// Creates <see cref="Texture2D"/> objects from compressed image data during glTF import.
    /// </summary>
    public interface ITextureCreator
    {
        /// <summary>
        /// Sets the logger used during texture creation. Pass null to clear.
        /// </summary>
        /// <param name="logger">Logger instance, or null to unset.</param>
        void SetLogger(ICodeLogger logger);

#if UNITY_IMAGECONVERSION
        /// <summary>
        /// Creates and loads a <see cref="Texture2D"/> from JPEG or PNG encoded data.
        /// </summary>
        /// <param name="data">Raw JPEG or PNG bytes.</param>
        /// <param name="img">Source glTF image descriptor.</param>
        /// <param name="imageIndex">Index of the image within the glTF.</param>
        /// <param name="forceSampleLinear">When true, the texture is created with a linear color format.</param>
        /// <param name="markNonReadable">When true, CPU-side pixel data is discarded after upload.</param>
        /// <param name="generateMipMaps">When true, mip maps are generated for the texture.</param>
        /// <param name="anisoLevel">Anisotropic filtering level for the texture.</param>
        /// <returns>The created and loaded <see cref="Texture2D"/>.</returns>
        Texture2D CreateTextureFromJpegOrPng(
            System.ReadOnlySpan<byte> data,
            Image img,
            int imageIndex,
            bool forceSampleLinear,
            bool markNonReadable,
            bool generateMipMaps,
            int anisoLevel);
#endif // UNITY_IMAGECONVERSION

#if KTX_IS_ENABLED
        /// <summary>
        /// Creates and loads a <see cref="Texture2D"/> from KTX2 encoded data asynchronously.
        /// </summary>
        /// <param name="data">Raw KTX2 bytes.</param>
        /// <param name="img">Source glTF image descriptor.</param>
        /// <param name="imageIndex">Index of the image within the glTF.</param>
        /// <param name="forceSampleLinear">When true, the texture is sampled in linear color space.</param>
        /// <param name="readable">When true, the texture remains CPU-readable after creation.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The created <see cref="Texture2D"/>, or null on failure.</returns>
        Task<Texture2D> CreateTextureFromKtxAsync(
            NativeArray<byte>.ReadOnly data,
            Image img,
            int imageIndex,
            bool forceSampleLinear,
            bool readable,
            CancellationToken cancellationToken);
#endif // KTX_IS_ENABLED
    }
}
