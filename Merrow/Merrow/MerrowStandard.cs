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
        string rndErrorString = "WARNING: Current patch options will cause checksum errors." + Environment.NewLine + "Use the CRC REPAIR TOOL below to fix your patched rom.";
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
        string[] spoilerbossdrops = new string[7];
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
        int lasttextpaletteoffset = 0;
        int laststaffpaletteoffset = 0;
        int[] lostkeysbossitemlist = { 255, 255, 255, 255, 255, 255, 255 };
        int[] lostkeysdrops = new int[67];
        string staffPaletteHex = "";
        int[] rndspellcolours = new int[79];
        int[] rndbgms = new int[73];
        bool lightdark;
        double hueOffset;
        bool lightdarkicon = true;

        //INITIALIZATION----------------------------------------------------------------

        public MerrowStandard() {
            //required Winforms initialization, do not edit or remove
            InitializeComponent();

            //initial randomization
            SysRand = new Random(); //reinitializing because otherwise seed always produces same value, possibly due to order error.
            rngseed = SysRand.Next(100000000, 1000000000); //default seed set to a random 9-digit number
            expSeedTextBox.Text = rngseed.ToString();

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
            for (int j = 0; j < chests.Length; j++) { chests[j] = library.chestdata[j * 4 + 1]; }

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
                itemListView1.Items[i].Checked = false;
                itemListView2.Items[i].Checked = false;
                itemListView3.Items[i].Checked = false;
                itemListView4.Items[i].Checked = false;
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
            rndLostKeysDropdown.SelectedIndex = 0;
            rndStaffPaletteDropdown.SelectedIndex = 0;
            expFakeZ64Button.Size = new System.Drawing.Size(110, 78);

            //fill the reference from arrays
            PopulateReference();

            loadfinished = true; //loadfinished being false prevents some UI elements from taking incorrect action during the initial setup
            UpdateCode();
            Shuffling(true);
        }

        //MERROW CUSTOM FUNCTIONS--------------------------------------------------------------------
        //Anything that isn't directly tied to randomization (in Shuffle.cs) and Winforms interactions (below).

        //simplified CRC function call to return error messages
        public int RepairCRC() {
            int check = -1;
            if (crcFileSelected) { check = fix_crc(fullPath); }
            return check;
        }

        //update risk value
        public void UpdateRisk() {
            float variance = extremity + 1.2f; //slight scale up on randomness, cause it can easily be more punishing
            if (difficultyscale >= 1.0) { //scaling matters more than variance, above 1.0
                riskvalue = Math.Round((difficultyscale * difficultyscale * difficultyscale) * (variance * variance) * 10);
            }
            if (difficultyscale < 1.0) {
                riskvalue = Math.Round((difficultyscale * difficultyscale) * (variance * variance) * 10);
            }
            if (rndBossOrderToggle.Checked) { riskvalue *= 1.2; } //late solvaring WILL murder you
            if (rndBossElementToggle.Checked) { riskvalue *= 0.9; } //makes Guilty easier
            if (rndInvalidityToggle.Checked) { riskvalue *= 0.9; } //makes every boss easier
            if (!rndMonsterExpToggle.Checked) { //makes higher difficulties easier and lower ones harder
                if (difficultyscale >= 1.0) { riskvalue *= 0.9; }
                if (difficultyscale < 1.0) { riskvalue *= 1.1; }
            } 

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

                if (riskvalue >= 2 && riskvalue < 8) {
                    rndRiskLabel.Text = "RISK " + riskvalue.ToString("N0") + " (BREEZE)";
                    rndRiskLabelText.Text = "Smooth and easy";
                }
                if (riskvalue >= 8 && riskvalue < 14) {
                    rndRiskLabel.Text = "RISK " + riskvalue.ToString("N0") + " (MODERATE)";
                    rndRiskLabelText.Text = "Roughly vanilla";
                }
                if (riskvalue >= 14 && riskvalue < 25) {
                    rndRiskLabel.Text = "RISK " + riskvalue.ToString("N0") + " (GALE)";
                    rndRiskLabelText.Text = "Difficult without grinding";
                }
                if (riskvalue >= 25 && riskvalue < 41) {
                    rndRiskLabel.Text = "RISK " + riskvalue.ToString("N0") + " (STORM)";
                    rndRiskLabelText.Text = "Extremely challenging and grindy";
                }
                if (riskvalue >= 41 && riskvalue < 80) {
                    rndRiskLabel.Text = "RISK " + riskvalue.ToString("N0") + " (HURRICANE)";
                    rndRiskLabelText.Text = "Possibly impossible";
                }
                if (riskvalue >= 80) {
                    rndRiskLabel.Text = "RISK " + riskvalue.ToString("N0") + " (SUPERCELL)";
                    rndRiskLabelText.Text = "Mammon approaches";
                }

                rndRiskLabel.Visible = true;
                rndRiskLabelText.Visible = true;
            }
        }

        //populate Quest reference
        public void PopulateReference() {
            //spell reference
            for (int i = 2; i < 6; i++) { //set the integer columns
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

            //adding third attack notes to Judgment
            monsterDataGridView.Rows[63].Cells[8].Value += "*"; 
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
                rndTextViewToggle.Enabled = true; 
            } else {
                rndTextPaletteDropdown.Enabled = false;
                rndTextViewPanel.Visible = false;
                rndTextViewToggle.Enabled = false;
                rndTextViewToggle.Checked = false; //hide again to avoid spoilers if things change
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
            if (rndTextViewToggle.Checked) {
                rndTextViewToggle.Text = "View random colours:";
                Shuffling(true); //if you don't do this, the colour doesn't update the first time, despite my best efforts
                rndTextViewPanel.Visible = true;
            }
            else {
                rndTextViewToggle.Text = "View random colours";
                rndTextViewPanel.Visible = false;
            }
            UpdateCode();
        }

        private void rndTextPaletteDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            if (rndTextPaletteDropdown.SelectedIndex == 0) { rndTextViewToggle.Checked = false; }
            if (rndTextPaletteDropdown.SelectedIndex == 1) { rndTextViewToggle.Checked = true; }
            if (rndTextPaletteDropdown.SelectedIndex >= 2) { rndTextViewToggle.Checked = false; }
            UpdateCode();
            Shuffling(true);
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
                if(!rndLostKeysToggle.Checked) { rndShuffleShannonToggle.Enabled = true; }
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
            rndSpellNamesDropdown.Enabled = rndSpellNamesToggle.Checked;
            UpdateCode();
        }

        private void rndColorViewToggle_CheckedChanged(object sender, EventArgs e) {
            rndTextViewPanel.Visible = rndTextViewToggle.Checked;
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
            rndShuffleShannonToggle.Enabled = rndGiftersToggle.Checked;
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

                if (errorcode != 0) {
                    updatingcode = true;
                    rndShortcodeText.Text = "";
                    if (errorcode == 1) { Console.WriteLine("Shortcode too short."); }
                    if (errorcode == 2) { Console.WriteLine("Shortcode incorrect version number."); }
                    if (errorcode == 3) { Console.WriteLine("Shortcode malformatted lengths."); }
                    if (errorcode == 4) { Console.WriteLine("Shortcode malformatted toggle string."); }
                    if (errorcode == 5) { Console.WriteLine("Shortcode malformatted dropdown string."); }
                    if (errorcode == 6) { Console.WriteLine("Shortcode malformatted slider string."); }
                    if (errorcode == 7) { Console.WriteLine("Shortcode malformatted second toggle string."); }
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

        private void rndHitMPTrackBar_Scroll(object sender, EventArgs e) {
            rndHitMPValue.Text = rndHitMPTrackBar.Value.ToString() + "x";
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

        private void rndHitMPToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndHitMPToggle.Checked) {
                rndHitMPLabel.ForeColor = SystemColors.ControlText;
                rndHitMPValue.ForeColor = SystemColors.ControlText;
            } else {
                rndHitMPLabel.ForeColor = SystemColors.ControlDark;
                rndHitMPValue.ForeColor = SystemColors.ControlDark;
            }
            rndHitMPTrackBar.Enabled = rndHitMPToggle.Checked;
            expUpdateWarning();
            UpdateCode();
        }

        private void rndUnlockDoorsToggle_CheckedChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndLevel2Toggle_CheckedChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndLostKeysToggle_CheckedChanged(object sender, EventArgs e) {
            if (loadfinished) { LostKeysHandling(); }
            UpdateCode();
        }

        private void rndLostKeysDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            if (loadfinished) { LostKeysHandling(); }
            UpdateCode();
        }

        private void rndIvoryWingsToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndIvoryWingsToggle.Checked) {
                itemListView1.Items[14].Text = "IW";
                itemListView2.Items[14].Text = "IW";
                itemListView3.Items[14].Text = "IW";
                itemListView4.Items[14].Text = "IW";
                itemListView1.Items[14].ImageIndex = 31;
                itemListView2.Items[14].ImageIndex = 31;
                itemListView3.Items[14].ImageIndex = 31;
                itemListView4.Items[14].ImageIndex = 31;
                library.items[42] = "IVORY WINGS";
                library.itemgranters[20] = 6208279;
                library.granternames[10] = "Lavaar (Shamwood Wingsmith)";
            } else {
                itemListView1.Items[14].Text = "WW";
                itemListView2.Items[14].Text = "WW";
                itemListView3.Items[14].Text = "WW";
                itemListView4.Items[14].Text = "WW";
                itemListView1.Items[14].ImageIndex = 14;
                itemListView2.Items[14].ImageIndex = 14;
                itemListView3.Items[14].ImageIndex = 14;
                itemListView4.Items[14].ImageIndex = 14;
                library.items[42] = "WHITE WINGS";
                library.itemgranters[20] = 4847891;
                library.granternames[10] = "Ingram (Melrode Wingsmith)";
            }
            UpdateCode();
        }

        private void rndFastShamwoodToggle_CheckedChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndLockedEndgameToggle_CheckedChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndBlueHouseWarpToggle_CheckedChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndStaffPaletteToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndStaffPaletteToggle.Checked) {
                rndStaffPaletteDropdown.Enabled = true;
                rndStaffViewToggle.Enabled = true;
            }
            else {
                rndStaffPaletteDropdown.Enabled = false;
                rndStaffViewPanel.Visible = false;
                rndStaffViewToggle.Enabled = false;
                rndStaffViewToggle.Checked = false; //hide again to avoid spoilers if things change
            }
            UpdateCode();
        }

        private void rndSpellPaletteToggle_CheckedChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndSpellPaletteDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateCode();
        }

        private void rndMusicShuffleToggle_CheckedChanged(object sender, EventArgs e) {
            expUpdateWarning();
            UpdateCode();
        }

        private void rndStaffPaletteDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            if (rndStaffPaletteDropdown.SelectedIndex == 0) { rndStaffViewToggle.Checked = false; }
            if (rndStaffPaletteDropdown.SelectedIndex == 1) { rndStaffViewToggle.Checked = true; }
            UpdateCode();
            Shuffling(true);
        }

        private void rndStaffViewToggle_CheckedChanged(object sender, EventArgs e) {
            rndStaffViewPanel.Visible = rndStaffViewToggle.Checked;
            UpdateCode();
        }

        private void rndTextViewTextbox_TextChanged(object sender, EventArgs e) {
            var textboxSender = (TextBox)sender; //restricts to numeric only
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, "[^0-9]", "");
            textboxSender.SelectionStart = cursorPosition;
            if (rndTextViewTextbox.Text != "" && rndTextViewTextbox.Text != null && loadfinished) {
                int currval = Convert.ToInt32(rndTextViewTextbox.Text);
                if (currval > 359) { currval = 359; rndTextViewTextbox.Text = "359"; }
                lasttextpaletteoffset = currval;
                Shuffling(true);
            }
        }

        private void rndStaffViewTextbox_TextChanged(object sender, EventArgs e) {
            var textboxSender = (TextBox)sender; //restricts to numeric only
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, "[^0-9]", "");
            textboxSender.SelectionStart = cursorPosition;
            if (rndStaffViewTextbox.Text != "" && rndStaffViewTextbox.Text != null && loadfinished) {
                int currval = Convert.ToInt32(rndStaffViewTextbox.Text);
                if (currval > 359) { currval = 359; rndStaffViewTextbox.Text = "359"; }
                laststaffpaletteoffset = currval;
                Shuffling(true);
            }
        }


        private void rndTextLightDark_Click(object sender, EventArgs e) {
            if (lightdarkicon) { //light to dark ◌●
                rndTextLightDark.Text = "●";
                lightdarkicon = false;
            }
            else { //dark to light ●◌
                rndTextLightDark.Text = "◌";
                lightdarkicon = true;
            }
            Shuffling(true);
        }

        public void LostKeysHandling() {
            //Checked
            if (rndLostKeysToggle.Checked) {
                rndLostKeysDropdown.Enabled = true;  

                rndChestToggle.Checked = true; //Shared (between both types of Lost Keys)
                rndChestToggle.Enabled = false;
                rndWeightedChestDropdown.SelectedIndex = 1;
                rndGiftersToggle.Checked = true;
                rndGiftersToggle.Enabled = false;
                rndWingsmithsToggle.Checked = false;
                rndFastMonasteryToggle.Checked = true;
                rndFastShamwoodToggle.Checked = true;
                rndFastMammonToggle.Checked = true;
                rndIvoryWingsToggle.Checked = true;
                rndIvoryWingsToggle.Enabled = false;

                //disable all boss items in all item lists. they can be re-added after if so desired, but having only one is the whole point
                int currItemTab = itemListTabs.SelectedIndex;
                rndChestDropdown.SelectedIndex = 1; //just forcing these to STANDARD for simplicity's sake.
                rndDropsDropdown.SelectedIndex = 1;
                rndGiftersDropdown.SelectedIndex = 1;
                rndWingsmithsDropdown.SelectedIndex = 0; //now setting wingsmiths to unchanged by default.      
                itemListTabs.SelectedIndex = currItemTab; //changing values changes table, so reset back to last one

                //enable crystal valley postern
                rndCrystalReturnToggle.Checked = true;

                //disable shuffling of final shannons, for simplicity, and to ensure key remains in place
                rndShuffleShannonToggle.Checked = true;
                rndShuffleShannonToggle.Enabled = false;

                if (rndLostKeysDropdown.SelectedIndex == 0) { //Progressive
                    rndUnlockDoorsToggle.Checked = false;
                    rndUnlockDoorsToggle.Enabled = true;
                    rndLockedEndgameToggle.Checked = false; 
                    rndLockedEndgameToggle.Enabled = true;
                }

                if (rndLostKeysDropdown.SelectedIndex == 1) { //Open World
                    rndUnlockDoorsToggle.Checked = true;
                    rndUnlockDoorsToggle.Enabled = false;
                    rndLockedEndgameToggle.Checked = true;
                    rndLockedEndgameToggle.Enabled = false;
                }
            }

            //Unchecked
            if (!rndLostKeysToggle.Checked) {
                rndLostKeysDropdown.Enabled = false; 

                rndChestToggle.Checked = false; //Shared
                rndChestToggle.Enabled = true;
                rndWeightedChestDropdown.SelectedIndex = 0;
                rndGiftersToggle.Checked = false;
                rndGiftersToggle.Enabled = true;
                rndFastMonasteryToggle.Checked = false;
                rndFastShamwoodToggle.Checked = false;
                rndFastMammonToggle.Checked = false;
                rndIvoryWingsToggle.Checked = false;
                rndIvoryWingsToggle.Enabled = true;
                rndCrystalReturnToggle.Checked = false;

                rndShuffleShannonToggle.Checked = false;
                rndShuffleShannonToggle.Enabled = true;

                rndUnlockDoorsToggle.Checked = false;
                rndUnlockDoorsToggle.Enabled = true;
                rndLockedEndgameToggle.Checked = false;
                rndLockedEndgameToggle.Enabled = true;
            }

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
                rndDriftToggle.Checked || 
                rndMusicShuffleToggle.Checked ||
                rndHitMPToggle.Checked) 
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
            //Console.WriteLine(message);
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
