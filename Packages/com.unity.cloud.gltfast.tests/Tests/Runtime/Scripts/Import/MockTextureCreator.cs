// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace GLTFast.Tests.Import
{
    using Logging;
    using Schema;

    class MockTextureCreator : ITextureCreator
    {
        public ICodeLogger LastLogger { get; private set; }

        public int CreateTextureCallCount { get; private set; }
        public ImageFormat LastImageFormat { get; private set; }
        public ICodeLogger LoggerAtTimeOfCreation { get; private set; }

        public void SetLogger(ICodeLogger logger)
        {
            LastLogger = logger;
        }

        public Task<Texture2D> CreateTextureAsync(
            NativeArray<byte>.ReadOnly data,
            ImageFormat imageFormat,
            Image img,
            int imageIndex,
            bool forceSampleLinear,
            bool readable,
            bool generateMipMaps,
            int anisoLevel,
            CancellationToken cancellationToken)
        {
            LoggerAtTimeOfCreation = LastLogger;
            CreateTextureCallCount++;
            LastImageFormat = imageFormat;
            return Task.FromResult(new Texture2D(1, 1)
            {
                name = string.IsNullOrEmpty(img?.name) ? $"mock_image_{imageIndex}" : img.name
            });
        }
    }
}
