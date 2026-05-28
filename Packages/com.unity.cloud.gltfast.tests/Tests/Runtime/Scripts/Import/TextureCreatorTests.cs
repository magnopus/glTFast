// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if KTX_IS_RECENT
#define KTX_IS_ENABLED
#elif KTX_IS_INSTALLED
#error You have to update the *KTX for Unity* package in package manager to enable support for KTX textures in *glTFast*.
#endif

using System.Collections;
using System.Threading.Tasks;
using GLTFast.Logging;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GLTFast.Tests.Import
{
    [Category("Import")]
    class TextureCreatorTests
    {
        // Minimal valid 1x1 pixel PNG, base64-encoded.
        const string k_1x1PngBase64 =
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJ" +
            "AAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

        static string EmbeddedPngGltf(string imageName = null)
        {
            var nameField = imageName != null ? $@"""name"":""{imageName}"","  : string.Empty;
            return $@"{{""images"":[{{{nameField}""uri"":""data:image/png;base64,{k_1x1PngBase64}""}}],""textures"":[{{""source"":0}}]}}";
        }

#if UNITY_IMAGECONVERSION
        [UnityTest]
        public IEnumerator DefaultTextureCreatorIsUsedWhenNoneInjected()
        {
            var task = DefaultCreatorLoadsTexture();
            yield return AsyncWrapper.WaitForTask(task);
        }

        static async Task DefaultCreatorLoadsTexture()
        {
            var logger = new CollectingLogger();
            var import = new GltfImport(logger: logger);
            Assert.IsTrue(await import.LoadGltfJson(EmbeddedPngGltf()));
            Assert.IsNotNull(import.GetTexture(0),
                "GetTexture(0) should return a non-null Texture2D when using the default texture creator");
            LoggerTest.AssertLogger(logger);
        }

        [UnityTest]
        public IEnumerator MockTextureCreatorIsCalledForJpegOrPng()
        {
            var task = MockCreatorCalledForJpegOrPng();
            yield return AsyncWrapper.WaitForTask(task);
        }

        static async Task MockCreatorCalledForJpegOrPng()
        {
            var mock = new MockTextureCreator();
            var logger = new CollectingLogger();
            var import = new GltfImport(logger: logger, textureCreator: mock);
            Assert.IsTrue(await import.LoadGltfJson(EmbeddedPngGltf("test_image")));
            Assert.AreEqual(1, mock.CreateTextureFromJpegOrPngCallCount,
                "CreateTextureFromJpegOrPng should be called exactly once");
            var texture = import.GetTexture(0);
            Assert.IsNotNull(texture, "GetTexture(0) should return the mock's Texture2D");
            Assert.AreEqual("test_image", texture.name, "Texture name should come from the image descriptor");
            LoggerTest.AssertLogger(logger);
        }

        [UnityTest]
        public IEnumerator SetLoggerIsCalledBeforeTextureCreation()
        {
            var task = LoggerSetBeforeCreation();
            yield return AsyncWrapper.WaitForTask(task);
        }

        static async Task LoggerSetBeforeCreation()
        {
            var mock = new MockTextureCreator();
            var logger = new CollectingLogger();
            var import = new GltfImport(logger: logger, textureCreator: mock);
            Assert.IsTrue(await import.LoadGltfJson(EmbeddedPngGltf()));
            Assert.IsNotNull(mock.LoggerAtTimeOfJpegOrPngCreation,
                "SetLogger should be called with a non-null logger before CreateTextureFromJpegOrPng");
            LoggerTest.AssertLogger(logger);
        }

        [UnityTest]
        public IEnumerator SetLoggerIsClearedAfterLoading()
        {
            var task = LoggerClearedAfterLoad();
            yield return AsyncWrapper.WaitForTask(task);
        }

        static async Task LoggerClearedAfterLoad()
        {
            var mock = new MockTextureCreator();
            var logger = new CollectingLogger();
            var import = new GltfImport(logger: logger, textureCreator: mock);
            Assert.IsTrue(await import.LoadGltfJson(EmbeddedPngGltf()));
            Assert.IsNull(mock.LastLogger,
                "SetLogger(null) should be called after loading completes");
            LoggerTest.AssertLogger(logger);
        }
#endif // UNITY_IMAGECONVERSION

#if KTX_IS_ENABLED
        [UnityTest]
        public IEnumerator MockTextureCreatorIsCalledForKtx()
        {
            var task = MockCreatorCalledForKtx();
            yield return AsyncWrapper.WaitForTask(task);
        }

        static async Task MockCreatorCalledForKtx()
        {
            // Valid base64 encoding of 4 arbitrary bytes — not a valid KTX2 file,
            // but the mock ignores the data and returns a texture unconditionally.
            const string k_AnyBase64 = "rQbwDQ==";
            var gltf = $@"{{""images"":[{{""uri"":""data:image/ktx2;base64,{k_AnyBase64}""}}],""textures"":[{{""source"":0}}]}}";
            var mock = new MockTextureCreator();
            var logger = new CollectingLogger();
            var import = new GltfImport(logger: logger, textureCreator: mock);
            await import.LoadGltfJson(gltf);
            Assert.AreEqual(1, mock.CreateTextureFromKtxAsyncCallCount,
                "CreateTextureFromKtxAsync should be called exactly once");
        }
#endif // KTX_IS_ENABLED
    }
}
