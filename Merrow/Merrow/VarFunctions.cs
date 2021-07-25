using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Security;
using System.Drawing;
using System.Threading.Tasks;

namespace Merrow {
    public partial class MerrowStandard {
        //GENERAL DATA/VARIABLE TRANSLATION OPERATIONS

        public void ShuffleArray(int[] arr) { //Shuffle supplied int array
            int j = arr.Length;
            while (j > 1) {
                j--;
                int k = SysRand.Next(j + 1);
                int temp = arr[k];
                arr[k] = arr[j];
                arr[j] = temp;
            }
        }

        public void CountAndShuffleArray(int[] arr) { //Populate array with ascending numbers, then shuffle
            for (int i = 0; i < arr.Length; i++) { arr[i] = i; }
            int j = arr.Length;
            while (j > 1) {
                j--;
                int k = SysRand.Next(j + 1);
                int temp = arr[k];
                arr[k] = arr[j];
                arr[j] = temp;
            }
        }

        public static string ByteArrayToString(byte[] ba) { //Convert byte array to hex string
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba) { hex.AppendFormat("{0:x2}", b); }
            return hex.ToString();
        }

        public static string ByteToString(byte ba) { //Convert byte to hex string
            StringBuilder hex = new StringBuilder();
            hex.AppendFormat("{0:x2}", ba);
            return hex.ToString();
        }

