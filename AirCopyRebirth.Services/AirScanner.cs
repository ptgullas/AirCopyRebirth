using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace AirCopyRebirth.Services {
    public static class AirScanner {

        public enum ScannerStatus { BatteryLow, NoPaper, DevBusy, ScanReady, ScanningStarted, Invalid }
        public enum Dpi { Standard, Fine }

        public static string AirCopyIpAddress = "192.168.18.33";
        public static int AirCopyPort = 23;

        public const int AirCopyResponseSize = 16;

        private static readonly byte[] GET_VERSION = { 0x30, 0x30, 0x20, 0x20 };
        private static readonly byte[] GET_STATUS = { 0x00, 0x60, 0x00, 0x50 };

        private static readonly byte[] START_CLEANING = { 0x80, 0x80, 0x70, 0x70 };
        private static readonly byte[] START_CALIBRATION = { 0x00, 0x50, 0x00, 0x60 };

        private static readonly byte[] SET_DPI_STANDARD = { 0x40, 0x30, 0x20, 0x10 };
        private static readonly byte[] SET_DPI_HIGH = { 0x80, 0x70, 0x60, 0x50 };

        private static readonly byte[] START_SCAN = { 0x00, 0x20, 0x00, 0x10 };
        private static readonly byte[] SEND_PREVIEW_DATA = { 0x40, 0x40, 0x30, 0x30 };

        private static readonly byte[] GET_JPEG_SIZE = { 0x00, 0x30, 0x00, 0x40 };
        private static readonly byte[] SEND_JPEG_DATA = { 0x00, 0x10, 0x00, 0x20 };
                
        public static ScannerStatus GetStatusWithTcpClient() {
            (byte[] response, int byteRead) = SendMessage(GET_STATUS, 200);
            string responseStr = System.Text.Encoding.ASCII.GetString(response, 0, byteRead);
            return MapResponseToScannerStatus(responseStr);
        }

        public static string GetVersion() {
            (byte[] response, int byteRead) = SendMessage(GET_VERSION, 200);
            string responseStr = System.Text.Encoding.ASCII.GetString(response, 0, byteRead);
            return responseStr;
        }

        public static string StartScan() {
            (byte[] response, int byteRead) = SendMessage(START_SCAN, 200);
            string responseStr = System.Text.Encoding.ASCII.GetString(response, 0, byteRead);
            return responseStr;
        }

        public static byte[] StartScanAndGetScan() {
            (byte[] response, int byteRead) = SendMessageToScanThenReceiveJpegData(20000);
            return response;
        }

        public static string GetJpegSize(Dpi dpi) {
            int sleepInMs;
            if (dpi == Dpi.Standard) {
                sleepInMs = 12000;
            }
            else { sleepInMs = 35000; }

            (byte[] response, int byteRead) = SendMessage(GET_JPEG_SIZE, sleepInMs, 100);
            string responseStr = System.Text.Encoding.ASCII.GetString(response, 0, byteRead);
            return responseStr;
        }

        public static (byte[], int) GetJpegData() {
            return SendMessage(START_SCAN, 500, 30720);
        }

        private static (byte[], int) SendMessage(byte[] command, int sleepInMs = 0, int bufferSize = AirCopyResponseSize) {
            string response = "";
            using (TcpClient client = new TcpClient()) {
                client.Connect(AirCopyIpAddress, AirCopyPort);
                // using (SslStream stream = new SslStream(client.GetStream(), true)) {
                using (NetworkStream stream = client.GetStream()) {

                    // stream.AuthenticateAsClient(AirCopyIpAddress);
                    StreamReader reader = new StreamReader(stream);
                    StreamWriter writer = new StreamWriter(stream) {
                        AutoFlush = true
                    };

                    // stream.Write(GET_VERSION);
                    // stream.Write(Encoding.ASCII.GetBytes(dataToSend));
                    stream.Write(command, 0, command.Length);

                    // bastel waits 200ms here but it works without it
                    Thread.Sleep(sleepInMs);

                    int byteRead = 0;
                    // byte[] buffer = new byte[airCopyResponseSize];
                    byte[] buffer = Enumerable.Repeat((byte)0, bufferSize).ToArray();

                    byteRead = stream.Read(buffer, 0, bufferSize);
                    // response += System.Text.Encoding.ASCII.GetString(buffer, 0, byteRead);
                    return (buffer, byteRead);
                }
            }
        }

        private static ScannerStatus MapResponseToScannerStatus(string response) {
            if (response.StartsWith("battlow")) { return ScannerStatus.BatteryLow; }
            else if (response.StartsWith("nopaper")) { return ScannerStatus.NoPaper; }
            else if (response.StartsWith("devbusy")) { return ScannerStatus.DevBusy; }
            else if (response.StartsWith("scanready")) { return ScannerStatus.ScanReady; }
            else if (response.StartsWith("scango")) { return ScannerStatus.ScanningStarted;  }
            else { return ScannerStatus.Invalid; }
        }

        private static (byte[], int) SendMessageToScanThenReceiveJpegData(int sleepInMs, int bufferSize = AirCopyResponseSize) {
            string response = "";
            using (TcpClient client = new TcpClient()) {
                client.Connect(AirCopyIpAddress, AirCopyPort);
                // using (SslStream stream = new SslStream(client.GetStream(), true)) {
                using (NetworkStream stream = client.GetStream()) {

                    // stream.AuthenticateAsClient(AirCopyIpAddress);
                    StreamReader reader = new StreamReader(stream);
                    StreamWriter writer = new StreamWriter(stream) {
                        AutoFlush = true
                    };
                    byte[] command = START_SCAN;
                    stream.Write(command, 0, command.Length);

                    // bastel waits 200ms here but it works without it
                    int sleepAfterScanCommand = 200;
                    Thread.Sleep(sleepAfterScanCommand);

                    int byteRead = 0;
                    // byte[] buffer = new byte[airCopyResponseSize];
                    byte[] buffer = Enumerable.Repeat((byte)0, bufferSize).ToArray();

                    byteRead = stream.Read(buffer, 0, bufferSize);
                    // response += System.Text.Encoding.ASCII.GetString(buffer, 0, byteRead);

                    byte[] previewInfo = null;
                    // the response should be "scango", otherwise return it
                    response = System.Text.Encoding.ASCII.GetString(buffer, 0, byteRead);
                    if (MapResponseToScannerStatus(response) == ScannerStatus.ScanningStarted) {
                        Thread.Sleep(sleepInMs);
                        Socket socket = client.Client;
                        previewInfo = ReceiveAll(socket);
                    }
                    return (previewInfo, byteRead);
                }
            }
        }

        // ok, apparently you need to StartScan then get the preview before you can get the size
        public static (byte[], int) StartScanThenGetJpegSize() {
            using (TcpClient client = new TcpClient()) {
                client.Connect(AirCopyIpAddress, AirCopyPort);
                using (NetworkStream stream = client.GetStream()) {

                    // stream.AuthenticateAsClient(AirCopyIpAddress);
                    StreamReader reader = new StreamReader(stream);
                    StreamWriter writer = new StreamWriter(stream) {
                        AutoFlush = true
                    };

                    byte[] command = START_SCAN;
                    int sleepInMs = 200;

                    (byte[] buffer, int bytesRead) = SendCommandToScannerAndGetResponse(stream, command, sleepInMs);

                    string response = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    if (MapResponseToScannerStatus(response) == ScannerStatus.ScanningStarted) {
                        // OK, here, let's try to get the preview
                        command = SEND_PREVIEW_DATA;
                        int sleepPreview = 1000;
                        

                        // now try to get Jpeg Size
                        command = GET_JPEG_SIZE;
                        int sleepStandardSize = 12000;
                        (buffer, bytesRead) = SendCommandToScannerAndGetResponse(stream, command, sleepStandardSize);

                        response = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        Console.WriteLine(response);

                    }

                    return (buffer, bytesRead);
                }
            }
        }

        public static (byte[] buffer, int bytesRead) SendCommandToScannerAndGetResponse(NetworkStream stream, byte[] command, int sleepInMs, int responseSize = AirCopyResponseSize) {
            SendCommandThenSleep(stream, command, sleepInMs);

            return GetResponse(stream);
        }

        private static void SendCommandThenSleep(NetworkStream stream, byte[] command, int sleepInMs) {
            stream.Write(command, 0, command.Length);
            Thread.Sleep(sleepInMs);
        }
        private static (byte[] buffer, int bytesRead) GetResponse(NetworkStream stream) {
            // byte[] buffer = new byte[airCopyResponseSize];
            byte[] buffer = Enumerable.Repeat((byte)0, AirCopyResponseSize).ToArray();

            int bytesRead = stream.Read(buffer, 0, AirCopyResponseSize);
            if (bytesRead == 0) {
                // if it's 0, then the connection closed unexpectedly?
                // what's a good exception for this?
                throw new Exception("Socket closed?");
            }
            return (buffer, bytesRead);
        }

        private static (byte[] buffer, int bytesRead) ReadFromStream(NetworkStream stream) {
            int tagLength = SEND_PREVIEW_DATA.Length + 1;
            var buffer = new byte[30270 + tagLength];
            var bytesRead = 0;

            return (buffer, bytesRead);
        }


        public static byte[] ReceiveAll(this Socket socket) {
            var buffer = new List<byte>();
            while (socket.Available > 0) {
                var currByte = new Byte[1];
                var byteCounter = socket.Receive(currByte, currByte.Length, SocketFlags.None);
                if (byteCounter.Equals(1)) {
                    buffer.Add(currByte[0]);
                }
            }
            return buffer.ToArray();
        }



        // borrowed from https://www.csharp-examples.net/socket-send-receive
        private static void Receive(Socket socket, byte[] buffer, int offset, int size, int timeout) {
            int startTickCount = Environment.TickCount;
            int received = 0;  // how many bytes is already received
            do {
                if (Environment.TickCount > startTickCount + timeout)
                    throw new Exception("Timeout.");
                try {
                    received += socket.Receive(buffer, offset + received, size - received, SocketFlags.None);
                }
                catch (SocketException ex) {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable) {
                        // socket buffer is probably empty, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                        throw ex;  // any serious error occurr
                }
            } while (received < size);
        }

        private static void TestingSocketStuff() {
            byte[] ByteBuffer = System.Text.Encoding.ASCII.GetBytes("hi there");
        }

    }
}
