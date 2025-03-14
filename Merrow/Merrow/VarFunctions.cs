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

        public object[] TranslateString(string str) {
            List<int> asciivals = new List<int>();
            string masterstring = "";
            int lastrow = -1;
            int bytelength = 0;
            bool OKchar = false;
            object[] returnvals = new object[2];

            for (int i = 0; i < str.Length; i++) { asciivals.Add(str[i]); /*Console.WriteLine(str[i] + " " + asciivals[i]);*/ }

            foreach(int val in asciivals) {
                OKchar = false;
                if (val >= 48 && val <= 57) { //numerals = ascii - 48 (48-57) row 80
                    if (lastrow != 80) { lastrow = 80; masterstring += "80"; bytelength++; }
                    masterstring += (val - 48).ToString("X2");
                    bytelength++;
                    OKchar = true;
                }
                else if (val >= 65 && val <= 90) { //uppercase = ascii - 65 (65-90) row 81
                    if (lastrow != 81) { lastrow = 81; masterstring += "81"; bytelength++; }
                    masterstring += (val - 65).ToString("X2");
                    bytelength++;
                    OKchar = true;
                }
                else if (val >= 97 && val <= 122) { //lowercase = ascii - 97 (97-122) row 82
                    if (lastrow != 82) { lastrow = 82; masterstring += "82"; bytelength++; }
                    masterstring += (val - 97).ToString("X2");
                    bytelength++;
                    OKchar = true;
                } else { //punctuation = special check list: ASCII/ROW/HEX in DEC
                    for (int i = 0; i < 14; i++) {
                        if (val == library.punctuationvals[i * 3]) { //punctuation
                            if (lastrow != library.punctuationvals[i * 3 + 1]) {
                                lastrow = library.punctuationvals[i * 3 + 1];
                                masterstring += library.punctuationvals[i * 3 + 1].ToString();
                                bytelength++;
                            }
                            masterstring += library.punctuationvals[i * 3 + 2].ToString("X2");
                            bytelength++;
                            OKchar = true;
                        }
                        if (OKchar) { break; }
                    }
                    for (int i = 0; i < 4; i++) {
                        if (val == library.specialvals[i * 2]) { //punctuation
                            masterstring += library.specialvals[i * 2 + 1].ToString("X2");
                            bytelength++;
                            OKchar = true;
                        }
                        if (OKchar) { break; }
                    }
                }
            }

            if (!OKchar) { return null; } //invalid char, get out

            returnvals[0] = masterstring; //don't need to add A0C0, addresses are past it already
            returnvals[1] = bytelength;
            return returnvals; //don't forget about A0C0
        }

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

        public void ShuffleList(List<int> lis) { //Shuffle supplied int list
            int j = lis.Count;
            while (j > 1) {
                j--;
                int k = SysRand.Next(j + 1);
                int temp = lis[k];
                lis[k] = lis[j];
                lis[j] = temp;
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

        public int[] NumbersIn(int value) {
            var numbers = new Stack<int>();
            for (; value > 0; value /= 10) { numbers.Push(value % 10); }
            return numbers.ToArray();
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

        public static byte[] StringToByteArray(string hex, bool addZero) { //Convert hex string to byte array
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
            return System.Convert.ToBase64String(StringToByteArray(input, true));
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

        public static string Bin6ToAsc64(string input) { //Convert 6-digit binary to compressed ascii 6-bit value
            int tempInt = Convert.ToInt32(input, 2);
            int asciiOffset = 0;
            
            if (tempInt <= 9) { asciiOffset = 48; } //0-9, 48-57
            else if (tempInt >= 10 && tempInt <= 37) { asciiOffset = 53; } //?@A-Z, 63-90
            else if (tempInt >= 38) { asciiOffset = 59; } //a-z, 97-122

            return IntToAscii(tempInt + asciiOffset).ToString();
        }

        public static string Asc64ToBin6(string input) { //Convert compressed ascii 6-bit value to 6-digit binary
            char tempChar = input[0]; 
            int tempInt = AsciiToInt(tempChar);
            int asciiOffset = 0;

            if (tempInt <= 57) { asciiOffset = 48; } //0-9, 48-57
            else if (tempInt >= 63 && tempInt <= 90) { asciiOffset = 53; } //?@A-Z, 63-90
            else if (tempInt >= 97) { asciiOffset = 59; } //a-z, 97-122

            string binString = Convert.ToString(tempInt -= asciiOffset, 2);

            return binString;
        }

        public static int AsciiToInt(char ch) {
            return (int)ch;
        }

        public static char IntToAscii(int num) {
            return (char)num;
        }

        public static Color RGBAToColor(string hexvalue) { //Convert 4-char hex string to Color
            string binCol = Convert.ToString(Convert.ToInt32(hexvalue, 16), 2).PadLeft(16, '0'); //convert the hex string to an int, and then to binary string

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

            intA = intA * 255; //alpha must be either 255 or 0

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

        public static Color HueShift(Color col, double degrees) { //circular hue-shift of supplied colour by supplied degrees
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

        public static int Clamp16(double input) { //clamps double between 0-255
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
    }
}
