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

            byte[] endMarker = { (byte)'p', (byte)'r', (byte)'e', (byte)'v', (byte)'i', (byte)'e', (byte)'w', (byte)'e', (byte)'n', (byte)'d' };
            byte[] previewWithMarker = previewBytes.Concat(endMarker).ToArray();
            int previewWithMarkerSize = 156809;
            Assert.Equal(previewWithMarkerSize, previewWithMarker.Length);

            byte[] endMarkerToCheck = AirScannerResponse.PREVIEW_END;
            var result = previewWithMarker.Contains(endMarkerToCheck);
            Assert.True(result);
        }

        [Fact]
        public void Contains_ReadScanPreviewWithEndTagFromFileStream_ReturnsTrue() {
            string pathToPreview = @"..\..\..\SampleData\previewWithEndMarker.jpg";
            // quick way to test the path to the Sample Data
            Assert.True(File.Exists(pathToPreview));

            
            byte[] endMarker = { (byte)'p', (byte)'r', (byte)'e', (byte)'v', (byte)'i', (byte)'e', (byte)'w', (byte)'e', (byte)'n', (byte)'d' };

            /* // CODE FOR SAVING JPG WITH END MARKER
            var tempBytes = File.ReadAllBytes(pathToPreview);
            var tempBytesWithEndMarker = tempBytes.Concat(endMarker).ToArray();
            string dir = Path.GetDirectoryName(pathToPreview);
            string newFileName = "previewWithEndMarker.jpg";
            string newPath = Path.Combine(dir, newFileName);
            File.WriteAllBytes(newPath, tempBytesWithEndMarker);
            */
            // we want to test what happens when the end marker is torn apart
            // (i.e., when it is read in separate loops)
            byte[] buffer = new byte[500000];
            int loops = 0;
            using (FileStream fs = new FileStream(pathToPreview, FileMode.Open)) {
                int bytesToReadPerLoop = 1920;
                int totalBytesRead = 0;
                int bytesReadThisLoop;
                while ((bytesReadThisLoop = fs.Read(buffer, totalBytesRead, bytesToReadPerLoop)) > 0) {
                    totalBytesRead += bytesReadThisLoop;
                    loops++;
                }
                Console.Write("out of while loop\n");
            }
            var result = buffer.Contains(endMarker);
            Assert.True(result);


            //byte[] endMarker = { (byte)'p', (byte)'r', (byte)'e', (byte)'v', (byte)'i', (byte)'e', (byte)'w', (byte)'e', (byte)'n', (byte)'d' };
            //byte[] previewWithMarker = previewBytes.Concat(endMarker).ToArray();
            //int previewWithMarkerSize = 156809;
            //Assert.Equal(previewWithMarkerSize, previewWithMarker.Length);

            //byte[] endMarkerToCheck = AirScannerResponse.PREVIEW_END;
            //var result = previewWithMarker.Contains(endMarkerToCheck);
            //Assert.True(result);
        }

        [Fact]
        public void Locate_ByteArrayContainsScanPreviewWithEndTag_ReturnsLocationOfEndTag() {
            string pathToPreview = @"..\..\..\SampleData\preview.jpg";
            // quick way to test the path to the Sample Data
            Assert.True(File.Exists(pathToPreview));

            byte[] previewBytes = File.ReadAllBytes(pathToPreview);
            int expectedPreviewBytesSize = 156799;
            Assert.Equal(expectedPreviewBytesSize, previewBytes.Length);

            byte[] endMarker = { (byte)'p', (byte)'r', (byte)'e', (byte)'v', (byte)'i', (byte)'e', (byte)'w', (byte)'e', (byte)'n', (byte)'d' };
            byte[] previewWithMarker = previewBytes.Concat(endMarker).ToArray();
            int expectedPreviewWithMarkerSize = 156809;
            Assert.Equal(expectedPreviewWithMarkerSize, previewWithMarker.Length);

            var endMarkerToCheck = AirScannerResponse.PREVIEW_END;
            var result = previewWithMarker.Locate(endMarkerToCheck);
            int expectedEndMarkerLocation = expectedPreviewWithMarkerSize - endMarkerToCheck.Length;
            Assert.Equal(expectedEndMarkerLocation, result[0]);
        }

        [Fact]
        public void StripEndMarker_ByteArrayContainsMessage_ReturnsArrayWithoutEndMarker() {
            byte[] response = { 0x00, 0x30, 0x00, 0x40, (byte)'s', (byte)'c', (byte)'a', (byte)'n', (byte)'g', (byte)'o', 0x22 };
            byte[] expected = { 0x00, 0x30, 0x00, 0x40 };

            byte[] result = response.StripEndMarker(AirScannerResponse.SCAN_GO);

            Assert.Equal(expected, result);
        }


        [Fact]
        public void StripEndMarker_ByteArrayContainsScanPreviewWithEndTag_ReturnsPreviewWithoutTag() {
            string pathToPreview = @"..\..\..\SampleData\preview.jpg";
            // quick way to test the path to the Sample Data
            // Assert.True(File.Exists(pathToPreview));

            byte[] previewBytes = File.ReadAllBytes(pathToPreview);
            // int expectedPreviewBytesSize = 156799;
            // Assert.Equal(expectedPreviewBytesSize, previewBytes.Length);

            byte[] endMarker = { (byte)'p', (byte)'r', (byte)'e', (byte)'v', (byte)'i', (byte)'e', (byte)'w', (byte)'e', (byte)'n', (byte)'d' };
            byte[] previewWithMarker = previewBytes.Concat(endMarker).ToArray();
            // int expectedPreviewWithMarkerSize = 156809;
            // Assert.Equal(expectedPreviewWithMarkerSize, previewWithMarker.Length);

            var endMarkerToCheck = AirScannerResponse.PREVIEW_END;
            int expectedresultSize = 156799;

            byte[] result = previewWithMarker.StripEndMarker(endMarkerToCheck);

            Assert.Equal(expectedresultSize, result.Length);
        }

        [Fact]
        public void StripEndMarker_ByteArrayDoesNotContainMarker_ReturnsSameArray() {
            byte[] response = { 0x00, 0x30, 0x00, 0x40, (byte)'n', (byte)'g', (byte)'o', 0x22 };
            byte[] expected = { 0x00, 0x30, 0x00, 0x40, (byte)'n', (byte)'g', (byte)'o', 0x22 };

            byte[] result = response.StripEndMarker(AirScannerResponse.SCAN_GO);

            Assert.Equal(expected, result);
        }


    }
}
