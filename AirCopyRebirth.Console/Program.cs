using AirCopyRebirth.Services;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AirCopyRebirth.ConsoleApp {
    class Program {
        static void Main(string[] args) {
            string AirCopyIpAddress = "192.168.18.33";
            int AirCopyPort = 23;
            
            Console.WriteLine($"Will attempt to connect to {AirCopyIpAddress}:{AirCopyPort}");

            string dataToSend = "48483232";
            byte[] GET_VERSION = { 0x30, 0x30, 0x20, 0x20 };
            byte[] GET_STATUS = { 0x00, 0x60, 0x00, 0x50 };

            StartScanAndGetSize();


            var status = AirScanner.GetStatusWithTcpClient();

            Console.WriteLine(status.ToString());


        }

        public static void StartScanAndGetSize() {
            AirScanner.StartScanThenGetJpegSize();
            //string response = AirScanner.StartScan();
            //if (response.StartsWith("scango")) {
            //    Console.WriteLine($"It said {response}");
            //    string sizeResponse = AirScanner.GetJpegSize(AirScanner.Dpi.Standard);
            //    Console.WriteLine($"JPEG size is {sizeResponse}");
            //}
        }

        public static void StartScanAndGetPreview() {
            // this mostly doesn't work yet
            byte[] byteJpeg = AirScanner.StartScanAndGetScan();
            // Save the thing here
            string currentDateTime = $"DateTime.Now:yyyy.MM.dd.HH.mm.ss";
            string pathToSave = @$"c:\temp\scannedJpeg_{currentDateTime}";
            File.WriteAllBytes(pathToSave, byteJpeg);
            // Console.WriteLine($"This should be 'scango': {responseToStartScan}");

            // it hangs here for some reason
            /*
            Thread.Sleep(15000);

            var jpegSize = AirScanner.GetJpegSize();
            Console.WriteLine($"JPEG Size is {jpegSize}");

            (byte[] jpgBytes, int bytesRead) = AirScanner.GetJpegData();
            string responseStr = System.Text.Encoding.ASCII.GetString(jpgBytes, 0, bytesRead);
            */

        }

        public static void OldManualStuff() {
            /*
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            string response = "";
            using (TcpClient client = new TcpClient()) {
                Socket socket = client.Client;
                socket.Blocking = false;
                client.Connect(AirCopyIpAddress, AirCopyPort);
                // using (SslStream stream = new SslStream(client.GetStream(), true)) {
                using (NetworkStream stream = client.GetStream() ) {
                    
                    // stream.AuthenticateAsClient(AirCopyIpAddress);
                    StreamReader reader = new StreamReader(stream);
                    StreamWriter writer = new StreamWriter(stream) {
                        AutoFlush = true
                    };

                    // stream.Write(GET_VERSION);
                    // stream.Write(Encoding.ASCII.GetBytes(dataToSend));
                    stream.Write(GET_STATUS);

                    

                    int byteRead = 0;
                    int airCopyResponseSize = 16;
                    byte[] buffer = new byte[airCopyResponseSize];

                    byteRead = stream.Read(buffer, 0, airCopyResponseSize);
                    response += System.Text.Encoding.ASCII.GetString(buffer, 0, byteRead);

                    //do {
                    //}
                    //while (byteRead > 0);
                }
            }
            */

        }
    }
}
