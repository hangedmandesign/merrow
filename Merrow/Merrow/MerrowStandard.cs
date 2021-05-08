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
        bool updatingcode = false;

        //collection arrays and lists
        byte[] patcharray;
        int[] shuffles = new int[60];
        List<int> reorg = new List<int>();
        int[] spellitemID = { 58, 52, 38, 57, 17, 22 };
        int[] defaultspellitemrules = {  };
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
        string[] spoilerbossdrops = new string[6];
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
        int[] newbossorder = new int[6]; //V30: changed to <6 to exclude beigis
        int[] newbosselem = { 4, 4 };

        //INITIALIZATION----------------------------------------------------------------

        public MerrowStandard() {
            //required Winforms initialization, do not edit or remove
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

            //initiate spell list with SHUFFLED option ahead of the individual ones
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
            for (int i = 0; i < 6; i++) { newbossorder[i] = i; } //V30: changed to <6 to exclude beigis

            //fix item list order, because we had to change it. even though they look wrong in editor this fixes them on run
            for (int i = 0; i < 26; i++) {
                itemListView1.Items[i].ImageIndex = i;
                itemListView2.Items[i].ImageIndex = i;
                itemListView3.Items[i].ImageIndex = i;
                itemListView4.Items[i].ImageIndex = i;
            }

            //initiate UI elements
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

            //fill the reference from arrays
            PopulateReference();

            //initial randomization
            rngseed = SysRand.Next(100000000, 1000000000); //default seed set to a random 9-digit number
            expSeedTextBox.Text = rngseed.ToString();
            loadfinished = true; //loadfinished being false prevents some UI elements from taking incorrect action during the initial setup
            Shuffling(true);
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
            if (!crashpro) {
                for (int i = 0; i < playerspells; i++) {
                    shuffles[i] = reorg[i];
                }
            }

            //crash protection enabled
            if (crashpro) {
                bool step = false;

                //early healing 
                if (rndSpellToggle.Checked && rndEarlyHealingToggle.Checked) {
                    for (int i = 0; i < playerspells; i++) {
                        library.crashlock[(i * playerspells) + 32] = library.earlyhealingmodifier[i];
                    }
                }

                //no early healing, fix the array in case it's been changed
                if (!rndEarlyHealingToggle.Checked) {
                    for (int i = 0; i < playerspells; i++) {
                        library.crashlock[(i * playerspells) + 32] = library.noearlyhealing[i];
                    }
                }

                //spell distribution
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

                //using world-only spell items in battle (or vice versa?) can cause issues up to softlocks.
                //spell items should have updated rules based on what they are modified to.
                //silent flute - silence 2 - id58
                //celine's bell - restriction 2 - id52
                //replica - escape - id38
                //giant's shoes - wind walk - id57
                //silver amulet - spirit armor 1 - id17
                //golden amulet - spirit armor 2 - id22

                //i need to check the modifier spells, since their default rules are being transplanted

                //item softlock protection
                for (int i = 0; i < 6; i++) {
                    newitemspells[i] = shuffles[spellitemID[i]];
                    string rule = library.spells[(newitemspells[i] * 4) + 3].Substring(6,2);

                    if (rule == "12" || rule == "03") {
                        if (rule == "12") { //out of battle only (exit, return)
                            itemspellfix[i] = 1;
                        }
                        if (rule == "03") { //either (healing)
                            itemspellfix[i] = 2;
                        }
                    }
                    else { //anything else battle only
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

            //RANDOM CHESTS

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
                        if (c >= setlength && c < setlength * 2) { //double up
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

            //chest shuffling, happens for both random/shuffle options
            int d = chests.Length;
            while (d > 1) {
                d--;
                k = SysRand.Next(d + 1);
                int temp = chests[k];
                chests[k] = chests[d];
                chests[d] = temp;
            }

            //RANDOM DROPS

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
                        if (c >= setlength && c < setlength * 2) { //double up
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

            //drop shuffling, happens for both random/shuffle options
            d = drops.Length;
            while (d > 1) {
                d--;
                k = SysRand.Next(d + 1);
                int temp = drops[k];
                drops[k] = drops[d];
                drops[d] = temp;
            }

            //RANDOM GIFTS

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

            //gift shuffling, happens for both random/shuffle options
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

            //RANDOM WINGSMITHS

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

            //wing shuffling, happens for both random/shuffle options
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

            //MONSTER STAT AND BOSS ORDER RANDOMIZATION

            //initiate monster stats again, in case this is happening for the nth time
            for (int i = 0; i < 450; i++) { newmonsterstats[i] = library.monsterstatvanilla[i]; }
            for (int i = 0; i < 6; i++) { newbossorder[i] = i; } //V30: changed to <6 to exclude beigis
            for (int i = 0; i < 2; i++) { newbosselem[i] = 4; }

            if (rndBossOrderToggle.Checked) {
                //shuffle boss order array

                d = newbossorder.Length;
                while (d > 1) {
                    d--;
                    k = SysRand.Next(d + 1);
                    int temp = newbossorder[k];
                    newbossorder[k] = newbossorder[d];
                    newbossorder[d] = temp;
                }
            }

            if (rndBossElementToggle.Checked) {
                newbosselem[0] = SysRand.Next(0, 4); //roll Guilty element
                newmonsterstats[437] = newbosselem[0]; //assign into array for spoiler log

                newbosselem[1] = SysRand.Next(0, 4); //roll Mammon element
                newmonsterstats[449] = newbosselem[1]; //assign into array for spoiler log
            }

            //As of V30, I'm testing a new setup where I'm halving the effect of variance. Right now it's wayyy too wide, even with low values, due to existing region variances.

            //if monster stat randomization is active
            if (rndMonsterStatsToggle.Checked) {
                for (int i = 16; i > -1; i--) { //16 areas, no bosses
                    int locals = library.mon_enemycount[i];
                    int locale = library.mon_locationsindex[i];
                    for (int j = 0; j < locals; j++) { //steps through each area's monster set one by one
                        int currentmonster = library.mon_locations[locale + j];
                        for (int m = 0; m < 5; m++) { //sets each of their 5 new stats, based on area average, variance, and difficulty modifier
                            double currentstat = library.avg_monster[(i * 7) + m];
                            double highend = ((library.avg_monster[(i * 7) + 5] + 10) / 2) * (1 + extremity); //+10/2 halves variance
                            double lowend = ((library.avg_monster[(i * 7) + 6] + 10) / 2) * (1 - extremity); //+10/2 halves variance
                            double variance = SysRand.NextDouble() * (highend - lowend) + lowend;
                            double modifiedstat = (currentstat * (variance / 10) * (difficultyscale));
                            newmonsterstats[(currentmonster * 6) + m] = (int)Math.Round(modifiedstat);
                            if (newmonsterstats[(currentmonster * 6) + m] == 0) { newmonsterstats[(currentmonster * 6) + m] = 1; }
                        }
                    }
                }
            }

            //boss randomization is done separately, ranges are hard-coded rn based on local area
            //variance is thirded on bosses to restrain balance, since higher values mean wilder varied numbers
            if (rndMonsterStatsToggle.Checked || rndBossOrderToggle.Checked) {
                for (int i = 0; i < 8; i++) {
                    int currentmonster = 67 + i;
                    int region = library.boss_regions[i];
                    for (int m = 0; m < 5; m++) { //sets each of their 5 new stats, based on area average, variance, and difficulty modifier
                        double currentstat = library.monsterstatvanilla[currentmonster * 6 + m];

                        //new method for ATK/DEF/AGI based on BST calc. i<7 check because it doesn't apply to mammon.
                        //the new stat will be old boss's BST multiplied by the new bosses BST-stat ratio.
                        //V30: changed to i<6 check because it doesn't apply to mammon or beigis.
                        if (m > 0 && m < 4 && i < 6) {
                            currentstat = library.bossbstratios[i * 4] * library.bossbstratios[(newbossorder[i] * 4) + m];
                        }

                        if (rndMonsterStatsToggle.Checked) { 
                            //if randomized
                            double highend = ((library.avg_monster[(region * 7) + 5] + 20) / 3) * (1 + extremity); //+20/3 thirds variance
                            double lowend = ((library.avg_monster[(region * 7) + 6] + 20) / 3) * (1 - extremity); //+20/3 thirds variance
                            double variance = SysRand.NextDouble() * (highend - lowend) + lowend;
                            double modifiedstat = (currentstat * (variance / 10) * (difficultyscale));
                            newmonsterstats[(currentmonster * 6) + m] = (int)Math.Round(modifiedstat);
                            if (newmonsterstats[(currentmonster * 6) + m] == 0) { newmonsterstats[(currentmonster * 6) + m] = 1; }
                        }
                        else { 
                            //if unrandomized, just redistribute the BST-adjusted stats
                            newmonsterstats[(currentmonster * 6) + m] = (int)Math.Round(currentstat);
                            if (newmonsterstats[(currentmonster * 6) + m] == 0) { newmonsterstats[(currentmonster * 6) + m] = 1; }
                        }
                    }
                }
            }

            //scale all if monster stat randomization isn't active but scaling is
            if (!rndMonsterStatsToggle.Checked && rndMonsterScaleToggle.Checked) {
                for (int i = 0; i < 75; i++) {
                    for (int j = 0; j < 5; j++) {
                        newmonsterstats[(i * 6) + j] = (int)Math.Round(newmonsterstats[(i * 6) + j] * (difficultyscale));
                    }
                }
            }

            //adjust EXP by BST
            if (rndMonsterExpToggle.Checked) {

                //don't bother doing this if bosses are shuffled but no scaling/randomization has happened
                if (rndMonsterScaleToggle.Checked || rndMonsterStatsToggle.Checked) { 
                    for (int i = 17; i > -1; i--) { //17: 16 areas and bosses
                        int locals = library.mon_enemycount[i];
                        int locale = library.mon_locationsindex[i];

                        //if boss order isn't checked, don't bother recalculating and writing bosses

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

        //MERROW CUSTOM FUNCTIONS--------------------------------------------------------------------
        //Anything that isn't directly tied to randomization (above this section) and Winforms interactions (below).

        //simplified CRC function call to return error messages
        public int RepairCRC() {
            int check = -1;
            if (crcFileSelected) { check = fix_crc(fullPath); }
            return check;
        }

        //update risk value
        public void UpdateRisk() {
            float variance = extremity + 1.1f;
            if (difficultyscale > 1.0) {
                riskvalue = Math.Round((difficultyscale * difficultyscale * difficultyscale) * (variance * variance) * 10);
            }
            if (difficultyscale <= 1.0) {
                riskvalue = Math.Round((difficultyscale * difficultyscale) * (variance * variance) * 10);
            }
            if (rndBossOrderToggle.Checked) { riskvalue *= 1.2; }
            if (rndBossElementToggle.Checked) { riskvalue *= 0.9; }
            if (rndInvalidityToggle.Checked) { riskvalue *= 0.9; }
            if (!rndMonsterExpToggle.Checked) { riskvalue *= 1.2; }

            if (!rndMonsterScaleToggle.Checked &&
                !rndMonsterStatsToggle.Checked &&
                !rndBossOrderToggle.Checked &&
                !rndBossElementToggle.Checked &&
                !rndInvalidityToggle.Checked) {

                rndRiskLabel.Visible = false;
                rndRiskLabelText.Visible = false;
            }
            if (rndMonsterScaleToggle.Checked ||
                rndMonsterStatsToggle.Checked ||
                rndBossOrderToggle.Checked ||
                rndBossElementToggle.Checked ||
                rndInvalidityToggle.Checked) {

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

        //populate Quest reference
        public void PopulateReference() {
            //spell reference
            for (int i = 2; i < 6; i++) {
                if (i != 3) { spellsDataGridView.Columns[i].ValueType = typeof(int); }
            }

            for (int i = 0; i < 60; i++) {
                spellsDataGridView.Rows.Add();
                for (int j = 0; j < 6; j++) {
                    if (j == 0) {
                        spellsDataGridView.Rows[i].Cells[j].Value = library.spelldatatable[i * 6 + j].ToUpper();
                        if (i < 15) { spellsDataGridView.Rows[i].Cells[j].Style.BackColor = Color.MistyRose; }
                        if (i >= 15 && i < 30) { spellsDataGridView.Rows[i].Cells[j].Style.BackColor = Color.Bisque; }
                        if (i >= 30 && i < 45) { spellsDataGridView.Rows[i].Cells[j].Style.BackColor = Color.Azure; }
                        if (i >= 45) { spellsDataGridView.Rows[i].Cells[j].Style.BackColor = Color.Honeydew; }
                    }
                    if (j == 1) {
                        if (i < 15) {
                            spellsDataGridView.Rows[i].Cells[j].Value = itemImageList.Images[27];
                            spellsDataGridView.Rows[i].Cells[j].Style.BackColor = Color.MistyRose;
                        }
                        if (i >= 15 && i < 30) {
                            spellsDataGridView.Rows[i].Cells[j].Value = itemImageList.Images[28];
                            spellsDataGridView.Rows[i].Cells[j].Style.BackColor = Color.Bisque;
                        }
                        if (i >= 30 && i < 45) {
                            spellsDataGridView.Rows[i].Cells[j].Value = itemImageList.Images[29];
                            spellsDataGridView.Rows[i].Cells[j].Style.BackColor = Color.Azure;
                        }
                        if (i >= 45) {
                            spellsDataGridView.Rows[i].Cells[j].Value = itemImageList.Images[30];
                            spellsDataGridView.Rows[i].Cells[j].Style.BackColor = Color.Honeydew;
                        }
                    }
                    if (j == 2 || j >= 4) {
                        spellsDataGridView.Rows[i].Cells[j].Value = Convert.ToInt32(library.spelldatatable[i * 6 + j]);
                    }
                    if (j == 3) {
                        spellsDataGridView.Rows[i].Cells[j].Value = library.spelldatatable[i * 6 + j];
                    }
                }
            }

            //monster reference
            for (int i = 1; i < 6; i++) {
                monsterDataGridView.Columns[i].ValueType = typeof(int);
            }

            for (int i = 0; i < 75; i++) {
                monsterDataGridView.Rows.Add();
                for (int j = 0; j < 10; j++) {
                    if (j == 0) {
                        monsterDataGridView.Rows[i].Cells[j].Value = library.monsterdatatable[i * 10 + j];
                    }
                    if (j == 6) {
                        //monsterDataGridView.Rows[i].Cells[j].Value = library.monsterdatatable[i * 10 + j].Substring(0,2);
                        if (library.monsterdatatable[i * 10 + j] == "FIRE") {
                            monsterDataGridView.Rows[i].Cells[j].Value = itemImageList.Images[27];
                            monsterDataGridView.Rows[i].Cells[j].Style.BackColor = Color.MistyRose;
                            monsterDataGridView.Rows[i].Cells[0].Style.BackColor = Color.MistyRose;
                        }
                        if (library.monsterdatatable[i * 10 + j] == "EARTH") {
                            monsterDataGridView.Rows[i].Cells[j].Value = itemImageList.Images[28];
                            monsterDataGridView.Rows[i].Cells[j].Style.BackColor = Color.Bisque;
                            monsterDataGridView.Rows[i].Cells[0].Style.BackColor = Color.Bisque;
                        }
                        if (library.monsterdatatable[i * 10 + j] == "WATER") {
                            monsterDataGridView.Rows[i].Cells[j].Value = itemImageList.Images[29];
                            monsterDataGridView.Rows[i].Cells[j].Style.BackColor = Color.Azure;
                            monsterDataGridView.Rows[i].Cells[0].Style.BackColor = Color.Azure;
                        }
                        if (library.monsterdatatable[i * 10 + j] == "WIND") {
                            monsterDataGridView.Rows[i].Cells[j].Value = itemImageList.Images[30];
                            monsterDataGridView.Rows[i].Cells[j].Style.BackColor = Color.Honeydew;
                            monsterDataGridView.Rows[i].Cells[0].Style.BackColor = Color.Honeydew;
                        }
                        if (library.monsterdatatable[i * 10 + j] == "DARK") {
                            monsterDataGridView.Rows[i].Cells[j].Value = itemImageList.Images[26];
                            monsterDataGridView.Rows[i].Cells[j].Style.BackColor = Color.Thistle;
                            monsterDataGridView.Rows[i].Cells[0].Style.BackColor = Color.Thistle;
                        }
                    }
                    if (j >= 1 && j <= 5) {
                        monsterDataGridView.Rows[i].Cells[j].Value = Convert.ToInt32(library.monsterdatatable[i * 10 + j]);
                    }
                    if (j == 7) {
                        if (library.dropdata[i * 2 + 1] == 255) { monsterDataGridView.Rows[i].Cells[j].Value = itemImageList.Images[26]; }
                        if (library.dropdata[i * 2 + 1] != 255) {
                            monsterDataGridView.Rows[i].Cells[j].Value = itemImageList.Images[library.dropdata[i * 2 + 1]];
                            monsterDataGridView.Rows[i].Cells[j].ToolTipText = library.items[library.dropdata[i * 2 + 1] * 3];
                        }
                    }
                    if (j >= 8) {
                        string spellcode = library.monsterdatatable[i * 10 + j];
                        int spellelement = 0;
                        int spellvalue = 0;

                        if (spellcode == "M") { monsterDataGridView.Rows[i].Cells[j].Value = "MELEE"; }
                        if (spellcode == "X") { monsterDataGridView.Rows[i].Cells[j].Value = "-"; }
                        if (spellcode != "M" && spellcode != "X") {
                            spellelement = Convert.ToInt32(spellcode.Substring(1, 1)); //just get second value, it's element index
                            spellvalue = Convert.ToInt32(spellcode.Substring(2), 16); //convert 3rd/4th to hex, to int

                            monsterDataGridView.Rows[i].Cells[j].Value = library.spelldatatable[((15 * spellelement) + spellvalue) * 6].ToUpper();
                        }
                    }

                }
            }
            monsterDataGridView.Rows[63].Cells[8].Value += "*"; //adding third attack notes to Judgment
            monsterDataGridView.Rows[63].Cells[8].ToolTipText = "JUDGMENT has a third attack: FIRE PILLAR";
            monsterDataGridView.Rows[63].Cells[9].Value += "*";
            monsterDataGridView.Rows[63].Cells[9].ToolTipText = "JUDGMENT has a third attack: FIRE PILLAR";


            //hinted name reference
            for (int i = 0; i < 30; i++) {
                hintedDataGridView.Rows.Add();
                for (int j = 0; j < 3; j++) {
                    if (j == 0) { //grab spell name
                        hintedDataGridView.Rows[i].Cells[j].Value = library.spelldatatable[i * 6].ToUpper();
                        if (i < 15) { hintedDataGridView.Rows[i].Cells[j].Style.BackColor = Color.MistyRose; }
                        if (i >= 15) { hintedDataGridView.Rows[i].Cells[j].Style.BackColor = Color.Bisque; }
                    }
                    if (j > 0) { //grab 0,1 hints
                        hintedDataGridView.Rows[i].Cells[j].Value = library.shuffleNames[(i * 5) + (j - 1)];
                    }
                }

                for (int j = 0; j < 3; j++) {
                    if (j == 0) { //grab spell name
                        hintedDataGridView.Rows[i].Cells[j + 3].Value = library.spelldatatable[(i + 30) * 6].ToUpper();
                        if (i < 15) { hintedDataGridView.Rows[i].Cells[j + 3].Style.BackColor = Color.Azure; }
                        if (i >= 15) { hintedDataGridView.Rows[i].Cells[j + 3].Style.BackColor = Color.Honeydew; }
                    }
                    if (j > 0) { //grab 0,1 hints
                        hintedDataGridView.Rows[i].Cells[j + 3].Value = library.shuffleNames[(i + 30) * 5 + (j - 1)];
                    }
                }
            }

            //hpmp reference
            for (int i = 0; i < 57; i++) {
                hpmpDataGridView.Rows.Add();
                for (int j = 0; j < 6; j++) {
                    hpmpDataGridView.Rows[i].Cells[j].Value = library.hpmpdatatable[i * 6 + j];
                }
            }

            //agidef reference
            for (int i = 0; i < 57; i++) {
                agidefDataGridView.Rows.Add();
                for (int j = 0; j < 6; j++) {
                    agidefDataGridView.Rows[i].Cells[j].Value = library.agidefdatatable[i * 6 + j];
                }
            }

            //combat level reference
            for (int i = 0; i < 49; i++) {
                combatDataGridView.Rows.Add();
                for (int j = 0; j < 6; j++) {
                    combatDataGridView.Rows[i].Cells[j].Value = library.combatlvldatatable[i * 6 + j];
                }
            }
        }



        //WINFORMS UI INTERACTIONS-------------------------------------------------------------------
        //These declarations should not be manually edited without care, as they're partially auto-generated by the Winforms Designer.
        //The content can be edited as needed. They're kept here for logical sake, because this is the :Form type function.

        //GENERAL FUNCTIONS

        private void tabsControl_SelectedIndexChanged(object sender, EventArgs e) {
            binErrorLabel.Visible = false;
            binFileLoaded = false;
            binFileBytes = null;
            crcErrorLabel.Visible = false;
            crcFileSelected = false;
            if (tabsControl.SelectedIndex > 2) { helpLabel.Visible = false; }
            else { helpLabel.Visible = true; }
            expModeSet();
        }

        private void terms__LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/hangedmandesign/merrow");
        }

        //RND - Randomizer functions

        private void rndSpellToggle_CheckedChanged(object sender, EventArgs e) {
            if(rndSpellToggle.Checked) {
                rndSpellDropdown.Enabled = true;
                rndSpellNamesToggle.Enabled = true;
                rndEarlyHealingToggle.Enabled = true;
                if (rndSpellNamesToggle.Checked) { rndSpellNamesDropdown.Enabled = true; }
            } else {
                rndSpellDropdown.Enabled = false;
                rndSpellNamesToggle.Enabled = false;
                rndSpellNamesDropdown.Enabled = false;
                rndEarlyHealingToggle.Enabled = false;
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
            rndZoomDropdown.Enabled = rndZoomToggle.Checked;
            expUpdateWarning();
            UpdateCode();
        }

        private void rndAccuracyToggle_CheckedChanged(object sender, EventArgs e) {
            rndAccuracyDropdown.Enabled = rndAccuracyToggle.Checked;
            UpdateCode();
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
            rndWingUnlockDropdown.Enabled = rndWingUnlockToggle.Checked;
            expUpdateWarning();
            UpdateCode();
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
            UpdateRisk();
            UpdateCode();
        }

        private void rndInvalidityToggle_CheckedChanged(object sender, EventArgs e) {
            UpdateRisk();
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

        private void rndSpellDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndSpellNamesDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndShortcodeText_TextChanged(object sender, EventArgs e) {
            if (!updatingcode && loadfinished) { //if the code is not auto-updated, ie only when user input happens
                int errorcode = ApplyCode();

                if (errorcode == 0) {
                    updatingcode = true;
                    rndShortcodeText.Text = "";
                    updatingcode = false;
                }
            }
        }

        private void rndHPTrackBar_ValueChanged(object sender, EventArgs e) {
            rndStartHPValue.Text = rndHPTrackBar.Value.ToString();
            UpdateCode();
        }

        private void rndMPTrackBar_ValueChanged(object sender, EventArgs e) {
            rndStartMPValue.Text = rndMPTrackBar.Value.ToString();
            UpdateCode();
        }

        private void rndAgiTrackBar_ValueChanged(object sender, EventArgs e) {
            rndStartAgiValue.Text = rndAgiTrackBar.Value.ToString();
            UpdateCode();
        }

        private void rndDefTrackBar_ValueChanged(object sender, EventArgs e) {
            rndStartDefValue.Text = rndDefTrackBar.Value.ToString();
            UpdateCode();
        }

        private void rndMPRegainTrackBar_ValueChanged(object sender, EventArgs e) {
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

        private void rndDriftToggle_CheckedChanged(object sender, EventArgs e) {
            expUpdateWarning();
            UpdateCode();
        }

        private void rndBossOrderToggle_CheckedChanged(object sender, EventArgs e) {
            UpdateRisk();
            UpdateCode();
        }

        private void rndEarlyHealingToggle_CheckedChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndBossElementToggle_CheckedChanged(object sender, EventArgs e) {
            UpdateRisk();
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
                rndMPRegainToggle.Checked ||
                rndDriftToggle.Checked) 
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
    }
}
