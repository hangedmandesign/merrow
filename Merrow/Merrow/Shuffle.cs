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

        //UNIFIED SHUFFLING/RANDOMIZING FUNCTION----------------------------------------------------------------

        public void Shuffling(bool crashpro) {
            if(shufflingnow) { return; }

            int k = 0;
            shufflingnow = true;
            unlockcount = 0;

            //REINITIATE RANDOM WITH SEED
            SysRand = new System.Random(rngseed);

            //SPELL SHUFFLING
            //'shuffles' is an array that will contain spells as indices, and modifiers as values. 
            //we start by filling it with -1 to denote unedited spells
            for (int i = 0; i < playerspells; i++) { shuffles[i] = -1; }

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

            //crash protection disabled - uses specified spell for all spells
            //don't need to unlock crashlocked spells because i'm not even using it anyway
            if (!crashpro) {
                for (int i = 0; i < playerspells; i++) {
                    shuffles[i] = rndSpellDropdown.SelectedIndex - 1;
                }
            }

            //crash protection enabled
            if (crashpro) {
                //before we can shuffle spells, we apply any rules/changes to the spell distribution
                //the crashlock array (which restricts spell combos to avoid crashes) is also used to restrict spells by element
                bool step = false;

                //first, reset H1/SS1 and AVA/MBR/WP3/LC to default crashlock lists, in case they've changed before.
                for (int i = 0; i < playerspells; i++) {
                    library.crashlock[(i * playerspells) + 32] = library.noearlyhealing[i];
                    library.crashlock[(i * playerspells) + 33] = library.noearlyhealing[i];

                    library.crashlock[(i * playerspells) + 23] = library.defaultavalanche[i];
                    library.crashlock[(i * playerspells) + 27] = library.defaultmagicbarrier[i];
                    library.crashlock[(i * playerspells) + 34] = library.defaultwaterpillar3[i];
                    library.crashlock[(i * playerspells) + 51] = library.defaultlargecutter[i];
                }

                //Spell Combination Fixes: Unlocking fixed crashlocked spells
                //This half of this function has to happen BEFORE spells are shuffled, using the default crashlock array.
                //The other half (applying the fixes themselves) has to happen in QuestPatchBuild.cs, after everything
                if (rndSpellToggle.Checked && rndSpellOverridesToggle.Checked) {
                    for (int i = 0; i < library.crashlock.Length; i++) {
                        //check the entire spellfixes array against the crashlock array, looking for fix cases
                        //a fix case is determined when a non-negative number exists in both slots (crash and fix)
                        if (library.crashlock[i] >= 0 && library.spellfixes[i] >= 0) {
                            //that spell's crashlock value is then set to -1, to show that it's safe to be shuffled
                            library.crashlock[i] = -1;
                            Console.WriteLine("Crashlock unlocked: Spell " + ((i - (i % 15)) / 15).ToString() + " Mod " + (i % 15).ToString());
                            unlockcount++;
                        }
                    }
                }

                //EARLY/EXTRA HEALING
                //pick which element healing spell(s) will be added to
                int[] healelements = new int[4];
                CountAndShuffleArray(healelements);

                //early healing: overwrite crashlock to give H1 only early slots
                if (rndSpellToggle.Checked && rndEarlyHealingToggle.Checked) {
                    for (int i = 0; i < playerspells; i++) {
                        library.crashlock[(i * playerspells) + 32] = library.earlyhealingmodifier[i];
                        //if extra healing is on, make sure SS1 and VT are limited to early healing too
                        if (rndExtraHealingToggle.Checked) { 
                            library.crashlock[(i * playerspells) + 33] = library.earlyhealingmodifier[i]; //SS1
                            library.crashlock[(i * playerspells) + 11] = library.earlyhealingmodifier[i]; //VT
                        }
                    }
                }

                //now lock H1/SS1/VT into healelement[0-2], by locking them out of the other three elements.
                //if Early/Extra are disabled, this has no negative effects, it just puts H1 into one element as it would anyway
                for (int i = 0; i < playerspells; i++) {
                    if (i < 15) { //fire
                        if (healelements[0] != 0) { library.crashlock[(i * playerspells) + 32] = 32; }
                        if (healelements[1] != 0 && rndExtraHealingToggle.Checked) { library.crashlock[(i * playerspells) + 33] = 33; }
                        if (healelements[2] != 0 && rndExtraHealingToggle.Checked) { library.crashlock[(i * playerspells) + 11] = 11; }
                    }
                    if (i >= 15 && i < 30) { //earth
                        if (healelements[0] != 1) { library.crashlock[(i * playerspells) + 32] = 32; }
                        if (healelements[1] != 1 && rndExtraHealingToggle.Checked) { library.crashlock[(i * playerspells) + 33] = 33; }
                        if (healelements[2] != 1 && rndExtraHealingToggle.Checked) { library.crashlock[(i * playerspells) + 11] = 11; }
                    }
                    if (i >= 30 && i < 45) { //water
                        if (healelements[0] != 2) { library.crashlock[(i * playerspells) + 32] = 32; }
                        if (healelements[1] != 2 && rndExtraHealingToggle.Checked) { library.crashlock[(i * playerspells) + 33] = 33; }
                        if (healelements[2] != 2 && rndExtraHealingToggle.Checked) { library.crashlock[(i * playerspells) + 11] = 11; }
                    }
                    if (i >= 45) { //wind
                        if (healelements[0] != 3) { library.crashlock[(i * playerspells) + 32] = 32; }
                        if (healelements[1] != 3 && rndExtraHealingToggle.Checked) { library.crashlock[(i * playerspells) + 33] = 33; }
                        if (healelements[2] != 3 && rndExtraHealingToggle.Checked) { library.crashlock[(i * playerspells) + 11] = 11; }
                    }
                }

                //DISTRIBUTE POWERFUL SPELLS
                //pick which elements each powerful spell will be locked into
                int[] powerelements = new int[4];
                CountAndShuffleArray(powerelements);

                //element lockout functions the same as healing does above
                if (rndSpellToggle.Checked && rndDistributeSpellsToggle.Checked) {
                    for (int i = 0; i < playerspells; i++) {
                        if (i < 15) { //fire
                            if (powerelements[0] != 0) { library.crashlock[(i * playerspells) + 23] = 23; }
                            if (powerelements[1] != 0) { library.crashlock[(i * playerspells) + 27] = 27; }
                            if (powerelements[2] != 0) { library.crashlock[(i * playerspells) + 34] = 34; }
                            if (powerelements[3] != 0) { library.crashlock[(i * playerspells) + 51] = 51; }
                        }
                        if (i >= 15 && i < 30) { //earth
                            if (powerelements[0] != 1) { library.crashlock[(i * playerspells) + 23] = 23; }
                            if (powerelements[1] != 1) { library.crashlock[(i * playerspells) + 27] = 27; }
                            if (powerelements[2] != 1) { library.crashlock[(i * playerspells) + 34] = 34; }
                            if (powerelements[3] != 1) { library.crashlock[(i * playerspells) + 51] = 51; }
                        }
                        if (i >= 30 && i < 45) { //water
                            if (powerelements[0] != 2) { library.crashlock[(i * playerspells) + 23] = 23; }
                            if (powerelements[1] != 2) { library.crashlock[(i * playerspells) + 27] = 27; }
                            if (powerelements[2] != 2) { library.crashlock[(i * playerspells) + 34] = 34; }
                            if (powerelements[3] != 2) { library.crashlock[(i * playerspells) + 51] = 51; }
                        }
                        if (i >= 45) { //wind
                            if (powerelements[0] != 3) { library.crashlock[(i * playerspells) + 23] = 23; }
                            if (powerelements[1] != 3) { library.crashlock[(i * playerspells) + 27] = 27; }
                            if (powerelements[2] != 3) { library.crashlock[(i * playerspells) + 34] = 34; }
                            if (powerelements[3] != 3) { library.crashlock[(i * playerspells) + 51] = 51; }
                        }
                    }
                }

                //SPELL SHUFFLE
                //the actual random spell distribution happens after all above restrictive rules are set

                //random spell distribution after all rules are set
                while (reorg.Count > 0) {
                    for (int i = playerspells - 1; i > -1; i--) { //spell number. inverted order to tend to put powerful spells at the end, rather than the start
                        step = false;

                        for (int j = 0; j < reorg.Count; j++) { //modifier number.
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

                //item softlock protection
                for (int i = 0; i < 6; i++) {
                    newitemspells[i] = shuffles[spellitemID[i]];
                    string rule = library.spells[(newitemspells[i] * 4) + 3].Substring(6, 2);

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

            //Console.WriteLine("seed:" + rngseed.ToString());
            for (int i = 0; i < playerspells; i++) {
                bool fiftyfifty = SysRand.NextDouble() > 0.5; ;
                if (rndSpellNamesDropdown.SelectedIndex == 1) { fiftyfifty = true; } //"Linear" option
                if (fiftyfifty) {
                    hintnames[i] = library.shuffleNames[i * 5];
                    hintnames[i] += " " + library.shuffleNames[(shuffles[i] * 5) + 1];
                }
                else {
                    hintnames[i] = library.shuffleNames[shuffles[i] * 5];
                    hintnames[i] += " " + library.shuffleNames[(i * 5) + 1];
                }
            }

            //RANDOM CHESTS

            //reinitiate chest list, in case user has gone back to Shuffle
            for (int j = 0; j < chests.Length; j++) { chests[j] = library.chestdata[j * 4 + 1]; }

            int[] itemset = itemListView1.CheckedIndices.Cast<int>().ToArray();
            int setlength = itemset.Length;

            if (rndChestDropdown.SelectedIndex >= 1 && setlength > 0) {
                for (int c = 0; c < chests.Length; c++) {
                    if (rndWeightedChestToggle.Checked && rndWeightedChestDropdown.SelectedIndex == 0) {
                        if (c < setlength) { //bottom of array is all weighted items
                            k = itemset[c];
                        }
                        else { //top of array is random items within set
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

            if (!rndLostKeysToggle.Checked) { drops = new int[67]; }
            if (rndLostKeysToggle.Checked) { drops = new int[74]; } //include all bosses but Mammon

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

            //after drop shuffling, fill out the new boss item array that'll get overridden
            //V50: beigis
            if (rndLostKeysToggle.Checked) {
                for (int i = 0; i < bossCount; i++) {
                    lostkeysbossitemlist[i] = drops[67 + i];
                }
                for (int i = 0; i < 67; i++) {
                    lostkeysdrops[i] = drops[i];
                }
            }

            //RANDOM GIFTS

            //option to exclude final shannons, forcing them to be vanilla. Either way, the game will prevent softlocks.
            if (!rndShuffleShannonToggle.Checked) { gifts = new int[10]; }
            if (rndShuffleShannonToggle.Checked && !rndLostKeysToggle.Checked) { gifts = new int[8]; } //8-indice array will just never overwrite the last two items, so they'll be vanilla.
            if (rndShuffleShannonToggle.Checked && rndLostKeysToggle.Checked) { gifts = new int[9]; } //9-indice array to leave only the key vanilla.

            //initiate item gifts list with vanilla items
            for (int l = 0; l < gifts.Length; l++) { gifts[l] = library.itemgranters[l * 2 + 1]; }

            //added toggle so that base gifter list is unshuffled if not enabled
            if (rndGiftersToggle.Checked) { 
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
            }

            //RANDOM WINGSMITHS

            //initiate wingsmiths list
            for (int l = 0; l < wings.Length; l++) { wings[l] = library.itemgranters[20 + (l * 2 + 1)]; } //20 ahead to get to wingsmiths

            //added the check for the toggle so that the default wing list goes unshuffled
            if (rndWingsmithsToggle.Checked) { 
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
            }

            //MONSTER STAT AND BOSS ORDER RANDOMIZATION

            //initiate monster stats again, in case this is happening for the nth time
            for (int i = 0; i < 450; i++) { newmonsterstats[i] = library.monsterstatvanilla[i]; }
            for (int i = 0; i < bossCount; i++) { newbossorder[i] = i; } //V30: changed to <6 to exclude beigis //v50: changed back to 7 to include beigis
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

                //newbosselem[1] = SysRand.Next(0, 4); //roll Mammon element
                //newmonsterstats[449] = newbosselem[1]; //assign into array for spoiler log
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
                            if (newmonsterstats[(currentmonster * 6) + m] == 0) { newmonsterstats[(currentmonster * 6) + m] = 1; } //if any stat would become 0, make it 1 instead
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
                        //V30: changed to i<6 check because it doesn't apply to mammon or beigis. V50: changed back to i<7 to include beigis.
                        if (m > 0 && m < 4 && i < bossCount) {
                            currentstat = library.bossbstratios[i * 4] * library.bossbstratios[(newbossorder[i] * 4) + m];
                        }

                        if (rndMonsterStatsToggle.Checked) {
                            //if randomized
                            double highend = ((library.avg_monster[(region * 7) + 5] + 20) / 3) * (1 + extremity); //+20/3 thirds variance
                            double lowend = ((library.avg_monster[(region * 7) + 6] + 20) / 3) * (1 - extremity); //+20/3 thirds variance
                            double variance = SysRand.NextDouble() * (highend - lowend) + lowend;
                            double modifiedstat = (currentstat * (variance / 10) * (difficultyscale));
                            newmonsterstats[(currentmonster * 6) + m] = (int)Math.Round(modifiedstat); 
                            if (newmonsterstats[(currentmonster * 6) + m] == 0) { newmonsterstats[(currentmonster * 6) + m] = 1; } //if any stat would become 0, make it 1 instead
                        }
                        else {
                            //if unrandomized, just redistribute the BST-adjusted stats
                            newmonsterstats[(currentmonster * 6) + m] = (int)Math.Round(currentstat); 
                            if (newmonsterstats[(currentmonster * 6) + m] == 0) { newmonsterstats[(currentmonster * 6) + m] = 1; } //if any stat would become 0, make it 1 instead
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

            //scale all combat xp regardless of other options
            if (rndEXPBoostTrackBar.Value != 4) {
                for (int i = 0; i < 75; i++) {
                    if (rndEXPBoostTrackBar.Value != 0) { 
                        newmonsterstats[(i * 6) + 4] = (int)Math.Round(newmonsterstats[(i * 6) + 4] * (difficultyscale) * (rndEXPBoostTrackBar.Value * 0.25f));
                        if (newmonsterstats[(i * 6) + 4] == 0) { newmonsterstats[(i * 6) + 4] = 1; } //if any stat would become 0, make it 1 instead
                    } else { newmonsterstats[(i * 6) + 4] = 0; } //unless exp is set to 0x, in which case make them all 0
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

            //monster flying effect shuffle
            if (rndFlyingShuffle.Checked) {
                for (int i = 0; i < 74; i++) {
                    rndflying[i] = SysRand.Next(2); //0 or 1
                }
            }

            //RULESET OVERRIDE: Lost Keys
            if (rndLostKeysToggle.Checked) {
                //21-07-11: after getting some feedback, we're going to try a couple changes.
                //move the black wings to the fire area, so skipping fargo becomes possible: DONE
                //remove wingsmiths from randomization so they can be vanilla/shuffled for more freedom: DONE
                //have shamwood exit send you somewhere more reasonable: DONE
                //lock castle door with fire ruby: DONE
                //open world alternative: DONE
                //maybe have random useless brannoch door send you to shamwood?

                //PROGRESSIVE option
                if (rndLostKeysDropdown.SelectedIndex == 0) {
                    //pick locations for boss items and wings, they cannot overlap
                    int[] earthval = new int[3];
                    int[] windval = new int[3];
                    int[] waterval = new int[1];
                    int[] fireval = new int[3];
                    int[] bookval = new int[1];
                    gemIDs = new int[5] { 20, 21, 22, 23, 24 }; //reset this so the spoiler log is back to default order
                    hints = new int[5] { -1, -1, -1, -1, -1 };

                    if (rndIvoryWingsToggle.Checked) {
                        earthval = new int[2];
                        fireval = new int[4];
                    }

                    int[] earthitemIDs = { 20, 14, 15 }; //earth, white, yellow
                    int[] winditemIDs = { 21, 16, 17 }; //wind, blue, green
                    int[] wateritemIDs = { 22 }; //water
                    int[] fireitemIDs = { 23, 18, 19 }; //fire, red, black
                    int[] bookitemIDs = { 24 }; //book

                    if (rndIvoryWingsToggle.Checked) {
                        earthitemIDs = new int[2] { 20, 15 }; //earth, yellow
                        fireitemIDs = new int[4] { 23, 14, 18, 19 }; //fire, ivory, red, black
                    }

                    //populate ascending arrays to be shuffled, to guarantee no overlap. don't need arrays for water or book because they're only one item.
                    int[] earthlocs = new int[19];
                    int[] windlocs = new int[18];
                    int[] firelocs = new int[37];
                    CountAndShuffleArray(earthlocs);
                    CountAndShuffleArray(windlocs);
                    CountAndShuffleArray(firelocs);

                    //move data across. don't need to account for ivory wings manually in for loop if i check for array length
                    for (int i = 0; i < earthval.Length; i++) { earthval[i] = earthlocs[i]; }

                    for (int i = 0; i < windval.Length; i++) { windval[i] = windlocs[i]; }

                    //since there's only one water, just roll it
                    waterval[0] = SysRand.Next(26); //water first 18/20 overlaps with wind, which is accounted for below
                    while (windval[2] == waterval[0] || windval[1] == waterval[0] || windval[0] == waterval[0]) { waterval[0] = SysRand.Next(26); }

                    for (int i = 0; i < fireval.Length; i++) { fireval[i] = firelocs[i]; }

                    if (!rndFireBookToggle.Checked) { bookval[0] = SysRand.Next(22); } //21 for nobeigis //22 for beigis
                    else {
                        bookval[0] = SysRand.Next(58); 
                        while (fireval[3] == bookval[0] || fireval[2] == bookval[0] || fireval[1] == bookval[0] || fireval[0] == bookval[0]) { bookval[0] = SysRand.Next(58); }
                    }

                    //distribute individual boss items and wings according to values.

                    //earth orb:    1 boss, 15 chests, 3 gifters, 2 wingsmiths -     0,1-15,16-18, //X19-20
                    //wind jade:    1 boss, 15 chests, 2 gifters, 2 wingsmiths -     0,1-15,16-17, //X18-19
                    //water jewel:  above + 1 boss, 7 chests -	    		         0,1-15,16-17,18,19-25 //X18-19,20,21-27
                    //fire ruby:    2 bosses, 33 chests, 2 gifters, 1 wingsmith	-    0-1,2-34,35-36, //X37
                    //eletale book: 2 bosses, 18 chests, 2 gifters, 1 wingsmith	-    0-1,2-19,20-21, //X21

                    for (int i = 0; i < earthval.Length; i++) { //earth gem and wings
                        if (earthval[i] == 0) { lostkeysbossitemlist[0] = earthitemIDs[i]; if (hints[0] == -1) { hints[0] = earthval[i]; } } //solvaring
                        if (earthval[i] >= 1 && earthval[i] <= 15) { chests[library.area_earth[earthval[i]]] = earthitemIDs[i]; if (hints[0] == -1) { hints[0] = earthval[i]; } } //chests
                        if (earthval[i] >= 16 && earthval[i] <= 18) { gifts[library.area_earth[earthval[i]]] = earthitemIDs[i]; if (hints[0] == -1) { hints[0] = earthval[i]; } } //gifts
                    }

                    for (int i = 0; i < windval.Length; i++) { //wind gem and wings
                        if (windval[i] == 0) { lostkeysbossitemlist[1] = winditemIDs[i]; if (hints[1] == -1) { hints[1] = windval[i]; } } //zelse
                        if (windval[i] >= 1 && windval[i] <= 15) { chests[library.area_wind[windval[i]]] = winditemIDs[i]; if (hints[1] == -1) { hints[1] = windval[i]; } } //chests
                        if (windval[i] >= 16 && windval[i] <= 17) { gifts[library.area_wind[windval[i]]] = winditemIDs[i]; if (hints[1] == -1) { hints[1] = windval[i]; } } //gifts
                    }

                    //water gem
                    if (waterval[0] == 0) { lostkeysbossitemlist[1] = wateritemIDs[0]; if (hints[2] == -1) { hints[2] = waterval[0]; } } //zelse
                    if (waterval[0] >= 1 && waterval[0] <= 15) { chests[library.area_water_nowings[waterval[0]]] = wateritemIDs[0]; if (hints[2] == -1) { hints[2] = waterval[0]; } } //chests
                    if (waterval[0] >= 16 && waterval[0] <= 17) { gifts[library.area_water_nowings[waterval[0]]] = wateritemIDs[0]; if (hints[2] == -1) { hints[2] = waterval[0]; } } //gifts
                    if (waterval[0] == 18) { lostkeysbossitemlist[2] = wateritemIDs[0]; if (hints[2] == -1) { hints[2] = waterval[0]; } } //nepty
                    if (waterval[0] >= 19 && waterval[0] <= 25) { chests[library.area_water_nowings[waterval[0]]] = wateritemIDs[0]; if (hints[2] == -1) { hints[2] = waterval[0]; } } //water-only chests

                    for (int i = 0; i < fireval.Length; i++) { //fire gem and wings
                        if (fireval[i] == 0) { lostkeysbossitemlist[3] = fireitemIDs[i]; if (hints[3] == -1) { hints[3] = fireval[i]; } } //shilf
                        if (fireval[i] == 1) { lostkeysbossitemlist[4] = fireitemIDs[i]; if (hints[3] == -1) { hints[3] = fireval[i]; } } //fargo
                        if (fireval[i] >= 2 && fireval[i] <= 34) { chests[library.area_fire[fireval[i]]] = fireitemIDs[i]; if (hints[3] == -1) { hints[3] = fireval[i]; } } //chests
                        if (fireval[i] >= 35 && fireval[i] <= 36) { gifts[library.area_fire[fireval[i]]] = fireitemIDs[i]; if (hints[3] == -1) { hints[3] = fireval[i]; } } //gifts
                    }

                    //book
                    if (!rndFireBookToggle.Checked) { 
                       if (bookval[0] == 0) { lostkeysbossitemlist[5] = bookitemIDs[0]; if (hints[4] == -1) { hints[4] = bookval[0]; } } //guilty
                       if (bookval[0] == 1) { lostkeysbossitemlist[6] = bookitemIDs[0]; hints[4] = 1; } //beigis
                       if (bookval[0] >= 2 && bookval[0] <= 19) { chests[library.area_book[bookval[0]]] = bookitemIDs[0]; if (hints[4] == -1) { hints[4] = bookval[0]; } } //chests
                       if (bookval[0] >= 20 && bookval[0] <= 21) { gifts[library.area_book[bookval[0]]] = bookitemIDs[0]; if (hints[4] == -1) { hints[4] = bookval[0]; } } //gifts
                    } else {
                        if (bookval[0] == 0) { lostkeysbossitemlist[3] = bookitemIDs[0]; if (hints[4] == -1) { hints[4] = bookval[0]; } } //shilf
                        if (bookval[0] == 1) { lostkeysbossitemlist[4] = bookitemIDs[0]; if (hints[4] == -1) { hints[4] = bookval[0]; } } //fargo
                        if (bookval[0] >= 2 && bookval[0] <= 34) { chests[library.area_fire[bookval[0]]] = bookitemIDs[0]; if (hints[4] == -1) { hints[4] = bookval[0]; } } //chests
                        if (bookval[0] >= 35 && bookval[0] <= 36) { gifts[library.area_fire[bookval[0]]] = bookitemIDs[0]; if (hints[4] == -1) { hints[4] = bookval[0]; } } //gifts
                        if (bookval[0] == 37) { lostkeysbossitemlist[5] = bookitemIDs[0]; if (hints[4] == -1) { hints[4] = bookval[0]; } } //guilty
                        if (bookval[0] == 38) { lostkeysbossitemlist[6] = bookitemIDs[0]; hints[4] = 1; } //beigis
                        if (bookval[0] >= 39 && bookval[0] <= 56) { chests[library.area_bookf_beigis_nowings[bookval[0]]] = bookitemIDs[0]; if (hints[4] == -1) { hints[4] = bookval[0]; } } //chests
                        if (bookval[0] >= 57 && bookval[0] <= 58) { gifts[library.area_bookf_beigis_nowings[bookval[0]]] = bookitemIDs[0]; if (hints[4] == -1) { hints[4] = bookval[0]; } } //gifts
                    }

                    //Console.WriteLine(library.area_wind[windval[0]] + " " + library.area_wind[windval[1]] + " " + library.area_wind[windval[2]]);
                    //Console.WriteLine(library.area_water_nowings[waterval[0]]);
                    //Console.WriteLine(library.area_fire[fireval[0]] + " " + library.area_fire[fireval[1]] + " " + library.area_fire[fireval[2]] + " " + library.area_fire[fireval[3]]);
                    //Console.WriteLine(library.area_book_nobeigis[bookval[0]]);
                }

                //OPEN WORLD option
                if (rndLostKeysDropdown.SelectedIndex == 1) {
                    //pick locations for boss items and wings, they cannot overlap
                    int[] possiblevals = new int[104]; //103 for nobeigis //104 for beigis
                    int[] gemvals = new int[5] { -1, -1, -1, -1, -1 };
                    int[] wingvals = new int[6];
                    int[] wingIDs = { 14, 15, 16, 17, 18, 19 };
                    gemIDs = new int[5] { 20, 21, 22, 23, 24 };
                    hints = new int[5] { -1, -1, -1, -1, -1 };

                    //fill and randomize possiblevals to guarantee unique values
                    CountAndShuffleArray(possiblevals);

                    //populate wings from possible values
                    for (int i = 0; i < 6; i++) {
                        wingvals[i] = possiblevals[i];
                    }

                    //randomize gem order
                    ShuffleArray(gemIDs);

                    //find the next five entries in possiblevals that match each region's range. then just place them like wings.
                    for (int i = 6; i < possiblevals.Length; i++) {
                        if (gemvals[0] == -1) {
                            if (possiblevals[i] >= 0 && possiblevals[i] <= 18) { gemvals[0] = possiblevals[i]; hints[0] = possiblevals[i]; }
                        }
                        if (gemvals[1] == -1) {
                            if (possiblevals[i] >= 19 && possiblevals[i] <= 36 && possiblevals[i] != gemvals[2]) { gemvals[1] = possiblevals[i]; hints[1] = possiblevals[i] - 19; }
                        }
                        if (gemvals[2] == -1) {
                            if (possiblevals[i] >= 19 && possiblevals[i] <= 44 && possiblevals[i] != gemvals[1]) { gemvals[2] = possiblevals[i]; hints[2] = possiblevals[i] - 19; }
                        }
                        if (!rndFireBookToggle.Checked) { 
                            if (gemvals[3] == -1) {
                                if (possiblevals[i] >= 45 && possiblevals[i] <= 81) { gemvals[3] = possiblevals[i]; hints[3] = possiblevals[i] - 45; }
                            }
                            if (gemvals[4] == -1) { //102 for no beigis //103 for beigis
                                if (possiblevals[i] >= 82 && possiblevals[i] <= 103) { gemvals[4] = possiblevals[i]; hints[4] = possiblevals[i] - 82; }
                            }
                        } else {
                            if (gemvals[3] == -1) {
                                if (possiblevals[i] >= 45 && possiblevals[i] <= 81 && possiblevals[i] != gemvals[4]) { gemvals[3] = possiblevals[i]; hints[3] = possiblevals[i] - 45; }
                            }
                            if (gemvals[4] == -1) {
                                if (possiblevals[i] >= 45 && possiblevals[i] <= 103 && possiblevals[i] != gemvals[3]) { gemvals[4] = possiblevals[i]; hints[4] = possiblevals[i] - 45; }
                            }
                        }

                        if (gemvals[0] != -1 && gemvals[1] != -1 && gemvals[2] != -1 && gemvals[3] != -1 && gemvals[4] != -1) { break; }
                    }

                    Console.WriteLine(gemvals[0] + " " + gemvals[1] + " " + gemvals[2] + " " + gemvals[3] + " " + gemvals[4]);

                    //now that gem order is randomized, pick an area for each one
                    //gemvals[0] = SysRand.Next(19);
                    //gemvals[1] = SysRand.Next(18);
                    //gemvals[2] = SysRand.Next(26);
                    //while (gemvals[2] == gemvals[1]) { gemvals[2] = SysRand.Next(26); } //water first 18 overlaps with wind
                    //gemvals[3] = SysRand.Next(37);
                    //gemvals[4] = SysRand.Next(22);

                    //distribute individual boss items and wings according to values.

                    //earth area:    1 boss, 15 chests, 3 gifters -                         0,1-15,16-18
                    //wind/water:    1 boss, 15 chests, 2 gifters, 1 boss, 7 chests -       19,20-34,35-36,37,38-44 		         
                    //fire area:     2 bosses, 33 chests, 2 gifters	-                       45-46,47-79,80-81
                    //book area:     2 bosses, 18 chests, 2 gifters	-                       82-83,84-101,102-103

                    //put wings anywhere
                    for (int i = 0; i < 6; i++) {
                        if (wingvals[i] == 0) { lostkeysbossitemlist[0] = wingIDs[i]; } //solvaring
                        if (wingvals[i] >= 1 && wingvals[i] <= 15) { chests[library.area_open_beigis[wingvals[i]]] = wingIDs[i]; } //chests
                        if (wingvals[i] >= 16 && wingvals[i] <= 18) { gifts[library.area_open_beigis[wingvals[i]]] = wingIDs[i]; } //gifts
                        if (wingvals[i] == 19) { lostkeysbossitemlist[1] = wingIDs[i]; } //zelse
                        if (wingvals[i] >= 20 && wingvals[i] <= 34) { chests[library.area_open_beigis[wingvals[i]]] = wingIDs[i]; } //chests
                        if (wingvals[i] >= 35 && wingvals[i] <= 36) { gifts[library.area_open_beigis[wingvals[i]]] = wingIDs[i]; } //gifts
                        if (wingvals[i] == 37) { lostkeysbossitemlist[2] = wingIDs[i]; } //nepty
                        if (wingvals[i] >= 38 && wingvals[i] <= 44) { chests[library.area_open_beigis[wingvals[i]]] = wingIDs[i]; } //water-only chests
                        if (wingvals[i] == 45) { lostkeysbossitemlist[3] = wingIDs[i]; } //shilf
                        if (wingvals[i] == 46) { lostkeysbossitemlist[4] = wingIDs[i]; } //fargo
                        if (wingvals[i] >= 47 && wingvals[i] <= 79) { chests[library.area_open_beigis[wingvals[i]]] = wingIDs[i]; } //chests
                        if (wingvals[i] >= 80 && wingvals[i] <= 81) { gifts[library.area_open_beigis[wingvals[i]]] = wingIDs[i]; } //gifts
                        if (wingvals[i] == 82) { lostkeysbossitemlist[5] = wingIDs[i]; } //guilty
                        if (wingvals[i] == 83) { lostkeysbossitemlist[6] = wingIDs[i]; } //beigis
                        if (wingvals[i] >= 84 && wingvals[i] <= 101) { chests[library.area_open_beigis[wingvals[i]]] = wingIDs[i]; } //chests
                        if (wingvals[i] >= 102 && wingvals[i] <= 103) { gifts[library.area_open_beigis[wingvals[i]]] = wingIDs[i]; } //gifts
                    }

                    //put gems anywhere
                    for (int i = 0; i < 5; i++) {
                        if (gemvals[i] == 0) { lostkeysbossitemlist[0] = gemIDs[i]; } //solvaring
                        if (gemvals[i] >= 1 && gemvals[i] <= 15) { chests[library.area_open_beigis[gemvals[i]]] = gemIDs[i]; } //chests
                        if (gemvals[i] >= 16 && gemvals[i] <= 18) { gifts[library.area_open_beigis[gemvals[i]]] = gemIDs[i]; } //gifts
                        if (gemvals[i] == 19) { lostkeysbossitemlist[1] = gemIDs[i]; } //zelse
                        if (gemvals[i] >= 20 && gemvals[i] <= 34) { chests[library.area_open_beigis[gemvals[i]]] = gemIDs[i]; } //chests
                        if (gemvals[i] >= 35 && gemvals[i] <= 36) { gifts[library.area_open_beigis[gemvals[i]]] = gemIDs[i]; } //gifts
                        if (gemvals[i] == 37) { lostkeysbossitemlist[2] = gemIDs[i]; } //nepty
                        if (gemvals[i] >= 38 && gemvals[i] <= 44) { chests[library.area_open_beigis[gemvals[i]]] = gemIDs[i]; } //water-only chests
                        if (gemvals[i] == 45) { lostkeysbossitemlist[3] = gemIDs[i]; } //shilf
                        if (gemvals[i] == 46) { lostkeysbossitemlist[4] = gemIDs[i]; } //fargo
                        if (gemvals[i] >= 47 && gemvals[i] <= 79) { chests[library.area_open_beigis[gemvals[i]]] = gemIDs[i]; } //chests
                        if (gemvals[i] >= 80 && gemvals[i] <= 81) { gifts[library.area_open_beigis[gemvals[i]]] = gemIDs[i]; } //gifts
                        if (gemvals[i] == 82) { lostkeysbossitemlist[5] = gemIDs[i]; } //guilty
                        if (gemvals[i] == 83) { lostkeysbossitemlist[6] = gemIDs[i]; } //beigis
                        if (gemvals[i] >= 84 && gemvals[i] <= 101) { chests[library.area_open_beigis[gemvals[i]]] = gemIDs[i]; } //chests
                        if (gemvals[i] >= 102 && gemvals[i] <= 103) { gifts[library.area_open_beigis[gemvals[i]]] = gemIDs[i]; } //gifts
                    }
                }

                //flip ten coins for hint data randomization
                for (int i = 0; i < hintcoins.Length; i++) {
                    hintcoins[i] = SysRand.Next(2);
                }
            }

            //--------------COSMETIC OPTIONS BELOW THIS LINE-----------------------------------

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

            //RANDOM BGM
            //random bgm needs to be a hex value between 00 and 2a (42) and cannot be 1b-1e,2a-2b (27-30,42-43)
            for (int i = 0; i < rndbgms.Length; i++) {
                rndbgms[i] = SysRand.Next(38); //0-37. 0-26 is first 26 tracks, 27-37 is last 11 tracks 
                if (rndbgms[i] >= 27) { rndbgms[i] = rndbgms[i] + 4; } //add 4 because last 11 are numbered 31-41 in data
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

            //TEXT COLOUR SHUFFLING
            lightdark = SysRand.NextDouble() > 0.5;
            hueOffset = SysRand.NextDouble() * 360;

            //pick random hue offset. don't re-randomize if CUSTOM is selected.
            if (rndTextPaletteDropdown.SelectedIndex == 0) {
                lightdarkicon = lightdark;
                if (lightdarkicon) { rndTextLightDark.Text = "◌"; }
                else { rndTextLightDark.Text = "●"; }

                rndTextViewTextbox.Text = Convert.ToString((int)Math.Round(hueOffset));

                //convert base colours from hex, and hue shift
                if (lightdark) {
                    texPal1 = HueShift(RGBAToColor(library.baseRedTextPalette[0]), hueOffset);
                    texPal2 = HueShift(RGBAToColor(library.baseRedTextPalette[1]), hueOffset);
                    texPal3 = HueShift(RGBAToColor(library.baseRedTextPalette[2]), hueOffset);
                }
                else {
                    texPal1 = HueShift(RGBAToColor(library.baseDarkTextPalette[0]), hueOffset);
                    texPal2 = HueShift(RGBAToColor(library.baseDarkTextPalette[1]), hueOffset);
                    texPal3 = HueShift(RGBAToColor(library.baseDarkTextPalette[2]), hueOffset);
                }

                textPaletteHex = ColorToHex(texPal1) + ColorToHex(texPal2) + ColorToHex(texPal3);

                rndColourPanel2.BackColor = texPal1;
                rndColourPanel3.BackColor = texPal2;
                rndColourPanel4.BackColor = texPal3;
            }

            //if CUSTOM is selected, update the palette view
            if (rndTextPaletteDropdown.SelectedIndex == 1) {
                lightdark = lightdarkicon;
                hueOffset = Convert.ToDouble(rndTextViewTextbox.Text);

                //convert base colours from hex, and hue shift
                if (lightdark) {
                    texPal1 = HueShift(RGBAToColor(library.baseRedTextPalette[0]), hueOffset);
                    texPal2 = HueShift(RGBAToColor(library.baseRedTextPalette[1]), hueOffset);
                    texPal3 = HueShift(RGBAToColor(library.baseRedTextPalette[2]), hueOffset);
                }
                else {
                    texPal1 = HueShift(RGBAToColor(library.baseDarkTextPalette[0]), hueOffset);
                    texPal2 = HueShift(RGBAToColor(library.baseDarkTextPalette[1]), hueOffset);
                    texPal3 = HueShift(RGBAToColor(library.baseDarkTextPalette[2]), hueOffset);
                }

                textPaletteHex = ColorToHex(texPal1) + ColorToHex(texPal2) + ColorToHex(texPal3);

                rndColourPanel2.BackColor = texPal1;
                rndColourPanel3.BackColor = texPal2;
                rndColourPanel4.BackColor = texPal3;
            }

            lasttextpaletteoffset = (int)Math.Round(hueOffset);
            randbasetextpalette = SysRand.Next(3, 7);

            //STAFF COLOUR SHUFFLING
            hueOffset = SysRand.NextDouble() * 360;

            //pick random hue offset. don't re-randomize if CUSTOM is selected.
            if (rndStaffPaletteDropdown.SelectedIndex == 0) {
                rndStaffViewTextbox.Text = Convert.ToString((int)Math.Round(hueOffset));

                staffPaletteHex = "";
                string[] staffbytes = new string[768];
                for (int i = 0; i < 768; i++) {
                    staffbytes[i] = library.stafftexture.Substring(0 + (i * 4), 4);
                    staffPaletteHex += ColorToHex(HueShift(RGBAToColor(staffbytes[i]), hueOffset));
                }

                rndColourPanelS.BackColor = HueShift(RGBAToColor(staffbytes[36]), hueOffset);
            }

            //if CUSTOM is selected, ignore the random generation and override the palette view
            if (rndStaffPaletteDropdown.SelectedIndex == 1) {
                hueOffset = Convert.ToDouble(rndStaffViewTextbox.Text);

                staffPaletteHex = "";
                string[] staffbytes = new string[768];
                for (int i = 0; i < 768; i++) {
                    staffbytes[i] = library.stafftexture.Substring(0 + (i * 4), 4);
                    staffPaletteHex += ColorToHex(HueShift(RGBAToColor(staffbytes[i]), hueOffset));
                }

                rndColourPanelS.BackColor = HueShift(RGBAToColor(staffbytes[30]), hueOffset);
            }

            laststaffpaletteoffset = (int)Math.Round(hueOffset);

            //CLOAK COLOUR SHUFFLING
            string randcolor = String.Format("#{0:X6}", SysRand.Next(0x1000000)); // = random "#A197B9"

            //pick random color. don't re-randomize if CUSTOM is selected.
            if (rndCloakPaletteDropdown.SelectedIndex == 0) {
                rndCloakViewTextbox.Text = randcolor;
                rndColourPanelC.BackColor = System.Drawing.ColorTranslator.FromHtml(randcolor);
            }

            //if CUSTOM is selected, ignore the random generation and override the palette view
            if (rndCloakPaletteDropdown.SelectedIndex == 1) {
                randcolor = "#" + rndCloakViewTextbox.Text.PadRight(6, '8');
                rndCloakViewTextbox.Text = rndCloakViewTextbox.Text.PadRight(6, '8');
                rndColourPanelC.BackColor = System.Drawing.ColorTranslator.FromHtml(randcolor);
            }

            lastcloakpalettecolor = System.Drawing.ColorTranslator.FromHtml(randcolor);

            //SPELL PALETTE RANDOMIZATION
            for (int i = 0; i < rndspellcolours.Length; i++) {
                rndspellcolours[i] = SysRand.Next(17) + 1;
            }

            //BRIAN COLOUR SHUFFLING
            if (rndBrianClothesToggle.Checked) {
                hueOffset = SysRand.NextDouble() * 360;

                brianPaletteHex1 = "";
                string[] brian1bytes = new string[1216];
                for (int i = 0; i < brian1bytes.Length; i++) {
                    brian1bytes[i] = library.briantexture1.Substring(0 + (i * 4), 4);
                    brianPaletteHex1 += ColorToHex(HueShift(RGBAToColor(brian1bytes[i]), hueOffset));
                }

                hueOffset = SysRand.NextDouble() * 360;

                brianPaletteHex2 = "";
                string[] brian2bytes = new string[1328];
                for (int i = 0; i < brian1bytes.Length; i++) {
                    brian2bytes[i] = library.briantexture2.Substring(0 + (i * 4), 4);
                    brianPaletteHex2 += ColorToHex(HueShift(RGBAToColor(brian2bytes[i]), hueOffset));
                }
            }

            shufflingnow = false;
        }
    }
}
