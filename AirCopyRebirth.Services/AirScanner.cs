using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AirCopyRebirth.Services {
    public static class AirScanner {

        public enum ScannerStatus { BatteryLow, NoPaper, DevBusy, ScanReady, ScanningStarted, Invalid }
        public enum Dpi { Standard = 300, Fine = 600 }

        public static string AirCopyIpAddress = "192.168.18.33";
        public static int AirCopyPort = 23;

        public const int AirCopyResponseSize = 16;

        // Commands (Little Endian)
        private static readonly byte[] GET_VERSION = { 0x30, 0x30, 0x20, 0x20 };        // 20203030
        private static readonly byte[] GET_STATUS = { 0x00, 0x60, 0x00, 0x50 };         // 50006000
        private static readonly byte[] START_CLEANING = { 0x80, 0x80, 0x70, 0x70 };     // 70708080
        private static readonly byte[] START_CALIBRATION = { 0x00, 0xb0, 0x00, 0xa0 };  // a000b000
        private static readonly byte[] SET_DPI_STANDARD = { 0x40, 0x30, 0x20, 0x10 };   // 10203040
        private static readonly byte[] SET_DPI_HIGH = { 0x80, 0x70, 0x60, 0x50 };       // 50607080
        private static readonly byte[] START_SCAN = { 0x00, 0x20, 0x00, 0x10 };         // 10002000
        private static readonly byte[] SEND_PREVIEW_DATA = { 0x40, 0x40, 0x30, 0x30 };  // 30304040
        private static readonly byte[] GET_JPEG_SIZE = { 0x00, 0xd0, 0x00, 0xc0 };      // c000d000
        private static readonly byte[] SEND_JPEG_DATA = { 0x00, 0xf0, 0x00, 0xe0 };      // e000f000

        public static byte[] PerformScan(Dpi dpi, Action<string> logger = null) {
            logger?.Invoke($"Connecting to {AirCopyIpAddress}:{AirCopyPort}...");
            using (TcpClient client = new TcpClient()) {
                client.Connect(AirCopyIpAddress, AirCopyPort);
                using (NetworkStream stream = client.GetStream()) {
                    stream.ReadTimeout = 10000;

                    // 1. Check Status
                    logger?.Invoke("Checking status...");
                    string statusStr = SendCommandAndReadResponse(stream, GET_STATUS);
                    logger?.Invoke($"Status: {statusStr}");
                    var status = MapResponseToScannerStatus(statusStr);
                    if (status != ScannerStatus.ScanReady) {
                        throw new Exception($"Scanner not ready: {statusStr}");
                    }

                    // 2. Set DPI
                    logger?.Invoke($"Setting DPI to {dpi}...");
                    byte[] dpiCommand = dpi == Dpi.Fine ? SET_DPI_HIGH : SET_DPI_STANDARD;
                    string dpiResponse = SendCommandAndReadResponse(stream, dpiCommand);
                    logger?.Invoke($"DPI Response: {dpiResponse}");

                    // 3. Start Scan
                    logger?.Invoke("Starting scan...");
                    string scanResponse = SendCommandAndReadResponse(stream, START_SCAN);
                    logger?.Invoke($"Scan Response: {scanResponse}");
                    if (MapResponseToScannerStatus(scanResponse) != ScannerStatus.ScanningStarted) {
                        throw new Exception($"Failed to start scan: {scanResponse}");
                    }

                    // 4. (Optional) Preview Data
                    // We'll skip reading preview data to get to the JPEG faster,
                    // but we need to wait for the scan to finish.
                    logger?.Invoke("Waiting for scan to complete...");
                    int waitMs = dpi == Dpi.Fine ? 35000 : 12000;
                    Thread.Sleep(waitMs);

                    // 5. Get JPEG Size
                    logger?.Invoke("Requesting JPEG size...");
                    // Need a longer timeout for this one as the scanner might still be processing
                    stream.ReadTimeout = 20000;
                    byte[] sizeBuffer = SendCommandAndGetResponseBytes(stream, GET_JPEG_SIZE, 16);
                    string sizeMarker = Encoding.ASCII.GetString(sizeBuffer, 0, 8);
                    if (!sizeMarker.StartsWith("jpegsize")) {
                        throw new Exception($"Unexpected JPEG size response: {Encoding.ASCII.GetString(sizeBuffer)}");
                    }

                    int jpegSize = BitConverter.ToInt32(sizeBuffer, 8);
                    logger?.Invoke($"JPEG size: {jpegSize} bytes");

                    if (jpegSize <= 0 || jpegSize > 20 * 1024 * 1024) {
                        throw new Exception($"Invalid JPEG size: {jpegSize}");
                    }

                    // 6. Get JPEG Data
                    logger?.Invoke("Requesting JPEG data...");
                    stream.ReadTimeout = 30000;
                    SendCommand(stream, SEND_JPEG_DATA, 500);

                    byte[] jpegData = new byte[jpegSize];
                    int totalRead = 0;
                    while (totalRead < jpegSize) {
                        int read = stream.Read(jpegData, totalRead, jpegSize - totalRead);
                        if (read == 0) break;
                        totalRead += read;
                        if (totalRead % (100 * 1024) == 0 || totalRead == jpegSize) {
                            logger?.Invoke($"Read {totalRead}/{jpegSize} bytes...");
                        }
                    }

                    if (totalRead < jpegSize) {
                        logger?.Invoke($"Warning: Only read {totalRead} of {jpegSize} bytes.");
                    }

                    logger?.Invoke("Scan complete!");
                    return jpegData;
                }
            }
        }

        private static string SendCommandAndReadResponse(NetworkStream stream, byte[] command) {
            SendCommand(stream, command);
            return ReadResponseString(stream);
        }

        private static byte[] SendCommandAndGetResponseBytes(NetworkStream stream, byte[] command, int size) {
            SendCommand(stream, command);
            byte[] buffer = new byte[size];
            int read = stream.Read(buffer, 0, size);
            if (read < size) {
                // Try to read the rest if it's fragmented
                int remaining = size - read;
                while (remaining > 0) {
                    int r = stream.Read(buffer, read, remaining);
                    if (r == 0) break;
                    read += r;
                    remaining -= r;
                }
            }
            return buffer;
        }

        private static void SendCommand(NetworkStream stream, byte[] command, int delayAfterMs = 200) {
            stream.Write(command, 0, command.Length);
            Thread.Sleep(delayAfterMs);
        }

        private static string ReadResponseString(NetworkStream stream) {
            byte[] buffer = new byte[AirCopyResponseSize];
            int read = stream.Read(buffer, 0, AirCopyResponseSize);
            if (read == 0) return string.Empty;
            return Encoding.ASCII.GetString(buffer, 0, read).TrimEnd('\0', ' ');
        }

        public static ScannerStatus GetStatusWithTcpClient() {
            using (TcpClient client = new TcpClient()) {
                client.Connect(AirCopyIpAddress, AirCopyPort);
                using (NetworkStream stream = client.GetStream()) {
                    string response = SendCommandAndReadResponse(stream, GET_STATUS);
                    return MapResponseToScannerStatus(response);
                }
            }
        }

        private static ScannerStatus MapResponseToScannerStatus(string response) {
            if (response.StartsWith("battlow")) { return ScannerStatus.BatteryLow; }
            else if (response.StartsWith("nopaper")) { return ScannerStatus.NoPaper; }
            else if (response.StartsWith("devbusy")) { return ScannerStatus.DevBusy; }
            else if (response.StartsWith("scanready")) { return ScannerStatus.ScanReady; }
            else if (response.StartsWith("scango")) { return ScannerStatus.ScanningStarted; }
            else { return ScannerStatus.Invalid; }
        }

        // Legacy methods kept for compatibility but marked as obsolete or updated
        [Obsolete("Use PerformScan instead")]
        public static string GetVersion() {
            using (TcpClient client = new TcpClient()) {
                client.Connect(AirCopyIpAddress, AirCopyPort);
                using (NetworkStream stream = client.GetStream()) {
                    return SendCommandAndReadResponse(stream, GET_VERSION);
                }
            }
        }
    }
}
