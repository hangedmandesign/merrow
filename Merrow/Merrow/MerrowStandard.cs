using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Security;
using System.Drawing;

namespace Merrow {
    public partial class MerrowStandard : Form {
        //data structures
        DataStore library;
        Random SysRand = new Random();
        private OpenFileDialog binOpenFileDialog;

        //variables
        int spellstart = 13941344; //D4BA60
        int spelloffset = 68;
        int playerspells = 60;
        public string filePath = @"Patches\\";
        string fileName = "merrowpatch";
        string headerHex = "5041544348"; //PATCH
        string patchcontent = "";
        string footerHex = "454F46"; //EOF
        string patchbuild = "";
        int tempaddr;
        string tempstr1;
        string tempstr2;
        public int rngseed;
        int zoomvalue = 1;
        int passcheck = 0;
        bool loadfinished = false;
        bool verboselog = true;
        int binFileLength = 0;
        bool binFileLoaded = false;
        string textPaletteHex = "00008888FFFF"; //busted default palette to make errors obvious

        //collection arrays and lists
        byte[] patcharray;
        int[] shuffles = new int[60];
        List<int> reorg = new List<int>();
        int[] chests = new int[87];
        int[] drops = new int[67];
        int[] texts = new int[208];
        int[] inntexts = new int[17];
        string[] hintnames = new string[60];
        string[] spoilerspells = new string[60];
        string[] spoilerchests = new string[87];
        string[] spoilerdrops = new string[67];
        byte[] binFileBytes;

        //crash sets and safe lists
        int[] crashset1 = { 3, 9, 12, 45, 46, 50, 51 }; //HA1, HA2, MBL, WC1, WC2, WC3, LC
        int[] safeset1 = { 0, 1, 3, 5, 9, 12, 18, 28, 35, 42, 44, 45, 46, 50, 51, 53, 55 };
        int[] crashset2 = { 26 }; //AVA
        int[] UNsafeset2 = { 3, 5, 9, 12, 23, 46, 50 };
        int[] crashset3 = { 40 }; //H2
        int[] UNsafeset3 = { 23, 50 };

        //categories
        int[] statusspells = { 6, 17, 19, 22, 24, 25, 29, 43, 44, 47, 48, 49, 52, 54, 56, 57, 58 };
        int[] offenseSpells = { 0, 1, 3, 4, 5, 8, 9, 10, 12, 13, 14, 15, 16, 18, 20, 21, 23, 26, 28, 30, 31, 34, 35, 36, 42, 45, 46, 50, 51, 53, 55, 59 };
        int[] effectSpells = { 17, 19, 22, 25, 27, 32, 38, 40, 43, 44, 47, 48, 49, 52, 54, 56, 57, 58 };
        int[] brianSpells = { 2, 6, 7, 11, 24, 29, 33, 37, 39, 41 };

        //INITIALIZATION----------------------------------------------------------------

        public MerrowStandard() {
            InitializeComponent();

            //prepare variables
            if (!Directory.Exists(filePath)) { Directory.CreateDirectory(filePath); }
            binOpenFileDialog = new OpenFileDialog() {
                FileName = "Select a file...",
                Title = "Open binary file"
            };
            shuffles = new int[playerspells];
            spoilerspells = new string[playerspells];
            library = new DataStore();
            rngseed = SysRand.Next(100000000, 1000000000); //a 9-digit number
            SysRand = new Random(rngseed);
            seedTextBox.Text = rngseed.ToString();
            fileName = "merrowpatch_" + rngseed.ToString();

            //initiate spell list with SHUFFLED option before
            List<string> options = new List<string> { "SHUFFLED" };
            for (int i = 0; i < playerspells; i++) {
                shuffles[i] = -1;
                options.Add(library.spells[i * 4]);
            }
            rndSpellDropdown.Items.Clear();
            rndSpellDropdown.Items.AddRange(options.ToArray<object>());

            //initiate chest content list
            for (int j = 0; j < chests.Length; j++) { chests[j] = library.chestdata[j * 2 + 1]; }

            //initiate item drops list
            for (int k = 0; k < drops.Length; k++) { drops[k] = library.dropdata[k * 2 + 1]; }

            //initiate text lists
            int lenS = library.singletextdata.Length / 3; //72
            int lenD = library.doubletextdata.Length / 4; //68
            for (int m = 0; m < lenS; m++) { texts[m] = library.singletextdata[m * 3 + 2]; } //add single texts
            for (int m = 0; m < lenD; m++) { texts[lenS + m] = library.doubletextdata[m * 4 + 2]; } //add double texts
            for (int m = 0; m < lenD; m++) { texts[lenS + lenD + m] = library.doubletextdata[m * 4 + 3]; } //i know this could be tidier but getting it right was annoying
            for (int m = 0; m < inntexts.Length; m++) { inntexts[m] = library.inntextdata[m * 3 + 2]; } //add inn texts

            //now that dropdowns have content, fix them
            PrepareDropdowns();

            //initial shuffle
            Shuffling(true);
            loadfinished = true;
        }

