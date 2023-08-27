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
        //BUILD QUEST PATCH----------------------------------------------------------------

        public void BuildPatch() {
            //check if nothing is enabled, if not, don't make a patch
            if (!rndSpellToggle.Checked &&
                !rndElement99Toggle.Checked &&
                !rndSoulToggle.Checked &&
                !rndAccuracyToggle.Checked &&
                !rndLevelToggle.Checked &&
                !rndLevel2Toggle.Checked &&
                !rndSpellRebalanceToggle.Checked &&

                !rndFrenchVanillaToggle.Checked &&
                !rndLostKeysToggle.Checked &&

                !rndChestToggle.Checked &&
                !rndDropsToggle.Checked &&
                !rndDropLimitToggle.Checked &&
                !rndGiftersToggle.Checked &&
                !rndWingsmithsToggle.Checked &&
                !rndWingUnlockToggle.Checked &&
                !rndBetterDewDrop.Checked &&

                !rndMonsterStatsToggle.Checked &&
                !rndMonsterScaleToggle.Checked &&
                !rndBossOrderToggle.Checked &&
                !rndInvalidityToggle.Checked &&
                !rndBossElementToggle.Checked &&
                !rndCombatExpToggle.Checked &&

                rndHPTrackBar.Value == 50 &&
                rndMPTrackBar.Value == 15 &&
                rndAgiTrackBar.Value == 5 &&
                rndDefTrackBar.Value == 4 &&
                rndMPRegainTrackBar.Value == 10 &&
                rndHitMPTrackBar.Value == 1 &&
                rndEncounterTrackBar.Value == 2 &&

                !rndFastMonasteryToggle.Checked &&
                !rndFastMammonToggle.Checked &&
                !rndFastBlueToggle.Checked &&
                !rndFastShamwoodToggle.Checked &&
                !rndCrystalReturnToggle.Checked &&
                !rndUnlockDoorsToggle.Checked &&
                !rndIvoryWingsToggle.Checked &&
                !rndFastShamwoodToggle.Checked &&
                !rndLockedEndgameToggle.Checked &&
                !rndBlueHouseWarpToggle.Checked &&
                !rndShannonHintsToggle.Checked &&
                !rndRevealSpiritsToggle.Checked &&

                !rndTextPaletteToggle.Checked &&
                !rndStaffPaletteToggle.Checked &&
                !rndSpellPaletteToggle.Checked &&
                !rndCloakPaletteToggle.Checked &&
                !rndBrianClothesToggle.Checked &&
                !rndZoomToggle.Checked &&
                !rndMaxMessageToggle.Checked &&
                !rndHUDLockToggle.Checked &&
                !rndTextImprovementsToggle.Checked &&

                !rndRestlessToggle.Checked &&
                !rndVowelsToggle.Checked &&
                !rndTextContentToggle.Checked &&
                !rndDriftToggle.Checked &&
                !rndMusicShuffleToggle.Checked &&
                !rndFlyingShuffle.Checked
               ) { return; }
            //eventually i maybe will replace this with a sort of 'binary state' checker that'll be way less annoying and also have the side of effect of creating enterable shortcodes for option sets

            //some functionality should be locked while writing a patch, so:
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

            //debug option with no crash protection
            if (rndSpellDropdown.SelectedIndex > 0) { Shuffling(false); }

            if (crashlockoutput) { 
                File.WriteAllText(filePath + fileName + "_crashlock.txt", "MERROW " + labelVersion.Text + " building patch..." + Environment.NewLine);
                for (int i = 0; i < 60; i++) {
                    string crashline = "";
                    for (int j = 0; j < 60; j++) {
                        crashline += (string)library.crashlock[i * 60 + j].ToString().PadLeft(2,'0') + " ";
                    }
                    File.AppendAllText(filePath + fileName + "_crashlock.txt", i.ToString().PadLeft(2,'0') + crashline + Environment.NewLine);
                }
            }

            //start spoiler log and initialize empty patch content strings
            File.WriteAllText(filePath + fileName + "_spoiler.txt", "MERROW " + labelVersion.Text + " building patch..." + Environment.NewLine);
            File.AppendAllText(filePath + fileName + "_spoiler.txt", "Seed: " + rngseed.ToString() + Environment.NewLine);
            File.AppendAllText(filePath + fileName + "_spoiler.txt", "Shortcode: " + rndShortcodeText.Text + Environment.NewLine);
            File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "PSA: You can use the same Controller Pak save file across multiple different rando patches." + Environment.NewLine + "Only inventory, stats, defeated bosses, and collected chests/spirits will be retained." + Environment.NewLine + "Save states override all settings and cannot be used for testing randomization across different patches." + Environment.NewLine);
            File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "Please report any bugs or quirks (or funny stuff): @JonahD on Twitter, or hangedman#5757 on Discord." + Environment.NewLine);
            File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "PATCH MODIFIERS:" + Environment.NewLine);
            patchbuild = "";
            patchcontent = "";
            patchstrings.Clear();
            spoilerextra.Clear();

            //RANDOMIZATION FEATURES

            //Lost Keys spoiler (actual patch content is added last)
            if (rndLostKeysToggle.Checked && rndLostKeysDropdown.SelectedIndex == 0) {
                File.AppendAllText(filePath + fileName + "_spoiler.txt", "LOST KEYS mode enabled: Progressive." + Environment.NewLine);
            }
            if (rndLostKeysToggle.Checked && rndLostKeysDropdown.SelectedIndex == 1) {
                File.AppendAllText(filePath + fileName + "_spoiler.txt", "LOST KEYS mode enabled: Open World." + Environment.NewLine);
            }

            //Spell Damage Rebalance
            int rebalanced = 0;
            if (rndSpellRebalanceToggle.Checked) { //rebalance
                for (int i = 0; i < 18; i++) {
                    string spelldmghex = library.damageRebalance[i * 3 + 2].ToString("X4");
                    string oldspell = library.spells[library.damageRebalance[i * 3] * 4 + 3];
                    string newspell = oldspell.Substring(0, 24) + spelldmghex + oldspell.Substring(28);
                    library.spells[library.damageRebalance[i * 3] * 4 + 3] = newspell;
                }
                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Spells rebalanced." + Environment.NewLine);
                rebalanced = 2;
            }
            //else { // 2022-01-30: removed this because it overwrote shuffled spell values on repatch, duh
            //    for (int i = 0; i < 18; i++) { //back to default
            //        string spelldmghex = library.damageRebalance[i * 3 + 1].ToString("X4");
            //        string oldspell = library.spells[library.damageRebalance[i * 3] * 4 + 3];
            //        string newspell = oldspell.Substring(0, 24) + spelldmghex + oldspell.Substring(28);
            //        library.spells[library.damageRebalance[i * 3] * 4 + 3] = newspell;
            //    }
            //    //File.AppendAllText(filePath + fileName + "_spoiler.txt", "Spells reset to default." + Environment.NewLine);
            //    rebalanced = 1;
            //}
            if (rebalanced != 0 && !rndSpellToggle.Checked) { //special writing of damage values direct
                for (int i = 0; i < 18; i++) {
                    string spelldmgaddr = (Convert.ToInt32(library.spells[library.damageRebalance[i * 3] * 4 + 2]) + 12).ToString("X6");
                    string spelldmghex = library.damageRebalance[i * 3 + rebalanced].ToString("X4");
                    //Console.WriteLine(spelldmgaddr + " " + spelldmghex);
                    patchstrings.Add(spelldmgaddr); //address advanced by 24
                    patchstrings.Add("0002");
                    patchstrings.Add(spelldmghex);
                }
            }

            //fix for skelebat group in Blue Cave that can cause crashes due to lag
            patchstrings.Add("667260");
            patchstrings.Add("000C");
            patchstrings.Add("000000060000000100000001");

            //Spell Shuffle
            if (rndSpellToggle.Checked) { 
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

                //Early Healing
                if (rndEarlyHealingToggle.Checked) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Early Healing enabled." + Environment.NewLine);
                }

                //Extra Healing
                if (rndExtraHealingToggle.Checked) {
                    if (!rndSpellNamesToggle.Checked) { //if you don't have hinted names, add Mending name over SS1
                        for (int i = 0; i < 3; i++) { patchstrings.Add(library.mendingdata[i]); }
                    }
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Extra Healing enabled." + Environment.NewLine);
                }

                //Hinted Spell Names
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

                //spell item softlock protection
                for (int i = 0; i < 6; i++) {
                    patchstrings.Add(library.items[25 + (i * 3)]); //fetching item addresses starting from silent flute address
                    patchstrings.Add("0002");

                    if (itemspellfix[i] == 0) { //can be used in battle only 
                        //v42: changed 000A (battle only but not bosses) to 0002
                        patchstrings.Add("0002");
                    }
                    if (itemspellfix[i] == 1) { //out of battle
                        patchstrings.Add("0001");
                    }
                    if (itemspellfix[i] == 2) { //anytime
                        patchstrings.Add("0003");
                    }
                }

                //Updated Spell Item Names/Descriptions
                if (true) { //rndSpellItemsToggle.Checked) { ALWAYS DO, OPTION REMOVED
                    //name
                    for (int i = 0; i < 6; i++) {
                        object[] hintdata = new object[2];
                        string hintstring = "OOPS%";

                        int currelement = 0; //fire
                        if (newitemspells[i] > 15) { currelement = 1; } //earth
                        if (newitemspells[i] > 30) { currelement = 2; } //water
                        if (newitemspells[i] > 45) { currelement = 3; } //wind

                        //string is written
                        hintstring = library.newSpellItemName[(i * 4) + currelement];

                        hintdata = TranslateString(hintstring);

                        int hintlen = (int)hintdata[1];
                        //Console.WriteLine(hintdata[0]);
                        //Console.WriteLine(hintdata[1]);

                        //string is encoded to patch
                        patchstrings.Add(library.spellItemNameAddr[i]);
                        patchstrings.Add(hintlen.ToString("X4"));
                        patchstrings.Add((string)hintdata[0]);

                        //update capitalcase item list for hints and gifters
                        library.itemcapitalcase[i + 8] = library.newSpellItemCapCase[(i * 4) + currelement];
                    }
                    //desc
                    for (int i = 0; i < 6; i++) {
                        object[] hintdata = new object[2];
                        string hintstring = "OOPS%";

                        //string is written
                        hintstring = library.newSpellItemDesc[shuffles[library.spellItemIDs[i]]];

                        hintdata = TranslateString(hintstring);

                        int hintlen = (int)hintdata[1];
                        //Console.WriteLine(hintdata[0]);
                        //Console.WriteLine(hintdata[1]);

                        //string is encoded to patch
                        patchstrings.Add(library.spellItemDescAddr[i]);
                        patchstrings.Add(hintlen.ToString("X4"));
                        patchstrings.Add((string)hintdata[0]);
                    }
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Spell item names/descriptions updated." + Environment.NewLine);
                }
            }

            //special avalanche fix testing
            if (rndSpellToggle.Checked && rndSpellDropdown.SelectedIndex == 24) {
                for (int i = 0; i < 9; i++) { //write 9 spells
                    int spelladd = library.avalancheFix[i];
                    for (int j = 0; j < 5; j++) { //write 5 times
                        patchstrings.Add(Convert.ToString(spelladd + library.avalancheFix[(j * 2) + 9], 16));
                        patchstrings.Add("0001");
                        patchstrings.Add(Convert.ToString(library.avalancheFix[(j * 2) + 10], 16).PadLeft(2, '0'));
                    }
                }
            }

            //Spell Combination Fixes: Fix writing
            //This half of this function has to happen AFTER spells are shuffled and edited in other ways.
            //The other half (unlocking locked spells thanks to fixes) has to happen in Shuffle.cs, before shuffling
            if (rndSpellToggle.Checked && rndSpellOverridesToggle.Checked) {
                fixcount = 0;
                //check each spell combination in turn
                for (int i = 0; i < playerspells; i++) {
                    int spellfixnum = library.spellfixes[(i * playerspells) + shuffles[i]];
                    string[] fixstrings = new string[2];

                    //check spellfix array index at [base X/modifier Y] for a positive number, which indicate a fix exists
                    if (spellfixnum >= 0) {
                        //grab fix addr/code from fix data array
                        fixstrings[0] = library.spellfixdata[spellfixnum * 2];
                        fixstrings[1] = library.spellfixdata[(spellfixnum * 2) + 1];
                        int fixlen = fixstrings[1].Length / 2;

                        //TODO: Write code for handling address-agnostic "Z" spell fixes, esp for crash repair.

                        patchstrings.Add(fixstrings[0]);
                        patchstrings.Add(fixlen.ToString("X4"));
                        patchstrings.Add(fixstrings[1]);

                        fixcount++;
                    }
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", unlockcount.ToString() + " crash repair fixes applied." + Environment.NewLine);
                File.AppendAllText(filePath + fileName + "_spoiler.txt", fixcount.ToString() + " spell combination fixes applied." + Environment.NewLine);
            }

            //Chest shuffle
            if (rndChestToggle.Checked) {
                //add chest addresses, and new byte
                for (int i = 0; i < chests.Length; i++) {
                    int temp = library.chestdata[i * 4] + 33; //33 is offset to chest item byte
                    patchstrings.Add(Convert.ToString(temp, 16));
                    patchstrings.Add("0001");
                    patchstrings.Add(chests[i].ToString("X2"));

                    //spoilerchests[i] = i.ToString("00") + ": " + library.items[(chests[i] * 3)]; old method
                    spoilerchests[library.chestdata[i * 4 + 2]] = library.chestlocnames[i] + ": " + library.items[(chests[i] * 3)];
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

            //Item Drop shuffle
            if (rndDropsToggle.Checked) {
                //add drop addresses, and new byte
                for (int i = 0; i < 67; i++) { //only change item drops for non-bosses, so don't go to end of array
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
                }
                else {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Enemy drops randomized (" + library.randomtype[rndDropsDropdown.SelectedIndex] + ")." + Environment.NewLine);
                }
            }

            //Item Gift shuffle
            if (rndGiftersToggle.Checked) {
                //add gift addresses, and new byte
                for (int i = 0; i < gifts.Length; i++) {
                    int temp = library.itemgranters[i * 2]; //don't need to offset because gift hex loc list is pre-offset
                    
                    patchstrings.Add(Convert.ToString(temp, 16));
                    patchstrings.Add("0001");
                    patchstrings.Add(gifts[i].ToString("X2"));

                    spoilergifts[i] = library.gifternames[i] + ": " + library.items[gifts[i] * 3];
                }

                if (rndShuffleShannonToggle.Checked && !rndLostKeysToggle.Checked) { //if shannons are forced vanilla, add this note about them being vanilla
                    spoilergifts[8] = "Shannon (Brannoch Castle): ELETALE BOOK (unrandomized)";
                    spoilergifts[9] = "Shannon (Mammon's World): DARK GAOL KEY (unrandomized)";
                }

                if (rndShuffleShannonToggle.Checked && rndLostKeysToggle.Checked) { //only overwrites book shannon
                    spoilergifts[9] = "Shannon (Mammon's World): DARK GAOL KEY (unrandomized)";
                }

                if (rndGiftersDropdown.SelectedIndex == 0) { //shuffle
                    if (rndShuffleShannonToggle.Checked) {
                        File.AppendAllText(filePath + fileName + "_spoiler.txt", "NPC gifts shuffled (Shannons excluded)." + Environment.NewLine);
                    }
                    else {
                        File.AppendAllText(filePath + fileName + "_spoiler.txt", "NPC gifts shuffled." + Environment.NewLine);
                    }
                }

                if (rndGiftersDropdown.SelectedIndex > 0) { //random
                    if (rndShuffleShannonToggle.Checked) {
                        File.AppendAllText(filePath + fileName + "_spoiler.txt", "NPC gifts randomized (" + library.randomtype[rndGiftersDropdown.SelectedIndex] + "), Shannons excluded." + Environment.NewLine);
                    }
                    else {
                        File.AppendAllText(filePath + fileName + "_spoiler.txt", "NPC gifts randomized (" + library.randomtype[rndGiftersDropdown.SelectedIndex] + ")." + Environment.NewLine);
                    }
                }
            }

            //Wingsmiths shuffle
            if (rndWingsmithsToggle.Checked) {
                //add wings addresses, and new byte
                for (int i = 0; i < wings.Length; i++) {
                    int temp = library.itemgranters[20 + (i * 2)]; //gift hex loc list is pre-offset, advance 20 to skip gifters
                    patchstrings.Add(Convert.ToString(temp, 16));
                    patchstrings.Add("0001");
                    patchstrings.Add(wings[i].ToString("X2"));

                    spoilerwings[i] = library.gifternames[i + 10] + ": " + library.items[wings[i] * 3]; //advance 10 to skip gifters
                }

                if (rndWingsmithsDropdown.SelectedIndex == 0) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Wingsmiths shuffled." + Environment.NewLine);
                }
                else {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Wingsmiths randomized (" + library.randomtype[rndWingsmithsDropdown.SelectedIndex] + ")." + Environment.NewLine);
                }
            }

            //Updated Gifter/Wingsmith text
            if (rndGiftersToggle.Checked || rndWingsmithsToggle.Checked) { //only has an effect if one or both are enabled
                if (true) { //rndGifterTextToggle.Checked) { ALWAYS DO, OPTION REMOVED
                    if (rndIvoryWingsToggle.Checked) { library.itemcapitalcase[14] = "Ivory Wings"; }
                    int[] gifternum = { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 11, 11, 12, 12, 13, 13, 14, 14, 15, 15, 10, 10 };
                    for (int i = 0; i < 28; i++) {
                        object[] hintdata = new object[2];
                        string hintstring = "OOPS%";

                        //gifters
                        if (i <= 15) { 
                            hintstring = library.newgiftertext[(i * 3) + 1] + library.itemcapitalcase[gifts[gifternum[i]]] + library.newgiftertext[(i * 3) + 2];
                        }
                        //regular wingsmiths
                        if (i >= 16 && i <= 25) {
                            hintstring = library.newgiftertext[(i * 3) + 1] + library.itemcapitalcase[wings[gifternum[i] - 10]] + library.newgiftertext[(i * 3) + 2];
                        }
                        //Melrode wingsmith A/B
                        if (i == 26) { hintstring = library.newgiftertext[79]; }
                        if (i == 27) { hintstring = library.newgiftertext[81]; }

                        hintdata = TranslateString(hintstring);
                        int hintlen = (int)hintdata[1] + 2; //add 2 for A0C0

                        //string is encoded to patch
                        if (i < 26) { patchstrings.Add(library.newgiftertext[i * 3]); }
                        if (i == 26) { patchstrings.Add(library.newgiftertext[78]); }
                        if (i == 27) { patchstrings.Add(library.newgiftertext[80]); }
                        patchstrings.Add(hintlen.ToString("X4"));
                        patchstrings.Add("A0C0" + (string)hintdata[0]);
                    }

                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Gifter/Wingsmith text updated." + Environment.NewLine);
                }
            }

            //Enemy drop limit toggle
            if (rndDropLimitToggle.Checked) {
                patchstrings.Add("0042B1");
                patchstrings.Add("0001");
                patchstrings.Add("42");

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Enemy drop limit disabled." + Environment.NewLine);
            }

            //MONSTER SCALING AND BOSS SHUFFLING FEATURES

            //Stat randomization/scaling, Boss order shuffle
            if (rndMonsterStatsToggle.Checked || rndMonsterScaleToggle.Checked || rndBossOrderToggle.Checked) {
                int moncount = 0;
                for (int i = 0; i < newmonsterstats.Length; i++) {
                    moncount = (i - (i % 6)) / 6; //monster index counter just to make boss checks smoother

                    if (moncount < 67 || moncount >= 73) { //V30: changed to >= 73 to exclude Beigis for now
                        patchstrings.Add(library.monsterstatlocations[i].ToString("X6"));
                        patchstrings.Add("0002");
                        patchstrings.Add(newmonsterstats[i].ToString("X4"));

                        if (i % 6 == 0) { //if the current value is HP2, write it again at the HP1 location, offset 04 (HP2 + 2).
                            patchstrings.Add((library.monsterstatlocations[i] + 2).ToString("X6"));
                            patchstrings.Add("0002");
                            patchstrings.Add(newmonsterstats[i].ToString("X4"));
                        }
                    }
                    if (moncount >= 67 && moncount < 73) { //V30: changed to < 73 to exclude Beigis for now
                        int columnstep = i % 6;
                        int rowstep = newbossorder[moncount - 67];
                        patchstrings.Add(library.monsterstatlocations[402 + (rowstep * 6) + columnstep].ToString("X6"));
                        patchstrings.Add("0002");
                        patchstrings.Add(newmonsterstats[i].ToString("X4"));

                        if (i % 6 == 0) { //if the current value is HP2, write it again at the HP1 location, offset 04 (HP2 + 2).
                            patchstrings.Add((library.monsterstatlocations[402 + (rowstep * 6) + columnstep] + 2).ToString("X6"));
                            patchstrings.Add("0002");
                            patchstrings.Add(newmonsterstats[i].ToString("X4"));
                        }
                    }
                }

                //populate spoiler log
                for (int i = 0; i < 75; i++) {
                    if (i < 67 || i >= 73) { //V30: changed to >= 73 to exclude Beigis for now
                        if (!rndVowelsToggle.Checked) { spoilerscales[i] = library.monsternames[i * 2] + ": "; }
                        if (rndVowelsToggle.Checked) { spoilerscales[i] = voweled[i] + ": "; }
                        spoilerscales[i] += newmonsterstats[i * 6].ToString() + " ";
                        spoilerscales[i] += newmonsterstats[(i * 6) + 1].ToString() + " ";
                        spoilerscales[i] += newmonsterstats[(i * 6) + 2].ToString() + " ";
                        spoilerscales[i] += newmonsterstats[(i * 6) + 3].ToString() + " ";
                        spoilerscales[i] += newmonsterstats[(i * 6) + 4].ToString() + " ";
                        if (newmonsterstats[(i * 6) + 5] == 0) { spoilerscales[i] += "FIRE"; }
                        if (newmonsterstats[(i * 6) + 5] == 1) { spoilerscales[i] += "EARTH"; }
                        if (newmonsterstats[(i * 6) + 5] == 2) { spoilerscales[i] += "WATER"; }
                        if (newmonsterstats[(i * 6) + 5] == 3) { spoilerscales[i] += "WIND"; }
                        if (newmonsterstats[(i * 6) + 5] == 4) { spoilerscales[i] += "DARK"; }
                    }
                    if (i >= 67 && i < 73) { //V30: changed to < 73 to exclude Beigis for now
                        int falsei = i;
                        if (rndBossOrderToggle.Checked) { falsei = 67 + newbossorder[i - 67]; }
                        if (!rndVowelsToggle.Checked) { spoilerscales[i] = library.monsternames[falsei * 2] + ": "; }
                        if (rndVowelsToggle.Checked) { spoilerscales[i] = voweled[falsei] + ": "; }
                        spoilerscales[i] += newmonsterstats[i * 6].ToString() + " ";
                        spoilerscales[i] += newmonsterstats[(i * 6) + 1].ToString() + " ";
                        spoilerscales[i] += newmonsterstats[(i * 6) + 2].ToString() + " ";
                        spoilerscales[i] += newmonsterstats[(i * 6) + 3].ToString() + " ";
                        spoilerscales[i] += newmonsterstats[(i * 6) + 4].ToString() + " ";
                        if (newmonsterstats[(i * 6) + 5] == 0) { spoilerscales[i] += "FIRE"; }
                        if (newmonsterstats[(i * 6) + 5] == 1) { spoilerscales[i] += "EARTH"; }
                        if (newmonsterstats[(i * 6) + 5] == 2) { spoilerscales[i] += "WATER"; }
                        if (newmonsterstats[(i * 6) + 5] == 3) { spoilerscales[i] += "WIND"; }
                        if (newmonsterstats[(i * 6) + 5] == 4) { spoilerscales[i] += "DARK"; }
                    }
                }

                if (rndMonsterStatsToggle.Checked && extremity == 0) { File.AppendAllText(filePath + fileName + "_spoiler.txt", "Monster stats randomized within regions." + Environment.NewLine); }
                if (rndMonsterStatsToggle.Checked && extremity != 0) { File.AppendAllText(filePath + fileName + "_spoiler.txt", "Monster stats randomized, with Variance " + (extremity + 1).ToString() + "x." + Environment.NewLine); }
                if (rndMonsterScaleToggle.Checked) { File.AppendAllText(filePath + fileName + "_spoiler.txt", "Monster stats scaled by " + difficultyscale.ToString("n1") + "x." + Environment.NewLine); }

                //don't bother saying EXP is scaled if only boss order is enabled and not scaling/random
                if (rndMonsterExpToggle.Checked) {
                    if (rndMonsterScaleToggle.Checked || rndMonsterStatsToggle.Checked) { File.AppendAllText(filePath + fileName + "_spoiler.txt", "Monster experience scaled to new stat values." + Environment.NewLine); }
                }
            }

            //Boss Order Shuffle
            if (rndBossOrderToggle.Checked) { 
                //this data could all be extracted from the library in code but you know what, this is easier
                int[] bossitemslist = { 20, 21, 22, 255, 23, 255 };
                if (rndLostKeysToggle.Checked) {
                    for (int i = 0; i < 6; i++) { bossitemslist[i] = lostkeysbossitemlist[i]; }
                }

                string[] bossitemnames = { "EARTH ORB", "WIND JADE", "WATER JEWEL", "NOTHING", "FIRE RUBY", "NOTHING" };
                int[] bossaddresses = { 14186532, 14186588, 14186644, 14186700, 14186756, 14186812 };
                int[] backassign = new int[6];

                //data extraction code for items, in boss order. please do not remove until we're done fighting with boss order
                //14186532,4,14186588,4,14186644,4,14186700,4,14186756,4,14186812,4,14186868,4,14186924,4 
                //D87824,D8785C,D87894,D878CC,D87904,D8793C

                //items have to be handed backwards, so we're abstracting out the boss order
                for (int j = 0; j < 6; j++) { 
                    backassign[j] = bossaddresses[newbossorder[j]];
                }

                //writing locations, additional data, and items
                for (int i = 0; i < 6; i++) { //V30: changed this to 6 to exclude Beigis for now
                    patchstrings.Add(library.bosslocdata[newbossorder[i] * 4]); //location data replacement
                    patchstrings.Add("0004");
                    patchstrings.Add(library.bosslocdata[(i * 4) + 1]);

                    patchstrings.Add(library.bosslocdata[(newbossorder[i] * 4) + 2]); //other boss data replacement
                    patchstrings.Add("000C");
                    patchstrings.Add(library.bosslocdata[(i * 4) + 3]);

                    //boss reward hp values
                    patchstrings.Add(library.rewardhpdata[newbossorder[i] * 2]);
                    patchstrings.Add("0001");
                    patchstrings.Add(library.rewardhpdata[(i * 2) + 1]);

                    //item drops
                    int newitemaddr = library.dropdata[(newbossorder[i] + 67) * 2];
                    if (!rndLostKeysToggle.Checked) { spoilerbossdrops[i] = (library.monsternames[(newbossorder[i] + 67) * 2] + " carries " + bossitemnames[i]); }
                    if (rndLostKeysToggle.Checked) { spoilerbossdrops[i] = (library.monsternames[(newbossorder[i] + 67) * 2] + " carries " + library.items[lostkeysbossitemlist[i] * 3]); }
                    patchstrings.Add(backassign[i].ToString("X6"));
                    patchstrings.Add("0001");
                    patchstrings.Add(bossitemslist[i].ToString("X2"));
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Boss order shuffled." + Environment.NewLine);
            }

            //Randomize Guilty's element
            if (rndBossElementToggle.Checked) {
                //Guilty: 14186798/D8792E
                patchstrings.Add("D8792C");
                patchstrings.Add("0004");
                patchstrings.Add("000" + newbosselem[0].ToString() + "000" + newbosselem[0].ToString());

                //Mammon: 14186910/D8799E ---- Has been disabled, as it causes his AI to always use his projectiles
                //patchstrings.Add("D8799C");
                //patchstrings.Add("0004");
                //patchstrings.Add("000" + newbosselem[1].ToString() + "000" + newbosselem[1].ToString());

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Randomized Guilty's element." + Environment.NewLine);
            }

            //Invalidity
            if (rndInvalidityToggle.Checked) { 
                for (int i = 0; i < 8; i++) {
                    patchstrings.Add(library.invalidityLocations[i]);
                    patchstrings.Add("0001");
                    patchstrings.Add("00");
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Boss spell passive invalidity disabled." + Environment.NewLine);
            }

            //Maximum Accuracy
            if (rndAccuracyToggle.Checked) {
                //spell accuracy: status 100
                if (rndAccuracyDropdown.SelectedIndex == 0) { 
                    for (int i = 0; i < 17; i++) {
                        string spellloc = library.spells[(library.statusspells[i] * 4) + 2];
                        int temploc = Convert.ToInt32(spellloc) + 15;
                        patchstrings.Add(Convert.ToString(temploc, 16));
                        patchstrings.Add("0001");
                        patchstrings.Add("64");
                    }

                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Status effects' accuracy normalized to 100." + Environment.NewLine);
                }

                //spell accuracy: all 100
                if (rndAccuracyDropdown.SelectedIndex == 1) { 
                    for (int z = spellstart + 15; z < ((spelloffset * playerspells) + spellstart); z += spelloffset) {
                        patchstrings.Add(Convert.ToString(z, 16));
                        patchstrings.Add("0001");
                        patchstrings.Add("64");
                    }

                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "All spells' accuracy normalized to 100." + Environment.NewLine);
                }
            }

            //Level 1 Unlock All
            if (rndLevelToggle.Checked) { 
                for (int s = spellstart; s < ((spelloffset * playerspells) + spellstart); s += spelloffset) {
                    patchstrings.Add(Convert.ToString(s, 16));
                    patchstrings.Add("0002");
                    patchstrings.Add("0001");
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Spell unlock levels reduced to 1." + Environment.NewLine);
            }

            //Soul Search
            if (rndSoulToggle.Checked) { 
                for (int z = spellstart + 57; z < ((spelloffset * playerspells) + spellstart); z += spelloffset) {
                    patchstrings.Add(Convert.ToString(z, 16));
                    patchstrings.Add("0001");
                    patchstrings.Add("01");
                }

                //writing new magnifying glass sprite.
                //it has to be written in this weird way because the graphical data it's replacing is wider than 16x16.
                //eventually i actually want to replace this with an even simpler little asterisk or something.
                //but i plan to save this pixelart for an overall pixelart cleanup i might do because i'm insane, apparently.
                tempaddr = Convert.ToInt32("D81C30", 16);
                for (int i = 0; i < 16; i++) {
                    patchstrings.Add(Convert.ToString(tempaddr, 16)); ; //tempaddr converted back to hex
                    patchstrings.Add("0010"); //replace 16 bytes
                    patchstrings.Add(library.magnifier[i]); //add the 16 replacement bytes from the array
                    tempaddr += 128; //step to next case
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Soul search applied to all spells." + Environment.NewLine);
            }

            //Fast Monastery
            if (rndFastMonasteryToggle.Checked) { 
                patchstrings.Add("4361A0");
                patchstrings.Add("0004");
                patchstrings.Add("00090002"); // write 00090002 as new door target ID at 4361A0

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Fast Monastery enabled." + Environment.NewLine);
            }

            //Fast Blue Cave
            if (rndFastBlueToggle.Checked) {
                patchstrings.Add("65F1E3");
                patchstrings.Add("0009");
                patchstrings.Add("180018001A00000005"); // write first door target ID

                patchstrings.Add("65F273");
                patchstrings.Add("0009");
                patchstrings.Add("180018001A00000007"); // write second door target ID

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Fast Blue Cave enabled." + Environment.NewLine);
            }

            //Wing unlock toggle
            if (rndWingUnlockToggle.Checked) {
                //both of these happen if index is 2. neat trick, right?
                if (rndWingUnlockDropdown.SelectedIndex == 0 || rndWingUnlockDropdown.SelectedIndex == 2) {
                    patchstrings.Add("022ECB"); //Indoors unlock
                    patchstrings.Add("0001");
                    patchstrings.Add("00");

                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Wings enabled indoors." + Environment.NewLine);
                }
                if (rndWingUnlockDropdown.SelectedIndex == 1 || rndWingUnlockDropdown.SelectedIndex == 2) {
                    patchstrings.Add("022EE4"); //Skye unlock
                    patchstrings.Add("0001");
                    patchstrings.Add("10");

                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Wings enabled on the Isle of Skye." + Environment.NewLine);
                }
            }

            //Starting stats
            if (rndHPTrackBar.Value != 50 || rndMPTrackBar.Value != 15 || rndAgiTrackBar.Value != 5 || rndDefTrackBar.Value != 4) {
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

                patchstrings.Add("003ED7"); //Overworld spirit grab cap
                patchstrings.Add("0001");
                patchstrings.Add("63");

                //individual elemental caps, and menu elemental caps
                for (int i = 0; i < 8; i++) { 
                    patchstrings.Add(library.elementCapLocations[i]);
                    patchstrings.Add("0001");
                    patchstrings.Add("63");
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Element level maximum raised to 99." + Environment.NewLine);
            }

            //MP regain rate
            if (rndMPRegainTrackBar.Value != 10) {
                //the trackbar is restricted to x2 and x3 in either direction. Value 7 = OFF.
                if (rndMPRegainTrackBar.Value != 7) {
                    //the old method multiplied the speed value, but the effect of that was logarithmic, not multiplicative.
                    //instead, I've selected hex values that map closely to the number of seconds spent running, which was the intent all along.
                    //for reference, I got these values by testing MP gained while running a set distance.
                    string[] speeds = new string[5] { "28", "31", "41", "51", "58" }; //40, 49, 65, 81, 88

                    patchstrings.Add("071B39");
                    patchstrings.Add("0001");
                    patchstrings.Add(speeds[rndMPRegainTrackBar.Value-8]);
                }

                //if OFF, instead of changing rate, just disable it entirely elsewhere
                if (rndMPRegainTrackBar.Value == 7) { 
                    patchstrings.Add("00445B");
                    patchstrings.Add("0001");
                    patchstrings.Add("00");
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Walking MP regen rate set to " + rndMPRegainValue.Text + "." + Environment.NewLine);
            }

            //Fast Mammon's World
            if (rndFastMammonToggle.Checked) {
                patchstrings.Add("84EDFE");
                patchstrings.Add("0004");
                patchstrings.Add("000E000D");

                patchstrings.Add("607920");
                patchstrings.Add("0008");
                patchstrings.Add("0000000F000D0000");

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Fast Mammon's World enabled." + Environment.NewLine);
            }

            //Crystal Valley Postern
            if (rndCrystalReturnToggle.Checked) {
                patchstrings.Add("206EB0");
                patchstrings.Add("0024");
                if (!rndUnlockDoorsToggle.Checked) { patchstrings.Add("42020000C3BC0000BFC90FF940C0000040E0000001160016000000020000001A00020003"); }
                //need to test the below data to make sure it works as intended, removes water jewel requirement
                if (rndUnlockDoorsToggle.Checked) { patchstrings.Add("42020000C3BC0000BFC90FF940C0000040E0000001060000000000020000001A00020003"); }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Crystal Valley return warp enabled." + Environment.NewLine);
            }

            //Staff Hit MP Regain
            if (rndHitMPTrackBar.Value != 1) {
                patchstrings.Add("0050DB");
                patchstrings.Add("0001");
                patchstrings.Add("0" + rndHitMPTrackBar.Value.ToString());

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Staff hit MP regain set to " + rndHitMPValue.Text + "." + Environment.NewLine);
            }

            //Unlock All Progression Locks
            if (rndUnlockDoorsToggle.Checked) {
                //any mode other than open world lost keys
                //unlocks everything
                if (rndLostKeysDropdown.SelectedIndex != 1) { 
                    for (int i = 0; i < 18; i++) {
                        patchstrings.Add(library.unlockedDoorData[i * 2]);
                        if (i != 7) {
                            patchstrings.Add("0004");
                            patchstrings.Add(library.unlockedDoorData[i * 2 + 1]);
                        }
                        else if (i == 7) {
                            patchstrings.Add("0010");
                            patchstrings.Add(library.unlockedDoorData[i * 2 + 1]);
                        }
                    }
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "All progression locks (gems/book/key) unlocked." + Environment.NewLine);
                }

                //open world lost keys 
                //does not unlock final shannons, adds shamwood shortcut and locks final castle roof
                if (rndLostKeysDropdown.SelectedIndex == 1) {
                    for (int i = 0; i < 16; i++) {
                        patchstrings.Add(library.unlockedDoorData[i * 2]);
                        if (i != 7) {
                            patchstrings.Add("0004");
                            patchstrings.Add(library.unlockedDoorData[i * 2 + 1]);
                        }
                        else if (i == 7) {
                            patchstrings.Add("0010");
                            patchstrings.Add(library.unlockedDoorData[i * 2 + 1]);
                        }
                    }

                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "LOST KEYS OPEN WORLD: All progression locks (gems) moved to endgame." + Environment.NewLine);
                }

            }

            //Level 2 Base Spells
            if (rndLevel2Toggle.Checked) {
                for (int i = 0; i < 60; i += 15) {
                    patchstrings.Add(library.spells[(i * 4) + 1]);
                    patchstrings.Add("0002");
                    patchstrings.Add("0002");
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Base spells unlocked at level 2." + Environment.NewLine);
            }

            //Shamwood Exit to Baragoon Exit
            if (rndFastShamwoodToggle.Checked) {
                patchstrings.Add(library.unlockedDoorData[19 * 2]); 
                patchstrings.Add("0001");
                patchstrings.Add(library.unlockedDoorData[19 * 2 + 1]);

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Fast Shamwood enabled." + Environment.NewLine);

            }

            //Ivory Wings
            if (rndIvoryWingsToggle.Checked) {
                //wing function override
                for (int i = 0; i < 4; i++) {
                    patchstrings.Add(library.ivorywings[i * 3]);
                    patchstrings.Add(library.ivorywings[i * 3 + 1]);
                    patchstrings.Add(library.ivorywings[i * 3 + 2]);
                }

                //Melrode Wingsmith actor type override
                patchstrings.Add("49F90F"); 
                patchstrings.Add("0001");
                patchstrings.Add("01");

                //Lavaar NPC type, 2nd dialogue override
                patchstrings.Add("5EBB13");
                patchstrings.Add("0001");
                patchstrings.Add("02");
                patchstrings.Add("5EBB1A"); 
                patchstrings.Add("0002");
                patchstrings.Add("1CA0");

                //account for not applying item replacement and spoiler 
                if (!rndWingsmithsToggle.Checked) {
                    patchstrings.Add("5EBB16");
                    patchstrings.Add("0002");
                    patchstrings.Add("000E");

                    for (int i = 0; i < wings.Length; i++) {
                        spoilerwings[i] = library.gifternames[i + 10] + ": " + library.items[(14 + i) * 3]; //advance 10 to skip gifters
                    }
                }

                //item distribution change is handled elsewhere
                File.AppendAllText(filePath + fileName + "_spoiler.txt", "White Wings replaced with Ivory Wings (granted by Lavaar)." + Environment.NewLine);
            }

            //LOST KEYS
            if (rndLostKeysToggle.Checked) {
                if (!rndUnlockDoorsToggle.Checked) { //this is redundant if progression locks are open, only happens in Progressive
                    //items and stuff already redistributed, just have to unlock Colleen at the end here to guarantee it's applied correctly
                    patchstrings.Add(library.unlockedDoorData[7 * 2]); //Epona Teleporter A
                    patchstrings.Add("0010"); 
                    patchstrings.Add(library.unlockedDoorData[7 * 2 + 1]);

                    patchstrings.Add(library.unlockedDoorData[8 * 2]); //Epona Teleporter B
                    patchstrings.Add("0004");
                    patchstrings.Add(library.unlockedDoorData[8 * 2 + 1]);

                    patchstrings.Add(library.unlockedDoorData[9 * 2]); //Colleen's Back Door
                    patchstrings.Add("0004");
                    patchstrings.Add(library.unlockedDoorData[9 * 2 + 1]);

                    patchstrings.Add(library.unlockedDoorData[10 * 2]); //Crystal to Larapool
                    patchstrings.Add("0004");
                    patchstrings.Add(library.unlockedDoorData[10 * 2 + 1]);

                    patchstrings.Add(library.unlockedDoorData[18 * 2]); //Lock Brannoch Castle Gate with Fire Ruby
                    patchstrings.Add("0004");
                    patchstrings.Add(library.unlockedDoorData[18 * 2 + 1]);

                    patchstrings.Add("0E72F0"); //Add fire symbol to Brannoch Castle Gate
                    patchstrings.Add("1000");
                    patchstrings.Add(library.firegate);
                }

                if (!rndBossOrderToggle.Checked) {
                    //this data could all be extracted from the library in code but you know what, this is easier
                    int[] bossitemslist = new int[6];
                    for (int i = 0; i < 6; i++) { bossitemslist[i] = lostkeysbossitemlist[i]; }

                    string[] bossitemnames = { "EARTH ORB", "WIND JADE", "WATER JEWEL", "NOTHING", "FIRE RUBY", "NOTHING" };
                    int[] bossaddresses = { 14186532, 14186588, 14186644, 14186700, 14186756, 14186812 };
                    int[] backassign = new int[6];

                    //data extraction code for items, in boss order. please do not remove until we're done fighting with boss order
                    //14186532,4,14186588,4,14186644,4,14186700,4,14186756,4,14186812,4,14186868,4,14186924,4 
                    //D87824,D8785C,D87894,D878CC,D87904,D8793C

                    //items have to be handed backwards, so we're abstracting out the boss order
                    //item drops
                    for (int i = 0; i < 6; i++) {
                        int newitemaddr = 0;
                        backassign[i] = bossaddresses[newbossorder[i]];
                        newitemaddr = library.dropdata[(newbossorder[i] + 67) * 2];
                        
                        spoilerbossdrops[i] = (library.monsternames[(newbossorder[i] + 67) * 2] + " carries " + library.items[lostkeysbossitemlist[i] * 3]);
                        patchstrings.Add(backassign[i].ToString("X6"));
                        patchstrings.Add("0001");
                        patchstrings.Add(lostkeysbossitemlist[i].ToString("X2"));
                    }
                }
            }

            //Brannoch warp back to Shamwood
            if (rndBlueHouseWarpToggle.Checked) { 
                patchstrings.Add(library.unlockedDoorData[20 * 2]); 
                patchstrings.Add("0010");
                patchstrings.Add(library.unlockedDoorData[20 * 2 + 1]);

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Brannoch return warp enabled." + Environment.NewLine);
            }

            //Final staircase locks with Gems
            if (rndLockedEndgameToggle.Checked) { 
                for (int i = 21; i < 25; i++) {
                    patchstrings.Add(library.unlockedDoorData[i * 2]);
                    patchstrings.Add("0004");
                    patchstrings.Add(library.unlockedDoorData[i * 2 + 1]);
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Approach to Mammon's World locked by Elemental Gems." + Environment.NewLine);
            }

            //Shannon Hints
            if (rndShannonHintsToggle.Checked) {
                for (int i = 0; i < 5; i++) {
                    object[] hintdata = new object[2];
                    string hintstring = "This is a hint.#Did it break?$If so, RIP%";
                    string localgem = library.gemnames[gemIDs[i] - 20];
                    int coinflip = hintcoins[i]; //first coinflip: which word to use
                    string hintword = "";

                    if (i == 0) { hintword = library.earthhints[(hints[0] * 2) + coinflip]; }
                    if (i == 1) { hintword = library.windhints[(hints[1] * 2) + coinflip]; }
                    if (i == 2) { hintword = library.waterhints[(hints[2] * 2) + coinflip]; }
                    if (i == 3) { hintword = library.firehints[(hints[3] * 2) + coinflip]; }
                    if (i == 4) {
                        if (!rndFireBookToggle.Checked) { hintword = library.bookhints[(hints[4] * 2) + coinflip]; } //book only
                        else { hintword = library.bookfhints[(hints[4] * 2) + coinflip]; } //fire+book
                    }

                    //123456789012345678901234567
                    //good of all Celtland.

                    spoilerextra.Add(hintword + " Loc:" + hints[i].ToString() + " Item:" + i.ToString() + " Coin:" + coinflip.ToString());

                    coinflip = hintcoins[i + 5]; //second coinflip: which string to use

                    //string is written
                    if (coinflip == 0) { hintstring = "The " + localgem + " currently#rests somewhere " + hintword + ".#You must find it before any#darker purposes befall it.%"; }
                    if (coinflip == 1) { hintstring = "The " + localgem + " resides#in a " + hintword + " place.#We must retrieve it for the#good of all Celtland.%"; }

                    hintdata = TranslateString(hintstring);

                    int hintlen = (int)hintdata[1];
                    //Console.WriteLine(hintdata[0]);
                    //Console.WriteLine(hintdata[1]);

                    //string is encoded to patch
                    patchstrings.Add(library.shannonhints[i]);
                    patchstrings.Add(hintlen.ToString("X4"));
                    patchstrings.Add((string)hintdata[0]);

                    //account for boss item shuffle - remove secondary dialogue from Shannons
                    //by doing this, I don't really need to worry about item order
                    patchstrings.Add(library.shannonrules[i]);
                    patchstrings.Add("0001");
                    patchstrings.Add("01");

                    spoilerextra.Add(hintstring);
                }
                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Shannon Hints enabled." + Environment.NewLine);
            }

            //Better Dew Drop
            if (rndBetterDewDrop.Checked) {
                patchstrings.Add("D86B01"); //function patch
                patchstrings.Add("0001");
                patchstrings.Add("14");

                patchstrings.Add(library.newdewdropdesc[0]); //description patch
                patchstrings.Add(75.ToString("X4"));
                patchstrings.Add(library.newdewdropdesc[1]);

                patchstrings.Add(library.newdewdropdesc[2]); //dragon's potion desc pointer
                patchstrings.Add("0004");
                patchstrings.Add(library.newdewdropdesc[3]);

                patchstrings.Add(library.newdewdropdesc[4]); //dew drop desc pointer
                patchstrings.Add("0004");
                patchstrings.Add(library.newdewdropdesc[5]);

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Dew Drop made useful." + Environment.NewLine);
            }

            //Reveal hidden spirits
            if (rndRevealSpiritsToggle.Checked) {
                patchstrings.Add("609970");
                patchstrings.Add("0006");
                patchstrings.Add("A20800003208");

                patchstrings.Add("56FAE0");
                patchstrings.Add("0006");
                patchstrings.Add("C210000041E8");

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Hidden spirits moved to clearer locations." + Environment.NewLine);
            }

            //Monster flying shuffle
            if (rndFlyingShuffle.Checked) {
                for (int i = 0; i < 74; i++) {
                    patchstrings.Add(library.monsterflyingvanilla[i * 2].ToString("X6"));
                    patchstrings.Add("0002");
                    patchstrings.Add(rndflying[i].ToString("X4"));
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Gravity Disaster enabled." + Environment.NewLine);
            }

            //Encounter rate changes
            if (rndEncounterTrackBar.Value != 2) {
                patchstrings.Add(library.encountervalues[0]);
                patchstrings.Add("0001");
                patchstrings.Add(library.encountervalues[3 + rndEncounterTrackBar.Value]);

                patchstrings.Add(library.encountervalues[1]);
                patchstrings.Add("0001");
                patchstrings.Add(library.encountervalues[8 + rndEncounterTrackBar.Value]);

                patchstrings.Add(library.encountervalues[2]);
                patchstrings.Add("0002");
                patchstrings.Add(library.encountervalues[13 + rndEncounterTrackBar.Value]);

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Encounter rate: " + rndEncounterValue.Text + Environment.NewLine);
            }

        //--------------------------COSMETIC EFFECTS BELOW THIS LINE SO THEY DON'T AFFECT ANYTHING ELSE-------------------------

            //Text Colour palette
            if (rndTextPaletteToggle.Checked) {
                //default black palette is stored at D3E240
                // black: F83E9C1B6AD5318D
                // red: F83E9C1BBA0DD009
                // blue: F83E9C1B629D19AB
                // white: F83E318DBDEFF735

                int temp = rndTextPaletteDropdown.SelectedIndex;
                if (temp == 2) { temp = randbasetextpalette; }

                patchstrings.Add("D3E240");
                patchstrings.Add("0008");

                if (temp == 0) {
                    patchcontent = "F83E";
                    patchcontent += textPaletteHex;
                    patchstrings.Add(patchcontent);
                    if (lightdarkicon) {
                        File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text palette set to random (" + lasttextpaletteoffset.ToString().PadLeft(3, '0') + " light)." + Environment.NewLine);
                    }
                    else {
                        File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text palette set to random (" + lasttextpaletteoffset.ToString().PadLeft(3, '0') + " dark)." + Environment.NewLine);
                    }
                }
                if (temp == 1) {
                    patchcontent = "F83E";
                    patchcontent += textPaletteHex;
                    patchstrings.Add(patchcontent);
                    if (lightdarkicon) {
                        File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text palette set to custom (" + lasttextpaletteoffset.ToString().PadLeft(3, '0') + " light)." + Environment.NewLine);
                    }
                    else {
                        File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text palette set to custom (" + lasttextpaletteoffset.ToString().PadLeft(3, '0') + " dark)." + Environment.NewLine);
                    }
                }
                if (temp == 3) {
                    patchstrings.Add("F83E9C1BBA0DD009");
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text palette set to red." + Environment.NewLine);
                }
                if (temp == 4) {
                    patchstrings.Add("F83E9C1B629D19AB");
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text palette set to blue." + Environment.NewLine);
                }
                if (temp == 5) {
                    patchstrings.Add("F83E318DBDEFF735");
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text palette set to white." + Environment.NewLine);
                }
                if (temp == 6) {
                    patchstrings.Add("F83E9C1B6AD5318D");
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text palette set to black (default)." + Environment.NewLine);
                }
            }

            //Staff Colour palette
            if (rndStaffPaletteToggle.Checked) {
                patchstrings.Add("86EB70");
                patchstrings.Add("0600");
                patchstrings.Add(staffPaletteHex);

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Staff palette set to custom (" + laststaffpaletteoffset.ToString().PadLeft(3, '0') + ")." + Environment.NewLine);
            }

            //Text content shuffle
            if (rndTextContentToggle.Checked) {
                int temp = 0;
                //add single text addresses, and new byte
                for (int i = 0; i < singletextcount; i++) {
                    temp = library.singletextdata[i * 3] + 8; //text byte at offset 8
                    patchstrings.Add(Convert.ToString(temp, 16));
                    patchstrings.Add("0002");
                    patchstrings.Add(texts[i].ToString("X4"));
                }

                //add double text addresses, and new byte
                for (int i = 0; i < doubletextcount; i++) {
                    temp = library.doubletextdata[i * 4] + 8; //first text at offset 8
                    patchstrings.Add(Convert.ToString(temp, 16));
                    patchstrings.Add("0002");
                    patchstrings.Add(texts[i + singletextcount].ToString("X4"));

                    temp = library.doubletextdata[i * 4] + 10; //second text at offset 10
                    patchstrings.Add(Convert.ToString(temp, 16));
                    patchstrings.Add("0002");
                    patchstrings.Add(texts[i + singletextcount + doubletextcount].ToString("X4"));
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

            //Zoom Option
            if (rndZoomToggle.Checked) {
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

            //Restless NPCs
            if (rndRestlessToggle.Checked) {
                for (int i = 0; i < library.npcmovement.Length; i++) {
                    patchstrings.Add(Convert.ToString(library.npcmovement[i], 16));
                    patchstrings.Add("0001");
                    patchstrings.Add("02"); //Replace movement byte with 02 to cause wandering
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "NPCs are restless." + Environment.NewLine);
            }

            //Max Message Speed
            if (rndMaxMessageToggle.Checked) {
                patchstrings.Add("060600");
                patchstrings.Add("0001");
                patchstrings.Add("00");

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Message speed set to maximum." + Environment.NewLine);
            }

            //Vowel Shuffle
            if (rndVowelsToggle.Checked) {
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

            //Celtland Drift
            if (rndDriftToggle.Checked) {
                patchstrings.Add("071B50");
                patchstrings.Add("0004");
                patchstrings.Add("3ff44444");

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Celtland Drift enabled." + Environment.NewLine);
            }

            //Random spell palette
            if (rndSpellPaletteToggle.Checked) {
                for (int i = 0; i < rndspellcolours.Length; i++) {
                    patchstrings.Add(library.allcolors[i * 2]);
                    patchstrings.Add("0002");
                    patchstrings.Add(library.allcolors[i * 2 + 1].Substring(0,2) + rndspellcolours[i].ToString("X2"));
                }
                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Spell palettes randomized." + Environment.NewLine);
            }

            //Random cloak palette
            if (rndCloakPaletteToggle.Checked) {
                for (int i = 0; i < library.cloaklocations.Length; i++) {
                    patchstrings.Add(library.cloaklocations[i]);
                    patchstrings.Add("0004");
                    patchstrings.Add(rndCloakViewTextbox.Text + "FF");
                }
                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Cloak colour randomized." + Environment.NewLine);
            }

            //Random BGM
            if (rndMusicShuffleToggle.Checked) {
                for (int i = 0; i < rndbgms.Length; i++) {
                    patchstrings.Add(library.bgmdata[i * 2]);
                    patchstrings.Add("0001");
                    patchstrings.Add(rndbgms[i].ToString("X2"));
                }
                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Background music randomized." + Environment.NewLine);
            }

            //Brian Colour palette
            if (rndBrianClothesToggle.Checked) {
                patchstrings.Add("86D5F0");
                patchstrings.Add("0980");
                patchstrings.Add(brianPaletteHex1);

                patchstrings.Add("86DFF0");
                patchstrings.Add("0980");
                patchstrings.Add(brianPaletteHex2);

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Brian palette set to random." + Environment.NewLine);
            }

            //Text improvements
            if (rndTextImprovementsToggle.Checked) {
                object[] hintdata = new object[2]; //save text translation
                string hintstring = library.newsavetext;
                hintdata = TranslateString(hintstring);
                int hintlen = (int)hintdata[1] + 2;

                patchstrings.Add("06B268"); //save text patch
                patchstrings.Add(hintlen.ToString("X4"));
                patchstrings.Add("A0C0" + (string)hintdata[0]);

                if (rndLostKeysToggle.Checked) {
                    hintdata = new object[2]; //LK Abbott Intro translation
                    hintstring = library.newAbbottIntroLK;
                    hintdata = TranslateString(hintstring);
                    hintlen = (int)hintdata[1] + 2;
                    patchstrings.Add("055428"); //LK Abbott Intro patch
                    patchstrings.Add(hintlen.ToString("X4"));
                    patchstrings.Add("A0C0" + (string)hintdata[0]);

                    hintdata = new object[2]; //LK Shannon Melrode translation
                    hintstring = library.newMelrodeShannonLK;
                    hintdata = TranslateString(hintstring);
                    hintlen = (int)hintdata[1] + 2;
                    patchstrings.Add("D305E0"); //LK Shannon Melrode patch
                    patchstrings.Add(hintlen.ToString("X4"));
                    patchstrings.Add("A0C0" + (string)hintdata[0]);
                }

                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Text improvements added." + Environment.NewLine);
            }

            //Combat XP display
            if (rndCombatExpToggle.Checked) {
                for (int i = 0; i < 14; i++) {
                    patchstrings.Add(library.combatexpdisplay[i * 2]);
                    patchstrings.Add("0002");
                    patchstrings.Add(library.combatexpdisplay[(i * 2) + 1]);
                }
            }

            //FINAL ASSEMBLY/OUTPUT

            //Merrow/Randomizer logo
            patchstrings.Add("DAC040");
            patchstrings.Add("393C"); //main menu logo address/length
            patchstrings.Add(library.merrowlogo);
            patchstrings.Add("DCE070");
            patchstrings.Add("393C"); //animation logo address/length
            patchstrings.Add(library.merrowlogo);

            //Initialize patch authoring by counting number of items to write
            int patchparts = patchstrings.Count();

            //Patching mode: PATCH Z64 FILE
            //rather than use patchstrings as IPS instructions, use the same values to overwrite directly
            if (expModePatchZ64.Checked && rndFileSelected) {

                //advance by 3 (address, length, and data) writing as you go
                for (int i = 0; i < patchparts; i += 3) { 
                    int targetAddr = Convert.ToInt32(patchstrings[i], 16);
                    byte[] targetData = StringToByteArray(patchstrings[i + 2]);
                    int targetLength = targetData.Length;
                    //Console.WriteLine(patchstrings[i]);
                    //Console.WriteLine(patchstrings[i + 1]);
                    //Console.WriteLine(patchstrings[i + 2]);
                    for (int j = 0; j < targetLength; j++) { 
                        rndFileBytes[targetAddr + j] = targetData[j];
                    }
                    if (debugoutput) { File.AppendAllText(filePath + fileName + "_spoiler.txt", patchstrings[i] + " " + targetLength.ToString("X4") + " " + patchstrings[i + 2] + Environment.NewLine); }
                }

                //write updated file bytearray to new file
                string thisFile = filePath + fileName + ".z64";
                File.WriteAllBytes(thisFile, rndFileBytes);
                var crctext = fix_crc(thisFile); //repair checksum
                filePath = filePath.Replace(@"/", @"\");   // explorer doesn't like front slashes
                System.Diagnostics.Process.Start("explorer.exe", Application.StartupPath + "\\Patches\\");
                rndErrorLabel.Text = "Z64 file creation complete. CRC repaired.";
                File.AppendAllText(filePath + fileName + "_spoiler.txt", "Z64 file patched. Checksum repaired." + Environment.NewLine);
            }

            //Patching mode: CREATE IPS PATCH
            if (expModePatchIPS.Checked) {

                //assemble patch
                patchbuild += headerHex;
                for (int ps = 0; ps < patchparts; ps++) {
                    patchbuild += patchstrings[ps];

                    if (debugoutput) {
                        if (ps % 3 == 0) { File.AppendAllText(filePath + fileName + "_spoiler.txt", patchstrings[ps] + " "); }
                        if (ps % 3 == 1) { File.AppendAllText(filePath + fileName + "_spoiler.txt", patchstrings[ps] + " "); }
                        if (ps % 3 == 2) { File.AppendAllText(filePath + fileName + "_spoiler.txt", patchstrings[ps] + Environment.NewLine); }
                    }
                }
                patchbuild += footerHex;

                //write patch to file
                patcharray = StringToByteArray(patchbuild);
                File.WriteAllBytes(filePath + fileName + ".ips", patcharray);
                filePath = filePath.Replace(@"/", @"\");   // explorer doesn't like front slashes
                System.Diagnostics.Process.Start("explorer.exe", Application.StartupPath + "\\Patches\\");
                rndErrorLabel.Text = "IPS Patch creation complete.";
                File.AppendAllText(filePath + fileName + "_spoiler.txt", "IPS patch generated." + Environment.NewLine);
            }



            //Verbose spoiler log is all down at the bottom to allow checks without spoilers.
            if (verboselog) {

                //move it down a bit
                if (rndSpellToggle.Checked ||
                    rndChestToggle.Checked ||
                    rndDropsToggle.Checked ||
                    rndGiftersToggle.Checked ||
                    rndWingsmithsToggle.Checked ||
                    rndMonsterStatsToggle.Checked ||
                    rndMonsterScaleToggle.Checked ||
                    rndBossOrderToggle.Checked) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + Environment.NewLine + Environment.NewLine + "----------EXTENDED SPOILER LOG BELOW----------" + Environment.NewLine + Environment.NewLine + Environment.NewLine);
                }

                //spell spoilers
                int spellscount = 0;
                if (rndSpellToggle.Checked) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "ALTERED SPELLS:" + Environment.NewLine);
                    foreach (string line in spoilerspells) {
                        if (spellscount % 15 == 0 && spellscount != 0) { File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine); }
                        File.AppendAllText(filePath + fileName + "_spoiler.txt", line + Environment.NewLine);
                        spellscount++;
                    }
                }

                //chest spoilers
                //lost keys regions: 15,15,7,33,18: 0-14,15-29,30-36,37-69,70-87
                int chestcount = 0;
                if (rndChestToggle.Checked) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "ALTERED CHESTS:" + Environment.NewLine);
                    foreach (string line in spoilerchests) {
                        if(rndLostKeysToggle.Checked) {
                            if (chestcount == 0) { File.AppendAllText(filePath + fileName + "_spoiler.txt", "-EARTH REGION-" + Environment.NewLine); }
                            if (chestcount == 15) { File.AppendAllText(filePath + fileName + "_spoiler.txt", "-WIND REGION-" + Environment.NewLine); }
                            if (chestcount == 30) { File.AppendAllText(filePath + fileName + "_spoiler.txt", "-WATER REGION-" + Environment.NewLine); }
                            if (chestcount == 37) { File.AppendAllText(filePath + fileName + "_spoiler.txt", "-FIRE REGION-" + Environment.NewLine); }
                            if (chestcount == 70) { File.AppendAllText(filePath + fileName + "_spoiler.txt", "-BOOK REGION-" + Environment.NewLine); }
                            chestcount++;
                        }
                        File.AppendAllText(filePath + fileName + "_spoiler.txt", line + Environment.NewLine);
                    }
                }

                //drop spoilers
                if (rndDropsToggle.Checked) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "ALTERED DROPS:" + Environment.NewLine);
                    foreach (string line in spoilerdrops) { File.AppendAllText(filePath + fileName + "_spoiler.txt", line + Environment.NewLine); }
                }

                //boss order/item spoilers
                if (rndBossOrderToggle.Checked || rndLostKeysToggle.Checked) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "ALTERED BOSS DROPS:" + Environment.NewLine);
                    foreach (string line in spoilerbossdrops) { File.AppendAllText(filePath + fileName + "_spoiler.txt", line + Environment.NewLine); }
                }

                //gifter spoilers
                if (rndGiftersToggle.Checked) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", /*Environment.NewLine +*/ "ALTERED GIFTS:" + Environment.NewLine); //extra newline removed
                    foreach (string line in spoilergifts) { File.AppendAllText(filePath + fileName + "_spoiler.txt", line + Environment.NewLine); }
                }

                //wingsmith spoilers
                if (rndWingsmithsToggle.Checked || rndIvoryWingsToggle.Checked) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "ALTERED WINGSMITHS:" + Environment.NewLine);
                    foreach (string line in spoilerwings) { File.AppendAllText(filePath + fileName + "_spoiler.txt", line + Environment.NewLine); }
                }

                //monster stat spoilers
                if (rndMonsterStatsToggle.Checked || rndMonsterScaleToggle.Checked || rndBossOrderToggle.Checked) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "ALTERED MONSTER STATS (HP, ATK, DEF, AGI, EXP, ELEMENT):" + Environment.NewLine);
                    foreach (string line in spoilerscales) { File.AppendAllText(filePath + fileName + "_spoiler.txt", line + Environment.NewLine); }
                }

                //extra/debug spoilers
                if (spoilerextra.Count > 0 && debugoutput) {
                    File.AppendAllText(filePath + fileName + "_spoiler.txt", Environment.NewLine + "DEBUG OUTPUT:" + Environment.NewLine);
                    foreach (string line in spoilerextra) { File.AppendAllText(filePath + fileName + "_spoiler.txt", line + Environment.NewLine); }
                }
            }

            //Unlock locked UI functionality at the end:
            writingPatch = false;
        }

        //ITEM RANDOMIZER LIST FUNCTIONS-------------------------------------------------

        //update item list by wiping and refilling
        public void itemListUpdate(ListView currentList, int currentIndex) {
            lockItemUpdates = true;
            if (currentIndex == 0) { itemListWipe(currentList); }
            if (currentIndex >= 1 && currentIndex <= 7) {
                itemListWipe(currentList);
                itemListSet(currentList, itemListSelect(currentIndex));
            }
            lockItemUpdates = false;
        }

        //grab the item array from the library
        public int[] itemListSelect(int option) { 
            if (option == 1) { return library.itemlist_standard; }
            if (option == 2) { return library.itemlist_standardwings; }
            if (option == 3) { return library.itemlist_standardgems; }
            if (option == 4) { return library.itemlist_chaosEX; } //changed to include key and book, by request
            if (option == 5) { return library.itemlist_wings; }
            if (option == 6) { return library.itemlist_gems; }
            if (option == 7) { return library.itemlist_wingsgems; }

            return library.itemlist_standard; //return this as a backup, should never happen
        }

        //wipe the specified item list so we can set it
        public void itemListWipe(ListView listID) { 
            for (int i = 0; i < listID.Items.Count; i++) {
                ListViewItem listChestItem = listID.Items[i];
                listChestItem.Checked = false;
            }
        }

        //set the specified item list
        public void itemListSet(ListView listID, int[] listArray) { 
            for (int i = 0; i < listArray.Length; i++) {
                ListViewItem listChestItem = listID.Items[listArray[i]];
                listChestItem.Checked = true;
            }
        }

        //turn target item list into hex string of IDs
        public string itemListSkim(ListView listID) {
            string hexencode = "";

            //write bools as binary string
            for (int i = 0; i < listID.Items.Count; i++) {
                ListViewItem listChestItem = listID.Items[i];
                //Console.WriteLine(listID.Name + " " + i.ToString());
                if (listChestItem != null) {
                    if (!listChestItem.Checked) { hexencode += "0"; }
                    if (listChestItem.Checked) { hexencode += "1"; }
                } else { hexencode += "0"; }
            }

            int bintohex = Convert.ToInt32(hexencode, 2); //binary string to int

            return bintohex.ToString("X8"); //return int as 8char hex string
        }

        //take skimmed hex string and unpack into item list
        public void itemListUnpack(ListView listID, string hexdecode) {
            bool[] unpacks = new bool[listID.Items.Count];
            int hextoint = Convert.ToInt32(hexdecode, 16);
            string inttobin = Convert.ToString(hextoint, 2);

            //if binary string is shorter than the number of items, there must be missing leading zeroes
            while (inttobin.Length < listID.Items.Count) { inttobin = "0" + inttobin; }

            //interpret binary string as a series of bools
            for (int i = 0; i < listID.Items.Count; i++) {
                ListViewItem listChestItem = listID.Items[i];
                if (inttobin[i] == '1') { listChestItem.Checked = true; }
                if (inttobin[i] == '0') { listChestItem.Checked = false; }
            }
        }
    }
}
