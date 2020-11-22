using AirCopyRebirth.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AirCopyRebirth.Tests {
    public class ByteArrayExtensionsTests {
        [Fact]
        public void Contains_ByteArrayContainsMessage_ReturnsTrue() {
            byte[] response = { 0x00, 0x30, 0x00, 0x40, (byte)'s', (byte)'c', (byte)'a', (byte)'n', (byte)'g', (byte)'o', 0x22 };
            var result = response.Contains(AirScannerResponse.SCAN_GO);
            Assert.True(result);
        }
        [Fact]
        public void Contains_ByteArrayContainsInterruptedMessage_ReturnsFalse() {
            byte[] response = { 0x00, 0x30, 0x00, 0x40, (byte)'s', (byte)'c', 0x44, (byte)'a', (byte)'n', (byte)'g', (byte)'o', 0x22 };
            var result = response.Contains(AirScannerResponse.SCAN_GO);
            Assert.False(result);
        }

        [Fact]
        public void Contains_ByteArrayContainsScanPreviewWithEndTag_ReturnsTrue() {
            string pathToPreview = @"..\..\..\SampleData\preview.jpg";
            // quick way to test the path to the Sample Data
            Assert.True(File.Exists(pathToPreview));

            byte[] previewBytes = File.ReadAllBytes(pathToPreview);
            int previewBytesSize = 156799;
            Assert.Equal(previewBytesSize, previewBytes.Length);

            byte[] endMarker = { (byte)'s', (byte)'c', (byte)'a', (byte)'n', (byte)'g', (byte)'o' };
            byte[] previewWithMarker = previewBytes.Concat(endMarker).ToArray();
            int previewWithMarkerSize = 156805;
            Assert.Equal(previewWithMarkerSize, previewWithMarker.Length);

            var result = previewWithMarker.Contains(AirScannerResponse.SCAN_GO);
            Assert.True(result);
        }

        [Fact]
        public void Locate_ByteArrayContainsScanPreviewWithEndTag_ReturnsLocationOfEndTag() {
            string pathToPreview = @"..\..\..\SampleData\preview.jpg";
            // quick way to test the path to the Sample Data
            Assert.True(File.Exists(pathToPreview));

            byte[] previewBytes = File.ReadAllBytes(pathToPreview);
            int expectedPreviewBytesSize = 156799;
            Assert.Equal(expectedPreviewBytesSize, previewBytes.Length);

            byte[] endMarker = { (byte)'s', (byte)'c', (byte)'a', (byte)'n', (byte)'g', (byte)'o' };
            byte[] previewWithMarker = previewBytes.Concat(endMarker).ToArray();
            int expectedPreviewWithMarkerSize = 156805;
            Assert.Equal(expectedPreviewWithMarkerSize, previewWithMarker.Length);

            var result = previewWithMarker.Locate(AirScannerResponse.SCAN_GO);
            int expectedEndMarkerLocation = expectedPreviewWithMarkerSize - AirScannerResponse.SCAN_GO.Length;
            Assert.Equal(expectedEndMarkerLocation, result[0]);
        }
    }
}
