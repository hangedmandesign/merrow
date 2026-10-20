using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Merrow.Util
{
    public enum SpellReplacementLogic
    {
        None = 0, 
        RandomSpell = 1,
        RandomBuff = 2,
        RandomDebuff = 3,
        RandomStatusSpell = 4,
        RandomDamageSpell = 5,
        SpecificSpell = 6,
    }

    public enum SpellReplacementPreset
    {
        None = 0,
        Recommended = 1,
        ReplaceSimilar = 2,
        BubbleOnly = 3,
        Random = 4,
    }

    public struct SpellReplacementEntry
    {
        public SpellNameEnum spellBeingAdded;
        public SpellReplacementLogic logic;
        public SpellNameEnum spellBeingReplaced;

        public int GetSortPriority()
        {
            switch (this.logic)
            {
                case SpellReplacementLogic.SpecificSpell: return 0;
                case SpellReplacementLogic.RandomBuff: return 1;
                case SpellReplacementLogic.RandomDebuff: return 2;
                case SpellReplacementLogic.RandomStatusSpell: return 3;
                case SpellReplacementLogic.RandomDamageSpell: return 4;
                case SpellReplacementLogic.RandomSpell: return 5;
                
                default: 
                    return int.MinValue;
            };
        }
    }

    public class BossSpellReplacementWorkflow
    {
        public event Action<BossSpellReplacementWorkflow, SpellReplacementPreset> onPresetApplied;

        private Dictionary<SpellNameEnum, SpellReplacementLogic> logicMapping;
        private Dictionary<SpellNameEnum, SpellNameEnum> explicitReplacementMapping;
        private Dictionary<SpellNameEnum, bool> enabledMapping;

        private List<SpellNameEnum> availableReplacementCache;
        private List<SpellReplacementEntry> entryResultCache;

        private const int BOSS_SPELL_INDEX_START = 60;
        private const int BOSS_SPELL_INDEX_END = 76;

        public BossSpellReplacementWorkflow()
        {
            this.logicMapping = new Dictionary<SpellNameEnum, SpellReplacementLogic>();
            this.explicitReplacementMapping = new Dictionary<SpellNameEnum, SpellNameEnum>();
            this.enabledMapping = new Dictionary<SpellNameEnum, bool>();

            this.availableReplacementCache = new List<SpellNameEnum>();
            this.entryResultCache = new List<SpellReplacementEntry>();
        }

        public bool IsBeingAdded(SpellNameEnum spellName)
        {
            if (this.enabledMapping.ContainsKey(spellName))
            {
                return true;
            }
            return false;
        }

        public void SetSpellUsageActive(SpellNameEnum spellName, bool isActive)
        {
            if (this.enabledMapping.ContainsKey(spellName))
            {
                this.enabledMapping[spellName] = isActive;
                return;
            }

            this.enabledMapping.Add(spellName, isActive);
        }

        public List<SpellNameEnum> GetSpellsAvailableAsReplacements()
        {
            var availableSpells = this.GetAllPlayerSpellEnums();

            for (int k=0; k<availableSpells.Count; k++)
            {
                var spell = availableSpells[k];
                var alreadyMapped = this.explicitReplacementMapping.ContainsValue(spell);
                if (alreadyMapped)
                {
                    availableSpells.RemoveAt(k--);
                    continue;
                }
            }

            return availableSpells;
        }

        private List<SpellNameEnum> GetAllPlayerSpellEnums()
        {
            this.availableReplacementCache.Clear();
            for (int k=0; k<BOSS_SPELL_INDEX_START; k++)
            {
                this.availableReplacementCache.Add((SpellNameEnum)k);
            }
            return this.availableReplacementCache;
        }

        public List<SpellReplacementEntry> GetPlannedSpellReplacementData()
        {
            this.entryResultCache.Clear();

            for (int k = BOSS_SPELL_INDEX_START; k <= BOSS_SPELL_INDEX_END; k++)
            {
                var spellEnum = (SpellNameEnum)k;
                
                if (this.logicMapping.ContainsKey(spellEnum) == false)
                {
                    this.entryResultCache.Add(new SpellReplacementEntry
                    {
                        logic = SpellReplacementLogic.None,
                        spellBeingAdded = spellEnum
                    });
                    continue;
                }

                var logic = this.logicMapping[spellEnum];
                var replacementData = new SpellReplacementEntry
                {
                    spellBeingAdded = spellEnum,
                    logic = logic,
                };

                var hasExplicitReplacement = this.explicitReplacementMapping.ContainsKey(spellEnum);
                if (hasExplicitReplacement)
                    replacementData.spellBeingReplaced = this.explicitReplacementMapping[spellEnum];

                this.entryResultCache.Add(replacementData);
            }

            return this.entryResultCache;
        }

        public List<SpellReplacementEntry> GetVerbatimSpellReplacementOperations(bool useSpecialSpells = false)
        {
            this.entryResultCache.Clear();

            var safeBossSpells = new List<SpellNameEnum>(SpellDefinitions.BossSpellsCastableAnywhere);

            var availableReplacementSpells = this.GetAllPlayerSpellEnums();
            var selectedReplacements = this.logicMapping.Values;

            foreach (var replacementPair in this.logicMapping)
            {
                var spellToAdd = replacementPair.Key;
                var replacementLogic = replacementPair.Value;

                var isSpecialBossSpell = safeBossSpells.Contains(spellToAdd) == false;
                if (isSpecialBossSpell && !useSpecialSpells)
                {
                    continue;
                }

                if (replacementLogic == SpellReplacementLogic.None)
                    continue;

                this.entryResultCache.Add(new SpellReplacementEntry
                {
                    spellBeingAdded = spellToAdd,
                    logic = replacementLogic
                });
            }

            // Sort things by priority, this will help ensure that narrower rules 
            this.entryResultCache.Sort((a, b) =>
            {
                var weightA = a.GetSortPriority();
                var weightB = b.GetSortPriority();

                return weightA.CompareTo(weightB);
            });

            for (int k=0; k<this.entryResultCache.Count; k++)
            {
                var entry = this.entryResultCache[k];

                var foundReplacement = this.TryGetReplacementSpell(entry.spellBeingAdded, entry.logic, availableReplacementSpells, out var replacementSpell);
                if (foundReplacement)
                {
                    entry.spellBeingReplaced = replacementSpell;

                    Console.WriteLine($"[Boss Spells] Replacing {replacementSpell} with {entry.spellBeingAdded}");
                }
                else
                {
                    throw (new Exception(($"ERROR -- Could not find replacement spell with current setup:{entry.spellBeingReplaced} -> {entry.logic}")));
                }

                this.entryResultCache[k] = entry;
            }

            return this.entryResultCache;
        }

        private bool TryGetReplacementSpell(SpellNameEnum spellBeingAdded, SpellReplacementLogic logic, List<SpellNameEnum> avilableReplacements, out SpellNameEnum chosenReplacement)
        {
            switch (logic)
            {
                default:
                case SpellReplacementLogic.RandomSpell:
                    return this.InternalTryGetReplacementSpell(spellBeingAdded, avilableReplacements, SpellDefinitions.AllBaseSpells, out chosenReplacement);

                case SpellReplacementLogic.RandomDamageSpell:
                    return this.InternalTryGetReplacementSpell(spellBeingAdded, avilableReplacements, SpellDefinitions.OffenseSpells, out chosenReplacement);

                case SpellReplacementLogic.RandomStatusSpell:
                    return this.InternalTryGetReplacementSpell(spellBeingAdded, avilableReplacements, SpellDefinitions.StatusSpells, out chosenReplacement);

                case SpellReplacementLogic.RandomBuff:
                    return this.InternalTryGetReplacementSpell(spellBeingAdded, avilableReplacements, SpellDefinitions.BuffSpells, out chosenReplacement);

                case SpellReplacementLogic.RandomDebuff:
                    return this.InternalTryGetReplacementSpell(spellBeingAdded, avilableReplacements, SpellDefinitions.DebuffSpells, out chosenReplacement);

                case SpellReplacementLogic.SpecificSpell:
                    return this.TryGetExplicitPairing(spellBeingAdded, avilableReplacements, out chosenReplacement);
            }
        }

        private bool TryGetExplicitPairing(SpellNameEnum toReplace, List<SpellNameEnum> availableReplacements, out SpellNameEnum chosenReplacement)
        {
            if (this.explicitReplacementMapping.ContainsKey(toReplace))
            {
                chosenReplacement = this.explicitReplacementMapping[toReplace];
                return true;
            }

            chosenReplacement = default;
            return false;
        }

        private List<SpellNameEnum> randomCache = new List<SpellNameEnum>();
        private bool InternalTryGetReplacementSpell(SpellNameEnum spellBeingAdded, List<SpellNameEnum> availableReplacements, SpellNameEnum[] selectionPool, out SpellNameEnum chosenReplacement, bool isFallback = false)
        {
            this.randomCache.Clear();
            this.randomCache.AddRange(selectionPool);
            this.randomCache.Shuffle();

            var forbiddenReplacements = ReplacementOperations.ForbiddenReplacements;

            for (int k = 0; k < this.randomCache.Count; k++)
            {
                var possibleReplacement = this.randomCache[k];

                var hasExclusions = forbiddenReplacements.ContainsKey(spellBeingAdded);
                if (hasExclusions && forbiddenReplacements[spellBeingAdded].Contains(possibleReplacement))
                    continue;

                var isAvailable = availableReplacements.Contains(possibleReplacement);
                if (isAvailable)
                {
                    chosenReplacement = possibleReplacement;
                    availableReplacements.Remove(possibleReplacement);
                    return true;
                }

            }

            if (isFallback == false)
            {
                return this.InternalTryGetReplacementSpell(spellBeingAdded, availableReplacements, SpellDefinitions.OffenseSpells, out chosenReplacement, isFallback: true);
            }

            chosenReplacement = default;
            return false;
        }

        public void ClearReplacementLogic(SpellNameEnum spellToClear)
        {
            if (this.logicMapping.ContainsKey(spellToClear))
                this.logicMapping.Remove(spellToClear);

            if (this.explicitReplacementMapping.ContainsKey(spellToClear))
                this.explicitReplacementMapping.Remove(spellToClear);
        }

        public void SetReplacementLogic(SpellNameEnum spell, SpellReplacementLogic logic)
        {
            if (this.logicMapping.ContainsKey(spell) == false)
            {
                this.logicMapping.Add(spell, logic);
                return;
            }

            this.logicMapping[spell] = logic;
        }

        public void SetExplicitReplacementMapping(SpellNameEnum spellToAdd, SpellNameEnum spellToReplace)
        {
            this.SetReplacementLogic(spellToAdd, SpellReplacementLogic.SpecificSpell);

            if (this.explicitReplacementMapping.ContainsKey(spellToAdd) == false)
                this.explicitReplacementMapping.Add(spellToAdd, spellToReplace);
            else
                this.explicitReplacementMapping[spellToAdd] = spellToReplace;
        }

        public void ApplyPreset(SpellReplacementPreset replacementPreset)
        {
            switch (replacementPreset)
            {
                default:
                case SpellReplacementPreset.None:
                    this.ApplyNotUsedPreset();
                    break;

                case SpellReplacementPreset.Recommended:
                    this.ApplyRecommendedPreset();
                    break;

                case SpellReplacementPreset.ReplaceSimilar:
                    this.ApplyReplaceSimilarPreset();
                    break;

                case SpellReplacementPreset.BubbleOnly:
                    this.ApplyBubbleOnlyPreset();
                    break;

                case SpellReplacementPreset.Random:
                    this.ApplyRandomPreset();
                    break;
            }

            this.onPresetApplied?.Invoke(this, replacementPreset);

            Console.WriteLine($"Applied Replacement Preset: {replacementPreset}");
        }


        private void ApplyRandomPreset()
        {
            this.InternalApplyPresetEntries(this.randomReplacements);
        }


        private void ApplyBubbleOnlyPreset()
        {
            this.InternalApplyPresetEntries(this.bubbleOnlyReplacements);
        }

        private void ApplyRecommendedPreset()
        {
            this.InternalApplyPresetEntries(this.recommendedReplacements);
        }

        private void ApplyReplaceSimilarPreset()
        {
            this.InternalApplyPresetEntries(this.similarSpellReplacements);
        }

        private void InternalApplyPresetEntries(List<SpellReplacementEntry> entries)
        {
            this.explicitReplacementMapping.Clear();
            this.logicMapping.Clear();
            this.enabledMapping.Clear();

            for (int k = 0; k < entries.Count; k++)
            {
                var recommendedEntry = entries[k];

                var logic = recommendedEntry.logic;
                var spellToAdd = recommendedEntry.spellBeingAdded;
                var spellToReplace = recommendedEntry.spellBeingReplaced;

                this.logicMapping.Add(spellToAdd, logic);

                if (logic == SpellReplacementLogic.SpecificSpell)
                {
                    this.explicitReplacementMapping.Add(spellToAdd, spellToReplace);
                }

                this.enabledMapping.Add(spellToAdd, true);
            }
        }

        private void ApplyNotUsedPreset()
        {
            this.explicitReplacementMapping.Clear();
            this.logicMapping.Clear();
            this.enabledMapping.Clear();
        }

        private readonly List<SpellReplacementEntry> recommendedReplacements = new List<SpellReplacementEntry>
        {
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.ZelseWindRazor, logic = SpellReplacementLogic.RandomDebuff, spellBeingReplaced = SpellNameEnum.HomingArrowLv1 },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.ZelseWindZipper, logic = SpellReplacementLogic.RandomDamageSpell },

            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.NeptyBubbleShot, logic = SpellReplacementLogic.SpecificSpell, spellBeingReplaced = SpellNameEnum.SoulSearcherLv1 },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.ShilfDoveRazor, logic = SpellReplacementLogic.RandomDamageSpell },

            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.FargoLavaBall, logic = SpellReplacementLogic.RandomStatusSpell },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.FargoExplosion, logic = SpellReplacementLogic.RandomDamageSpell },

            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.BeigisSpiritSword, logic = SpellReplacementLogic.RandomSpell },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.MammonFlameWaves, logic = SpellReplacementLogic.RandomBuff },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.MammonFireArrows, logic = SpellReplacementLogic.RandomSpell },
        };

        private readonly List<SpellReplacementEntry> similarSpellReplacements = new List<SpellReplacementEntry>
        {
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.ZelseWindRazor, logic = SpellReplacementLogic.SpecificSpell, spellBeingReplaced = SpellNameEnum.HomingArrowLv1 },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.ZelseWindZipper, logic = SpellReplacementLogic.SpecificSpell, spellBeingReplaced = SpellNameEnum.RollingRockLv1 },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.NeptyBubbleShot, logic = SpellReplacementLogic.SpecificSpell, spellBeingReplaced = SpellNameEnum.SoulSearcherLv1 },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.ShilfDoveRazor, logic = SpellReplacementLogic.SpecificSpell, spellBeingReplaced = SpellNameEnum.WindCutterLv2 },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.FargoLavaBall, logic = SpellReplacementLogic.SpecificSpell, spellBeingReplaced = SpellNameEnum.FireBallLv1 },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.FargoExplosion, logic = SpellReplacementLogic.SpecificSpell, spellBeingReplaced = SpellNameEnum.FireBomb},
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.BeigisSpiritSword, logic = SpellReplacementLogic.SpecificSpell, spellBeingReplaced = SpellNameEnum.Cyclone },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.MammonFlameWaves, logic = SpellReplacementLogic.SpecificSpell, spellBeingReplaced = SpellNameEnum.MagmaBall },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.MammonFireArrows, logic = SpellReplacementLogic.SpecificSpell, spellBeingReplaced = SpellNameEnum.HomingArrowLv1 },

        };

        private readonly List<SpellReplacementEntry> bubbleOnlyReplacements = new List<SpellReplacementEntry>
        {
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.NeptyBubbleShot, logic = SpellReplacementLogic.SpecificSpell, spellBeingReplaced = SpellNameEnum.SoulSearcherLv1 },
        };

        private readonly List<SpellReplacementEntry> randomReplacements = new List<SpellReplacementEntry>
        {
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.ZelseWindRazor, logic = SpellReplacementLogic.RandomDebuff, spellBeingReplaced = SpellNameEnum.HomingArrowLv1 },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.ZelseWindZipper, logic = SpellReplacementLogic.RandomDamageSpell },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.NeptyBubbleShot, logic = SpellReplacementLogic.SpecificSpell, spellBeingReplaced = SpellNameEnum.SoulSearcherLv1 },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.ShilfDoveRazor, logic = SpellReplacementLogic.RandomDamageSpell },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.FargoLavaBall, logic = SpellReplacementLogic.RandomStatusSpell },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.FargoExplosion, logic = SpellReplacementLogic.RandomDamageSpell },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.BeigisSpiritSword, logic = SpellReplacementLogic.RandomSpell },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.MammonFlameWaves, logic = SpellReplacementLogic.RandomBuff },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.MammonFireArrows, logic = SpellReplacementLogic.RandomSpell },

            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.SolvaringFireBlast, logic = SpellReplacementLogic.RandomSpell },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.SolvaringEarthSpikes, logic = SpellReplacementLogic.RandomBuff },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.NeptyBubbleShield, logic = SpellReplacementLogic.RandomDebuff },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.ShilfLaser, logic = SpellReplacementLogic.RandomDamageSpell },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.GuiltyGroundPound, logic = SpellReplacementLogic.RandomDamageSpell },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.GuiltySlash, logic = SpellReplacementLogic.RandomDamageSpell },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.BeigisGreenLaser, logic = SpellReplacementLogic.RandomSpell },
            new SpellReplacementEntry{ spellBeingAdded = SpellNameEnum.MammonLightBlast, logic = SpellReplacementLogic.RandomSpell },
        };
    }

    public static class ReplacementOperations
    {
        public static string ReplaceSpellLogicData(SpellData original, SpellData replacement)
        {
            var originalAttributes = original.GetAttributeData();
            var replacementAttributes = replacement.GetAttributeData();

            // Grab the spell rule and menu path of the original spell,
            // but everything else from the replacement spell.
            //
            const int MENU_LEVEL_REQ_OFFSET = 0x0;
            const int MENU_LEVEL_REQ_LENGTH = 0x2;
            const int MENU_MEM_OFFSET = 0x4;
            const int MENU_MEM_LENGTH = 0x6;

            const int STR_LEVEL_START = 2 * MENU_LEVEL_REQ_OFFSET;
            const int STR_LEVEL_LENGTH = 2 * MENU_LEVEL_REQ_LENGTH;
            const int STR_MENU_START = 2 * MENU_MEM_OFFSET;
            const int STR_MENU_LENGTH = 2 * MENU_MEM_LENGTH;

            var originalLevelReq = originalAttributes.Substring(STR_LEVEL_START, STR_LEVEL_LENGTH);
            var originalMenuData = originalAttributes.Substring(STR_MENU_START, STR_MENU_LENGTH);

            replacementAttributes = replacementAttributes
                .ReplaceAt(STR_LEVEL_START, originalLevelReq)
                .ReplaceAt(STR_MENU_START, originalMenuData);

            return replacementAttributes;
        }

        private static string ReplaceSpellLogicDataRaw(string originalData, string replacementData)
        {
            // Grab the spell rule and menu path of the original spell,
            // but everything else from the replacement spell.
            //
            const int MENU_LEVEL_REQ_OFFSET = 0x0;
            const int MENU_LEVEL_REQ_LENGTH = 0x2;
            const int MENU_MEM_OFFSET = 0x4;
            const int MENU_MEM_LENGTH = 0x6;

            const int STR_LEVEL_START = 2 * MENU_LEVEL_REQ_OFFSET;
            const int STR_LEVEL_LENGTH = 2 * MENU_LEVEL_REQ_LENGTH;
            const int STR_MENU_START = 2 * MENU_MEM_OFFSET;
            const int STR_MENU_LENGTH = 2 * MENU_MEM_LENGTH;

            var originalLevelReq = originalData.Substring(STR_LEVEL_START, STR_LEVEL_LENGTH);
            var originalMenuData = originalData.Substring(STR_MENU_START, STR_MENU_LENGTH);

            replacementData = replacementData
                .ReplaceAt(STR_LEVEL_START, originalLevelReq)
                .ReplaceAt(STR_MENU_START, originalMenuData);

            return replacementData;
        }

        public static MerrowPatchOperation GetSpellDataReplacementOperation(string originalRomAddress, string originalDataRaw, string replacementDataRaw)
        {
            var replacedDataString = ReplaceSpellLogicDataRaw(originalDataRaw, replacementDataRaw);

            return new MerrowPatchOperation
            {
                romAddress = originalRomAddress,
                patchContents = replacedDataString
            };
        }

        public static MerrowPatchOperation GetSpellDataReplacementOperation(SpellData originalData, SpellData replacementData)
        {
            var replacedDataString = ReplaceSpellLogicData(originalData, replacementData);

            return new MerrowPatchOperation
            {
                romAddress = originalData.RomAddress,
                patchContents = replacedDataString
            };
        }

        public static MerrowPatchOperation GetSpellAnimReplacementOperation(SpellAnimationData originalAnim, SpellAnimationData replacementAnim)
        {
            return new MerrowPatchOperation
            {
                romAddress = originalAnim.RomAddress,
                patchContents = replacementAnim.AnimationData
            };
        }

        public static readonly Dictionary<SpellNameEnum, SpellNameEnum[]> ForbiddenReplacements = new Dictionary<SpellNameEnum, SpellNameEnum[]>
        {
            { SpellNameEnum.MammonFlameWaves, new SpellNameEnum[] { SpellNameEnum.RockShower, SpellNameEnum.MagnetRock } },
        };
    }

    public static class BossSpellFunctionReplacements
    {
        /// <summary>
        /// Replaces func_8000A284
        /// 
        /// This function queues the special boss spell animations whenever a corresponding spell
        /// is cast during a boss encounter.
        /// 
        /// https://decomp.me/scratch/rlY5X
        /// </summary>
        /// <returns></returns>
        public static MapWriteOperation ReplaceAISpellInit()
        {
            return new MapWriteOperation(0x0AE84, new string[]
            {
                "27BDFFD0", // # addiu
                "AFB20028", // # sw
                "AFB10024", // # sw
                "AFB00020", // # sw
                "00808025", // # move
                "00A08825", // # move
                "00C09025", // # move
                "AFBF002C", // # sw
                "96420000", // # lhu
                "304E8000", // # andi
                "11C00012", // # beqz
                "01C01025", // # move
                "960F0090", // # lhu
                "02202025", // # move
                "00002825", // # move
                "31F80040", // # andi
                "1300000C", // # beqz
                "24060001", // # li
                "A6000000", // # sh
                "24190001", // # li
                "24080001", // # li
                "AFA80014", // # sw
                "AFB90010", // # sw
                "0C00762C", // # jal
                "00003825", // # move
                "0C0074D6", // # jal
                "02202025", // # move
                "10000039", // # b
                "8FBF002C", // # lw
                "10400004", // # beqz
                "240E0001", // # li
                "24090005", // # li
                "10000003", // # b
                "A6090000", // # sh
                "240A0004", // # li
                "A60A0000", // # sh
                "960C0008", // # lhu
                "A6000006", // # sh
                "964B0004", // # lhu
                "318DFFF7", // # andi
                "A60D0008", // # sh
                "A60B0002", // # sh
                "3C028008", // # lui
                "2442BAB8", // # addiu
                "A62E0060", // # sh
                "C448001C", // # lwc1
                "C4440014", // # lwc1
                "C62A0008", // # lwc1
                "C6260000", // # lwc1
                "460A4381", // # sub.s
                "0C008C84", // # jal
                "46062301", // # sub.s
                "E6200010", // # swc1
                "8642000A", // # lh
                "2401FFFF", // # li
                "02202025", // # move
                "14410003", // # bne
                "24060001", // # li
                "10000002", // # b
                "00003825", // # move
                "00403825", // # move
                "86450008", // # lh
                "240F0001", // # li
                "24180001", // # li
                "AFB80014", // # sw
                "0C00762C", // # jal
                "AFAF0010", // # sw
                "96440004", // # lhu
                "02202825", // # move
                "0C00503B", // # jal
                "2484000F", // # addiu
                "86190002", // # lh
                "24080001", // # li
                "02002825", // # move
                "17200003", // # bnezl
                "96420000", // # lhu
                "A6080002", // # sh
                "96420000", // # lhu
                "24018400", // # li
                "30498400", // # andi
                "00000000", // # bnel
                "8FBF002C", // # lw
                "0C002CB2", // # jal
                "304400FF", // # andi
                "8FBF002C", // # lw
                "8FB00020", // # lw
                "8FB10024", // # lw
                "8FB20028", // # lw
                "03E00008", // # jr
                "27BD0030", // # addiu
            });
        }


        /// <summary>
        /// Replaces func_8000B300
        /// 
        /// This function updates the state machine for processing the special boss spell animations.
        /// 
        /// https://decomp.me/scratch/vP3Yw
        /// </summary>
        /// <returns></returns>
        public static MapWriteOperation ReplaceBossSpellAnimUpdate()
        {
            return new MapWriteOperation(0x0BF00, new string[]
        {
            "27BDFF88", // # addiu
            "3C0A8008", // # lui
            "254AD0C0", // # addiu
            "AFBF0034", // # sw
            "AFB10030", // # sw
            "AFB0002C", // # sw
            "8D500004", // # lw
            "5200007C", // # beqzl
            "8FBF0034", // # lw
            "854E0000", // # lh
            "3C048008", // # lui
            "3C058008", // # lui
            "25CF0001", // # addiu
            "A54F0000", // # sh
            "85590000", // # lh
            "96180000", // # lhu
            "24A5BACC", // # addiu
            "2484C9BC", // # addiu
            "0319082A", // # slt
            "1020006F", // # beqz
            "3C0F8005", // # lui
            "960D0002", // # lhu
            "25EF4690", // # addiu
            "00801025", // # move
            "000D7100", // # sll
            "01CF4021", // # addu
            "85180000", // # lh
            "3C0E8009", // # lui
            "95CEC592", // # lhu
            "0018C880", // # sll
            "3C0D800C", // # lui
            "0338C821", // # addu
            "0019C840", // # sll
            "25AD1B90", // # addiu
            "31CF0002", // # andi
            "032D5821", // # addu
            "00A01825", // # move
            "11E00003", // # beqz
            "954C000A", // # lhu
            "00A01025", // # move
            "00801825", // # move
            "C6040008", // # lwc1
            "3C118008", // # lui
            "2631D0D0", // # addiu
            "E6240000", // # swc1
            "C6060010", // # lwc1
            "02202825", // # move
            "E6260004", // # swc1
            "C46C0010", // # lwc1
            "A7AC003E", // # sh
            "AFAB0048", // # sw
            "AFA8004C", // # sw
            "AFA30058", // # sw
            "AFA20054", // # sw
            "0C008CBD", // # jal
            "E7AC0038", // # swc1
            "96180004", // # lhu
            "8FA20054", // # lw
            "8FA30058", // # lw
            "33190002", // # andi
            "8FA8004C", // # lw
            "8FAB0048", // # lw
            "13200005", // # beqz
            "97AC003E", // # lhu
            "C4420000", // # lwc1
            "C44C0004", // # lwc1
            "10000004", // # b
            "C44E0008", // # lwc1
            "C4620000", // # lwc1
            "C46C0004", // # lwc1
            "C46E0008", // # lwc1
            "C4600024", // # lwc1
            "C6280000", // # lwc1
            "000C6A03", // # sra
            "C610000C", // # lwc1
            "46004282", // # mul.s
            "C5080008", // # lwc1
            "31AE000F", // # andi
            "C6240004", // # lwc1
            "3C018008", // # lui
            "000E7880", // # sll
            "3C18800C", // # lui
            "460A1080", // # add.s
            "030FC021", // # addu
            "319900FF", // # andi
            "E428D0D0", // # swc1
            "46008482", // # mul.s
            "C50A000C", // # lwc1
            "8F181B14", // # lw
            "00196900", // # sll
            "01B96821", // # addu
            "3C018008", // # lui
            "000D6880", // # sll
            "46002182", // # mul.s
            "E42AD0D4", // # swc1
            "030D4821", // # addu
            "C530001C", // # lwc1
            "3C018008", // # lui
            "46126300", // # add.s
            "E430D0D8", // # swc1
            "95050004", // # lhu
            "95040002", // # lhu
            "46067380", // # add.s
            "C7B20038", // # lwc1
            "44076000", // # mfc1
            "44061000", // # mfc1
            "E7AE0010", // # swc1
            "AFA30020", // # sw
            "AFB1001C", // # sw
            "AFAB0018", // # sw
            "0C005DFE", // # jal
            "E7B20014", // # swc1
            "96020004", // # lhu
            "3C0A8008", // # lui
            "254AD0C0", // # addiu
            "304E0004", // # andi
            "51C00007", // # beqzl
            "304F0001", // # andi
            "0C004FC8", // # jal
            "24040001", // # li
            "3C0A8008", // # lui
            "254AD0C0", // # addiu
            "96020004", // # lhu
            "304F0001", // # andi
            "51E00004", // # beqzl
            "8D590004", // # lw
            "10000004", // # b
            "AD400004", // # sw
            "8D590004", // # lw
            "27380018", // # addiu
            "AD580004", // # sw
            "8FBF0034", // # lw
            "8FB0002C", // # lw
            "8FB10030", // # lw
            "03E00008", // # jr
            "27BD0078", // # addiu
        });
        }


        /// <summary>
        /// Replaces func_8000B2C8
        /// 
        /// This function queues a given boss spell animation after a boss spell cast, we adjust this
        /// to be called from player also and supply the actor's transform as the source, not the first
        /// enemy transform as usual etc.
        /// 
        /// https://decomp.me/scratch/8bTWR
        /// </summary>
        /// <returns></returns>
        public static MapWriteOperation ReplaceQueueBossSpellAnim()
        {
            return new MapWriteOperation(0x0BEC8, new string[]
            {
                "3C028008", // # lui
                "2442D0C0", // # addiu
                "00047080", // # sll
                "3C0F8005", // # lui
                "A4400000", // # sh
                "01EE7821", // # addu
                "8DEF4640", // # lw
                "AC440008", // # sw
                "AC45000C", // # sw
                "AC4F0004", // # sw
                "03E00008", // # jr
                "00000000", // # nop
            });
        }

        /// <summary>
        /// Replaces func_8001CFE8
        /// 
        /// This function handles the combat state machine for all encounters.  We make a slight modification
        /// here to write the current actor's turn onto special addresses that can be read by the spell anim
        /// state machine, as the game otherwise has no direct way of determining whose turn it currently is.
        /// 
        /// https://decomp.me/scratch/bXJGs
        /// </summary>
        /// <returns></returns>
        public static MapWriteOperation ReplaceCombatStateMachine()
        {
            return new MapWriteOperation(0x1DBE8, new string[]
            {
                "27BDFFD8", // # addiu
                "3C068009", // # lui
                "24C6C592", // # addiu
                "AFBF001C", // # sw
                "94C50000", // # lhu
                "3C038009", // # lui
                "2463C594", // # addiu
                "30AE0004", // # andi
                "51C000CF", // # beqzl
                "8FBF001C", // # lw
                "94620000", // # lhu
                "10400003", // # beqz
                "244FFFFF", // # addiu
                "100000C9", // # b
                "A46F0000", // # sh
                "30B80002", // # andi
                "13000010", // # beqz
                "3C198009", // # lui
                "9739C598", // # lhu
                "3C0B8008", // # lui
                "3C0E8008", // # lui
                "00195040", // # sll
                "016A5821", // # addu
                "956BD0B0", // # lhu
                "25CEC998", // # addiu
                "000B60C0", // # sll
                "018B6021", // # addu
                "000C6080", // # sll
                "018B6021", // # addu
                "000C60C0", // # sll
                "258D0024", // # addiu
                "10000003", // # b
                "01AE3821", // # addu
                "3C078008", // # lui
                "24E7BACC", // # addiu
                "94EF0060", // # lhu
                "31F80001", // # andi
                "170000B1", // # bnez
                "3C198008", // # lui
                "8F39C990", // # lw
                "00002025", // # move
                "30AA0400", // # andi
                "13200003", // # beqz
                "34AC0008", // # ori
                "11400022", // # beqz
                "30B90010", // # andi
                "318D0100", // # andi
                "11A00015", // # beqz
                "A4CC0000", // # sh
                "3C048009", // # lui
                "8C84C570", // # lw
                "AFA70024", // # sw
                "2484FFFF", // # addiu
                "308EFFFF", // # andi
                "0C002E83", // # jal
                "01C02025", // # move
                "3C048009", // # lui
                "8C84C570", // # lw
                "0C002EDA", // # jal
                "2484FFFF", // # addiu
                "3C0F8009", // # lui
                "8DEFC568", // # lw
                "3C188009", // # lui
                "8F18C56C", // # lw
                "3C018008", // # lui
                "AC2FD0A8", // # sw
                "3C018008", // # lui
                "8FA70024", // # lw
                "AC38D0AC", // # sw
                "0C00760A", // # jal
                "AFA70024", // # sw
                "3C068009", // # lui
                "3C018008", // # lui
                "AC22BA70", // # sw
                "24C6C592", // # addiu
                "8FA70024", // # lw
                "94C50000", // # lhu
                "1000000F", // # b
                "24040001", // # li
                "13200003", // # beqz
                "30AA0100", // # andi
                "1000000B", // # b
                "24040001", // # li
                "15400009", // # bnez
                "3C0B8008", // # lui
                "956BBAC0", // # lhu
                "34AD0010", // # ori
                "316C0020", // # andi
                "11800004", // # beqz
                "00000000", // # nop
                "A4CD0000", // # sh
                "24040001", // # li
                "31A5FFFF", // # andi
                "10800024", // # beqz
                "30A3FFFB", // # andi
                "30AEFFFE", // # andi
                "A4CE0000", // # sh
                "3C018009", // # lui
                "3C048009", // # lui
                "A420C630", // # sh
                "0C006729", // # jal
                "9484C55C", // # lhu
                "0C0023DB", // # jal
                "00000000", // # nop
                "3C048008", // # lui
                "0C0050F0", // # jal
                "8C846ED8", // # lw
                "3C068009", // # lui
                "24C6C592", // # addiu
                "94C20000", // # lhu
                "3C058008", // # lui
                "2404002B", // # li
                "30580100", // # andi
                "13000003", // # beqz
                "304FFEFF", // # andi
                "A4CF0000", // # sh
                "31E2FFFF", // # andi
                "30590010", // # andi
                "13200007", // # beqz
                "00003025", // # move
                "3C048008", // # lui
                "8C844EE4", // # lw
                "0C009A13", // # jal
                "8CA54EE8", // # lw
                "1000005B", // # b
                "8FBF001C", // # lw
                "0C0099FE", // # jal
                "24050008", // # li
                "10000057", // # b
                "8FBF001C", // # lw
                "A4C30000", // # sh
                "306B0002", // # andi
                "11600009", // # beqz
                "01601825", // # move
                "0C0024CB", // # jal
                "AFA70024", // # sw
                "3C068009", // # lui
                "24C6C592", // # addiu
                "94C30000", // # lhu
                "8FA70024", // # lw
                "306C0002", // # andi
                "01801825", // # move
                "C4E40000", // # lwc1
                "3C088009", // # lui
                "2508C5A4", // # addiu
                "E5040000", // # swc1
                "C4E60008", // # lwc1
                "3C098009", // # lui
                "2529C430", // # addiu
                "10600010", // # beqz
                "E5260000", // # swc1
                "3C0D8009", // # lui
                "95ADC598", // # lhu
                "3C0F8008", // # lui
                "3C018008", // # lui
                "000D7040", // # sll
                "01EE7821", // # addu
                "95EFD0B0", // # lhu
                "000FC0C0", // # sll
                "030FC021", // # addu
                "0018C080", // # sll
                "030FC021", // # addu
                "0018C8C0", // # sll
                "00390821", // # addu
                "10000003", // # b
                "C420C9A8", // # lwc1
                "3C018008", // # lui
                "C420BAC4", // # lwc1
                "10600011", // # beqz
                "3C018009", // # lui
                "3C0A8009", // # lui
                "954AC598", // # lhu
                "3C0C8008", // # lui
                "3C0F8008", // # lui
                "000A5840", // # sll
                "018B6021", // # addu
                "958CD0B0", // # lhu
                "25EFC998", // # addiu
                "000C68C0", // # sll
                "01AC6821", // # addu
                "000D6880", // # sll
                "01AC6821", // # addu
                "000D68C0", // # sll
                "25AE0024", // # addiu
                "10000003", // # b
                "01CF1021", // # addu
                "3C028008", // # lui
                "2442BACC", // # addiu
                "AC27C530", // # sw
                "3C018009", // # lui
                "AC22C534", // # sw
                "8C580064", // # lw
                "8C590068", // # lw
                "C44A0024", // # lwc1
                "C7080018", // # lwc1
                "C7320090", // # lwc1
                "24040002", // # li
                "460A4402", // # mul.s
                "3C018007", // # lui
                "46009102", // # mul.s
                "10600003", // # beqz
                "46102080", // # add.s
                "10000001", // # b
                "24040001", // # li
                "D4281690", // # ldc1
                "460011A1", // # cvt.d.s
                "8C460004", // # lw
                "46283282", // # mul.d
                "8D050000", // # lw
                "8D270000", // # lw
                "462054A0", // # cvt.s.d
                "0C0066A6", // # jal
                "E7B20010", // # swc1
                "3C018009", // # lui
                "A422C55E", // # sh
                "8FBF001C", // # lw
                "27BD0028", // # addiu
                "03E00008", // # jr
                "00000000", // # nop
            });
        }

        /// <summary>
        /// Replaces func_800072A8
        /// 
        /// This function handles the actual confirmed input when Brian casts a spell, triggering the mana reduction
        /// and any sort of combat state machine updates that may need to be processed.
        /// 
        /// It has been updated to call a custom shim, injected below.
        /// 
        /// https://decomp.me/scratch/4mCvB
        /// </summary>
        /// <returns></returns>
        public static MapWriteOperation ReplaceBrianSpellCast()
        {
            return new MapWriteOperation(0x07EA8, new string[]
            {
                "27BDFFD8", // # addiu
                "AFB10020", // # sw
                "AFB0001C", // # sw
                "00808025", // # move
                "00A08825", // # move
                "AFBF0024", // # sw
                "AFA60030", // # sw
                "960F011C", // # lhu
                "8E220064", // # lw
                "3C19800C", // # lui
                "000FC040", // # sll
                "0338C821", // # addu
                "97391B26", // # lhu
                "944E0008", // # lhu
                "3C0C800C", // # lui
                "24010002", // # li
                "01D94023", // # subu
                "A4480008", // # sh
                "9603011E", // # lhu
                "44800000", // # mtc1
                "02202025", // # move
                "00034A03", // # sra
                "312A000F", // # andi
                "000A5880", // # sll
                "018B6021", // # addu
                "306D00FF", // # andi
                "8D8C1B14", // # lw
                "000D7900", // # sll
                "01ED7821", // # addu
                "000F7880", // # sll
                "018F3821", // # addu
                "94F80018", // # lhu
                "240B000E", // # li
                "240D0012", // # li
                "17010007", // # bne
                "24060001", // # li
                "960E0008", // # lhu
                "24050017", // # li
                "24070016", // # li
                "35D90004", // # ori
                "10000004", // # b
                "A6190008", // # sh
                "3C078008", // # lui
                "24050003", // # li
                "8CE7BC2C", // # lw
                "8FA80030", // # lw
                "00054880", // # sll
                "01254823", // # subu
                "00094880", // # sll
                "01095021", // # addu
                "95420002", // # lhu
                "A60B0000", // # sh
                "A60D0120", // # sh
                "A600011C", // # sh
                "A6020004", // # sh
                "962C0060", // # lhu
                "E6200018", // # swc1
                "E6200020", // # swc1
                "358F0001", // # ori
                "A62F0060", // # sh
                "24180001", // # li
                "240E0001", // # li
                "AFAE0014", // # sw
                "0C00762C", // # jal
                "AFB80010", // # sw
                "0C016000", // # jal
                "00000000", // # nop
                "8FBF0024", // # lw
                "8FB0001C", // # lw
                "8FB10020", // # lw
                "03E00008", // # jr
                "27BD0028", // # addiu
            });
        }

        /// <summary>
        /// Adds New func_800852A0
        /// 
        /// Custom function that will be dropped near the empty combat state memory 
        /// to shim the combat state increment call and additionally run a check for the
        /// spell's tag, which has been dropped into the last + unused 16-bit block at the end
        /// of its definition.
        /// 
        /// https://decomp.me/scratch/LPguP
        /// </summary>
        /// <returns></returns>
        public static MapWriteOperation CustomBrianSpellCastCallback()
        {
            return new MapWriteOperation(0x58C00, new string[]
            {
                "27BDFFD8", // # addiu
                "AFBF0014", // # sw
                "3C038008", // # lui
                "9463BBD6", // # lhu
                "3C19800C", // # lui
                "3C058008", // # lui
                "00037203", // # sra
                "31CF000F", // # andi
                "000FC080", // # sll
                "0338C821", // # addu
                "306800FF", // # andi
                "8F391B14", // # lw
                "00084900", // # sll
                "01284821", // # addu
                "00094880", // # sll
                "03295021", // # addu
                "AFAA001C", // # sw
                "24A5BACC", // # addiu
                "0C00503B", // # jal
                "2404002D", // # li
                "8FAB001C", // # lw
                "24018400", // # li
                "3C058008", // # lui
                "95620042", // # lhu
                "304C8400", // # andi
                "15810003", // # bne
                "304400FF", // # andi
                "0C002CB2", // # jal
                "24A5BACC", // # addiu
                "8FBF0014", // # lw
                "27BD0028", // # addiu
                "03E00008", // # jr
                "00000000", // # nop
            });
        }
    }
}

