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
using KtxUnity;
#endif
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;

namespace GLTFast
{
    using Logging;
    using Schema;

    /// <summary>
    /// Default implementation of <see cref="ITextureCreator"/> that creates
    /// <see cref="Texture2D"/> objects from compressed image data.
    /// </summary>
    public class DefaultTextureCreator : TextureCreator
    {
        static Texture2D CreateEmptyTexture(
            Image img,
            int index,
            bool forceSampleLinear,
            bool generateMipMaps,
            int anisoLevel)
        {
            var textureCreationFlags =
                TextureCreationFlags.DontUploadUponCreate | TextureCreationFlags.DontInitializePixels;
            if (generateMipMaps)
            {
                textureCreationFlags |= TextureCreationFlags.MipChain;
            }
            var txt = new Texture2D(
                4, 4,
                forceSampleLinear
                    ? GraphicsFormat.R8G8B8A8_UNorm
                    : GraphicsFormat.R8G8B8A8_SRGB,
                textureCreationFlags
            )
            {
                anisoLevel = anisoLevel,
                name = string.IsNullOrEmpty(img.name) ? $"image_{index}" : img.name
            };
            return txt;
        }

#if UNITY_IMAGECONVERSION
        /// <inheritdoc />
        public override Texture2D CreateTextureFromJpegOrPng(
            System.ReadOnlySpan<byte> data,
            Image img,
            int imageIndex,
            bool forceSampleLinear,
            bool markNonReadable,
            bool generateMipMaps,
            int anisoLevel)
        {
            var texture = CreateEmptyTexture(img, imageIndex, forceSampleLinear, generateMipMaps, anisoLevel);
            Profiler.BeginSample("Texture2D.LoadImage");
#if UNITY_6000_0_OR_NEWER
            texture.LoadImage(data, markNonReadable);
#else
            texture.LoadImage(data.ToArray(), markNonReadable);
#endif
            Profiler.EndSample();
            return texture;
        }
#endif // UNITY_IMAGECONVERSION

#if KTX_IS_ENABLED
        /// <inheritdoc />
        public override async Task<Texture2D> CreateTextureFromKtxAsync(
            NativeArray<byte>.ReadOnly data,
            Image img,
            int imageIndex,
            bool forceSampleLinear,
            bool readable,
            CancellationToken cancellationToken)
        {
            Profiler.BeginSample("LoadImageKtx");

            var ktxTexture = new KtxTexture();
            var errorCode = ktxTexture.Open(data);
            if (errorCode != ErrorCode.Success)
            {
                Logger?.Error(LogCode.EmbedImageLoadFailed);
                Profiler.EndSample();
                return null;
            }

            cancellationToken.ThrowIfCancellationRequestedWithTracking();

            // TODO implement cancellation in KTX package
            var result = await ktxTexture.LoadTexture2D(forceSampleLinear, readable);
            ktxTexture.Dispose();

            Texture2D texture = null;
            if (result.errorCode == ErrorCode.Success)
            {
                texture = result.texture;
                texture.name = string.IsNullOrEmpty(img.name) ? $"image_{imageIndex}" : img.name;
            }
            else
            {
                Logger?.Error(LogCode.EmbedImageLoadFailed);
            }

            Profiler.EndSample();
            return texture;
        }
#endif // KTX_IS_ENABLED
    }
}