        //list of initial UI cleanup/prep steps
        private void PrepareDropdowns() { 
            rndSpellDropdown.SelectedIndex = 0;
            rndChestDropdown.SelectedIndex = 0;
            rndTextPaletteDropdown.SelectedIndex = 0;
            rndTextContentDropdown.SelectedIndex = 0;
            rndDropsDropdown.SelectedIndex = 0;
            quaAccuracyDropdown.SelectedIndex = 0;
            quaZoomDropdown.SelectedIndex = 0;

            rndSpellDropdown.Visible = false;
            rndSpellNames.Visible = false;
            rndChestDropdown.Visible = false;
            rndTextPaletteDropdown.Visible = false;
            rndTextContentDropdown.Visible = false;
            rndDropsDropdown.Visible = false;
            rndWeightedChest.Visible = false;
            quaAccuracyDropdown.Visible = false;
            quaZoomDropdown.Visible = false;
            crcWarningLabel.Visible = false;

            binAddrHEX.Checked = true;
            binLengthHEX.Checked = true;
        }

        //UNIFIED SHUFFLING FUNCTION----------------------------------------------------------------

        public void Shuffling(bool crashpro) {
            int k = 0;

            //REINITIATE RANDOM WITH NEW SEED
            SysRand = new System.Random(rngseed);

            //The reason we reset the seed and reshuffle everything every time, whether they're enabled or not, is because the number of times the random seed is used determines the sequence of random values.
            //So to guarantee the random seed to produce the same results with the same options, it has to do the same number of random checks each time. It might not always be necessary but it prevents errors.
            //That's also why this doesn't check for checkboxes: checkbox state & dropdown visible state, or not tied to rerolling the RNG so this removes having to know what state every object is in.

            //SPELL SHUFFLING
            //first, clear and fill the 'reorg' list in order, then shuffle it
            reorg.Clear();
            for (int i = 0; i < playerspells; i++) { reorg.Add(i); }
            int n = reorg.Count;
            while (n > 1) {
                n--;
                k = SysRand.Next(n + 1);
                int temp = reorg[k];
                reorg[k] = reorg[n];
                reorg[n] = temp;
            }

            //crash protection disabled - dumps reorg directly into shuffles arra
            if (!crashpro) {
                for (int i = 0; i < playerspells; i++) {
                    shuffles[i] = reorg[i];
                }
            }

            //crash protection enabled
            if (crashpro) {
                bool step = false;
                int c;
                int r;
                int s;

                //crash set 1, with safe set
                for (c = 0; c < crashset1.Length; c++)  //step through crash set
                {
                    step = false;

                    for (r = 0; r < reorg.Count; r++) //for each crash set value, step through reorg
                    {
                        for (s = 0; s < safeset1.Length; s++) //for each reorg value, check it against every safe value
                        {
                            if (reorg[r] == safeset1[s]) { //found a safe value
                                shuffles[crashset1[c]] = safeset1[s]; //replace the -1 value in the correct location in shuffles
                                reorg.RemoveAt(r); //and then Remove it from reorg
                                step = true; //need to exit both inner loops and move forward in the crash set. I'm sure there's a much neater way to do this
                            }

                            if (step) { break; } //escape to outermost loop
                        }

                        if (step) { break; } //escape to outermost loop
                    }
                }

                //crash set 2, with UNsafe set (since safe set would be longer)
                for (c = 0; c < crashset2.Length; c++) {
                    step = false;

                    for (r = 0; r < reorg.Count; r++) {
                        for (s = 0; s < UNsafeset2.Length; s++) //for each reorg value, check it against every *unsafe* value
                        {
                            if (reorg[r] == UNsafeset2[s]) { step = true; } //if it's listed as unsafe, break out of this loop, check the next reorg
                            if (step) { break; }
                        }

                        if (!step) { //if innermost loop ended without finding an unsafe value, then set it to the current value and break out
                            shuffles[crashset2[c]] = reorg[r];
                            reorg.RemoveAt(r);
                            step = true; //get back to outermost loop, we're done
                        }

                        if (step) { break; } //escape to outermost loop
                    }
                }

                //crash set 3, with UNsafe set
                for (c = 0; c < crashset3.Length; c++) {
                    step = false;

                    for (r = 0; r < reorg.Count; r++) {
                        for (s = 0; s < UNsafeset3.Length; s++) {
                            if (reorg[r] == UNsafeset3[s]) { step = true; }
                            if (step) { break; }
                        }

                        if (!step) {
                            shuffles[crashset3[c]] = reorg[r];
                            reorg.RemoveAt(r);
                            step = true;
                        }

                        if (step) { break; }
                    }
                }

                r = 0;
                for (s = 0; s < shuffles.Length; s++) //now that we're done with all the crash sets, we can just fill in the rest of the array
                {
                    if (shuffles[s] == -1) //if this value hasn't been set yet, set it to the first remaining value in reorg
                    {
                        shuffles[s] = reorg[r];
                        r++;
                    }
                }

                //this should prevent all crashes.
                Console.WriteLine("Crash protection complete.");
            }

            //SPELL NAME SHUFFLING (based on shuffles array and existing data)
            for (int i = 0; i < playerspells; i++) {
                bool fiftyfifty = SysRand.NextDouble() > 0.5;
                if(fiftyfifty) { hintnames[i] = library.shuffleNames[i*5] + " " + library.shuffleNames[(shuffles[i] * 5) + 1]; }
                else { hintnames[i] = library.shuffleNames[shuffles[i] * 5] + " " + library.shuffleNames[(i * 5) + 1]; }
            }

            //CHEST SHUFFLING (based on Chest Shuffle dropdown value)
            if (rndChestDropdown.SelectedIndex == 1) { //RANDOM: STANDARD CHEST ITEMS 0-13
                int c = chests.Length;
                while (c > 1) {
                    c--;
                    if(rndWeightedChest.Checked) {
                        if (c > 14) { k = SysRand.Next(14); } else { k = c - 1; }
                    }
                    if (!rndWeightedChest.Checked) {
                        k = SysRand.Next(14);
                    }
                    chests[c] = k;
                }
            }

            if (rndChestDropdown.SelectedIndex == 2) { //RANDOM: STANDARD AND WINGS 0-19
                int c = chests.Length;
                while (c > 1) {
                    c--;
                    if (rndWeightedChest.Checked) {
                        if (c > 20) { k = SysRand.Next(20); } else { k = c - 1; }
                    }
                    if (!rndWeightedChest.Checked) {
                        k = SysRand.Next(20);
                    }
                    chests[c] = k;
                }
            }

            if (rndChestDropdown.SelectedIndex == 3) { //RANDOM: STANDARD AND GEMS 0-13, 20-23
                int c = chests.Length;
                while (c > 1) {
                    c--;
                    if (rndWeightedChest.Checked) {
                        if (c > 18) { k = SysRand.Next(18); } else { k = c - 1; }
                    }
                    if (!rndWeightedChest.Checked) {
                        k = SysRand.Next(18); //produces 0-17
                    }
                    if (k > 13) { k += 6; } //adds 6 if above 13, to get 20-23
                    chests[c] = k;
                }
            }

            if (rndChestDropdown.SelectedIndex == 4) { //CHAOS: STANDARD, WINGS, GEMS 0-23
                int c = chests.Length;
                while (c > 1) {
                    c--;
                    if (rndWeightedChest.Checked) {
                        if (c > 24) { k = SysRand.Next(24); } else { k = c - 1; }
                    }
                    if (!rndWeightedChest.Checked) {
                        k = SysRand.Next(24);
                    }
                    chests[c] = k;
                }
            }

            //STANDARD CHEST SHUFFLE (always happens afterwards, or if dropdown index is 0)
            int d = chests.Length;
            while (d > 1) {
                d--;
                k = SysRand.Next(d + 1);
                int temp = chests[k];
                chests[k] = chests[d];
                chests[d] = temp;
            }

            //DROP SHUFFLING (based on Item Drops dropdown value)
            if (rndDropsDropdown.SelectedIndex == 0) { //STANDARD SHUFFLE
                int c = drops.Length;
                while (c > 1) {
                    c--;
                    k = SysRand.Next(c + 1);
                    int temp = drops[k];
                    drops[k] = drops[c];
                    drops[c] = temp;
                }
            }

            if (rndDropsDropdown.SelectedIndex == 1) { //RANDOM: STANDARD CHEST ITEMS 0-13
                int c = drops.Length;
                while (c > 1) {
                    c--;
                    k = SysRand.Next(14);
                    drops[c] = k;
                }
            }

            if (rndDropsDropdown.SelectedIndex == 2) { //RANDOM: STANDARD AND WINGS 0-19
                int c = drops.Length;
                while (c > 1) {
                    c--;
                    k = SysRand.Next(20);
                    drops[c] = k;
                }
            }

            if (rndDropsDropdown.SelectedIndex == 3) { //RANDOM: STANDARD AND GEMS 0-13, 20-23
                int c = drops.Length;
                while (c > 1) {
                    c--;
                    k = SysRand.Next(18); //produces 0-17
                    if (k > 13) { k += 6; } //adds 6 if above 13, to get 20-23
                    drops[c] = k;
                }
            }

            if (rndDropsDropdown.SelectedIndex == 4) { //CHAOS: STANDARD, WINGS, GEMS 0-23
                int c = drops.Length;
                while (c > 1) {
                    c--;
                    k = SysRand.Next(24);
                    drops[c] = k;
                }
            }

            //TEXT SHUFFLING (May be based on dropdown value more later, text shortening, whatever)
            if (rndTextContentDropdown.SelectedIndex == 0) {
                int c = texts.Length;
                while (c > 1) {
                    c--;
                    k = SysRand.Next(c + 1);
                    int temp = texts[k];
                    texts[k] = texts[c];
                    texts[c] = temp;
                }

                c = inntexts.Length;
                while (c > 1) {
                    c--;
                    k = SysRand.Next(c + 1);
                    int temp = inntexts[k];
                    inntexts[k] = inntexts[c];
                    inntexts[c] = temp;
                }
            }

            //TEST COLOUR SHUFFLING (Trying on text colour, cause it's simple)
            if(rndTextPaletteDropdown.SelectedIndex == 5) {
                Color texPal1 = RGBAToColor(library.baseRedTextPalette[0]); //convert base colours from hex
                Color texPal2 = RGBAToColor(library.baseRedTextPalette[1]);
                Color texPal3 = RGBAToColor(library.baseRedTextPalette[2]);

                float hueOffset = (float)SysRand.Next(0,360); //pick random hue offset

                texPal1 = TransformHSV(texPal1, hueOffset, 1f, 1f); //apply hue offset to all colours
                texPal2 = TransformHSV(texPal2, hueOffset, 1f, 1f);
                texPal3 = TransformHSV(texPal3, hueOffset, 1f, 1f);

                Console.WriteLine(texPal1.ToString());

                textPaletteHex = ColorToHex(texPal1) + ColorToHex(texPal2) + ColorToHex(texPal3);
            }
        }

