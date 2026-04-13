using AirCopyRebirth.Services;
using System;
using System.IO;

namespace AirCopyRebirth.ConsoleApp {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("AirCopy Rebirth - Scanner Tool");
            Console.WriteLine("------------------------------");

            try {
                // You can change the IP here if needed
                // AirScanner.AirCopyIpAddress = "192.168.18.33";

                var status = AirScanner.GetStatusWithTcpClient();
                Console.WriteLine($"Current Scanner Status: {status}");

                if (status == AirScanner.ScannerStatus.ScanReady) {
                    Console.WriteLine("Paper detected! Starting scan in 300 DPI...");
                    byte[] jpegData = AirScanner.PerformScan(AirScanner.Dpi.Standard, Console.WriteLine);

                    string fileName = $"scan_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                    File.WriteAllBytes(fileName, jpegData);

                    Console.WriteLine($"Scan saved to {Path.GetFullPath(fileName)}");
                } else {
                    Console.WriteLine("Please insert paper and try again.");
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null) {
                    Console.WriteLine($"Inner Error: {ex.InnerException.Message}");
                }
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
