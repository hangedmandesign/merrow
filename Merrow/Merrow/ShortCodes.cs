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

namespace Merrow {
    public partial class MerrowStandard {
        //SHORTCODE WRITING AND READING FUNCTIONALITY
        //Basic concept: check the first three tabs of the randomizer tabControl, and grab all toggles, lists, and trackbars.
        //Then convert their values into (and back from):
        //Toggles - string of ".Checked" bools as binary
        //Dropdowns - collection of index values as ints, with additional bool binary strings if custom item lists are used
        //Sliders - collection of trackbar values as ints

        //Collect list of all checkboxes (toggles)
        private static List<CheckBox> GetAllToggles(Control container) {
            var controlList = new List<CheckBox>();
            foreach (Control c in container.Controls) {
                controlList.AddRange(GetAllToggles(c));

                if (c is CheckBox box)
                    controlList.Add(box);
            }
            return controlList;
        }

        //Collect list of all comboboxes (dropdowns)
        private static List<ComboBox> GetAllDropdowns(Control container) {
            var controlList = new List<ComboBox>();
            foreach (Control c in container.Controls) {
                controlList.AddRange(GetAllDropdowns(c));

                if (c is ComboBox box)
                    controlList.Add(box);
            }
            return controlList;
        }

        //Collect list of all trackbars (sliders)
        private static List<TrackBar> GetAllSliders(Control container) {
            var controlList = new List<TrackBar>();
            foreach (Control c in container.Controls) {
                controlList.AddRange(GetAllSliders(c));

                if (c is TrackBar box)
                    controlList.Add(box);
            }
            return controlList;
        }

        //UPDATING SHORTCODE---------------------------------------------------

        public void UpdateCode() {
            if (loadfinished) {
                updatingcode = true;
                //int tabpagestocheck = 3;
                string codeString = labelVersion.Text.Substring(1);
                string tempString;
                string binString2;
                //var toggles = new List<CheckBox>();
                //var dropdowns = new List<ComboBox>();
                //var sliders = new List<TrackBar>();

                ////check each page in turn, convert each page's values and add it to the code string
                //for (int i = 0; i < tabpagestocheck; i++) {
                //    toggles.AddRange(GetAllToggles(rndTabsControl.TabPages[i]));
                //    dropdowns.AddRange(GetAllDropdowns(rndTabsControl.TabPages[i]));
                //    sliders.AddRange(GetAllSliders(rndTabsControl.TabPages[i]));
                //}

                int steps = 0;
                tempString = "";
                binString2 = "";
                foreach (var toggle in toggles) { //build binary strings
                    steps++;
                    if (toggle.Checked) {
                        if (steps <= 32) { tempString += 1; }
                        if (steps > 32) { binString2 += 1; }
                    }
                    else {
                        if (steps <= 32) { tempString += 0; }
                        if (steps > 32) { binString2 += 0; }
                    }
                }
                int test = Convert.ToInt32(tempString, 2); //convert binary string to int
                int test2 = 0;
                if (steps > 32) { test2 = Convert.ToInt32(binString2, 2); }
                if (steps > 32) { codeString += ".T." + test.ToString("X") + "-" + test2.ToString("X") + "."; }//int to hex
                else { codeString += ".T." + test.ToString("X") + "."; }

                //this string needs to be encoded piece by piece
                //custom values will have custom delimiter and format
                //for custom values, collect each 4 items in each list as a binary>hex conversion, single hex character 0-F

                tempString = "";
                string itemString = "";
                codeString += "L.";

                foreach (var dropdown in dropdowns) {
                    bool bighex = false;
                    string listString = "";
                    if (dropdown.Items.Count > 16) { bighex = true; }

                    //one possible shortening method: count consecutive 0 values and amalgamate them in a separate counter
                    if (dropdown.SelectedIndex == 8) { //if custom items
                        if (dropdown.Name == "rndChestDropdown" || dropdown.Name == "rndDropsDropdown" || dropdown.Name == "rndGiftersDropdown" || dropdown.Name == "rndWingsmithsDropdown") {
                            if (dropdown.Name == "rndChestDropdown") {
                                listString = itemListSkim(itemListView1);
                                itemString += ":!" + HexToBase64(listString);
                            }
                            if (dropdown.Name == "rndDropsDropdown") {
                                listString = itemListSkim(itemListView2);
                                itemString += ":@" + HexToBase64(listString);
                            }
                            if (dropdown.Name == "rndGiftersDropdown") {
                                listString = itemListSkim(itemListView3);
                                itemString += ":#" + HexToBase64(listString);
                            }
                            if (dropdown.Name == "rndWingsmithsDropdown") {
                                listString = itemListSkim(itemListView4);
                                itemString += ":$" + HexToBase64(listString);
                            }
                            if (!bighex) { tempString += dropdown.SelectedIndex.ToString("X1"); } //also put the 8 in tempstring for reference
                        }
                        else { //convert values to hex and then add as base64
                            if (bighex) { tempString += dropdown.SelectedIndex.ToString("X2"); }
                            if (!bighex) { tempString += dropdown.SelectedIndex.ToString("X1"); }
                        }
                    }
                    else { //convert values to hex and then add as base64
                        if (bighex) { tempString += dropdown.SelectedIndex.ToString("X2"); }
                        if (!bighex) { tempString += dropdown.SelectedIndex.ToString("X1"); }
                    }
                }
                codeString += tempString + itemString + ".";//HexToBase64(tempString) + itemString + ".";

                tempString = "";
                if (sliders.Count > 0) {
                    foreach (var slider in sliders) {
                        tempString += slider.Value.ToString("X3"); //convert values to hex
                    }
                    if (tempString.Length % 2 != 0) { tempString = "0" + tempString; } //ensure it's an even number of characters
                    codeString += "S." + tempString;//HexToBase64(tempString);
                }
                else {
                    codeString += "SZ";
                }
                rndShortcodeText.Text = codeString;
            }
            updatingcode = false;
        }