        //BUILD QUEST PATCH----------------------------------------------------------------

        public void BuildPatch() {
            //update filename one more time here to avoid errors
            if (filenameTextBox.Text != "" && filenameTextBox.Text != null) {
                fileName = string.Join("", filenameTextBox.Text.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries)); //strip all whitespace to avoid errors
            }
            else { fileName = "merrowpatch_" + rngseed.ToString(); }

            //shuffle here so I don't have to shuffle after every option is changed
            Shuffling(true);

            //start spoiler log and initialize empty patch
            File.WriteAllText(filePath + fileName + "_spoiler.txt", "MERROW " + labelVersion.Text + " building patch..." + Environment.NewLine);
            File.AppendAllText(filePath + fileName + "_spoiler.txt", "Seed: " + rngseed.ToString() + Environment.NewLine);
            File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "PATCH MODIFIERS:" + Environment.NewLine);
            patchbuild = "";
            patchcontent = "";

            //RANDOMIZATION FEATURES

            if (rndSpellToggle.Checked) { //Spell Shuffle
                for (int q = 0; q < playerspells; q++) {
                    int tempq = 0;

                    if (rndSpellDropdown.SelectedIndex == 0) { tempq = shuffles[q]; } //set spell q to use spell shuffles[q] data
                    if (rndSpellDropdown.SelectedIndex != 0) { tempq = rndSpellDropdown.SelectedIndex - 1; }

                    tempaddr = Convert.ToInt32(library.spells[(q * 4) + 2]) + 3; //set rule address from dec version of hex, incrementing 3
                    tempstr1 = Convert.ToString(tempaddr, 16); //convert updated address back to hex string
                    tempstr2 = library.spells[(tempq * 4) + 3].Substring(6, 2); //copy other spell rule data
                    patchcontent += tempstr1; //current spell rule address
                    patchcontent += "0001"; //spell rule length, hex for 1
                    patchcontent += tempstr2; //copied spell rule data

                    tempaddr = Convert.ToInt32(library.spells[(q * 4) + 2]) + 11; //set remaining address from dec version of hex, incrementing 11
                    tempstr1 = Convert.ToString(tempaddr, 16); //convert updated address back to hex string
                    tempstr2 = library.spells[(tempq * 4) + 3].Substring(22); //copy other remaining data
                    patchcontent += tempstr1; //current remaining address
                    patchcontent += "0039"; //remaining length, hex for 57
                    patchcontent += tempstr2; //copied remaining data

                    spoilerspells[q] = library.spells[(q * 4)] + " > " + library.spells[(tempq * 4)];
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Spells overridden." + Environment.NewLine);

                if (rndSpellNames.Checked && rndSpellDropdown.SelectedIndex == 0) {
                    //boss spells
                    patchcontent += library.shuffleBossSpellNames[0];
                    patchcontent += library.shuffleBossSpellNames[1];

                    //spell pointers
                    for (int i = 0; i < playerspells; i++) {
                        patchcontent += library.shuffleNames[(i * 5) + 3]; //pointer location
                        patchcontent += "0004"; //write four bytes
                        patchcontent += library.shuffleNames[(i * 5) + 4]; //new pointer data
                    }

                    //spell names
                    for (int i = 0; i < playerspells; i++) {
                        string temps = ToHex(hintnames[i]);
                        int zeroes = 32 - temps.Length;
                        patchcontent += library.shuffleNames[(i * 5) + 2];
                        patchcontent += "0010";
                        patchcontent += temps;
                        for (int j = 0; j < zeroes; j++) { patchcontent += "0"; }
                    }

                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Shuffled spells use hinted names." + Environment.NewLine);
                }

                patchcontent += "6672600C000000060000000100000001"; //Fix for skelebat group in Blue Cave that can cause crashes due to lag
            }

            if (rndTextPaletteToggle.Checked) { //Text Colour
                //default black palette is stored at D3E240
                // black: F83E9C1B6AD5318D
                // red: F83E9C1BBA0DD009
                // blue: F83E9C1B629D19AB
                // white: F83E318DBDEFF735

                int temp = rndTextPaletteDropdown.SelectedIndex;
                if (rndTextPaletteDropdown.SelectedIndex == 4) { temp = SysRand.Next(0, 4); }

                patchcontent += "D3E2400008";
                if (temp == 0) {
                    patchcontent += "F83E9C1B6AD5318D";
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text palette set to black [default]." + Environment.NewLine);
                }
                if (temp == 1) {
                    patchcontent += "F83E9C1BBA0DD009";
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text palette set to red." + Environment.NewLine);
                }
                if (temp == 2) {
                    patchcontent += "F83E9C1B629D19AB";
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text palette set to blue." + Environment.NewLine);
                }
                if (temp == 3) {
                    patchcontent += "F83E318DBDEFF735";
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text palette set to white." + Environment.NewLine);
                }

                if (rndTextPaletteDropdown.SelectedIndex == 5) {
                    patchcontent += "F83E9C1BBA0DD009"; //uses red text palette since it's brightest default
                    patchcontent += "D3E2620006";
                    patchcontent += textPaletteHex;
                    Console.WriteLine(textPaletteHex);
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text palette set to random." + Environment.NewLine);
                }
            }

            if (rndChestToggle.Checked) { //Chest shuffle
                //add chest addresses, and new byte
                for (int i = 0; i < chests.Length; i++) {
                    int temp = library.chestdata[i * 2] + 33; //33 is offset to chest item byte
                    patchcontent += Convert.ToString(temp, 16);
                    patchcontent += "0001";
                    patchcontent += chests[i].ToString("X2");

                    spoilerchests[i] = i.ToString("00") + ": " + library.items[(chests[i] * 3)];
                }

                if (rndChestDropdown.SelectedIndex == 0) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Chest contents shuffled." + Environment.NewLine);
                }
                if (rndChestDropdown.SelectedIndex == 1) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Chest contents randomized (standard)." + Environment.NewLine);
                }
                if (rndChestDropdown.SelectedIndex == 2) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Chest contents randomized (standard and wings)." + Environment.NewLine);
                }
                if (rndChestDropdown.SelectedIndex == 3) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Chest contents randomized (standard and gems)." + Environment.NewLine);
                }
                if (rndChestDropdown.SelectedIndex == 4) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Chest contents randomized (chaos)." + Environment.NewLine);
                }
                if (rndWeightedChest.Checked) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Chest contents weighted (minimum one of each)." + Environment.NewLine);
                }
            }

            if (rndTextContentToggle.Checked) {
                int temp = 0;
                //add single text addresses, and new byte
                for (int i = 0; i < 72; i++) {
                    temp = library.singletextdata[i * 3] + 8; //text byte at offset 8
                    patchcontent += Convert.ToString(temp, 16);
                    patchcontent += "0002";
                    patchcontent += texts[i].ToString("X4");
                }

                //add double text addresses, and new byte
                for (int i = 0; i < 68; i++) {
                    temp = library.doubletextdata[i * 4] + 8; //first text at offset 8
                    patchcontent += Convert.ToString(temp, 16);
                    patchcontent += "0002";
                    patchcontent += texts[i + 72].ToString("X4");

                    temp = library.doubletextdata[i * 4] + 10; //second text at offset 10
                    patchcontent += Convert.ToString(temp, 16);
                    patchcontent += "0002";
                    patchcontent += texts[i + 72 + 68].ToString("X4");
                }

                //add inn text addresses, and new byte
                for (int i = 0; i < inntexts.Length; i++) {
                    temp = library.inntextdata[i * 3] + 8; //text byte at offset 8
                    patchcontent += Convert.ToString(temp, 16);
                    patchcontent += "0002";
                    patchcontent += inntexts[i].ToString("X4");
                }

                if (rndTextContentDropdown.SelectedIndex == 0) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text content shuffled." + Environment.NewLine);
                }
            }

            if (rndDropsToggle.Checked) { //Item Drop Shuffle
                //add drop addresses, and new byte
                for (int i = 0; i < drops.Length; i++) {
                    int temp = library.dropdata[i * 2]; //don't need to offset because drop list is pre-offset
                    patchcontent += Convert.ToString(temp, 16);
                    patchcontent += "0001";
                    patchcontent += drops[i].ToString("X2");

                    if (drops[i] != 255) { spoilerdrops[i] = library.monsternames[i] + ": " + library.items[drops[i] * 3]; }
                    if (drops[i] == 255) { spoilerdrops[i] = library.monsternames[i] + ": NONE"; }
                }

                if (rndDropsDropdown.SelectedIndex == 0) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Enemy drops shuffled." + Environment.NewLine);
                }
                if (rndDropsDropdown.SelectedIndex == 1) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Enemy drops randomized (standard)." + Environment.NewLine);
                }
                if (rndDropsDropdown.SelectedIndex == 2) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Enemy drops randomized (standard and wings)." + Environment.NewLine);
                }
                if (rndDropsDropdown.SelectedIndex == 3) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Enemy drops randomized (standard and gems)." + Environment.NewLine);
                }
                if (rndDropsDropdown.SelectedIndex == 4) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Enemy drops randomized (chaos)." + Environment.NewLine);
                }
            }

            //QUALITY OF LIFE FEATURES

            if (quaInvalidityToggle.Checked) { //Invalidity
                patchcontent += "D4CAE9000100"; //Zel_Cat
                patchcontent += "D4CBB0000100"; //Npt_Turn
                patchcontent += "D4CC3D000100"; //Far_Bom
                patchcontent += "D4CC81000100"; //Sil_Laser
                patchcontent += "D4CCC5000100"; //Sil_Cat
                patchcontent += "D4CD09000100"; //Gil_Punch
                patchcontent += "D4CD91000100"; //Ges_Cat
                patchcontent += "D4CE10000100"; //On_Light

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Boss spell passive invalidity disabled." + Environment.NewLine);
            }

            if (quaAccuracyToggle.Checked) {
                if (quaAccuracyDropdown.SelectedIndex == 0) { //Spell Accuracy: Status
                    for (int i = 0; i < 17; i++) {
                        string spellloc = library.spells[(statusspells[i] * 4) + 2];
                        int temploc = Convert.ToInt32(spellloc) + 15;
                        patchcontent += Convert.ToString(temploc, 16);
                        patchcontent += "000164";
                    }

                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Status effects' accuracy normalized to 100." + Environment.NewLine);
                }

                if (quaAccuracyDropdown.SelectedIndex == 1) { //Spell Accuracy: All
                    for (int z = spellstart + 15; z < ((spelloffset * playerspells) + spellstart); z += spelloffset) {
                        patchcontent += Convert.ToString(z, 16);
                        patchcontent += "000164";
                    }

                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "All spells' accuracy normalized to 100." + Environment.NewLine);
                }
            }

            if (quaZoomToggle.Checked) { //Zoom Option
                //width 03698A height 036A26
                //16368 = 3FF0 = default zoom value
                //16356 = 3FE4 = lowest stable zoom value

                zoomvalue = quaZoomDropdown.SelectedIndex + 2;
                patchcontent += "03698A0002";
                patchcontent += Convert.ToString(16368 - zoomvalue, 16);
                patchcontent += "036A260002";
                patchcontent += Convert.ToString(16368 - zoomvalue, 16);

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Zoom out factor set to " + zoomvalue.ToString() + " [Default:1]" + Environment.NewLine);
            }

            if (quaLevelToggle.Checked) { //Level 1
                for (int s = spellstart; s < ((spelloffset * playerspells) + spellstart); s += spelloffset) {
                    patchcontent += Convert.ToString(s, 16);
                    patchcontent += "00020001";
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Spell unlock levels reduced to 1." + Environment.NewLine);
            }

            if (quaSoulToggle.Checked) { //Soul Search
                for (int z = spellstart + 57; z < ((spelloffset * playerspells) + spellstart); z += spelloffset) {
                    patchcontent += Convert.ToString(z, 16);
                    patchcontent += "000101";
                }

                tempaddr = Convert.ToInt32("D81C30", 16);
                for (int i = 0; i < 16; i++) {
                    patchcontent += Convert.ToString(tempaddr, 16); ; //tempaddr converted back to hex
                    patchcontent += "0010"; //replace 16 bytes
                    patchcontent += library.magnifier[i]; //add the 16 replacement bytes from the array
                    tempaddr += 128; //step to next case
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Soul search applied to all spells." + Environment.NewLine);
            }

            if (quaRestlessToggle.Checked) { //Restless NPCs
                for (int i = 0; i < library.npcmovement.Length; i++) {
                    patchcontent += Convert.ToString(library.npcmovement[i], 16);
                    patchcontent += "000102"; //Replace movement byte with 02 to cause wandering
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "NPCs are restless." + Environment.NewLine);
            }

            //Verbose spoiler log down at the bottom just to not hide the enabled options above.
            if(verboselog) { 
                if (rndSpellToggle.Checked && rndSpellDropdown.SelectedIndex == 0) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "SHUFFLED SPELLS:" + Environment.NewLine);
                    foreach (string line in spoilerspells) { File.AppendAllText(filePath + fileName + "_spoiler.txt", line + Environment.NewLine); }
                }

                if (rndChestToggle.Checked) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "SHUFFLED CHESTS:" + Environment.NewLine);
                    foreach (string line in spoilerchests) { File.AppendAllText(filePath + fileName + "_spoiler.txt", line + Environment.NewLine); }
                }

                if (rndDropsToggle.Checked) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "SHUFFLED DROPS:" + Environment.NewLine);
                    foreach (string line in spoilerdrops) { File.AppendAllText(filePath + fileName + "_spoiler.txt", line + Environment.NewLine); }
                }
            }

            //check if nothing is enabled, if not, don't make a patch
            if (!rndSpellToggle.Checked && !rndChestToggle.Checked && !rndTextPaletteToggle.Checked && !rndTextContentToggle.Checked && !rndDropsToggle.Checked && !quaLevelToggle.Checked && !quaSoulToggle.Checked && !quaInvalidityToggle.Checked && !quaZoomToggle.Checked && !quaAccuracyToggle.Checked && !quaRestlessToggle.Checked) { return; }
            //eventually i maybe will replace this with a sort of 'binary state' checker that'll be way less annoying and also have the side of effect of creating enterable shortcodes for option sets

            patchcontent += "DAC040393C"; //main menu logo address/length
            patchcontent += library.randologo;
            patchcontent += "DCE070393C"; //animation logo address/length
            patchcontent += library.randologo;

            patchbuild += headerHex;
            patchbuild += patchcontent;
            patchbuild += footerHex;
            patcharray = StringToByteArray(patchbuild);
            File.WriteAllBytes(filePath + fileName + ".ips", patcharray);
            filePath = filePath.Replace(@"/", @"\");   // explorer doesn't like front slashes
            System.Diagnostics.Process.Start("explorer.exe", Application.StartupPath + "\\Patches\\");
        }

        //BUILD GENERIC PATCH----------------------------------------------------------------

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

            if(addr.Length < 6) { //add leading zeroes to short addresses
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

        //BINARY FILE READER----------------------------------------------------------------

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
                    Console.WriteLine(strarray[i]);
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

        //VARIABLE OPERATIONS----------------------------------------------------------------

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
            byte[] bytes = new byte[NumberChars / 2];
            //Console.WriteLine(hex);
            for (int i = 0; i < NumberChars; i += 2) { bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16); }
            return bytes;
        }

        public static string ToHex(string input) { //Convert ascii string to hex string
            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
                sb.AppendFormat("{0:X2}", (int)c);
            return sb.ToString().Trim();
        }

        public static Color RGBAToColor(string hexvalue) { //Convert 4-char hex string to Color
            string binCol = Convert.ToString(Convert.ToInt32(hexvalue, 16), 2); //convert the hex string to an int, and then to binary string
            //Console.WriteLine(binCol);
            if (binCol.Length < 16) { for (int i = 0; i < 16 - binCol.Length; i++) { binCol = "0" + binCol; } } //ensure it's 16 characters, conversion will cut it short

            int intR = Convert.ToInt32(binCol.Substring(0, 5), 2); //convert first five bits of binary to int 0-31
            int intG = Convert.ToInt32(binCol.Substring(5, 5), 2); //convert second five bits of binary to int 0-31
            int intB = Convert.ToInt32(binCol.Substring(10, 5), 2); //convert third five bits of binary to int 0-31
            int intA = Convert.ToInt32(binCol.Substring(15), 2); //grab the alpha as well
            //Console.WriteLine("intR={0}, intG={1}, intB={2}", intR, intG, intB);

            double dubR = (intR / 31d) * 255; //convert the 0-31 values to 0-255 for FromArgb
            double dubG = (intG / 31d) * 255;
            double dubB = (intB / 31d) * 255;

            intR = (int)Math.Round(dubR); //it was not doing the conversion properly so i've separated it out
            intG = (int)Math.Round(dubG);
            intB = (int)Math.Round(dubB);

            intA = intA * 255; //either 255 or 0
            //Console.WriteLine("intR={0}, intG={1}, intB={2}", intR, intG, intB);

            Color ret = Color.FromArgb(intA, intR, intG, intB); //return the color value so it can be used for TransformHSV
            return ret;
        }

        public static Color TransformHSV(Color col, float h, float s, float v) { //hsv shifting. we should probably stick mainly to just hue, no idea how the others will look.
            //h is a hue shift in degrees, 0-359.
            //s is a saturation multiplier, scalar. set to 1 to leave the same.
            //v is a value multiplier, scalar. set to 1 to leave the same.

            double vsu = v * s * Math.Cos(h * Math.PI / 180);
            double vsw = v * s * Math.Sin(h * Math.PI / 180);

            double colR = (.299 * v + .701 * vsu + .168 * vsw) * col.R //hsv->yiq matrix
                        + (.587 * v - .587 * vsu + .330 * vsw) * col.G
                        + (.114 * v - .114 * vsu - .497 * vsw) * col.B;
            double colG = (.299 * v - .299 * vsu - .328 * vsw) * col.R
                        + (.587 * v + .413 * vsu + .035 * vsw) * col.G
                        + (.114 * v - .114 * vsu + .292 * vsw) * col.B;
            double colB = (.299 * v - .300 * vsu + 1.25 * vsw) * col.R
                        + (.587 * v - .588 * vsu - 1.05 * vsw) * col.G
                        + (.114 * v + .886 * vsu - .203 * vsw) * col.B;

            int intR = (int)Math.Round(colR); //turning them back into ints, to convert back to color
            int intG = (int)Math.Round(colG);
            int intB = (int)Math.Round(colB);

            if (intR < 0) { intR += 255; }
            if (intG < 0) { intG += 255; }
            if (intB < 0) { intB += 255; }
            if (intR > 255) { intR -= 255; }
            if (intG > 255) { intG -= 255; }
            if (intB > 255) { intB -= 255; }

            Color ret = Color.FromArgb(col.A, intR, intG, intB); //convert back, retain alpha
            return ret;
        }

        public static string ColorToHex(Color col) { //Convert Color to 4-char hex string
            Console.WriteLine(col.ToString());
            double dubR = (col.R / 255d) * 31; //convert the 0-255 values to 0-31 for binary conversion
            double dubG = (col.G / 255d) * 31;
            double dubB = (col.B / 255d) * 31;

            int intR = (int)Math.Round(dubR); //it was not doing the conversion properly so i've separated it out
            int intG = (int)Math.Round(dubG);
            int intB = (int)Math.Round(dubB);
            //Console.WriteLine("intR={0}, intG={1}, intB={2}", intR, intG, intB);

            int intA = 1; //Alpha is either 1 or 0
            if (col.A == 0) { intA = 0; }

            Console.WriteLine("intR={0}, intG={1}, intB={2}", intR, intG, intB);

            string binR = Convert.ToString(intR, 2); //convert them to separate binary strings
            if (binR.Length < 5) { for (int i = 0; i < 5 - binR.Length; i++) { binR = "0" + binR; } } //ensure it's 5 characters, conversion will cut it short
            string binG = Convert.ToString(intG, 2);
            if (binG.Length < 5) { for (int i = 0; i < 5 - binG.Length; i++) { binG = "0" + binG; } }
            string binB = Convert.ToString(intB, 2);
            if (binB.Length < 5) { for (int i = 0; i < 5 - binB.Length; i++) { binB = "0" + binB; } }
            string binA = Convert.ToString(intA, 2);
            Console.WriteLine(binR + binG + binB + binA);

            int binCol = Convert.ToInt32(binR + binG + binB + binA, 2); //combine into one int
            string ret = Convert.ToString(binCol, 16); //convert that int to hex
            return ret;
        }

        //UI INTERACTIONS----------------------------------------------------------------

        private void rndSpellToggle_CheckedChanged(object sender, EventArgs e) {
            if(rndSpellToggle.Checked) {
                rndSpellDropdown.Visible = true;
                rndSpellNames.Visible = true;
            } else {
                rndSpellDropdown.Visible = false;
                rndSpellNames.Visible = false;
            }
        }

        private void rndChestToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndChestToggle.Checked) {
                rndChestDropdown.Visible = true;
                rndWeightedChest.Visible = true;
            } else {
                rndChestDropdown.Visible = false;
                rndWeightedChest.Visible = false;
            }
        }

        private void rndTextPaletteToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndTextPaletteToggle.Checked) { rndTextPaletteDropdown.Visible = true; } else { rndTextPaletteDropdown.Visible = false; }
        }

        private void rndTextContentToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndTextContentToggle.Checked) { rndTextContentDropdown.Visible = true; } else { rndTextContentDropdown.Visible = false; }
        }

        private void quaZoomToggle_CheckedChanged(object sender, EventArgs e) {
            if (quaZoomToggle.Checked) {
                quaZoomDropdown.Visible = true;
                crcWarningLabel.Visible = true;
            } else {
                quaZoomDropdown.Visible = false;
                crcWarningLabel.Visible = false;
            }
        }

        private void quaAccuracyToggle_CheckedChanged(object sender, EventArgs e) {
            if (quaAccuracyToggle.Checked) { quaAccuracyDropdown.Visible = true; } else { quaAccuracyDropdown.Visible = false; }
        }

        private void seedTextBox_TextChanged(object sender, EventArgs e) {
            var textboxSender = (TextBox)sender; //restricts to numeric only
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, "[^0-9]", "");
            textboxSender.SelectionStart = cursorPosition;
            if (seedTextBox.Text != "" && seedTextBox.Text != null && loadfinished) {
                rngseed = Convert.ToInt32(seedTextBox.Text);
                Shuffling(true);
            }
        }

        private void filenameTextBox_TextChanged(object sender, EventArgs e) {
            var textboxSender = (TextBox)sender; //restricts to alphanumeric/underscore only
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, "[^0-9a-zA-Z_]", "");
            textboxSender.SelectionStart = cursorPosition;
        }

        private void genButton_Click(object sender, EventArgs e) {
            BuildPatch();
        }

        private void rndDropsToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndDropsToggle.Checked) { rndDropsDropdown.Visible = true; } else { rndDropsDropdown.Visible = false; }
        }

        private void MerrowForm_Load(object sender, EventArgs e) {
            
        }

        private void verboseCheckBox_CheckedChanged(object sender, EventArgs e) {
            verboselog = verboseCheckBox.Checked;
        }

        private void advFilenameText_TextChanged(object sender, EventArgs e) {
            var textboxSender = (TextBox)sender; //restricts to alphanumeric/underscore only
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, "[^0-9a-zA-Z_]", "");
            textboxSender.SelectionStart = cursorPosition;
        }

        private void advAddressText_TextChanged(object sender, EventArgs e) {
            var textboxSender = (TextBox)sender; //restricts to hexadecimal only
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, "[^0-9a-fA-F]", "");
            textboxSender.SelectionStart = cursorPosition;
        }

        private void advContentText_TextChanged(object sender, EventArgs e) {
            var textboxSender = (TextBox)sender; //restricts to hexadecimal only
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, "[^0-9a-fA-F]", "");
            textboxSender.SelectionStart = cursorPosition;
        }

        private void advGenerateButton_Click(object sender, EventArgs e) {
            BuildCustomPatch(advAddressText.Text,advContentText.Text);
        }

        private void binFileSelectButton_Click(object sender, EventArgs e) {
            if (binOpenFileDialog.ShowDialog() == DialogResult.OK) {
                try {
                    binFileBytes = File.ReadAllBytes(binOpenFileDialog.FileName);
                    binFileLength = binFileBytes.Length;
                    binErrorLabel.Text = Path.GetFileName(binOpenFileDialog.FileName) + " loaded (" + binFileLength + " bytes).";
                    binErrorLabel.Visible = true;
                    binFileLoaded = true;
                }
                catch (SecurityException ex) {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }
        }

        private void binGenerateButton_Click(object sender, EventArgs e) {
            if (binFileLoaded && binContentTextBox.Text != "" && binContentTextBox.Text != null) {
                BinRead();
            }
        }

        private void binContentTextBox_TextChanged(object sender, EventArgs e) {
            var textboxSender = (TextBox)sender; //restricts to hexadecimal and comma only
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, "[^0-9a-fA-F,]", "");
            textboxSender.SelectionStart = cursorPosition;
        }

        private void binFilenameTextBox_TextChanged(object sender, EventArgs e) {
            var textboxSender = (TextBox)sender; //restricts to alphanumeric/underscore only
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, "[^0-9a-zA-Z_]", "");
            textboxSender.SelectionStart = cursorPosition;
        }

        private void tabsControl_SelectedIndexChanged(object sender, EventArgs e) {
            crcWarningLabel.Visible = false;
            binErrorLabel.Visible = false;
            binFileLoaded = false;
            binFileBytes = null;
        }

        private void terms__LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/hangedmandesign/merrow");
        }
    }
}
