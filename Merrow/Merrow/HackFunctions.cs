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
        //ADVANCED FEATURE FUNCTIONALITY: Generic Patch Generator, Binary File Reader

        //BUILD GENERIC PATCH-----------------------------------------------------------------------------
        public void BuildCustomPatch(string addr, string content) {
            //update filename
            if (advFilenameText.Text != "" && advFilenameText.Text != null) {
                fileName = string.Join("", advFilenameText.Text.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries)); //strip all whitespace to avoid errors
            }
            else { fileName = "merrowgenericpatch"; }

            passcheck = 0; //error check reset
            advErrorLabel.Visible = false;
            patchbuild = "";
            patchcontent = "";
            string lengthhex = Convert.ToString(content.Length / 2, 16);
            string addresshex = addr;

            if (addr.Length < 6) { //add leading zeroes to short addresses
                addresshex = "";
                for (int i = 0; i < 6 - addr.Length; i++) { addresshex += "0"; }
                addresshex += addr;
                advAddressText.Text = addresshex;
            }

            //error check
            if (content.Length == 0) { //check that the content is not null
                advErrorLabel.Text = "ERROR: Content cannot be empty.";
                passcheck = 1;
            }
            if (content.Length % 2 != 0) { //check that the content is a hex string of multiple 2 len, if not, fail
                advErrorLabel.Text = "ERROR: Odd number of characters in content.";
                passcheck = 2;
            }
            if (content.Length < 0 || addr.Length > 6 || lengthhex.Length > 4) { //if you somehow manage any of these then go to hell
                advErrorLabel.Text = "ERROR: Invalid content or address.";
                passcheck = 3;
            }
            //if error check failed, get out
            if (passcheck > 0) {
                advErrorLabel.Visible = true;
                return;
            }

            //ASSEMBLE CONTENT
            patchcontent += addresshex;
            if (lengthhex.Length < 4) { //add leading zeroes if IPS record size is too short
                for (int p = 0; p < 4 - lengthhex.Length; p++) { patchcontent += "0"; }
            }

            patchcontent += lengthhex; //add the remaining IPS record size in hex to the string
            patchcontent += content; //add the actual content of the patch

            //ASSEMBLE PATCH FILE
            patchbuild += headerHex;
            patchbuild += patchcontent;
            patchbuild += footerHex;
            patcharray = StringToByteArray(patchbuild, true);
            File.WriteAllBytes(filePath + fileName + ".ips", patcharray);
            filePath = filePath.Replace(@"/", @"\");   // explorer doesn't like front slashes
            System.Diagnostics.Process.Start("explorer.exe", Application.StartupPath + "\\Patches\\");
        }

        //BINARY FILE READER-------------------------------------------------------------------------------------
        public void BinRead() {
            Console.WriteLine("BinRead start.");
            //update filename
            if (binFilenameTextBox.Text != "" && binFilenameTextBox.Text != null) {
                fileName = string.Join("", binFilenameTextBox.Text.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries)); //strip all whitespace to avoid errors
            }
            else { fileName = "merrowreaderoutput"; }

            //read content and split
            string content = binContentTextBox.Text;
            string[] strarray = content.Split(',');

            //convert entire array to hex values if dec
            if (binAddrDEC.Checked || binLengthDEC.Checked) {
                for (int i = 0; i < strarray.Length; i++) {
                    int tempval = -1;
                    if (i % 2 == 0 && binAddrDEC.Checked) {
                        tempval = Convert.ToInt32(strarray[i]);
                        strarray[i] = tempval.ToString("X6");
                    }
                    if (i % 2 == 1 && binLengthDEC.Checked) {
                        tempval = Convert.ToInt32(strarray[i]);
                        strarray[i] = tempval.ToString("X2");
                    }
                }
            }

            //error check
            if (strarray.Length % 2 != 0) {
                advErrorLabel.Text = "ERROR: Odd number of entries in content.";
                return;
            }
            for (int i = 0; i < strarray.Length; i++) {
                if (i % 2 == 0 && strarray[i].Length != 6) {
                    advErrorLabel.Text = "ERROR: Entry " + i + " was not six characters";
                    return;
                }
                if (i % 2 == 1) {
                    if (strarray[i].Length > 4) {
                        advErrorLabel.Text = "ERROR: Entry " + i + " was too long (FFFF max)";
                        return;
                    }

                    int lastAddr = Convert.ToInt32(strarray[i - 1], 16);
                    int lastLen = Convert.ToInt32(strarray[i], 16);
                    if (lastAddr + lastLen > binFileLength) {
                        advErrorLabel.Text = "ERROR: Entry " + i + " overruns end of file.";
                        return;
                    }
                }
            }

            List<string> entries = new List<string>();
            string extractor;

            //step through array pulling bytes into strings
            for (int i = 0; i < strarray.Length; i += 2) {
                int currAddr = Convert.ToInt32(strarray[i], 16);
                int currLength = Convert.ToInt32(strarray[i + 1], 16);
                byte[] binArray = new byte[currLength];

                ArraySegment<byte> binSegment = new ArraySegment<byte>(binFileBytes, currAddr, currLength);
                int k = 0;
                for (int j = binSegment.Offset; j < (binSegment.Offset + binSegment.Count); j++) {
                    binArray[k] = binSegment.Array[j];
                    k++;
                }
                extractor = ByteArrayToString(binArray);
                entries.Add(extractor);
            }

            //output text into box
            string boxContent = "";
            for (int i = 0; i < strarray.Length; i += 2) {
                boxContent += entries[i / 2].ToUpper() + Environment.NewLine;
            }
            binOutputTextBox.Text = boxContent;

            //assemble file if checkbox checked
            if (binVerboseLog.Checked) {
                File.WriteAllText(filePath + fileName + ".txt", "MERROW " + labelVersion.Text + " Binary Output..." + Environment.NewLine);
                for (int i = 0; i < strarray.Length; i += 2) {
                    File.AppendAllText(filePath + fileName + ".txt", "0x" + strarray[i].ToUpper() + ":0x" + strarray[i + 1].ToUpper() + ":" + entries[i / 2].ToUpper() + Environment.NewLine);
                }
                filePath = filePath.Replace(@"/", @"\");   // explorer doesn't like front slashes
                System.Diagnostics.Process.Start("explorer.exe", Application.StartupPath + "\\Patches\\");
            }
        }

        //RLE DECOMPRESSOR-------------------------------------------------------------------------------------
        public static MemoryStream Decompress(byte[] input, uint inputSize) {
            byte marker, symbol;
            uint i, inputPosition, outputPosition, count;
            byte[] output = new byte[32768];

            if (inputSize < 1) { return null; }

            inputPosition = 0;
            marker = input[inputPosition++];
            outputPosition = 0;

            do {
                symbol = input[inputPosition++];

                if (symbol == marker) {
                    count = input[inputPosition++];

                    if (count <= 2) {
                        for (i = 0; i <= count; ++i) { output[outputPosition++] = marker; }

                    } else {
                        if (Convert.ToBoolean(count & 0x80)) { count = ((count & 0x7f) << 8) + input[inputPosition++]; }

                        symbol = input[inputPosition++];

                        for (i = 0; i <= count; ++i) { output[outputPosition++] = symbol; }
                    }
                } else { output[outputPosition++] = symbol; }
            } while (inputPosition < inputSize);

            MemoryStream ms = new MemoryStream(output);

            return ms;
        }

        public static int Compress(byte[] input, ref byte[] output, uint inputSize) {
            byte byte1, byte2, marker;
            uint i, inputPosition, outputPosition, count;
            uint[] histogram = new uint[256];

            if (inputSize < 1) {
                return 0;
            }

            for (i = 0; i < 256; ++i) {
                histogram[i] = 0;
            }

            for (i = 0; i < inputSize; ++i) {
                ++histogram[input[i]];
            }

            marker = 0;

            for (i = 1; i < 256; ++i) {
                if (histogram[i] < histogram[marker]) {
                    marker = (byte)i;
                }
            }

            output[0] = marker;
            outputPosition = 1;

            byte1 = input[0];
            inputPosition = 1;
            count = 1;

            if (inputSize >= 2) {
                byte2 = input[inputPosition++];
                count = 2;

                do {
                    if (byte1 == byte2) {
                        while ((inputPosition < inputSize) && (byte1 == byte2) && (count < 32768)) {
                            byte2 = input[inputPosition++];
                            ++count;
                        }

                        if (byte1 == byte2) {
                            encodeRepetition(output, ref outputPosition, marker, byte1, count);

                            if (inputPosition < inputSize) {
                                byte1 = input[inputPosition++];
                                count = 1;
                            } else {
                                count = 0;
                            }
                        } else {
                            encodeRepetition(output, ref outputPosition, marker, byte1, count - 1);
                            byte1 = byte2;
                            count = 1;
                        }
                    } else {
                        encodeNonRepetition(output, ref outputPosition, marker, byte1);
                        byte1 = byte2;
                        count = 1;
                    }
                    if (inputPosition < inputSize) {
                        byte2 = input[inputPosition++];
                        count = 2;
                    }
                } while ((inputPosition < inputSize) || (count >= 2));
            }

            if (count == 1) {
                encodeNonRepetition(output, ref outputPosition, marker, byte1);
            }

            return (int)outputPosition;
        }

        private static void encodeRepetition(byte[] output, ref uint outputPosition, byte marker, byte symbol, uint count) {
            uint index = outputPosition;

            if (count <= 3) {
                if (symbol == marker) {
                    output[index++] = marker;
                    output[index++] = (byte)(count - 1);
                } else {
                    for (uint i = 0; i < count; ++i) {
                        output[index++] = symbol;
                    }
                }
            } else {
                output[index++] = marker;
                --count;

                if (count >= 128) {
                    output[index++] = (byte)((count >> 8) | 0x80);
                }

                output[index++] = (byte)(count & 0xff);
                output[index++] = symbol;
            }

            outputPosition = index;
        }

        private static void encodeNonRepetition(byte[] output, ref uint outputPosition, byte marker, byte symbol) {
            uint index = outputPosition;

            if (symbol == marker) {
                output[index++] = marker;
                output[index++] = 0;
            } else {
                output[index++] = symbol;
            }

            outputPosition = index;
        }

        private static string decompressBytes(string input) {
            if (input.Length == 0 || input.Length % 2 != 0 || input == null) { return "error"; }

            byte[] tempin = StringToByteArray(input, false);

            MemoryStream ms = Decompress(tempin, (uint)tempin.Length);

            byte[] tempout = ReadFully(ms,0);

            string output = ByteArrayToString(tempout);

            return (string)output;
        }

        private static string decompressString(string input) {
            if (input.Length == 0 || input == null) { return "Error"; }

            string output = "";

            for (int i = 0; i < input.Length; i++) {

            }

            return output;
        }

        public static byte[] ReadFully(Stream stream, int initialLength)
        {
            // If we've been passed an unhelpful initial length, just use 32K.
            if (initialLength < 1) {
                initialLength = 32768;
            }

            byte[] buffer = new byte[initialLength];
            long read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, (int)read, buffer.Length - (int)read)) > 0)
            {
                read += chunk;

                // If we've reached the end of our buffer, check to see if there's any more information
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();

                    // End of stream? If so, we're done
                    if (nextByte == -1)
                    {
                        return buffer;
                    }

                    // Nope. Resize the buffer, put in the byte we've just read, and continue
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            // Buffer is now too big. Shrink it.
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }
    }
}