        //APPLY SHORTCODE--------------------------------------------------------------------
        //Basically as above, but in reverse, with some extra checks for unpacking of item strings and errors.
        
        public int ApplyCode() {
            updatingcode = true;
            string currentCode = rndShortcodeText.Text;
            //int tabpagestocheck = 3;
            string versionNumber = labelVersion.Text.Substring(1); //current version to check against
            string tempString;
            //var toggles = new List<CheckBox>();
            //var dropdowns = new List<ComboBox>();
            //var sliders = new List<TrackBar>();

            //all of the following could be an array but for the purposes of building this, i'm brute-forcing it.
            string togglestring = "";
            string dropdownstring = "";
            string sliderstring = "";
            string itemstring1 = "%"; //percent sign persists if unedited
            string itemstring2 = "%";
            string itemstring3 = "%";
            string itemstring4 = "%";
            int togglestart = 0; //starting locations of values in code
            int dropdownstart = 0;
            int sliderstart = 0;
            int dropdownend = 0;
            int firstcolon = -1; //-1 persists if no itemstrings

            //if it's too short, quit
            if (currentCode.Length < 10) { return 1; }

            //wrong version number, quit
            if (currentCode.Substring(0, 2) != versionNumber) { return 2; } 

            //check each page in turn, convert each page's counts and add it to the lists, to check against/update
            //for (int i = 0; i < tabpagestocheck; i++) {
            //    toggles.AddRange(GetAllToggles(rndTabsControl.TabPages[i]));
            //    dropdowns.AddRange(GetAllDropdowns(rndTabsControl.TabPages[i]));
            //    sliders.AddRange(GetAllSliders(rndTabsControl.TabPages[i]));
            //}

            //search string for starting and ending points
            for (int i = 0; i < currentCode.Length - 3; i++) { //only go near end of the string to prevent overflow

                //toggles
                if (currentCode.Substring(i, 3) == ".T.") { togglestart = i + 3; }

                //dropdowns (L for "lists" because D is a hex character)
                if (currentCode.Substring(i, 3) == ".L.") {
                    dropdownstart = i + 3;
                    togglestring = currentCode.Substring(togglestart, i - togglestart);
                }

                //sliders
                if (currentCode.Substring(i, 3) == ".S.") {
                    sliderstart = i + 3;
                    dropdownend = i - 1; //set dropdownstring below, depending on if there are itemstrings or not
                    sliderstring = currentCode.Substring(sliderstart);
                }
                if (currentCode.Substring(i, 3) == ".SZ") { //if there are no sliders
                    sliderstart = -1;
                    dropdownstring = currentCode.Substring(dropdownstart, i - dropdownstart);
                }

                //check if there is a colon, which marks the start of an item list string
                //if there isn't one by the end (i == -1), we know there's no item strings
                if (currentCode[i] == ':' && firstcolon == -1) { firstcolon = i; } 

                //grab specific item strings if they exist - they're always fixed length 8
                if (currentCode.Substring(i, 2) == ":!") { itemstring1 = currentCode.Substring(i + 2, 8); } //always 8 characters, so we can just grab them now
                if (currentCode.Substring(i, 2) == ":@") { itemstring2 = currentCode.Substring(i + 2, 8); }
                if (currentCode.Substring(i, 2) == ":#") { itemstring3 = currentCode.Substring(i + 2, 8); }
                if (currentCode.Substring(i, 2) == ":$") { itemstring4 = currentCode.Substring(i + 2, 8); }
            }

            //now define dropdownstring
            if (itemstring1 == "%" && itemstring2 == "%" && itemstring3 == "%" && itemstring4 == "%") { //no itemstrings
                dropdownstring = currentCode.Substring(dropdownstart, 1 + dropdownend - dropdownstart); //the (1 +) here is because length values are not zero-indexed
            }
            else { dropdownstring = currentCode.Substring(dropdownstart, firstcolon - dropdownstart); } //already (1 +)

            //kick out on malformatted strings to prevent crashes
            if (togglestart == 0 || dropdownstart == 0 || sliderstart == 0) { return 3; } 
            //if (dropdownstring.Length % 2 != 0) { return 5; }
            if (sliderstring.Length % 2 != 0) { return 6; }

            //DECODE TOGGLES
            //64b to hex to int to binary
            string toggletemp = togglestring;
            string toggletemp2 = "";
            for (int i = 0; i < togglestring.Length; i++) {
                if (togglestring[i] == '-') {
                    toggletemp = togglestring.Substring(0, i); //first half of string, up to hyphens
                    toggletemp2 = togglestring.Substring(i + 1); //separate out second half of string, skip hyphens
                }
            }

            //converting back to strings, PadLeft prevents the binary conversion from removing leading zeroes
            if (toggles.Count <= 32) {
                toggletemp = Convert.ToString(Convert.ToInt32(toggletemp, 16), 2).PadLeft(toggles.Count, '0');
                //if (togglestring.Length % 2 != 0) { return 4; }
            }
            if (toggles.Count > 32) {
                toggletemp = Convert.ToString(Convert.ToInt32(toggletemp, 16), 2).PadLeft(32, '0');

                //just add toggletemp2 to the binary string, since it's not bound by 32-char limit like the binary number
                toggletemp += Convert.ToString(Convert.ToInt32(toggletemp2, 16), 2).PadLeft(toggles.Count - 32, '0');
                //if (togglestring.Length % 2 != 0) { return 7; }
            }

            //then read binary string as bools
            var x = 0;
            foreach (var toggle in toggles) {
                if (toggletemp[x] == '1') { toggle.Checked = true; } else { toggle.Checked = false; } 
                x++;
            }

            //DECODE DROPDOWNS
            //set all them first: check the dropdown's length, and iterate the count by either 1 or 2 if bighex.
            //bighex: the dropdown has more than 16 entries and thus needs 2-char hex and not 1-char hex.
            //and then override the item lists after if needed

            string dropdowntemp = dropdownstring;//Base64ToHex(dropdownstring);
            x = 0;
            
            foreach (var dropdown in dropdowns) {
                Console.WriteLine(dropdown.Name);
                bool bighex = false;
                int currentvalue = 0;
                if (dropdown.Items.Count > 16) { bighex = true; }

                if (bighex) { //two hex chars as int
                    currentvalue = Convert.ToInt32(dropdowntemp.Substring(x, 2), 16); 
                    x += 2;
                }
                if (!bighex) { //one hex char as int
                    currentvalue = Convert.ToInt32(dropdowntemp.Substring(x, 1), 16); 
                    x += 1;
                }
                dropdown.SelectedIndex = currentvalue; //update the dropdown
            }

            //if any itemstrings exist, unpack them as bools and update the lists directly
            if (firstcolon != -1) { 
                if (itemstring1 != "%") {
                    tempString = Base64ToHex(itemstring1);
                    itemListUnpack(itemListView1, tempString);
                }
                if (itemstring2 != "%") {
                    tempString = Base64ToHex(itemstring2);
                    itemListUnpack(itemListView2, tempString);
                }
                if (itemstring3 != "%") {
                    tempString = Base64ToHex(itemstring3);
                    itemListUnpack(itemListView3, tempString);
                }
                if (itemstring4 != "%") {
                    tempString = Base64ToHex(itemstring4);
                    itemListUnpack(itemListView4, tempString);
                }
            }

            //DECODE SLIDERS

            if (sliderstart != -1) { //only if sliders exist
                string slidertemp = sliderstring;// Base64ToHex(sliderstring);
                if (slidertemp.Length % 3 != 0) { slidertemp = slidertemp.Substring(1); } //remove the optional leading zero
                x = 0;
                foreach (var slider in sliders) {
                    slider.Value = Convert.ToInt32(slidertemp.Substring(x, 3), 16); //convert each 3-char hex value to int
                    x += 3;
                }
            }

            //DONE
            updatingcode = false;
            return 0;
        }
    }
}