        public static byte[] StringToByteArray(string hex) { //Convert hex string to byte array
            int NumberChars = hex.Length;
            if (NumberChars % 2 != 0) {
                hex = "0" + hex;
                NumberChars++;
            }
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2) { bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16); }
            return bytes;
        }

        public static string HexToBase64(string input) { //convert hex string to base64 string
            return System.Convert.ToBase64String(StringToByteArray(input));
        }

        public static string Base64ToHex(string input) { //convert hex string to base64 string
            return ByteArrayToString(System.Convert.FromBase64String(input));
        }

        public static string ToHex(string input) { //Convert ascii string to hex string
            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
                sb.AppendFormat("{0:X2}", (int)c);
            return sb.ToString().Trim();
        }

        public static Color RGBAToColor(string hexvalue) { //Convert 4-char hex string to Color
            string binCol = Convert.ToString(Convert.ToInt32(hexvalue, 16), 2).PadLeft(16, '0'); //convert the hex string to an int, and then to binary string
            //if (binCol.Length < 16) { for (int i = 0; i < 16 - binCol.Length; i++) { binCol = "0" + binCol; } } //ensure it's 16 characters, conversion will cut it short
            //binCol.PadLeft(16, '0');
            Console.WriteLine(binCol);
            int intR = Convert.ToInt32(binCol.Substring(0, 5), 2); //convert first five bits of binary to int 0-31
            int intG = Convert.ToInt32(binCol.Substring(5, 5), 2); //convert second five bits of binary to int 0-31
            int intB = Convert.ToInt32(binCol.Substring(10, 5), 2); //convert third five bits of binary to int 0-31
            int intA = Convert.ToInt32(binCol.Substring(15), 2); //grab the alpha as well

            double dubR = (intR / 31d) * 255; //convert the 0-31 values to 0-255 for FromArgb
            double dubG = (intG / 31d) * 255;
            double dubB = (intB / 31d) * 255;

            intR = (int)Math.Round(dubR); //it was not doing the conversion properly so i've separated it out
            intG = (int)Math.Round(dubG);
            intB = (int)Math.Round(dubB);

            intA = intA * 255; //either 255 or 0

            Color ret = Color.FromArgb(intA, intR, intG, intB); //return the color value so it can be used for TransformHSV
            return ret;
        }

        public static string ColorToHex(Color col) { //Convert Color to 4-char hex string
            double dubR = (col.R / 255d) * 31; //convert the 0-255 values to 0-31 for binary conversion
            double dubG = (col.G / 255d) * 31;
            double dubB = (col.B / 255d) * 31;

            int intR = (int)Math.Round(dubR); //it was not doing the conversion properly so i've separated it out
            int intG = (int)Math.Round(dubG);
            int intB = (int)Math.Round(dubB);

            int intA = 1; //Alpha is either 1 or 0
            if (col.A == 0) { intA = 0; }

            string binR = Convert.ToString(intR, 2).PadLeft(5, '0'); //convert them to separate binary strings
            string binG = Convert.ToString(intG, 2).PadLeft(5, '0');
            string binB = Convert.ToString(intB, 2).PadLeft(5, '0');

            int binCol = Convert.ToInt32(binR + binG + binB + intA.ToString(), 2); //combine into one int
            string ret = binCol.ToString(("X4")); //convert that int to hex
            return ret;
        }

        public static Color HueShift(Color col, double degrees) {
            double[] matr = new double[9];
            double cosA = Math.Cos(DegToRad(degrees));
            double sinA = Math.Sin(DegToRad(degrees));

            matr[0] = cosA + (1.0 - cosA) / 3.0;
            matr[1] = 1d / 3d * (1.0 - cosA) - Math.Sqrt(1d / 3d) * sinA;
            matr[2] = 1d / 3d * (1.0 - cosA) + Math.Sqrt(1d / 3d) * sinA;
            matr[3] = 1d / 3d * (1.0 - cosA) + Math.Sqrt(1d / 3d) * sinA;
            matr[4] = cosA + 1d / 3d * (1.0 - cosA);
            matr[5] = 1d / 3d * (1.0 - cosA) - Math.Sqrt(1d / 3d) * sinA;
            matr[6] = 1d / 3d * (1.0 - cosA) - Math.Sqrt(1d / 3d) * sinA;
            matr[7] = 1d / 3d * (1.0 - cosA) + Math.Sqrt(1d / 3d) * sinA;
            matr[8] = cosA + 1d / 3d * (1.0 - cosA);

            double rx = col.R * matr[0] + col.G * matr[1] + col.B * matr[2];
            double gx = col.R * matr[3] + col.G * matr[4] + col.B * matr[5];
            double bx = col.R * matr[6] + col.G * matr[7] + col.B * matr[8];
            double ax = Math.Min(255, Math.Max(0, (int)col.A));

            return Color.FromArgb(Clamp16(ax), Clamp16(rx), Clamp16(gx), Clamp16(bx));
        }

        public static int Clamp16(double input) {
            int ret = Math.Min(255, Math.Max(0, (int)input));
            return ret;
        }

        public static double DegToRad(double degrees) {
            double radians = (Math.PI / 180) * degrees;
            return (radians);
        }

        public static double RadToDeg(double radians) {
            double degrees = (180 / Math.PI) * radians;
            return (degrees);
        }

        //
        //below this line is the old colour conversion method that doesn't work as intended
        //

        public static Color ColorFromHSL(float h, float s, float v) { 
            if (s == 0) { int L = (int)v; return Color.FromArgb(255, L, L, L); }

            double min, max, hx;
            hx = h / 360d;

            max = v < 0.5d ? v * (1 + s) : (v + s) - (v * s);
            min = (v * 2d) - max;

            Color c = Color.FromArgb(255, (int)(255 * RGBChannelFromHue(min, max, hx + 1 / 3d)),
                                          (int)(255 * RGBChannelFromHue(min, max, hx)),
                                          (int)(255 * RGBChannelFromHue(min, max, hx - 1 / 3d)));
            return c;
        }

        public static double RGBChannelFromHue(double m1, double m2, double h) {
            h = (h + 1d) % 1d;
            if (h < 0) h += 1;
            if (h * 6 < 1) return m1 + (m2 - m1) * 6 * h;
            else if (h * 2 < 1) return m2;
            else if (h * 3 < 2) return m1 + (m2 - m1) * 6 * (2d / 3d - h);
            else return m1;
        }

        public float getBrightness(Color c) { //more realistic brightness
            return (c.R * 0.299f + c.G * 0.587f + c.B * 0.114f) / 256f;
        }
    }
}
