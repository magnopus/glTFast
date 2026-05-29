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

        /// <inheritdoc />
        public abstract Task<Texture2D> CreateTextureAsync(
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
