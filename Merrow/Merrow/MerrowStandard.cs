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
    public partial class MerrowStandard : Form {
        //data structures
        DataStore library;
        Random SysRand = new Random();
        private OpenFileDialog binOpenFileDialog;
        private OpenFileDialog crcOpenFileDialog;

        //crc dll import
        [DllImport("crc64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int fix_crc(string crcPath);
        string fullPath;
        bool crcFileSelected = false;

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
        Color texPal1 = Color.Black;
        Color texPal2 = Color.Black;
        Color texPal3 = Color.Black;

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
        string[] spoilerscales = new string[75];
        int[] newmonsterstats = new int[450];
        float difficultyscale = 10;
        float extremity = 0;
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
            crcOpenFileDialog = new OpenFileDialog() {
                FileName = "Select a Z64...",
                Filter = "Z64 files (*.z64)|*.z64",
                Title = "Select Z64 file"
            };
            shuffles = new int[playerspells];
            spoilerspells = new string[playerspells];
            library = new DataStore();
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

            //initiate monster stats
            for (int i = 0; i < 450; i++) { newmonsterstats[i] = library.monsterstatvanilla[i]; }

            //now that dropdowns have content, fix them
            PrepareDropdowns();

            //initial shuffle
            rngseed = SysRand.Next(100000000, 1000000000); //a 9-digit number
            loadfinished = true;
            Shuffling(true);
        }

        //list of initial UI cleanup/prep steps
        private void PrepareDropdowns() { 
            rndSpellDropdown.SelectedIndex = 0;
            rndChestDropdown.SelectedIndex = 0;
            rndTextPaletteDropdown.SelectedIndex = 0;
            rndTextContentDropdown.SelectedIndex = 0;
            rndDropsDropdown.SelectedIndex = 0;
            rndSpellNamesDropdown.SelectedIndex = 0;
            rndExtremityDropdown.SelectedIndex = 0;
            quaAccuracyDropdown.SelectedIndex = 0;
            quaZoomDropdown.SelectedIndex = 0;
            quaScalingDropdown.SelectedIndex = 5;

            rndCRCWarningLabel.Visible = false;

            binAddrHEX.Checked = true;
            binLengthHEX.Checked = true;
        }

        //UNIFIED SHUFFLING FUNCTION----------------------------------------------------------------

        public void Shuffling(bool crashpro) {
            int k = 0;

            //REINITIATE RANDOM WITH NEW SEED
            SysRand = new System.Random(rngseed);
            expSeedTextBox.Text = rngseed.ToString();

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
                //Console.WriteLine("Crash protection complete.");
            }

            //SPELL NAME SHUFFLING (based on shuffles array and existing data)
            for (int i = 0; i < playerspells; i++) {
                bool fiftyfifty = SysRand.NextDouble() > 0.5; ; 
                if (rndSpellNamesDropdown.SelectedIndex == 1) { fiftyfifty = true; } //"Linear" option
                if (fiftyfifty) { hintnames[i] = library.shuffleNames[i*5] + " " + library.shuffleNames[(shuffles[i] * 5) + 1]; }
                else { hintnames[i] = library.shuffleNames[shuffles[i] * 5] + " " + library.shuffleNames[(i * 5) + 1]; }
            }

            //CHEST SHUFFLING (based on Chest Shuffle dropdown value)
            if (rndChestDropdown.SelectedIndex == 1) { //RANDOM: STANDARD CHEST ITEMS 0-13
                int c = chests.Length;
                while (c > 1) {
                    c--;
                    if(rndWeightedChestToggle.Checked) {
                        if (c > 14) { k = SysRand.Next(14); } else { k = c - 1; }
                    }
                    if (!rndWeightedChestToggle.Checked) {
                        k = SysRand.Next(14);
                    }
                    chests[c] = k;
                }
            }

            if (rndChestDropdown.SelectedIndex == 2) { //RANDOM: STANDARD AND WINGS 0-19
                int c = chests.Length;
                while (c > 1) {
                    c--;
                    if (rndWeightedChestToggle.Checked) {
                        if (c > 20) { k = SysRand.Next(20); } else { k = c - 1; }
                    }
                    if (!rndWeightedChestToggle.Checked) {
                        k = SysRand.Next(20);
                    }
                    chests[c] = k;
                }
            }

            if (rndChestDropdown.SelectedIndex == 3) { //RANDOM: STANDARD AND GEMS 0-13, 20-23
                int c = chests.Length;
                while (c > 1) {
                    c--;
                    if (rndWeightedChestToggle.Checked) {
                        if (c > 18) { k = SysRand.Next(18); } else { k = c - 1; }
                    }
                    if (!rndWeightedChestToggle.Checked) {
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
                    if (rndWeightedChestToggle.Checked) {
                        if (c > 24) { k = SysRand.Next(24); } else { k = c - 1; }
                    }
                    if (!rndWeightedChestToggle.Checked) {
                        k = SysRand.Next(24);
                    }
                    chests[c] = k;
                }
            }

            if (rndChestDropdown.SelectedIndex == 5) { //RANDOM: WINGS 14-19
                int c = chests.Length;
                while (c > 1) {
                    c--;
                    if (rndWeightedChestToggle.Checked) {
                        if (c > 6) { k = SysRand.Next(6) + 14; } else { k = c - 1; }
                    }
                    if (!rndWeightedChestToggle.Checked) {
                        k = SysRand.Next(6) + 14; //produces 0-5, adds 14
                    }
                    chests[c] = k;
                }
            }

            if (rndChestDropdown.SelectedIndex == 6) { //RANDOM: GEMS 20-23
                int c = chests.Length;
                while (c > 1) {
                    c--;
                    if (rndWeightedChestToggle.Checked) {
                        if (c > 4) { k = SysRand.Next(4) + 20; } else { k = c - 1; }
                    }
                    if (!rndWeightedChestToggle.Checked) {
                        k = SysRand.Next(4) + 20; //produces 0-3, adds 20
                    }
                    chests[c] = k;
                }
            }

            if (rndChestDropdown.SelectedIndex == 7) { //RANDOM: WINGS & GEMS 14-23
                int c = chests.Length;
                while (c > 1) {
                    c--;
                    if (rndWeightedChestToggle.Checked) {
                        if (c > 10) { k = SysRand.Next(10) + 14; } else { k = c - 1; }
                    }
                    if (!rndWeightedChestToggle.Checked) {
                        k = SysRand.Next(10) + 14; //produces 0-9, adds 14
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

            if (rndDropsDropdown.SelectedIndex == 5) { //RANDOM: WINGS 14-19
                int c = drops.Length;
                while (c > 1) {
                    c--;
                    k = SysRand.Next(6) + 14; //produces 0-5, adds 14
                    drops[c] = k;
                }
            }

            if (rndDropsDropdown.SelectedIndex == 6) { //RANDOM: GEMS 20-23
                int c = drops.Length;
                while (c > 1) {
                    c--;
                    k = SysRand.Next(4) + 20; //produces 0-3, adds 20
                    drops[c] = k;
                }
            }

            if (rndDropsDropdown.SelectedIndex == 7) { //RANDOM: WINGS & GEMS 14-23
                int c = drops.Length;
                while (c > 1) {
                    c--;
                    k = SysRand.Next(10) + 14; //produces 0-9, adds 14
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
            texPal1 = RGBAToColor(library.baseRedTextPalette[0]); //convert base colours from hex
            texPal2 = RGBAToColor(library.baseRedTextPalette[1]);
            texPal3 = RGBAToColor(library.baseRedTextPalette[2]);

            float hueOffset = (float)SysRand.NextDouble() * 360; //pick random hue offset
            float brightScale = (float)SysRand.NextDouble() / 3;

            float temph = texPal1.GetHue() + hueOffset; //alter palette colours according to HSV
            float temps = texPal1.GetSaturation();
            float tempb = getBrightness(texPal1);
            texPal1 = ColorFromHSL(temph, temps, tempb);
            temph = texPal2.GetHue() + hueOffset;
            temps = texPal2.GetSaturation();
            tempb = getBrightness(texPal2) + brightScale / 2;
            texPal2 = ColorFromHSL(temph, temps, tempb);
            temph = texPal3.GetHue() + hueOffset;
            temps = texPal3.GetSaturation();
            tempb = getBrightness(texPal3) + brightScale;
            texPal3 = ColorFromHSL(temph, temps, tempb);

            textPaletteHex = ColorToHex(texPal1) + ColorToHex(texPal2) + ColorToHex(texPal3);

            rndColourPanel2.BackColor = texPal1;
            rndColourPanel3.BackColor = texPal2;
            rndColourPanel4.BackColor = texPal3;

            //MONSTER STAT RANDOMIZATION

            //initiate monster stats again, in case this is happening for the nth time
            for (int i = 0; i < 450; i++) { newmonsterstats[i] = library.monsterstatvanilla[i]; }

            //if monster stat randomization is active
            if (rndMonsterStatsToggle.Checked) {
                for (int i = 16; i > -1; i--) { //17 areas
                    int locals = library.mon_enemycount[i];
                    int locale = library.mon_locationsindex[i];
                    for (int j = 0; j < locals; j++) { //steps through each area's monster set one by one
                        int currentmonster = library.mon_locations[locale + j];
                        for (int m = 0; m < 5; m++) { //sets each of their 5 new stats, based on area average, variance, and difficulty modifier
                            int currentstat = (int)Math.Round(library.avg_monster[(i * 7) + m]);
                            double highend = library.avg_monster[(i * 7) + 5] * (1 + extremity);
                            double lowend = library.avg_monster[(i * 7) + 6] * (1 - extremity);
                            double variance = SysRand.NextDouble() * (highend - lowend) + lowend;
                            double modifiedstat = (currentstat * (variance / 10) * (difficultyscale / 10));
                            newmonsterstats[(currentmonster * 6) + m] = (int)Math.Round(modifiedstat);
                            if (newmonsterstats[(currentmonster * 6) + m] == 0) { newmonsterstats[(currentmonster * 6) + m] = 1; }
                        }
                    }
                }
            }

            //scale bosses only if randomization is checked
            if (rndMonsterStatsToggle.Checked && quaMonsterScaleToggle.Checked) {
                for (int i = 67; i < 75; i++) {
                    for (int j = 0; j < 5; j++) {
                        newmonsterstats[(i * 6) + j] = (int)Math.Round(newmonsterstats[(i * 6) + j] * (difficultyscale / 10));
                    }
                }
            }

            //scale all if monster stat randomization isn't active but scaling is
            if (!rndMonsterStatsToggle.Checked && quaMonsterScaleToggle.Checked) {
                for (int i = 0; i < 75; i++) {
                    for (int j = 0; j < 5; j++) {
                        newmonsterstats[(i * 6) + j] = (int)Math.Round(newmonsterstats[(i * 6) + j] * (difficultyscale / 10));
                    }
                }
            }
        }
        //BUILD QUEST PATCH----------------------------------------------------------------

        public void BuildPatch() {
            //check if nothing is enabled, if not, don't make a patch
            if (!rndSpellToggle.Checked && 
                !rndChestToggle.Checked && 
                !rndTextPaletteToggle.Checked && 
                !rndTextContentToggle.Checked && 
                !rndDropsToggle.Checked && 
                !rndMonsterStatsToggle.Checked &&
                !quaLevelToggle.Checked && 
                !quaSoulToggle.Checked && 
                !quaInvalidityToggle.Checked && 
                !quaZoomToggle.Checked && 
                !quaAccuracyToggle.Checked && 
                !quaRestlessToggle.Checked &&
                !quaMaxMessageToggle.Checked &&
                !quaMonsterScaleToggle.Checked
               ) { return; }
            //eventually i maybe will replace this with a sort of 'binary state' checker that'll be way less annoying and also have the side of effect of creating enterable shortcodes for option sets

            //update filename one more time here to avoid errors
            if (expFilenameTextBox.Text != "" && expFilenameTextBox.Text != null) {
                fileName = string.Join("", expFilenameTextBox.Text.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries)); //strip all whitespace to avoid errors
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

                if (rndSpellNamesToggle.Checked && rndSpellDropdown.SelectedIndex == 0) {
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

                patchcontent += "667260000C000000060000000100000001"; //Fix for skelebat group in Blue Cave that can cause crashes due to lag
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
                    patchcontent += "F83E";
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
                if (rndWeightedChestToggle.Checked) {
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

            //MONSTER SCALING FEATURES

            if (rndMonsterStatsToggle.Checked || quaMonsterScaleToggle.Checked) {
                for (int i = 0; i < newmonsterstats.Length; i++) {
                    patchcontent += library.monsterstatlocations[i].ToString("X6");
                    patchcontent += "0002";
                    patchcontent += newmonsterstats[i].ToString("X4");
                    if (i % 6 == 0) { //if the current value is HP2, write it again at the HP1 location, offset 04 (HP2 + 2).
                        patchcontent += (library.monsterstatlocations[i] + 2).ToString("X6");
                        patchcontent += "0002";
                        patchcontent += newmonsterstats[i].ToString("X4");
                    }
                }

                for (int i = 0; i < 75; i++) {
                    spoilerscales[i] = library.monsternames[i] + ": ";
                    spoilerscales[i] += newmonsterstats[i * 6].ToString() + " ";
                    spoilerscales[i] += newmonsterstats[(i * 6) + 1].ToString() + " ";
                    spoilerscales[i] += newmonsterstats[(i * 6) + 2].ToString() + " ";
                    spoilerscales[i] += newmonsterstats[(i * 6) + 3].ToString() + " ";
                    spoilerscales[i] += newmonsterstats[(i * 6) + 4].ToString() + " ";
                    spoilerscales[i] += newmonsterstats[(i * 6) + 5].ToString();
                }

                if (rndMonsterStatsToggle.Checked && extremity == 0) { File.AppendAllText(filePath + fileName + "_spoiler.txt", "Monster stats randomized within regions." + Environment.NewLine); }
                if (rndMonsterStatsToggle.Checked && extremity != 0) { File.AppendAllText(filePath + fileName + "_spoiler.txt", "Monster stats randomized, with Risk level " + (extremity + 1).ToString() + "." + Environment.NewLine); }
                if (quaMonsterScaleToggle.Checked) { File.AppendAllText(filePath + fileName + "_spoiler.txt", "Monster stats scaled by " + (difficultyscale/10).ToString() + "x." + Environment.NewLine); }
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

            if (quaMaxMessageToggle.Checked) { //Max Message Speed
                patchcontent += "060600000100";

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Message speed set to maximum." + Environment.NewLine);
            }

            //FINAL ASSEMBLY/OUTPUT

            //Verbose spoiler log down at the bottom just to not hide the enabled options above.
            if (verboselog) { 
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

                if (rndMonsterStatsToggle.Checked || quaMonsterScaleToggle.Checked) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "ALTERED MONSTER STATS (HP, ATK, DEF, AGI, EXP, ELEMENT):" + Environment.NewLine);
                    foreach (string line in spoilerscales) { File.AppendAllText(filePath + fileName + "_spoiler.txt", line + Environment.NewLine); }
                }
            }

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

        //CRC REPAIR TOOL--------------------------------------------------------------------

        public int RepairCRC() {
            int check = -1;
            if (crcFileSelected) { check = fix_crc(fullPath); }
            Console.WriteLine(check);
            return check;
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
            if (binCol.Length < 16) { for (int i = 0; i < 16 - binCol.Length; i++) { binCol = "0" + binCol; } } //ensure it's 16 characters, conversion will cut it short

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

        public static string ColorToHex(Color col) { //Convert Color to 4-char hex string
            Console.WriteLine(col.ToString());
            double dubR = (col.R / 255d) * 31; //convert the 0-255 values to 0-31 for binary conversion
            double dubG = (col.G / 255d) * 31;
            double dubB = (col.B / 255d) * 31;

            int intR = (int)Math.Round(dubR); //it was not doing the conversion properly so i've separated it out
            int intG = (int)Math.Round(dubG);
            int intB = (int)Math.Round(dubB);

            int intA = 1; //Alpha is either 1 or 0
            if (col.A == 0) { intA = 0; }

            string binR = Convert.ToString(intR, 2); //convert them to separate binary strings
            if (binR.Length < 5) { for (int i = 0; i < 5 - binR.Length; i++) { binR = "0" + binR; } } //ensure it's 5 characters, conversion will cut it short
            string binG = Convert.ToString(intG, 2);
            if (binG.Length < 5) { for (int i = 0; i < 5 - binG.Length; i++) { binG = "0" + binG; } }
            string binB = Convert.ToString(intB, 2);
            if (binB.Length < 5) { for (int i = 0; i < 5 - binB.Length; i++) { binB = "0" + binB; } }
            string binA = Convert.ToString(intA, 2);

            int binCol = Convert.ToInt32(binR + binG + binB + binA, 2); //combine into one int
            string ret = binCol.ToString(("X4")); //convert that int to hex
            Console.WriteLine(ret);
            return ret;
        }

        //UI INTERACTIONS----------------------------------------------------------------

        private void tabsControl_SelectedIndexChanged(object sender, EventArgs e) {
            rndCRCWarningLabel.Visible = false;
            binErrorLabel.Visible = false;
            binFileLoaded = false;
            binFileBytes = null;
            crcErrorLabel.Visible = false;
            crcFileSelected = false;
        }

        private void terms__LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/hangedmandesign/merrow");
        }

        private void rndSpellToggle_CheckedChanged(object sender, EventArgs e) {
            if(rndSpellToggle.Checked) {
                rndSpellDropdown.Visible = true;
                rndSpellNamesToggle.Visible = true;
                rndSpellNamesDropdown.Visible = true;
            } else {
                rndSpellDropdown.Visible = false;
                rndSpellNamesToggle.Visible = false;
                rndSpellNamesDropdown.Visible = false;
            }
        }

        private void rndChestToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndChestToggle.Checked) {
                rndChestDropdown.Visible = true;
                rndWeightedChestToggle.Visible = true;
            } else {
                rndChestDropdown.Visible = false;
                rndWeightedChestToggle.Visible = false;
            }
        }

        private void rndTextPaletteToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndTextPaletteToggle.Checked) {
                rndTextPaletteDropdown.Visible = true;
            } else {
                rndTextPaletteDropdown.Visible = false;
            }
        }

        private void rndTextContentToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndTextContentToggle.Checked) {
                rndTextContentDropdown.Visible = true;
            } else {
                rndTextContentDropdown.Visible = false;
            }
        }

        private void rndColorViewCheckbox_CheckedChanged(object sender, EventArgs e) {
            if (rndColorViewToggle.Checked) {
                rndColorViewToggle.Text = "View random colours:";
                Shuffling(true); //if you don't do this, the colour doesn't update the first time, despite my best efforts
                rndColorViewPanel.Visible = true;
            }
            else {
                rndColorViewToggle.Text = "View random colours";
                rndColorViewPanel.Visible = false;
            }
        }

        private void rndTextPaletteDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            if (rndTextPaletteDropdown.SelectedIndex == 5) {
                rndColorViewToggle.Visible = true;
            }
            else {
                rndColorViewToggle.Visible = false;
                rndColorViewToggle.Checked = false; //make it false again so as not to spoil the next value, and so the panel's invisible
            }
        }

        private void rndDropsToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndDropsToggle.Checked) { rndDropsDropdown.Visible = true; } else { rndDropsDropdown.Visible = false; }
        }

        private void quaMaxMessageToggle_CheckedChanged(object sender, EventArgs e) {
            if (quaMaxMessageToggle.Checked) {
                rndCRCWarningLabel.Visible = true;
            }
            else {
                rndCRCWarningLabel.Visible = false;
            }
        }

        private void quaZoomToggle_CheckedChanged(object sender, EventArgs e) {
            if (quaZoomToggle.Checked) {
                quaZoomDropdown.Visible = true;
                rndCRCWarningLabel.Visible = true;
            } else {
                quaZoomDropdown.Visible = false;
                rndCRCWarningLabel.Visible = false;
            }
        }

        private void quaAccuracyToggle_CheckedChanged(object sender, EventArgs e) {
            if (quaAccuracyToggle.Checked) { quaAccuracyDropdown.Visible = true; } else { quaAccuracyDropdown.Visible = false; }
        }

        private void expSeedTextBox_TextChanged(object sender, EventArgs e) {
            var textboxSender = (TextBox)sender; //restricts to numeric only
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, "[^0-9]", "");
            textboxSender.SelectionStart = cursorPosition;
            if (expSeedTextBox.Text != "" && expSeedTextBox.Text != null && loadfinished) {
                rngseed = Convert.ToInt32(expSeedTextBox.Text);
                Shuffling(true);
            }
        }

        private void expFilenameTextBox_TextChanged(object sender, EventArgs e) {
            var textboxSender = (TextBox)sender; //restricts to alphanumeric/underscore only
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, "[^0-9a-zA-Z_]", "");
            textboxSender.SelectionStart = cursorPosition;
        }

        private void expGenerateButton_Click(object sender, EventArgs e) {
            BuildPatch();
        }

        private void expVerboseCheckBox_CheckedChanged(object sender, EventArgs e) {
            verboselog = expVerboseCheckBox.Checked;
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

        private void crcFileButton_Click(object sender, EventArgs e) {
            if (crcOpenFileDialog.ShowDialog() == DialogResult.OK) {
                try {
                    fullPath = crcOpenFileDialog.FileName;
                    crcErrorLabel.Text = "File loaded: " + crcOpenFileDialog.FileName + ".";
                    crcErrorLabel.Visible = true;
                    crcFileSelected = true;
                }
                catch (SecurityException ex) {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                    crcFileSelected = false;
                }
            }
        }

        private void crcRepairButton_Click(object sender, EventArgs e) {
            int message = RepairCRC();
            Console.WriteLine(message);
            if (message == 0) {
                crcErrorLabel.Visible = true;
                crcErrorLabel.Text = "Checksum repaired.";
            }

            if (message == 1) {
                crcErrorLabel.Visible = true;
                crcErrorLabel.Text = "ERROR: There was an error. More granular error codes coming soon.";
            }

            if (message == -1) {
                crcErrorLabel.Visible = true;
                crcErrorLabel.Text = "ERROR: File not selected or available.";
            }
        }

        private void quaMonsterScaleToggle_CheckedChanged(object sender, EventArgs e) {
            if (quaMonsterScaleToggle.Checked) {
                quaScalingDropdown.Visible = true;
            }
            else {
                quaScalingDropdown.Visible = false;
                quaScalingDropdown.SelectedIndex = 5;
                difficultyscale = 10;
            }
        }

        private void quaScalingDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            //index 5 is 1.0, which is difficulty scale 10
            difficultyscale = quaScalingDropdown.SelectedIndex + 5;
        }

        private void rndMonsterStatsToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndMonsterStatsToggle.Checked) {
                rndExtremityDropdown.Visible = true;
                rndExtremityLabel.Visible = true;
            }
            else {
                rndExtremityDropdown.Visible = false;
                rndExtremityLabel.Visible = false;
                rndExtremityDropdown.SelectedIndex = 0;
                extremity = 0;
            }
        }

        private void rndExtremityDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            //index 0 is 1.0, which is extremity scale 0
            extremity = rndExtremityDropdown.SelectedIndex * 0.1f;
        }
    }
}
