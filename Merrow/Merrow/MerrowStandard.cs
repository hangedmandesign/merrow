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
        //MAIN MERROW RANDOMIZER AND WINFORMS FUNCTIONALITY

        //data structures
        DataStore library;
        Random SysRand = new Random();
        private OpenFileDialog binOpenFileDialog;
        private OpenFileDialog crcOpenFileDialog;
        private OpenFileDialog rndOpenFileDialog;

        //crc dll import
        [DllImport("crc64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int fix_crc(string crcPath);

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
        bool seedName = true;
        bool dateName = false;
        int binFileLength = 0;
        bool binFileLoaded = false;
        string rndFileName = "";
        string fullPath;
        bool crcFileSelected = false;
        bool rndFileSelected = false;
        bool writingPatch = false;
        string textPaletteHex = "00008888FFFF"; //busted default palette to make errors obvious
        Color texPal1 = Color.Black;
        Color texPal2 = Color.Black;
        Color texPal3 = Color.Black;
        bool lockItemUpdates = false;
        string rndErrorString = "WARNING: Current patch options will cause checksum errors." + Environment.NewLine + "Use the CRC Repair Tool to fix your patched rom.";
        double riskvalue = 1.0;

        //collection arrays and lists
        byte[] patcharray;
        int[] shuffles = new int[60];
        List<int> reorg = new List<int>();
        int[] spellitemID = { 58, 52, 38, 57, 17, 22 };
        int[] newitemspells = new int[6];
        int[] itemspellfix = new int[6];
        int[] chests = new int[88];
        int[] drops = new int[67];
        int[] gifts = new int[10];
        int[] wings = new int[6];
        int[] texts = new int[208];
        int[] inntexts = new int[17];
        string[] hintnames = new string[60];
        string[] spoilerspells = new string[60];
        string[] spoilerchests = new string[88];
        string[] spoilerdrops = new string[67];
        string[] spoilerscales = new string[75];
        string[] spoilergifts = new string[10];
        string[] spoilerwings = new string[6];
        int[] newmonsterstats = new int[450];
        float difficultyscale = 1.0f;
        float extremity = 0;
        string[] voweled = new string[75];
        byte[] binFileBytes;
        byte[] rndFileBytes;
        List<string> patchstrings = new List<string>();

        //INITIALIZATION----------------------------------------------------------------

        public MerrowStandard() {
            //required Winforms function, do not edit or remove
            InitializeComponent(); 

            //initiate file-opening dialogs
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
            rndOpenFileDialog = new OpenFileDialog() {
                FileName = "Select a Z64...",
                Filter = "Z64 files (*.z64)|*.z64",
                Title = "Select Z64 file"
            };

            //initiate fundamental variables
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

            //initiate UI
            PrepareDropdowns();

            //initial randomization
            rngseed = SysRand.Next(100000000, 1000000000); //default seed set to a random 9-digit number
            expSeedTextBox.Text = rngseed.ToString();
            loadfinished = true; //loadfinished being false prevents some UI elements from taking incorrect action during the initial setup
            Shuffling(true);
        }

        //INITIAL UI CLEANUP/PREP------------------------------------------------------------------
        //this is separate just because it's a big chunk of similar nonsense

        private void PrepareDropdowns() { 
            rndSpellDropdown.SelectedIndex = 0;
            rndChestDropdown.SelectedIndex = 0;
            rndTextPaletteDropdown.SelectedIndex = 0;
            rndTextContentDropdown.SelectedIndex = 0;
            rndDropsDropdown.SelectedIndex = 0;
            rndSpellNamesDropdown.SelectedIndex = 0;
            rndExtremityDropdown.SelectedIndex = 0;
            rndGiftersDropdown.SelectedIndex = 0;
            rndWingsmithsDropdown.SelectedIndex = 0;
            rndWeightedChestDropdown.SelectedIndex = 0;
            rndWeightedDropsDropdown.SelectedIndex = 0;
            rndAccuracyDropdown.SelectedIndex = 0;
            rndZoomDropdown.SelectedIndex = 0;
            rndScalingDropdown.SelectedIndex = 5;
            rndWingUnlockDropdown.SelectedIndex = 0;
            rndPresetDropdown.SelectedIndex = 0;

            expFakeZ64Button.Size = new System.Drawing.Size(110, 78);
        }

        //UNIFIED SHUFFLING/RANDOMIZING FUNCTION----------------------------------------------------------------

        public void Shuffling(bool crashpro) {
            int k = 0;

            //REINITIATE RANDOM WITH SEED
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

            for (int i = 0; i < playerspells; i++) {
                shuffles[i] = -1;
            }

            //crash protection disabled - dumps reorg directly into shuffles array
            //if (!crashpro) {
            //    for (int i = 0; i < playerspells; i++) {
            //        shuffles[i] = reorg[i];
            //    }
            //}

            //crash protection enabled
            if (crashpro) {
                bool step = false;
                //int c;
                //int r;
                //int s;

                while (reorg.Count > 0) {
                    for (int i = 0; i < playerspells; i++) { //spell number
                        step = false;

                        for (int j = 0; j < reorg.Count; j++) { //across crashlock array
                            if (!step && library.crashlock[(i * playerspells) + reorg[j]] == -1 && shuffles[i] == -1) {
                                shuffles[i] = reorg[j];
                                reorg.RemoveAt(j);
                                step = true;
                            }

                            if (step) { break; } //escape to outermost loop
                        }
                    }

                    //if the assignment wasn't correct, then just reset and do it again.
                    //this should prevent any errors from ever occurring, since they're pretty infrequent now.
                    //this is kind of a brute force method of solving it, but fancy alternatives have so many issues.
                    if (reorg.Count > 0) {
                        
                        reorg.Clear();
                        for (int i = 0; i < playerspells; i++) { reorg.Add(i); }
                        for (int i = 0; i < playerspells; i++) { shuffles[i] = -1; }
                        n = reorg.Count;
                        while (n > 1) {
                            n--;
                            k = SysRand.Next(n + 1);
                            int temp = reorg[k];
                            reorg[k] = reorg[n];
                            reorg[n] = temp;
                        }
                    }
                }

                //ITEM SOFTLOCK PROTECTION
                //using world-only spell items in battle (or vice versa?) can cause issues up to softlocks.
                //spell items should have updated rules based on what they are modified to.
                //silent flute - silence 2 - id58
                //celine's bell - restriction 2 - id52
                //replica - escape - id38
                //giant's shoes - wind walk - id57
                //silver amulet - spirit armor 1 - id17
                //golden amulet - spirit armor 2 - id22

                for (int i = 0; i < 6; i++) {
                newitemspells[i] = shuffles[spellitemID[i]];
                string rule = library.spells[(spellitemID[i] * 4) + 3].Substring(6,2);
                    if (rule == "12" || rule == "03") {
                        if (rule == "12") { //out of battle only (exit, return)
                            itemspellfix[i] = 1;
                        }
                        if (rule == "03") { //either (healing)
                            itemspellfix[i] = 2;
                        }
                    } else { //anything else battle only
                        itemspellfix[i] = 0;
                    }
                }
            }

            //SPELL NAME SHUFFLING (based on shuffles array and existing data)
            Console.WriteLine(rngseed);
            for (int i = 0; i < playerspells; i++) {
                bool fiftyfifty = SysRand.NextDouble() > 0.5; ; 
                if (rndSpellNamesDropdown.SelectedIndex == 1) { fiftyfifty = true; } //"Linear" option
                if (fiftyfifty) { hintnames[i] = library.shuffleNames[i * 5];
                    hintnames[i] += " " + library.shuffleNames[(shuffles[i] * 5) + 1]; }
                else {
                    hintnames[i] = library.shuffleNames[shuffles[i] * 5];
                    hintnames[i] += " " + library.shuffleNames[(i * 5) + 1]; }
            }

            //RANDOM CHESTS--------------------------------------------------------------------------------

            //reinitiate chest list, in case user has gone back to Shuffle
            for (int j = 0; j < chests.Length; j++) { chests[j] = library.chestdata[j * 2 + 1]; }

            int[] itemset = itemListView1.CheckedIndices.Cast<int>().ToArray();
            int setlength = itemset.Length;

            if (rndChestDropdown.SelectedIndex >= 1 && setlength > 0) {
                for (int c = 0; c < chests.Length; c++) {
                    if (rndWeightedChestToggle.Checked && rndWeightedChestDropdown.SelectedIndex == 0) {
                        if (c < setlength) { //bottom of array is all weighted items
                            k = itemset[c];
                        } else { //top of array is random items within set
                            k = itemset[SysRand.Next(setlength)];
                        } 
                    }
                    if (rndWeightedChestToggle.Checked && rndWeightedChestDropdown.SelectedIndex == 1) {
                        if (c < setlength) { //bottom of array is all weighted items
                            k = itemset[c];
                        }
                        if (c < setlength * 2) { //double up
                            k = itemset[c - setlength];
                        }
                        if (c >= setlength * 2) { //top of array is random items within set
                            k = itemset[SysRand.Next(setlength)];
                        }
                    }
                    if (!rndWeightedChestToggle.Checked) { //whole array is random items within set
                        k = itemset[SysRand.Next(setlength)]; 
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


            //RANDOM DROPS--------------------------------------------------------------------------------

            //reinitiate item drops list
            for (int l = 0; l < drops.Length; l++) { drops[l] = library.dropdata[l * 2 + 1]; }

            itemset = itemListView2.CheckedIndices.Cast<int>().ToArray();
            setlength = itemset.Length;

            if (rndDropsDropdown.SelectedIndex >= 1 && setlength > 0) {
                for (int c = 0; c < drops.Length; c++) {
                    if (rndWeightedDropsToggle.Checked && rndWeightedDropsDropdown.SelectedIndex == 0) {
                        if (c < setlength) { //bottom of array is all weighted items
                            k = itemset[c];
                        }
                        else { //top of array is random items within set
                            k = itemset[SysRand.Next(setlength)];
                        }
                    }
                    if (rndWeightedDropsToggle.Checked && rndWeightedDropsDropdown.SelectedIndex == 1) {
                        if (c < setlength) { //bottom of array is all weighted items
                            k = itemset[c];
                        }
                        if (c < setlength * 2) { //double up
                            k = itemset[c - setlength];
                        }
                        if (c >= setlength * 2) { //top of array is random items within set
                            k = itemset[SysRand.Next(setlength)];
                        }
                    }
                    if (!rndWeightedDropsToggle.Checked) { //whole array is random items within set
                        k = itemset[SysRand.Next(setlength)];
                    }
                    drops[c] = k;
                }
            }

            //STANDARD DROP SHUFFLE (always happens afterwards, or if dropdown index is 0)
            d = drops.Length;
            while (d > 1) {
                d--;
                k = SysRand.Next(d + 1);
                int temp = drops[k];
                drops[k] = drops[d];
                drops[d] = temp;
            }

            //RANDOM GIFTS--------------------------------------------------------------------------------

            //option to exclude final shannons, forcing them to be vanilla. Either way, the game will prevent softlocks.
            if (!rndShuffleShannonToggle.Checked) { gifts = new int[10]; }
            if (rndShuffleShannonToggle.Checked) { gifts = new int[8]; } //8-indice array will just never overwrite the last two items, so they'll be vanilla.

            //initiate item gifts list with vanilla items
            for (int l = 0; l < gifts.Length; l++) { gifts[l] = library.itemgranters[l * 2 + 1]; }

            itemset = itemListView3.CheckedIndices.Cast<int>().ToArray();
            setlength = itemset.Length;

            //randomized gifts happen first so they can be shuffled after
            if (rndGiftersDropdown.SelectedIndex >= 1 && setlength > 0) {
                int c = gifts.Length;

                while (c > 0) { //fill the array with items
                    c--;
                    k = itemset[SysRand.Next(setlength)];
                    gifts[c] = k;
                }

                if (!rndShuffleShannonToggle.Checked) { //ensure there's at least one book and key in the array
                    gifts[8] = 24;
                    gifts[9] = 25;
                }
            }

            //shuffling, happens for both random/shuffle options
            d = gifts.Length;
            while (d > 1) {
                d--;
                k = SysRand.Next(d + 1);
                int temp = gifts[k];
                gifts[k] = gifts[d];
                gifts[d] = temp;
            }

            //if final shannons are not forced vanilla, guarantee key/book are never on them to avoid softlocks
            if (!rndShuffleShannonToggle.Checked) { 
                int newloc1 = 0;
                int newloc2 = 0;
                if (gifts[8] == 24 || gifts[8] == 25) { //if the first shannon item is book or key
                    newloc1 = SysRand.Next(8); //pick a gifter slot from 0-7 to rotate it into
                    int temp = gifts[8];
                    gifts[8] = gifts[newloc1];
                    gifts[newloc1] = temp;
                }
                if (gifts[9] == 24 || gifts[9] == 25) { //if the second shannon item is book or key
                    newloc2 = SysRand.Next(8);
                    while (newloc2 == newloc1) { newloc2 = SysRand.Next(8); } //guarantee it's in a different slot than the first
                    int temp = gifts[9];
                    gifts[9] = gifts[newloc2];
                    gifts[newloc2] = temp;
                }
            }

            //RANDOM WINGSMITHS-----------------------------------------------------------------------------

            //initiate wingsmiths list
            for (int l = 0; l < wings.Length; l++) { wings[l] = library.itemgranters[20 + (l * 2 + 1)]; } //20 ahead to get to wingsmiths

            itemset = itemListView4.CheckedIndices.Cast<int>().ToArray();
            setlength = itemset.Length;

            if (rndWingsmithsDropdown.SelectedIndex >= 1 && setlength > 0) {
                for (int c = 0; c < wings.Length; c++) {
                    k = itemset[SysRand.Next(setlength)];
                    wings[c] = k;
                }
            }

            //STANDARD WINGS SHUFFLE (always happens afterwards, or if dropdown index is 0)
            d = wings.Length;
            while (d > 1) {
                d--;
                k = SysRand.Next(d + 1);
                int temp = wings[k];
                wings[k] = wings[d];
                wings[d] = temp;
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
                            double currentstat = library.avg_monster[(i * 7) + m];
                            double highend = library.avg_monster[(i * 7) + 5] * (1 + extremity);
                            double lowend = library.avg_monster[(i * 7) + 6] * (1 - extremity);
                            double variance = SysRand.NextDouble() * (highend - lowend) + lowend;
                            double modifiedstat = (currentstat * (variance / 10) * (difficultyscale));
                            newmonsterstats[(currentmonster * 6) + m] = (int)Math.Round(modifiedstat);
                            if (newmonsterstats[(currentmonster * 6) + m] == 0) { newmonsterstats[(currentmonster * 6) + m] = 1; }
                        }
                    }
                }

                //boss randomization, ranges are hard-coded rn based on local area
                //variance is halved on bosses to restrain balance, since higher values mean wilder varied numbers
                for (int i = 0; i < 8; i++) {
                    int currentmonster = 67 + i;
                    int region = library.boss_regions[i];
                    for (int m = 0; m < 5; m++) { //sets each of their 5 new stats, based on area average, variance, and difficulty modifier
                        double currentstat = library.monsterstatvanilla[currentmonster * 6 + m];
                        double highend = ((library.avg_monster[(region * 7) + 5] + 10) / 2) * (1 + extremity); //+10/2 halves variance
                        double lowend = ((library.avg_monster[(region * 7) + 6] + 10) / 2) * (1 - extremity); //+10/2 halves variance
                        double variance = SysRand.NextDouble() * (highend - lowend) + lowend;
                        double modifiedstat = (currentstat * (variance / 10) * (difficultyscale));
                        newmonsterstats[(currentmonster * 6) + m] = (int)Math.Round(modifiedstat);
                        if (newmonsterstats[(currentmonster * 6) + m] == 0) { newmonsterstats[(currentmonster * 6) + m] = 1; }
                    }
                }
            }

            ////scale bosses only after, if randomization is checked
            //if (rndMonsterStatsToggle.Checked && rndMonsterScaleToggle.Checked) {
            //    for (int i = 67; i < 75; i++) {
            //        for (int j = 0; j < 5; j++) {
            //            newmonsterstats[(i * 6) + j] = (int)Math.Round(newmonsterstats[(i * 6) + j] * (difficultyscale));
            //        }
            //    }
            //}

            //scale all if monster stat randomization isn't active but scaling is
            if (!rndMonsterStatsToggle.Checked && rndMonsterScaleToggle.Checked) {
                for (int i = 0; i < 75; i++) {
                    for (int j = 0; j < 5; j++) {
                        newmonsterstats[(i * 6) + j] = (int)Math.Round(newmonsterstats[(i * 6) + j] * (difficultyscale));
                    }
                }
            }

            //Adjust EXP by BST
            if (rndMonsterExpToggle.Checked) {
                for (int i = 17; i > -1; i--) { //17: 16 areas and bosses
                    int locals = library.mon_enemycount[i];
                    int locale = library.mon_locationsindex[i];
                    for (int j = 0; j < locals; j++) { //steps through each area's monster set one by one
                        int currentmonster = library.mon_locations[locale + j];

                        //add ATK/DEF/AGI for BST
                        int bst = newmonsterstats[(currentmonster * 6) + 1] + newmonsterstats[(currentmonster * 6) + 2] + newmonsterstats[(currentmonster * 6) + 3];
                        //Set exp based on Raph's experience curve formula:
                        //x ^ 2 / 45 * tan(x / 375)
                        //double bstcalc = bst * bst / 45d * Math.Tan(bst / 375d);

                        //temporary holdover bstcalc while we tinker with the formula, simple scaling vs vanilla bst
                        double vanillabst = library.monsterstatvanilla[(currentmonster * 6) + 1] + library.monsterstatvanilla[(currentmonster * 6) + 2] + library.monsterstatvanilla[(currentmonster * 6) + 3];
                        double bstcalc = library.monsterstatvanilla[(currentmonster * 6) + 4] * (bst / vanillabst);
                        newmonsterstats[(currentmonster * 6) + 4] = (int)Math.Ceiling(bstcalc);
                    }
                }
            }

            //VOWEL SHUFFLE

            //populate updated monster name array and vowel list
            for (int l = 0; l < voweled.Length; l++) { voweled[l] = library.monsternames[l * 2]; }
            char[] vowels = { 'A', 'E', 'I', 'O', 'U' };

            for (int h = 0; h < voweled.Length; h++) { //iterate through all names
                char[] charArr = voweled[h].ToCharArray();

                for (int i = 0; i < charArr.Length; i++) { //iterate through name characters
                    if (charArr[i] == 'A' || charArr[i] == 'E' || charArr[i] == 'I' || charArr[i] == 'O' || charArr[i] == 'U') {
                        charArr[i] = vowels[SysRand.Next(5)];
                    }
                }

                voweled[h] = new string(charArr); //rebuild name
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
                !rndGiftersToggle.Checked &&
                !rndWingsmithsToggle.Checked &&
                !rndDropLimitToggle.Checked &&
                !rndLevelToggle.Checked &&
                !rndSoulToggle.Checked &&
                !rndInvalidityToggle.Checked &&
                !rndZoomToggle.Checked &&
                !rndAccuracyToggle.Checked &&
                !rndRestlessToggle.Checked &&
                !rndMaxMessageToggle.Checked &&
                !rndMonsterScaleToggle.Checked &&
                !rndFastMonasteryToggle.Checked &&
                !rndVowelsToggle.Checked &&
                !rndHUDLockToggle.Checked &&
                !rndStartingStatsToggle.Checked &&
                !rndElement99Toggle.Checked
               ) { return; }
            //eventually i maybe will replace this with a sort of 'binary state' checker that'll be way less annoying and also have the side of effect of creating enterable shortcodes for option sets

            writingPatch = true;

            //update filename one more time here to avoid errors
            if (expFilenameTextBox.Text != "" && expFilenameTextBox.Text != null) {
                fileName = string.Join("", expFilenameTextBox.Text.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries)); //strip all whitespace to avoid errors
                if (seedName) { fileName += "_" + rngseed.ToString(); }
                if (dateName) { fileName += "_" + DateTime.Now.ToString("yyMMdd-HHmmss"); }
            }
            else {
                if (expModePatchZ64.Checked) { fileName = rndFileName + "_p"; }
                if (expModePatchIPS.Checked) { fileName = "merrowpatch"; }
                if (seedName) { fileName += "_" + rngseed.ToString(); }
                if (dateName) { fileName += "_" + DateTime.Now.ToString("yyMMdd-HHmmss"); }
            }

            //reshuffle here so I don't have to shuffle after every option is changed in the UI, only certain ones
            Shuffling(true);

            //start spoiler log and initialize empty patch content strings
            File.WriteAllText(filePath + fileName + "_spoiler.txt", "MERROW " + labelVersion.Text + " building patch..." + Environment.NewLine);
            File.AppendAllText(filePath + fileName + "_spoiler.txt", "Seed: " + rngseed.ToString() + Environment.NewLine);
            File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "PATCH MODIFIERS:" + Environment.NewLine);
            patchbuild = "";
            patchcontent = "";
            patchstrings.Clear();

            //RANDOMIZATION FEATURES

            if (rndSpellToggle.Checked) { //Spell Shuffle
                for (int q = 0; q < playerspells; q++) {
                    int tempq = 0;

                    if (rndSpellDropdown.SelectedIndex == 0) { tempq = shuffles[q]; } //set spell q to use spell shuffles[q] data
                    if (rndSpellDropdown.SelectedIndex != 0) { tempq = rndSpellDropdown.SelectedIndex - 1; }

                    tempaddr = Convert.ToInt32(library.spells[(q * 4) + 2]) + 3; //set rule address from dec version of hex, incrementing 3
                    tempstr1 = Convert.ToString(tempaddr, 16); //convert updated address back to hex string
                    tempstr2 = library.spells[(tempq * 4) + 3].Substring(6, 2); //copy other spell rule data
                    patchstrings.Add(tempstr1); //current spell rule address
                    patchstrings.Add("0001"); //spell rule length, hex for 1
                    patchstrings.Add(tempstr2); //copied spell rule data

                    tempaddr = Convert.ToInt32(library.spells[(q * 4) + 2]) + 11; //set remaining address from dec version of hex, incrementing 11
                    tempstr1 = Convert.ToString(tempaddr, 16); //convert updated address back to hex string
                    tempstr2 = library.spells[(tempq * 4) + 3].Substring(22); //copy other remaining data
                    patchstrings.Add(tempstr1); //current remaining address
                    patchstrings.Add("0039"); //remaining length, hex for 57
                    patchstrings.Add(tempstr2); //copied remaining data

                    spoilerspells[q] = library.spells[(q * 4)] + " > " + library.spells[(tempq * 4)];
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Spells overridden." + Environment.NewLine);

                if (rndSpellNamesToggle.Checked && rndSpellDropdown.SelectedIndex == 0) {
                    //boss spells
                    for (int i = 0; i < 6; i++) {
                        patchstrings.Add(library.shuffleBossSpellNames[i]); //first three are new null name, second three are boss name pointers
                    }


                    //spell pointers
                    for (int i = 0; i < playerspells; i++) {
                        patchstrings.Add(library.shuffleNames[(i * 5) + 3]); //pointer location
                        patchstrings.Add("0004"); //write four bytes
                        patchstrings.Add(library.shuffleNames[(i * 5) + 4]); //new pointer data
                    }

                    //spell names
                    for (int i = 0; i < playerspells; i++) {
                        string temps = ToHex(hintnames[i]);
                        int zeroes = 32 - temps.Length;
                        patchstrings.Add(library.shuffleNames[(i * 5) + 2]);
                        patchstrings.Add("0010");
                        patchcontent = temps;
                        for (int j = 0; j < zeroes; j++) { patchcontent += "0"; }
                        patchstrings.Add(patchcontent);
                    }

                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Shuffled spells use hinted names." + Environment.NewLine);
                }

                patchstrings.Add("667260"); //Fix for skelebat group in Blue Cave that can cause crashes due to lag
                patchstrings.Add("000C");
                patchstrings.Add("000000060000000100000001");

                //spell item fixes
                for (int i = 0; i < 6; i++) {
                    patchstrings.Add(library.items[25 + (i * 3)]); //fetching item addresses starting from silent flute address
                    patchstrings.Add("0002");

                    if (itemspellfix[i] == 0) { //can be used in battle only
                        patchstrings.Add("000A");
                    }
                    if (itemspellfix[i] == 1) { //out of battle
                        patchstrings.Add("0001");
                    }
                    if (itemspellfix[i] == 2) { //anytime
                        patchstrings.Add("0003");
                    }
                }
            }

            if (rndTextPaletteToggle.Checked) { //Text Colour
                //default black palette is stored at D3E240
                // black: F83E9C1B6AD5318D
                // red: F83E9C1BBA0DD009
                // blue: F83E9C1B629D19AB
                // white: F83E318DBDEFF735

                int temp = rndTextPaletteDropdown.SelectedIndex;
                if (temp == 1) { temp = SysRand.Next(2, 6); }

                patchstrings.Add("D3E240");
                patchstrings.Add("0008");

                if (temp == 0) {
                    patchcontent = "F83E";
                    patchcontent += textPaletteHex;
                    patchstrings.Add(patchcontent);
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text palette set to random." + Environment.NewLine);
                }
                if (temp == 2) {
                    patchstrings.Add("F83E9C1BBA0DD009");
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text palette set to red." + Environment.NewLine);
                }
                if (temp == 3) {
                    patchstrings.Add("F83E9C1B629D19AB");
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text palette set to blue." + Environment.NewLine);
                }
                if (temp == 4) {
                    patchstrings.Add("F83E318DBDEFF735");
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text palette set to white." + Environment.NewLine);
                }
                if (temp == 5) {
                    patchstrings.Add("F83E9C1B6AD5318D");
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text palette set to black [default]." + Environment.NewLine);
                }
            }

            //Chest shuffle
            if (rndChestToggle.Checked) { 
                //add chest addresses, and new byte
                for (int i = 0; i < chests.Length; i++) {
                    int temp = library.chestdata[i * 2] + 33; //33 is offset to chest item byte
                    patchstrings.Add(Convert.ToString(temp, 16));
                    patchstrings.Add("0001");
                    patchstrings.Add(chests[i].ToString("X2"));
                    spoilerchests[i] = i.ToString("00") + ": " + library.items[(chests[i] * 3)];
                }

                if (rndChestDropdown.SelectedIndex == 0) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Chest contents shuffled." + Environment.NewLine);
                }
                if (rndChestDropdown.SelectedIndex > 0) {
                    string randomtypeS = library.randomtype[rndChestDropdown.SelectedIndex];
                    if (rndWeightedChestToggle.Checked) { randomtypeS += ", weighted"; }

                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Chest contents randomized (" + randomtypeS + ")." + Environment.NewLine);
                }
            }

            //Item Drop Shuffle
            if (rndDropsToggle.Checked) { 
                //add drop addresses, and new byte
                for (int i = 0; i < drops.Length; i++) {
                    int temp = library.dropdata[i * 2]; //don't need to offset because drop list is pre-offset
                    patchstrings.Add(Convert.ToString(temp, 16));
                    patchstrings.Add("0001");
                    patchstrings.Add(drops[i].ToString("X2"));

                    if (!rndVowelsToggle.Checked) {
                        if (drops[i] != 255) { spoilerdrops[i] = library.monsternames[i * 2] + ": " + library.items[drops[i] * 3]; }
                        if (drops[i] == 255) { spoilerdrops[i] = library.monsternames[i * 2] + ": NONE"; }
                    }
                    if (rndVowelsToggle.Checked) {
                        if (drops[i] != 255) { spoilerdrops[i] = voweled[i] + ": " + library.items[drops[i] * 3]; }
                        if (drops[i] == 255) { spoilerdrops[i] = voweled[i] + ": NONE"; }
                    }
                }

                if (rndDropsDropdown.SelectedIndex == 0) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Enemy drops shuffled." + Environment.NewLine);
                } else {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Enemy drops randomized (" + library.randomtype[rndDropsDropdown.SelectedIndex] + ")." + Environment.NewLine);
                }
            }

            //Item Gift Shuffle
            if (rndGiftersToggle.Checked) { 
                //add gift addresses, and new byte
                for (int i = 0; i < gifts.Length; i++) {
                    int temp = library.itemgranters[i * 2]; //don't need to offset because gift hex loc list is pre-offset
                    patchstrings.Add(Convert.ToString(temp, 16));
                    patchstrings.Add("0001");
                    patchstrings.Add(gifts[i].ToString("X2"));

                    spoilergifts[i] = library.granternames[i] + ": " + library.items[gifts[i] * 3];
                }

                if (rndShuffleShannonToggle.Checked) { //if shannons are forced vanilla, add this note about them being vanilla
                    spoilergifts[8] = "Shannon (Brannoch Castle): ELETALE BOOK (unrandomized)";
                    spoilergifts[9] = "Shannon (Mammon's World): DARK GAOL KEY (unrandomized)";
                }

                if (rndGiftersDropdown.SelectedIndex == 0) { //shuffle
                    if (rndShuffleShannonToggle.Checked) {
                        File.AppendAllText(filePath + fileName + "_spoiler.txt", "NPC gifts shuffled (Shannons excluded)." + Environment.NewLine);
                    } else {
                        File.AppendAllText(filePath + fileName + "_spoiler.txt", "NPC gifts shuffled." + Environment.NewLine);
                    }
                }

                if (rndGiftersDropdown.SelectedIndex > 0) { //random
                    if (rndShuffleShannonToggle.Checked) {
                        File.AppendAllText(filePath + fileName + "_spoiler.txt", "NPC gifts randomized (" + library.randomtype[rndGiftersDropdown.SelectedIndex] + "), Shannons excluded." + Environment.NewLine);
                    } else {
                        File.AppendAllText(filePath + fileName + "_spoiler.txt", "NPC gifts randomized (" + library.randomtype[rndGiftersDropdown.SelectedIndex] + ")." + Environment.NewLine);
                    }
                }
            }

            //Wingsmiths Shuffle
            if (rndWingsmithsToggle.Checked) { 
                //add wings addresses, and new byte
                for (int i = 0; i < wings.Length; i++) {
                    int temp = library.itemgranters[20 + (i * 2)]; //gift hex loc list is pre-offset, advance 20 to skip gifters
                    patchstrings.Add(Convert.ToString(temp, 16));
                    patchstrings.Add("0001");
                    patchstrings.Add(wings[i].ToString("X2"));

                    spoilerwings[i] = library.granternames[i + 10] + ": " + library.items[wings[i] * 3]; //advance 10 to skip gifters
                }

                if (rndWingsmithsDropdown.SelectedIndex == 0) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Wingsmiths shuffled." + Environment.NewLine);
                } else { 
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Wingsmiths randomized (" + library.randomtype[rndWingsmithsDropdown.SelectedIndex] + ")." + Environment.NewLine);
                }
            }

            //Enemy drop limit toggle
            if (rndDropLimitToggle.Checked) {
                patchstrings.Add("0042B1");
                patchstrings.Add("0001");
                patchstrings.Add("42");

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Enemy drop limit disabled." + Environment.NewLine);
            }

            //Text content shuffle
            if (rndTextContentToggle.Checked) {
                int temp = 0;
                //add single text addresses, and new byte
                for (int i = 0; i < 72; i++) {
                    temp = library.singletextdata[i * 3] + 8; //text byte at offset 8
                    patchstrings.Add(Convert.ToString(temp, 16));
                    patchstrings.Add("0002");
                    patchstrings.Add(texts[i].ToString("X4"));
                }

                //add double text addresses, and new byte
                for (int i = 0; i < 68; i++) {
                    temp = library.doubletextdata[i * 4] + 8; //first text at offset 8
                    patchstrings.Add(Convert.ToString(temp, 16));
                    patchstrings.Add("0002");
                    patchstrings.Add(texts[i + 72].ToString("X4"));

                    temp = library.doubletextdata[i * 4] + 10; //second text at offset 10
                    patchstrings.Add(Convert.ToString(temp, 16));
                    patchstrings.Add("0002");
                    patchstrings.Add(texts[i + 72 + 68].ToString("X4"));
                }

                //add inn text addresses, and new byte
                for (int i = 0; i < inntexts.Length; i++) {
                    temp = library.inntextdata[i * 3] + 8; //text byte at offset 8
                    patchstrings.Add(Convert.ToString(temp, 16));
                    patchstrings.Add("0002");
                    patchstrings.Add(inntexts[i].ToString("X4"));
                }

                if (rndTextContentDropdown.SelectedIndex == 0) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text content shuffled." + Environment.NewLine);
                }
            }

            //MONSTER SCALING FEATURES

            if (rndMonsterStatsToggle.Checked || rndMonsterScaleToggle.Checked) {
                for (int i = 0; i < newmonsterstats.Length; i++) {
                    patchstrings.Add(library.monsterstatlocations[i].ToString("X6"));
                    patchstrings.Add("0002");
                    patchstrings.Add(newmonsterstats[i].ToString("X4"));

                    if (i % 6 == 0) { //if the current value is HP2, write it again at the HP1 location, offset 04 (HP2 + 2).
                        patchstrings.Add((library.monsterstatlocations[i] + 2).ToString("X6"));
                        patchstrings.Add("0002");
                        patchstrings.Add(newmonsterstats[i].ToString("X4"));
                    }
                }

                for (int i = 0; i < 75; i++) {
                    if (!rndVowelsToggle.Checked) { spoilerscales[i] = library.monsternames[i * 2] + ": "; }
                    if (rndVowelsToggle.Checked) { spoilerscales[i] = voweled[i] + ": "; }
                    spoilerscales[i] += newmonsterstats[i * 6].ToString() + " ";
                    spoilerscales[i] += newmonsterstats[(i * 6) + 1].ToString() + " ";
                    spoilerscales[i] += newmonsterstats[(i * 6) + 2].ToString() + " ";
                    spoilerscales[i] += newmonsterstats[(i * 6) + 3].ToString() + " ";
                    spoilerscales[i] += newmonsterstats[(i * 6) + 4].ToString() + " ";
                    spoilerscales[i] += newmonsterstats[(i * 6) + 5].ToString();
                }

                if (rndMonsterStatsToggle.Checked && extremity == 0) { File.AppendAllText(filePath + fileName + "_spoiler.txt", "Monster stats randomized within regions." + Environment.NewLine); }
                if (rndMonsterStatsToggle.Checked && extremity != 0) { File.AppendAllText(filePath + fileName + "_spoiler.txt", "Monster stats randomized, with Variance " + (extremity + 1).ToString() + "x." + Environment.NewLine); }
                if (rndMonsterScaleToggle.Checked) { File.AppendAllText(filePath + fileName + "_spoiler.txt", "Monster stats scaled by " + difficultyscale.ToString("n1") + "x." + Environment.NewLine); }
                if (rndMonsterExpToggle.Checked) { File.AppendAllText(filePath + fileName + "_spoiler.txt", "Monster experience scaled to new stat values." + Environment.NewLine); }
            }

            //QUALITY OF LIFE FEATURES

            if (rndInvalidityToggle.Checked) { //Invalidity
                for (int i = 0; i < 8; i++) {
                    patchstrings.Add(library.invalidityLocations[i]);
                    patchstrings.Add("0001");
                    patchstrings.Add("00");
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Boss spell passive invalidity disabled." + Environment.NewLine);
            }

            if (rndAccuracyToggle.Checked) {
                if (rndAccuracyDropdown.SelectedIndex == 0) { //Spell Accuracy: Status 100
                    for (int i = 0; i < 17; i++) {
                        string spellloc = library.spells[(library.statusspells[i] * 4) + 2];
                        int temploc = Convert.ToInt32(spellloc) + 15;
                        patchstrings.Add(Convert.ToString(temploc, 16));
                        patchstrings.Add("0001");
                        patchstrings.Add("64");
                    }

                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Status effects' accuracy normalized to 100." + Environment.NewLine);
                }

                if (rndAccuracyDropdown.SelectedIndex == 1) { //Spell Accuracy: All 100
                    for (int z = spellstart + 15; z < ((spelloffset * playerspells) + spellstart); z += spelloffset) {
                        patchstrings.Add(Convert.ToString(z, 16));
                        patchstrings.Add("0001");
                        patchstrings.Add("64");
                    }

                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "All spells' accuracy normalized to 100." + Environment.NewLine);
                }
            }

            if (rndZoomToggle.Checked) { //Zoom Option
                //width 03698A height 036A26
                //16368 = 3FF0 = default zoom value
                //16356 = 3FE4 = lowest stable zoom value

                zoomvalue = rndZoomDropdown.SelectedIndex + 2;
                patchstrings.Add("03698A");
                patchstrings.Add("0002");
                patchstrings.Add(Convert.ToString(16368 - zoomvalue, 16));

                patchstrings.Add("036A26");
                patchstrings.Add("0002");
                patchstrings.Add(Convert.ToString(16368 - zoomvalue, 16));

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Zoom out factor set to " + zoomvalue.ToString() + " [Default:1]" + Environment.NewLine);
            }

            if (rndLevelToggle.Checked) { //Level 1
                for (int s = spellstart; s < ((spelloffset * playerspells) + spellstart); s += spelloffset) {
                    patchstrings.Add(Convert.ToString(s, 16));
                    patchstrings.Add("0002");
                    patchstrings.Add("0001");
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Spell unlock levels reduced to 1." + Environment.NewLine);
            }

            if (rndSoulToggle.Checked) { //Soul Search
                for (int z = spellstart + 57; z < ((spelloffset * playerspells) + spellstart); z += spelloffset) {
                    patchstrings.Add(Convert.ToString(z, 16));
                    patchstrings.Add("0001");
                    patchstrings.Add("01");
                }

                tempaddr = Convert.ToInt32("D81C30", 16);
                for (int i = 0; i < 16; i++) {
                    patchstrings.Add(Convert.ToString(tempaddr, 16)); ; //tempaddr converted back to hex
                    patchstrings.Add("0010"); //replace 16 bytes
                    patchstrings.Add(library.magnifier[i]); //add the 16 replacement bytes from the array
                    tempaddr += 128; //step to next case
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Soul search applied to all spells." + Environment.NewLine);
            }

            if (rndRestlessToggle.Checked) { //Restless NPCs
                for (int i = 0; i < library.npcmovement.Length; i++) {
                    patchstrings.Add(Convert.ToString(library.npcmovement[i], 16));
                    patchstrings.Add("0001");
                    patchstrings.Add("02"); //Replace movement byte with 02 to cause wandering
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "NPCs are restless." + Environment.NewLine);
            }

            if (rndMaxMessageToggle.Checked) { //Max Message Speed
                patchstrings.Add("060600");
                patchstrings.Add("0001");
                patchstrings.Add("00");

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Message speed set to maximum." + Environment.NewLine);
            }

            if (rndFastMonasteryToggle.Checked) { //Fast Monastery
                patchstrings.Add("4361A0");
                patchstrings.Add("0004");
                patchstrings.Add("00090002"); // write 00090002 as new door target ID at 4361A0

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Fast Monastery enabled." + Environment.NewLine);
            }

            if (rndVowelsToggle.Checked) { //Vowel Shuffle
                for (int i = 0; i < voweled.Length; i++) {
                    patchstrings.Add(library.monsternames[(i * 2) + 1]); //hex location

                    int decLength = voweled[i].Length;
                    patchstrings.Add(decLength.ToString("X4")); //name length in hex bytes

                    patchstrings.Add(ToHex(voweled[i]));
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Vowel play enabled." + Environment.NewLine);
            }

            //HUD lock toggle
            if (rndHUDLockToggle.Checked) {
                patchstrings.Add("01F0AF");
                patchstrings.Add("0001");
                patchstrings.Add("00");

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "HUD onscreen lock enabled." + Environment.NewLine);
            }

            //Wing unlock toggle
            if (rndWingUnlockToggle.Checked) {
                if (rndWingUnlockDropdown.SelectedIndex == 0 || rndWingUnlockDropdown.SelectedIndex == 2) {
                    patchstrings.Add("022ECB");
                    patchstrings.Add("0001");
                    patchstrings.Add("00");

                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Wings enabled indoors." + Environment.NewLine);
                }
                if (rndWingUnlockDropdown.SelectedIndex == 1 || rndWingUnlockDropdown.SelectedIndex == 2) {
                    patchstrings.Add("022EE4");
                    patchstrings.Add("0001");
                    patchstrings.Add("10");

                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Wings enabled on the Isle of Skye." + Environment.NewLine);
                }
            }

            //Starting stats
            if (rndStartingStatsToggle.Checked) { 
                patchstrings.Add("054908");
                patchstrings.Add("000C");
                string tempstats = "";
                tempstats += rndHPTrackBar.Value.ToString("X4");
                tempstats += rndHPTrackBar.Value.ToString("X4");
                tempstats += rndMPTrackBar.Value.ToString("X4");
                tempstats += rndMPTrackBar.Value.ToString("X4");
                tempstats += rndAgiTrackBar.Value.ToString("X4");
                tempstats += rndDefTrackBar.Value.ToString("X4");
                patchstrings.Add(tempstats);

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Starting stats modified: " + rndHPTrackBar.Value.ToString() + "/" + rndMPTrackBar.Value.ToString() + "/" + rndAgiTrackBar.Value.ToString() + "/" + rndDefTrackBar.Value.ToString() + Environment.NewLine);
            }

            //Max element uncap
            if (rndElement99Toggle.Checked) {
                patchstrings.Add("00850A"); //Elemental cap Part 1
                patchstrings.Add("0002");
                patchstrings.Add("0126");

                patchstrings.Add("008546"); //Elemental cap Part 2
                patchstrings.Add("0001");
                patchstrings.Add("64");

                patchstrings.Add("008563"); //EXP growth lock
                patchstrings.Add("0001");
                patchstrings.Add("63");

                for (int i = 0; i < 4; i++) { //Individual elemental caps
                    patchstrings.Add(library.elementCapLocations[i]);
                    patchstrings.Add("0001");
                    patchstrings.Add("63");
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Element level maximum raised to 99." + Environment.NewLine);
            }

            //MP regain rate
            if (rndMPRegainToggle.Checked) {
                //the trackbars are restricted to x3 in either direction because of FF limit
                //also doesn't bother writing if the value's default. Value 7 = OFF.
                if (rndMPRegainTrackBar.Value != 0 && rndMPRegainTrackBar.Value != 10) { 
                    double newrate = 1.0;
                    int newspeed = 65;
                    if (rndMPRegainTrackBar.Value < 10) {
                        newrate = 11 - rndMPRegainTrackBar.Value;
                        newspeed = Convert.ToInt32(65 * newrate); //increasing value slows it down
                    }
                    if (rndMPRegainTrackBar.Value > 10) {
                        newrate = rndMPRegainTrackBar.Value - 9;
                        newspeed = Convert.ToInt32(65 / newrate); //decreasing value speeds it up
                    }

                    patchstrings.Add("071B39");
                    patchstrings.Add("0001");
                    patchstrings.Add(newspeed.ToString("X2")); 
                }
                
                if (rndMPRegainTrackBar.Value == 7) { //if OFF, instead of changing rate, just disable it entirely
                    patchstrings.Add("00445B");
                    patchstrings.Add("0001");
                    patchstrings.Add("00");
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Walking MP regen rate set to " + rndMPRegainValue.Text + "." + Environment.NewLine);
            }

            //Fast Mammon's World
            if (rndFastMammonToggle.Checked) {
                patchstrings.Add("84EE01");
                patchstrings.Add("0001");
                patchstrings.Add("0D");

                patchstrings.Add("607920");
                patchstrings.Add("0008");
                patchstrings.Add("0000000F000D0000");
            }

            //FINAL ASSEMBLY/OUTPUT

            //Verbose spoiler log down at the bottom just to not hide the enabled options above.
            if (verboselog) { 
                if (rndSpellToggle.Checked) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "ALTERED SPELLS:" + Environment.NewLine);
                    foreach (string line in spoilerspells) { File.AppendAllText(filePath + fileName + "_spoiler.txt", line + Environment.NewLine); }
                }

                if (rndChestToggle.Checked) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "ALTERED CHESTS:" + Environment.NewLine);
                    foreach (string line in spoilerchests) { File.AppendAllText(filePath + fileName + "_spoiler.txt", line + Environment.NewLine); }
                }

                if (rndDropsToggle.Checked) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "ALTERED DROPS:" + Environment.NewLine);
                    foreach (string line in spoilerdrops) { File.AppendAllText(filePath + fileName + "_spoiler.txt", line + Environment.NewLine); }
                }

                if (rndGiftersToggle.Checked) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "ALTERED GIFTS:" + Environment.NewLine);
                    foreach (string line in spoilergifts) { File.AppendAllText(filePath + fileName + "_spoiler.txt", line + Environment.NewLine); }
                }

                if (rndWingsmithsToggle.Checked) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "ALTERED WINGSMITHS:" + Environment.NewLine);
                    foreach (string line in spoilerwings) { File.AppendAllText(filePath + fileName + "_spoiler.txt", line + Environment.NewLine); }
                }

                if (rndMonsterStatsToggle.Checked || rndMonsterScaleToggle.Checked) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "ALTERED MONSTER STATS (HP, ATK, DEF, AGI, EXP, ELEMENT):" + Environment.NewLine);
                    foreach (string line in spoilerscales) { File.AppendAllText(filePath + fileName + "_spoiler.txt", line + Environment.NewLine); }
                }
            }

            patchstrings.Add("DAC040");
            patchstrings.Add("393C"); //main menu logo address/length
            patchstrings.Add(library.randologo);
            patchstrings.Add("DCE070");
            patchstrings.Add("393C"); //animation logo address/length
            patchstrings.Add(library.randologo);

            int patchparts = patchstrings.Count();

            //Mode: PATCH Z64
            if (expModePatchZ64.Checked && rndFileSelected) {
                for (int i = 0; i < patchparts; i += 3) {
                    int targetAddr = Convert.ToInt32(patchstrings[i], 16);
                    byte[] targetData = StringToByteArray(patchstrings[i + 2]);
                    int targetLength = targetData.Length;

                    for (int j = 0; j < targetLength; j++) {
                        rndFileBytes[targetAddr + j] = targetData[j];
                    }
                }
                string thisFile = filePath + fileName + ".z64";
                File.WriteAllBytes(thisFile, rndFileBytes);
                fix_crc(thisFile);
                filePath = filePath.Replace(@"/", @"\");   // explorer doesn't like front slashes
                System.Diagnostics.Process.Start("explorer.exe", Application.StartupPath + "\\Patches\\");
                rndErrorLabel.Text = "Z64 file creation complete. CRC repaired.";
            }

            //Mode: CREATE IPS
            if (expModePatchIPS.Checked) { 
                patchbuild += headerHex;
                    for (int ps = 0; ps < patchparts; ps++) {
                        patchbuild += patchstrings[ps];
                    }
                patchbuild += footerHex;
                patcharray = StringToByteArray(patchbuild);
                File.WriteAllBytes(filePath + fileName + ".ips", patcharray);
                filePath = filePath.Replace(@"/", @"\");   // explorer doesn't like front slashes
                System.Diagnostics.Process.Start("explorer.exe", Application.StartupPath + "\\Patches\\");
                rndErrorLabel.Text = "IPS Patch creation complete.";
            }

            writingPatch = false;
        }

        //ITEM RANDOMIZER FUNCTIONS-------------------------------------------------

        public void itemListUpdate(ListView currentList, int currentIndex) {
            lockItemUpdates = true;
            if (currentIndex == 0) { itemListWipe(currentList); }
            if (currentIndex >= 1 && currentIndex <= 7) {
                itemListWipe(currentList);
                itemListSet(currentList, itemListSelect(currentIndex));
            }
            lockItemUpdates = false;
        }

        public int[] itemListSelect(int option) { //grab the item array from the library
            if (option == 1) { return library.itemlist_standard; }
            if (option == 2) { return library.itemlist_standardwings; }
            if (option == 3) { return library.itemlist_standardgems; }
            if (option == 4) { return library.itemlist_chaos; }
            if (option == 5) { return library.itemlist_wings; }
            if (option == 6) { return library.itemlist_gems; }
            if (option == 7) { return library.itemlist_wingsgems; }

            return library.itemlist_standard; //return this as a backup, should never happen
        }

        public void itemListWipe(ListView listID) { //wipe the specified item list so we can set it
            for (int i = 0; i < 26; i++) {
                ListViewItem listChestItem = listID.Items[i];
                listChestItem.Checked = false;
            }
        }

        public void itemListSet(ListView listID, int[] listArray) { //set the specified item list
            for (int i = 0; i < listArray.Length; i++) {
                ListViewItem listChestItem = listID.Items[listArray[i]];
                listChestItem.Checked = true;
            }
        }

        //CRC REPAIR TOOL--------------------------------------------------------------------

        public int RepairCRC() {
            int check = -1;
            if (crcFileSelected) { check = fix_crc(fullPath); }
            return check;
        }

        //WINFORMS UI INTERACTIONS-------------------------------------------------------------------
        //These declarations should not be manually edited without care, as they're partially auto-generated by the Winforms Designer.
        //The content can be edited as needed. They're kept here for logical sake, because this is the :Form type function.

        //General functions

        private void tabsControl_SelectedIndexChanged(object sender, EventArgs e) {
            binErrorLabel.Visible = false;
            binFileLoaded = false;
            binFileBytes = null;
            crcErrorLabel.Visible = false;
            crcFileSelected = false;
            expModeSet();
        }

        private void terms__LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/hangedmandesign/merrow");
        }

        public void UpdateRisk() {
            float variance = extremity + 1;
            riskvalue = Math.Round((difficultyscale * difficultyscale) * (variance * variance) * 10);
            if (!rndMonsterScaleToggle.Checked && !rndMonsterStatsToggle.Checked) {
                rndRiskLabel.Visible = false;
                rndRiskLabelText.Visible = false;
            }
            if (rndMonsterScaleToggle.Checked || rndMonsterStatsToggle.Checked) {
                int redRisk = 255;
                int greenRisk = 255;
                if (riskvalue <= 20) {
                    greenRisk = 225 - (int)(225 * (riskvalue / 20));
                    redRisk = (int)(255 * (riskvalue / 20));
                }
                if (riskvalue > 20) {
                    greenRisk = 0;
                    redRisk = 255 - (int)(255 * (riskvalue / 80));
                }

                redRisk = Math.Min(255, Math.Max(0, redRisk));
                greenRisk = Math.Min(255, Math.Max(0, greenRisk));

                rndRiskLabel.BackColor = Color.FromArgb(redRisk, greenRisk, 0);
                rndRiskLabelText.BackColor = Color.FromArgb(redRisk, greenRisk, 0);

                if (riskvalue >= 2 && riskvalue <= 7) {
                    rndRiskLabel.Text = "RISK " + riskvalue.ToString("N0") + " (BREEZE)";
                    rndRiskLabelText.Text = "Smooth and easy";
                }
                if (riskvalue >= 8 && riskvalue <= 13) {
                    rndRiskLabel.Text = "RISK " + riskvalue.ToString("N0") + " (MODERATE)";
                    rndRiskLabelText.Text = "Roughly vanilla";
                }
                if (riskvalue >= 14 && riskvalue <= 25) {
                    rndRiskLabel.Text = "RISK " + riskvalue.ToString("N0") + " (GALE)";
                    rndRiskLabelText.Text = "Difficult without grinding";
                }
                if (riskvalue > 25 && riskvalue < 40) {
                    rndRiskLabel.Text = "RISK " + riskvalue.ToString("N0") + " (STORM)";
                    rndRiskLabelText.Text = "Extremely challenging and grindy";
                }
                if (riskvalue >= 40) {
                    rndRiskLabel.Text = "RISK " + riskvalue.ToString("N0") + " (HURRICANE)";
                    rndRiskLabelText.Text = "Probably impossible";
                }

                rndRiskLabel.Visible = true;
                rndRiskLabelText.Visible = true;
            }
        }

        private static List<CheckBox> GetAllToggles(Control container) {
            var controlList = new List<CheckBox>();
            foreach (Control c in container.Controls) {
                controlList.AddRange(GetAllToggles(c));

                if (c is CheckBox box)
                    controlList.Add(box);
            }
            return controlList;
        }

        private static List<ComboBox> GetAllDropdowns(Control container) {
            var controlList = new List<ComboBox>();
            foreach (Control c in container.Controls) {
                controlList.AddRange(GetAllDropdowns(c));

                if (c is ComboBox box)
                    controlList.Add(box);
            }
            return controlList;
        }

        private static List<TrackBar> GetAllSliders(Control container) {
            var controlList = new List<TrackBar>();
            foreach (Control c in container.Controls) {
                controlList.AddRange(GetAllSliders(c));

                if (c is TrackBar box)
                    controlList.Add(box);
            }
            return controlList;
        }

        public void UpdateCode() {
            int tabpagestocheck = 3;
            string codeString = labelVersion.Text.Substring(1);
            string tempString;
            byte[] tempBytes = null;
            var toggles = new List<CheckBox>();
            var dropdowns = new List<ComboBox>();
            var sliders = new List<TrackBar>();

            if (loadfinished) { 
                //check each page in turn, convert each page's values and add it to the code string
                for (int i = 0; i < tabpagestocheck; i++) {
                    toggles.AddRange(GetAllToggles(rndTabs.TabPages[i]));
                    dropdowns.AddRange(GetAllDropdowns(rndTabs.TabPages[i]));
                    sliders.AddRange(GetAllSliders(rndTabs.TabPages[i]));
                }

                tempString = "";
                foreach (var toggle in toggles) {
                    if (toggle.Checked) { tempString += 1; } else { tempString += 0; } //build binary string
                }
                int test = Convert.ToInt32(tempString, 2); //convert binary string to int
                codeString += "T" + "." + HexToBase64(test.ToString("X")) + "."; //int to hex to 64b
                Console.WriteLine(codeString);

                tempString = "";
                foreach (var dropdown in dropdowns) {
                    //one possible shortening method: count consecutive 0 values and amalgamate them in a separate counter
                    if (dropdown.SelectedIndex == 8) { //if custom
                        if (dropdown.Name == "rndChestDropdown" || dropdown.Name == "rndDropsDropdown" || dropdown.Name == "rndGiftersDropdown" || dropdown.Name == "rndWingsmithsDropdown") {
                            //still need to account for custom values

                        }
                        else { tempString += dropdown.SelectedIndex.ToString("X2"); } //convert values to hex
                    }
                    else { tempString += dropdown.SelectedIndex.ToString("X2"); } //convert values to hex
                }
                codeString += "L" + "." + HexToBase64(tempString) + ".";
                Console.WriteLine(codeString);

                tempString = "";
                if(sliders.Count > 0) { 
                    foreach (var slider in sliders) {
                        tempString += slider.Value.ToString("X3"); //convert values to hex
                    }
                    if (tempString.Length % 2 != 0) { tempString = "0" + tempString; } //ensure it's an even number of characters
                    codeString += "S" + "." + HexToBase64(tempString);
                } else {
                    codeString += "SZ";
                }
                
                Console.WriteLine(codeString);

                rndShortcodeText.Text = codeString;
            }
        }

        //RND - Randomizer randomization

        private void rndSpellToggle_CheckedChanged(object sender, EventArgs e) {
            if(rndSpellToggle.Checked) {
                rndSpellDropdown.Enabled = true;
                rndSpellNamesToggle.Enabled = true;
                if (rndSpellNamesToggle.Checked) { rndSpellNamesDropdown.Enabled = true; }
            } else {
                rndSpellDropdown.Enabled = false;
                rndSpellNamesToggle.Enabled = false;
                rndSpellNamesDropdown.Enabled = false;
            }
            UpdateCode();
        }

        private void rndTextPaletteToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndTextPaletteToggle.Checked) {
                rndTextPaletteDropdown.Enabled = true;
                if (rndTextPaletteDropdown.SelectedIndex == 0) { rndColorViewToggle.Enabled = true; }
            } else {
                rndTextPaletteDropdown.Enabled = false;
                rndColorViewPanel.Visible = false;
                rndColorViewToggle.Enabled = false;
                rndColorViewToggle.Checked = false; //hide again to avoid spoilers if things change
            }
            UpdateCode();
        }

        private void rndTextContentToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndTextContentToggle.Checked) {
                rndTextContentDropdown.Enabled = true;
            } else {
                rndTextContentDropdown.Enabled = false;
            }
            UpdateCode();
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
            UpdateCode();
        }

        private void rndTextPaletteDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            if (rndTextPaletteDropdown.SelectedIndex == 0) {
                rndColorViewToggle.Enabled = true;
            }
            else {
                rndColorViewToggle.Enabled = false;
                rndColorViewToggle.Checked = false; //make it false again so as not to spoil the next value, and so the panel's invisible
            }
            UpdateCode();
        }

        private void rndChestToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndChestToggle.Checked) {
                rndChestDropdown.Enabled = true;
                if (rndChestDropdown.SelectedIndex != 0) { //make visible again if they've already been using it
                    rndWeightedChestToggle.Enabled = true;
                    rndWeightedChestDropdown.Enabled = true;
                } 
                itemListTabs.Visible = true;
                itemListTabs.SelectedIndex = 0;
            }
            else {
                rndChestDropdown.Enabled = false;
                rndWeightedChestToggle.Enabled = false;
                rndWeightedChestDropdown.Enabled = false;
                if (!rndChestToggle.Checked && !rndDropsToggle.Checked && !rndGiftersToggle.Checked && !rndWingsmithsToggle.Checked) {
                    itemListTabs.Visible = false;
                }
            }
            UpdateCode();
        }

        private void rndDropsToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndDropsToggle.Checked) {
                rndDropsDropdown.Enabled = true;
                if (rndDropsDropdown.SelectedIndex != 0) { //make visible again if they've already been using it
                    rndWeightedDropsToggle.Enabled = true;
                    rndWeightedDropsDropdown.Enabled = true;
                }
                itemListTabs.Visible = true;
                itemListTabs.SelectedIndex = 1;
            } else {
                rndDropsDropdown.Enabled = false;
                rndWeightedDropsToggle.Enabled = false;
                rndWeightedDropsDropdown.Enabled = false;
                if (!rndChestToggle.Checked && !rndDropsToggle.Checked && !rndGiftersToggle.Checked && !rndWingsmithsToggle.Checked) {
                    itemListTabs.Visible = false;
                }
            }
            UpdateCode();
        }

        private void rndGiftersToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndGiftersToggle.Checked) {
                rndGiftersDropdown.Enabled = true;
                rndShuffleShannonToggle.Enabled = true;
                itemListTabs.Visible = true;
                itemListTabs.SelectedIndex = 2;
            }
            else {
                rndGiftersDropdown.Enabled = false;
                rndShuffleShannonToggle.Enabled = false;
                if (!rndChestToggle.Checked && !rndDropsToggle.Checked && !rndGiftersToggle.Checked && !rndWingsmithsToggle.Checked) {
                    itemListTabs.Visible = false;
                }
            }
            UpdateCode();
        }

        private void rndWingsmithsToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndWingsmithsToggle.Checked) {
                rndWingsmithsDropdown.Enabled = true;
                itemListTabs.Visible = true;
                itemListTabs.SelectedIndex = 3;
            }
            else {
                rndWingsmithsDropdown.Enabled = false;
                if (!rndChestToggle.Checked && !rndDropsToggle.Checked && !rndGiftersToggle.Checked && !rndWingsmithsToggle.Checked) {
                    itemListTabs.Visible = false;
                }
            }
            UpdateCode();
        }

        private void rndMonsterStatsToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndMonsterStatsToggle.Checked) {
                rndMonsterExpToggle.Enabled = true;
                rndExtremityDropdown.Enabled = true;
                UpdateRisk();
            }
            else {
                if(!rndMonsterScaleToggle.Checked) { rndMonsterExpToggle.Enabled = false; }
                rndExtremityDropdown.Enabled = false;
                rndExtremityDropdown.SelectedIndex = 0;
                extremity = 0;
                UpdateRisk();
            }
            UpdateCode();
        }

        private void rndExtremityDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            //index 0 is 1.0, which is variance scale 0
            extremity = rndExtremityDropdown.SelectedIndex * 0.1f;
            UpdateRisk();
            UpdateCode();
        }

        private void rndSpellNamesToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndSpellNamesToggle.Checked) {
                rndSpellNamesDropdown.Enabled = true;
            }
            else {
                rndSpellNamesDropdown.Enabled = false;
            }
            UpdateCode();
        }

        private void rndColorViewToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndColorViewToggle.Checked) {
                rndColorViewPanel.Visible = true;
            }
            else {
                rndColorViewPanel.Visible = false;
            }
            UpdateCode();
        }

        private void rndChestDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            itemListTabs.SelectedIndex = 0;
            itemListUpdate(itemListView1, rndChestDropdown.SelectedIndex);
            if (rndChestDropdown.SelectedIndex > 0) {
                rndWeightedChestToggle.Enabled = true;
                rndWeightedChestDropdown.Enabled = true;
            }
            else {
                rndWeightedChestToggle.Enabled = false;
                rndWeightedChestDropdown.Enabled = false;
            }
            UpdateCode();
        }

        private void rndDropsDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            itemListTabs.SelectedIndex = 1;
            itemListUpdate(itemListView2, rndDropsDropdown.SelectedIndex);
            if (rndDropsDropdown.SelectedIndex > 0) {
                rndWeightedDropsToggle.Enabled = true;
                rndWeightedDropsDropdown.Enabled = true;
            }
            else {
                rndWeightedDropsToggle.Enabled = false;
                rndWeightedDropsDropdown.Enabled = false;
            }
            UpdateCode();
        }

        private void rndGiftersDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            itemListTabs.SelectedIndex = 2;
            itemListUpdate(itemListView3, rndGiftersDropdown.SelectedIndex);
            if (rndGiftersToggle.Checked) {
                rndShuffleShannonToggle.Enabled = true;
            }
            else {
                rndShuffleShannonToggle.Enabled = false;
            }
            UpdateCode();
        }

        private void rndWingsmithsDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            itemListTabs.SelectedIndex = 3;
            itemListUpdate(itemListView4, rndWingsmithsDropdown.SelectedIndex);
            UpdateCode();
        }

        private void rndDropLimitToggle_CheckedChanged(object sender, EventArgs e) {
            expUpdateWarning();
            UpdateCode();
        }

        private void rndSoulToggle_CheckedChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndLevelToggle_CheckedChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndWeightedChestToggle_CheckedChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndWeightedDropsToggle_CheckedChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndMaxMessageToggle_CheckedChanged(object sender, EventArgs e) {
            expUpdateWarning();
            UpdateCode();
        }

        private void rndZoomToggle_CheckedChanged(object sender, EventArgs e) {

        }

        private void rndAccuracyToggle_CheckedChanged(object sender, EventArgs e) {

        }

        private void rndMonsterScaleToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndMonsterScaleToggle.Checked) {
                rndMonsterExpToggle.Enabled = true;
                rndScalingDropdown.Enabled = true;
                UpdateRisk();
            }
            else {
                if (!rndMonsterStatsToggle.Checked) { rndMonsterExpToggle.Enabled = false; }
                rndScalingDropdown.Enabled = false;
                rndScalingDropdown.SelectedIndex = 5;
                difficultyscale = 1.0f;
                UpdateRisk();
            }
            UpdateCode();
        }

        private void rndScalingDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            //index 5 is 1.0, which is difficulty scale 10
            difficultyscale = 0.5f + (rndScalingDropdown.SelectedIndex * 0.1f);
            UpdateRisk();
            UpdateCode();
        }

        private void rndWingUnlockToggle_CheckedChanged(object sender, EventArgs e) {

        }

        private void rndHUDLockToggle_CheckedChanged(object sender, EventArgs e) {
            expUpdateWarning();
            UpdateCode();
        }

        private void rndHPTrackBar_Scroll(object sender, EventArgs e) {
            rndStartHPValue.Text = rndHPTrackBar.Value.ToString();
            UpdateCode();
        }

        private void rndMPTrackBar_Scroll(object sender, EventArgs e) {
            rndStartMPValue.Text = rndMPTrackBar.Value.ToString();
            UpdateCode();
        }

        private void rndAgiTrackBar_Scroll(object sender, EventArgs e) {
            rndStartAgiValue.Text = rndAgiTrackBar.Value.ToString();
            UpdateCode();
        }

        private void rndDefTrackBar_Scroll(object sender, EventArgs e) {
            rndStartDefValue.Text = rndDefTrackBar.Value.ToString();
            UpdateCode();
        }

        private void rndStartingStatsToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndStartingStatsToggle.Checked) {
                rndStartHPLabel.ForeColor = SystemColors.ControlText;
                rndStartMPLabel.ForeColor = SystemColors.ControlText;
                rndStartAgiLabel.ForeColor = SystemColors.ControlText;
                rndStartDefLabel.ForeColor = SystemColors.ControlText;
                rndStartHPValue.ForeColor = SystemColors.ControlText;
                rndStartMPValue.ForeColor = SystemColors.ControlText;
                rndStartAgiValue.ForeColor = SystemColors.ControlText;
                rndStartDefValue.ForeColor = SystemColors.ControlText;
            }
            else {
                rndStartHPLabel.ForeColor = SystemColors.ControlDark;
                rndStartMPLabel.ForeColor = SystemColors.ControlDark;
                rndStartAgiLabel.ForeColor = SystemColors.ControlDark;
                rndStartDefLabel.ForeColor = SystemColors.ControlDark;
                rndStartHPValue.ForeColor = SystemColors.ControlDark;
                rndStartMPValue.ForeColor = SystemColors.ControlDark;
                rndStartAgiValue.ForeColor = SystemColors.ControlDark;
                rndStartDefValue.ForeColor = SystemColors.ControlDark;
            }
            rndHPTrackBar.Enabled = rndStartingStatsToggle.Checked;
            rndMPTrackBar.Enabled = rndStartingStatsToggle.Checked;
            rndAgiTrackBar.Enabled = rndStartingStatsToggle.Checked;
            rndDefTrackBar.Enabled = rndStartingStatsToggle.Checked;
            expUpdateWarning();
            UpdateCode();
        }

        private void rndElement99Toggle_CheckedChanged(object sender, EventArgs e) {
            expUpdateWarning();
            UpdateCode();
        }

        private void rndMPRegainToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndMPRegainToggle.Checked) {
                rndMPRegainLabel.ForeColor = SystemColors.ControlText;
                rndMPRegainValue.ForeColor = SystemColors.ControlText;
            }
            else {
                rndMPRegainLabel.ForeColor = SystemColors.ControlDark;
                rndMPRegainValue.ForeColor = SystemColors.ControlDark;
            }
            rndMPRegainTrackBar.Enabled = rndMPRegainToggle.Checked;
            expUpdateWarning();
            UpdateCode();
        }

        private void rndMPRegainTrackBar_Scroll(object sender, EventArgs e) {
            if (rndMPRegainTrackBar.Value == 7) {
                rndMPRegainValue.Text = "OFF";
            }
            if (rndMPRegainTrackBar.Value < 10 && rndMPRegainTrackBar.Value > 7) {
                rndMPRegainValue.Text = "1/" + (11 - rndMPRegainTrackBar.Value).ToString() + "x";
            }
            if (rndMPRegainTrackBar.Value >= 10) {
                rndMPRegainValue.Text = (rndMPRegainTrackBar.Value - 9).ToString() + "x";
            }
            UpdateCode();
        }

        private void rndShuffleShannonToggle_CheckedChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndWeightedChestDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndWeightedDropsDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndMonsterExpToggle_CheckedChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndInvalidityToggle_CheckedChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndAccuracyDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndWingUnlockDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndFastMonasteryToggle_CheckedChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndFastMammonToggle_CheckedChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndRestlessToggle_CheckedChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndVowelsToggle_CheckedChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndTextContentDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndZoomDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        //ITEM - Randomizer granular item controls

        private void itemListView1_ItemChecked(object sender, ItemCheckedEventArgs e) {
            if (!lockItemUpdates) {
                rndChestDropdown.SelectedIndex = 8;
                if (e.Item.Checked) { rndChestToggle.Checked = true; }
                UpdateCode();
            }
        }

        private void itemListView2_ItemChecked(object sender, ItemCheckedEventArgs e) {
            if (!lockItemUpdates) {
                rndDropsDropdown.SelectedIndex = 8;
                if (e.Item.Checked) { rndDropsToggle.Checked = true; }
                UpdateCode();
            }
        }

        private void itemListView3_ItemChecked(object sender, ItemCheckedEventArgs e) {
            if (!lockItemUpdates) {
                rndGiftersDropdown.SelectedIndex = 8;
                if (e.Item.Checked) { rndGiftersToggle.Checked = true; }
                UpdateCode();
            }
        }

        private void itemListView4_ItemChecked(object sender, ItemCheckedEventArgs e) {
            if (!lockItemUpdates) {
                rndWingsmithsDropdown.SelectedIndex = 8;
                if (e.Item.Checked) { rndWingsmithsToggle.Checked = true; }
                UpdateCode();
            }
        }

        //EXP - Randomizer export

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
            if (!writingPatch) { BuildPatch(); }
            if (writingPatch) { rndErrorLabel.Text = "Patch/File currently writing."; }
        }

        private void expVerboseCheckBox_CheckedChanged(object sender, EventArgs e) {
            verboselog = expVerboseCheckBox.Checked;
        }

        private void expSeedNameToggle_CheckedChanged(object sender, EventArgs e) {
            seedName = expSeedNameToggle.Checked;
        }

        private void expDatetimeToggle_CheckedChanged(object sender, EventArgs e) {
            dateName = expDatetimeToggle.Checked;
        }

        private void expModePatchZ64_CheckedChanged(object sender, EventArgs e) {
            expModeSet();
        }

        private void expModePatchIPS_CheckedChanged(object sender, EventArgs e) {
            expModeSet();
        }

        public void expModeSet() {
            if (expModePatchZ64.Checked) {
                expSelectButton.Enabled = true;
                expFakeZ64Button.Visible = false;
                expUpdateWarning();
                rndFileBytes = null;
                rndFileSelected = false;
                patcharray = new byte[0];
                patchstrings.Clear();
            }
            if (expModePatchIPS.Checked) {
                expSelectButton.Enabled = false;
                expFakeZ64Button.Visible = true;
                expUpdateWarning();
                rndFileBytes = null;
                rndFileSelected = false;
                patcharray = new byte[0];
                patchstrings.Clear();
            }
        }

        public void expUpdateWarning() {
            rndErrorLabel.Text = "";

            if (expModePatchZ64.Checked) { rndErrorLabel.Text = "No file loaded."; }
            if (expModePatchIPS.Checked) {
                if (rndZoomToggle.Checked ||
                rndMaxMessageToggle.Checked ||
                rndDropLimitToggle.Checked ||
                rndWingUnlockToggle.Checked ||
                rndHUDLockToggle.Checked ||
                rndStartingStatsToggle.Checked ||
                rndElement99Toggle.Checked ||
                rndMPRegainToggle.Checked) 
                { rndErrorLabel.Text = rndErrorString; }
            }
        }

        private void expSelectButton_Click(object sender, EventArgs e) {
            if (!writingPatch) {
                if (rndOpenFileDialog.ShowDialog() == DialogResult.OK) {
                    try {
                        rndFileBytes = File.ReadAllBytes(rndOpenFileDialog.FileName);
                        fullPath = rndOpenFileDialog.FileName;
                        rndFileName = Path.GetFileNameWithoutExtension(fullPath);
                        rndErrorLabel.Text = "File loaded: " + rndOpenFileDialog.FileName + ".";
                        rndFileSelected = true;
                    }
                    catch (SecurityException ex) {
                        MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                        $"Details:\n\n{ex.StackTrace}");
                        rndFileSelected = false;
                    }
                }
            }
            if (writingPatch) { rndErrorLabel.Text = "Patch/File currently writing."; }
        }

        //ADV - Generic patch generator

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

        //BIN - Binary file reader

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

        //CRC - Checksum repair tool

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

        private void rndSpellDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndSpellNamesDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndShortcodeText_TextChanged(object sender, EventArgs e) {

        }
    }
}
