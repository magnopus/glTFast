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

        /// <inheritdoc />
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<Texture2D> CreateTextureAsync(
            NativeArray<byte>.ReadOnly data,
            ImageFormat imageFormat,
            Image img,
            int imageIndex,
            bool forceSampleLinear,
            bool readable,
            bool generateMipMaps,
            int anisoLevel,
            CancellationToken cancellationToken)
#pragma warning restore CS1998
        {
            switch (imageFormat)
            {
#if UNITY_IMAGECONVERSION
                case ImageFormat.PNG:
                case ImageFormat.Jpeg:
                    return CreateTextureFromJpegOrPng(data, img, imageIndex, forceSampleLinear, readable, generateMipMaps, anisoLevel);
#endif // UNITY_IMAGECONVERSION
#if KTX_IS_ENABLED
                case ImageFormat.Ktx:
                    return await CreateTextureFromKtxAsync(data, img, imageIndex, forceSampleLinear, readable, cancellationToken);
#endif // KTX_IS_ENABLED
                default:
                    Logger?.Error(LogCode.ImageFormatUnknown, imageIndex.ToString(), string.Empty);
                    return null;
            }
        }

#if UNITY_IMAGECONVERSION
        static Texture2D CreateTextureFromJpegOrPng(
            NativeArray<byte>.ReadOnly data,
            Image img,
            int imageIndex,
            bool forceSampleLinear,
            bool readable,
            bool generateMipMaps,
            int anisoLevel)
        {
            var texture = CreateEmptyTexture(img, imageIndex, forceSampleLinear, generateMipMaps, anisoLevel);
            Profiler.BeginSample("Texture2D.LoadImage");
#if UNITY_6000_0_OR_NEWER
            texture.LoadImage(data.AsReadOnlySpan(), !readable);
#else
            texture.LoadImage(data.ToArray(), !readable);
#endif
            Profiler.EndSample();
            return texture;
        }
#endif // UNITY_IMAGECONVERSION

#if KTX_IS_ENABLED
        async Task<Texture2D> CreateTextureFromKtxAsync(
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
