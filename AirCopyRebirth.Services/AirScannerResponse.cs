using System;
using System.Collections.Generic;
using System.Text;

namespace AirCopyRebirth.Services {
    public static class AirScannerResponse {
		// Responses (note that no response starts with another response, so no terminating character necessary)
		/** Response: Device busy */
		// public static readonly byte[] DEVICE_BUSY = { 'd', 'e', 'v', 'b', 'u', 's', 'y' };
		public static readonly byte[] DEVICE_BUSY = Encoding.ASCII.GetBytes("devbusy");
		/** Response: Battery low */
		public static readonly byte[] BATTERY_LOW = Encoding.ASCII.GetBytes("battlow"); //  { 'b', 'a', 't', 't', 'l', 'o', 'w' };
		/** Response: No paper inserted */
		public static readonly byte[] NOPAPER = Encoding.ASCII.GetBytes("nopaper"); // { 'n', 'o', 'p', 'a', 'p', 'e', 'r' };
		/** Response: Paper inserted, ready to scan, calibrate, clean */
		public static readonly byte[] SCAN_READY = Encoding.ASCII.GetBytes("scanready"); // { 's', 'c', 'a', 'n', 'r', 'e', 'a', 'd', 'y' };
		/** Response: Calibration has started */
		public static readonly byte[] CALIBRATE_GO = Encoding.ASCII.GetBytes("calgo"); // { 'c', 'a', 'l', 'g', 'o' };
		/** Response: Calibration has finished */
		public static readonly byte[] CALIBRATE_END = Encoding.ASCII.GetBytes("calibrate"); // { 'c', 'a', 'l', 'i', 'b', 'r', 'a', 't', 'e' };
		/** Response: Cleaning has started */
		public static readonly byte[] CLEAN_GO = Encoding.ASCII.GetBytes("cleango"); // { 'c', 'l', 'e', 'a', 'n', 'g', 'o' };
		/** Response: Cleaning has finished */
		public static readonly byte[] CLEAN_END = Encoding.ASCII.GetBytes("cleanend"); // { 'c', 'l', 'e', 'a', 'n', 'e', 'n', 'd' };
		/** Response: Standard DPI selected */
		public static readonly byte[] DPI_STANDARD = Encoding.ASCII.GetBytes("dpistd"); // { 'd', 'p', 'i', 's', 't', 'd' };
		/** Response: High DPI selected */
		public static readonly byte[] DPI_HIGH = Encoding.ASCII.GetBytes("dpifine"); // { 'd', 'p', 'i', 'f', 'i', 'n', 'e' };
		/** Response: Scanning has started */
		public static readonly byte[] SCAN_GO = Encoding.ASCII.GetBytes("scango"); // { 's', 'c', 'a', 'n', 'g', 'o' };
		/** Response: Preview data in stream end marker */
		public static readonly byte[] PREVIEW_END = Encoding.ASCII.GetBytes("previewend"); // { 'p', 'r', 'e', 'v', 'i', 'e', 'w', 'e', 'n', 'd' };
		/** Response: JPEG size */
		public static readonly byte[] JPEG_SIZE = Encoding.ASCII.GetBytes("jpegsize"); // { 'j', 'p', 'e', 'g', 's', 'i', 'z', 'e' };
		/** Artifical response: EOF */
		public static readonly byte[] EOF = { };
	}
}
