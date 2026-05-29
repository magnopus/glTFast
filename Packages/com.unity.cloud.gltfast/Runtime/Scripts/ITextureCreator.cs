// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
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

        /// <summary>
        /// Creates and loads a <see cref="Texture2D"/> from compressed image data asynchronously.
        /// </summary>
        /// <param name="data">Raw image bytes.</param>
        /// <param name="imageFormat">Format of the image data.</param>
        /// <param name="img">Source glTF image descriptor.</param>
        /// <param name="imageIndex">Index of the image within the glTF.</param>
        /// <param name="forceSampleLinear">When true, the texture is created with a linear color format.</param>
        /// <param name="readable">When true, the texture remains CPU-readable after creation.</param>
        /// <param name="generateMipMaps">When true, mip maps are generated for the texture.</param>
        /// <param name="anisoLevel">Anisotropic filtering level for the texture.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The created <see cref="Texture2D"/>, or null on failure.</returns>
        Task<Texture2D> CreateTextureAsync(
            NativeArray<byte>.ReadOnly data,
            ImageFormat imageFormat,
            Image img,
            int imageIndex,
            bool forceSampleLinear,
            bool readable,
            bool generateMipMaps,
            int anisoLevel,
            CancellationToken cancellationToken);
    }
}
