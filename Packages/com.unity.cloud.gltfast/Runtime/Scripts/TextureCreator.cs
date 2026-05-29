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
    /// Common base class for implementations of <see cref="ITextureCreator"/>.
    /// </summary>
    public abstract class TextureCreator : ITextureCreator
    {
        static ITextureCreator s_DefaultTextureCreator;

        /// <summary>
        /// Provides the default texture creator used when no custom implementation is provided.
        /// </summary>
        /// <returns>The default <see cref="ITextureCreator"/>.</returns>
        public static ITextureCreator GetDefaultTextureCreator()
        {
            return s_DefaultTextureCreator ??= new DefaultTextureCreator();
        }

        /// <summary>
        /// Logger to be used for messaging. Can be null.
        /// </summary>
        protected ICodeLogger Logger { get; private set; }

        /// <inheritdoc />
        public void SetLogger(ICodeLogger logger)
        {
            Logger = logger;
        }

#if UNITY_IMAGECONVERSION
        /// <inheritdoc />
        public abstract Texture2D CreateTextureFromJpegOrPng(
            System.ReadOnlySpan<byte> data,
            Image img,
            int imageIndex,
            bool forceSampleLinear,
            bool markNonReadable,
            bool generateMipMaps,
            int anisoLevel);
#endif // UNITY_IMAGECONVERSION

#if KTX_IS_ENABLED
        /// <inheritdoc />
        public abstract Task<Texture2D> CreateTextureFromKtxAsync(
            NativeArray<byte>.ReadOnly data,
            Image img,
            int imageIndex,
            bool forceSampleLinear,
            bool readable,
            CancellationToken cancellationToken);
#endif // KTX_IS_ENABLED
    }
}
