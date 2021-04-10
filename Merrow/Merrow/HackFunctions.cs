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
            patcharray = StringToByteArray(patchbuild);
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

    }
}
