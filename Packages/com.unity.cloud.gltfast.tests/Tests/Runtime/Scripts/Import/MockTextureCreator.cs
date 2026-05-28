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

namespace GLTFast.Tests.Import
{
    using Logging;
    using Schema;

    class MockTextureCreator : ITextureCreator
    {
        public ICodeLogger LastLogger { get; private set; }

#if UNITY_IMAGECONVERSION
        public int CreateTextureFromJpegOrPngCallCount { get; private set; }
        public ICodeLogger LoggerAtTimeOfJpegOrPngCreation { get; private set; }
#endif

#if KTX_IS_ENABLED
        public int CreateTextureFromKtxAsyncCallCount { get; private set; }
        public ICodeLogger LoggerAtTimeOfKtxCreation { get; private set; }
#endif

        public void SetLogger(ICodeLogger logger)
        {
            LastLogger = logger;
        }

#if UNITY_IMAGECONVERSION
        public Texture2D CreateTextureFromJpegOrPng(
            System.ReadOnlySpan<byte> data,
            Image img,
            int imageIndex,
            bool forceSampleLinear,
            bool markNonReadable,
            bool generateMipMaps,
            int anisoLevel)
        {
            LoggerAtTimeOfJpegOrPngCreation = LastLogger;
            CreateTextureFromJpegOrPngCallCount++;
            return new Texture2D(1, 1)
            {
                name = string.IsNullOrEmpty(img?.name) ? $"mock_image_{imageIndex}" : img.name
            };
        }
#endif // UNITY_IMAGECONVERSION

#if KTX_IS_ENABLED
        public Task<Texture2D> CreateTextureFromKtxAsync(
            NativeArray<byte>.ReadOnly data,
            Image img,
            int imageIndex,
            bool forceSampleLinear,
            bool readable,
            CancellationToken cancellationToken)
        {
            LoggerAtTimeOfKtxCreation = LastLogger;
            CreateTextureFromKtxAsyncCallCount++;
            return Task.FromResult(new Texture2D(1, 1)
            {
                name = string.IsNullOrEmpty(img?.name) ? $"mock_image_{imageIndex}" : img.name
            });
        }
#endif // KTX_IS_ENABLED
    }
}
