using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Xsl;

namespace Merrow.Util
{
    public static class RandomGen
    {
        private static Random RNG = new Random();

        public static int RandomInt(int maxExclusive)
        {
            return RNG.Next(maxExclusive);
        }
    }

    public class FullGameMapData
    {
        public AreaEncounterData[] allData;
        public bool isDirty;

        public const int BARAGOON_MOOR_INDEX_START = 13;
        public const int BARAGOON_MOOR_INDEX_LAST = 14;

        public const int BRANNOCH_INDEX_START = 15;
        public const int BRANNOCH_INDEX_LAST = 20;

        public const int MAMMON_INDEX_START = 21;
        public const int MAMMON_INDEX_LAST = 26;

        public FullGameMapData(AreaEncounterData[] data)
        {
            allData = data;
        }

        private List<MapWriteOperation> operationCache = new List<MapWriteOperation>();
        public MapWriteOperation[] GetMapWriteOperations()
        {
            if (this.isDirty == false)
                return null;

            operationCache.Clear();

            for (int k = 0; k < this.allData.Length; k++)
            {
                var data = this.allData[k];
                var ops = data.GetWriteOperations();

                operationCache.AddRange(ops);
            }

            return operationCache.ToArray();
        }

        private List<int> tableIndices = new List<int>();
        public void RandomizeMonsterTables()
        {
            tableIndices.Clear();
            for (int k=0; k<this.allData.Length; k++)
            {
                var data = this.allData[k];
                var tableIndex = data.mapData.tableIndex;
                tableIndices.Add(tableIndex);
            }
            tableIndices.Shuffle();

            for (int k = 0; k < this.allData.Length; k++)
            {
                var data = this.allData[k];
                var tableIndex = tableIndices[k];

                //Console.WriteLine("Randomizing Tables for: {0}", data.areaName);

                var newTable = data.globalEnemyTables[tableIndex];
                var newTableEnemyCount = newTable.enemies.Length;

                data.mapData.tableIndex = (ushort)tableIndex;

                var packDefinitions = data.packDefinitions;
                for (int packIndex = 0; packIndex < packDefinitions.Length; packIndex++)
                {
                    var def = packDefinitions[packIndex];
                    def.CapEnemyIDs(newTableEnemyCount);

                    packDefinitions[packIndex] = def;
                }

                this.allData[k] = data;
            }

            this.isDirty = true;
        }

        public void RandomizeAllMonsterPresets()
        {
            for (int k = 0; k < this.allData.Length; k++)
            {
                var data = this.allData[k];

                //Console.WriteLine("Randomizing Monster Presets for: {0}", data.areaName);

                data.RandomizeMonsterPresets();
                data.RandomizeEnemiesWithinPacks();

                this.allData[k] = data;
            }

            this.isDirty = true;
        }

        public void FixBaragoonMoor()
        {
            //Console.WriteLine("Fixing Baragoon Moor Enemy Pack Indices ...");
            this.CapEnemyIndicesAmongGroup(BARAGOON_MOOR_INDEX_START, BARAGOON_MOOR_INDEX_LAST);
        }

        public void FixBrannochCastle()
        {
            //Console.WriteLine("Fixing Brannoch Castle Enemy Pack Indices ...");
            this.CapEnemyIndicesAmongGroup(BRANNOCH_INDEX_START, BRANNOCH_INDEX_LAST);
        }

        public void FixMammonsWorld()
        {
            //Console.WriteLine("Fixing Mammons World Enemy Pack  IDs ...");
            this.CapEnemyIndicesAmongGroup(MAMMON_INDEX_START, MAMMON_INDEX_LAST);
        }
        
        private void CapEnemyIndicesAmongGroup(int indexStart, int indexEnd)
        {
            var enemyMin = 20;
            for (int k = indexStart; k <= indexEnd; k++)
            {
                var data = this.allData[k];
                var table = data.globalEnemyTables[data.mapData.tableIndex];
                var enemyCount = table.enemies.Length;

                if (enemyMin > enemyCount)
                    enemyMin = enemyCount;
            }

            //Console.WriteLine(" - applying minimum enemy count to {0}", enemyMin);

            for (int k = indexStart; k <= indexEnd; k++)
            {
                var data = this.allData[k];

                for (int p = 0; p < data.packDefinitions.Length; p++)
                {
                    var definition = data.packDefinitions[p];
                    definition.CapEnemyIDs(enemyMin);

                    data.packDefinitions[p] = definition;
                }
            }
        }

        private List<AreaEncounterData> cache = new List<AreaEncounterData>();
        public FullGameMapData Copy()
        {
            cache.Clear();
               
            for (int k = 0;k < this.allData.Length; k++)
            {
                var original = this.allData[k];
                var copied = original.Copy();

                cache.Add(copied);
            }

            return new FullGameMapData(this.cache.ToArray());
        }
    }

    public class AreaEncounterData
    {
        public string areaName;
        public AreaMapData mapData;
        public MonsterPackDefinition[] packDefinitions;
        public EncounterRegion[] encounterRegions;
        public EnemyTable[] globalEnemyTables;

        public AreaEncounterData(string areaName, AreaMapData mapData, MonsterPackDefinition[] packDefinitions, EncounterRegion[] encounterRegions, EnemyTable[] globalEnemyTables)
        {
            this.areaName = areaName;
            this.mapData = mapData;
            this.packDefinitions = packDefinitions;
            this.encounterRegions = encounterRegions;
            this.globalEnemyTables = globalEnemyTables;
        }

        private static List<MonsterPackDefinition> cache = new List<MonsterPackDefinition>();
        private MonsterPackDefinition[] CopyPackDefinitions()
        {
            cache.Clear();
            cache.AddRange(this.packDefinitions);
            return cache.ToArray();
        }

        private static List<EncounterRegion> regionCache = new List<EncounterRegion>();
        private EncounterRegion[] CopyRegions()
        {
            regionCache.Clear();
            for (int k = 0; k < this.encounterRegions.Length; k++)
            { 
                var copied = this.encounterRegions[k].Copy();
                regionCache.Add(copied);
            }
            return regionCache.ToArray();
        }

        public AreaEncounterData Copy()
        {
            return new AreaEncounterData(
                this.areaName, 
                this.mapData.Copy(), 
                this.CopyPackDefinitions(), 
                this.CopyRegions(), 
                this.globalEnemyTables
            );
        }

        public void RandomizeMonsterPresets()
        {
            var regions = this.encounterRegions;

            var totalPresetsAvailable = packDefinitions.Length;

            for (int regionIndex = 0; regionIndex < regions.Length; regionIndex++)
            {
                var region = regions[regionIndex];
                region.RandomizeEncounterPresets(totalPresetsAvailable);

                regions[regionIndex] = region;
            }
        }

        public void RandomizeEnemiesWithinPacks()
        {
            //Console.WriteLine(" -- randomizing enemies within packs ...");

            var packs = this.packDefinitions;

            var currentEnemyTable = this.globalEnemyTables[this.mapData.tableIndex];
            var totalEnemiesAvailable = currentEnemyTable.enemies.Length;

            for (int packIndex = 0; packIndex < packDefinitions.Length; packIndex++)
            {
                var pack = packDefinitions[packIndex];
                pack.RandomizeEnemyIDs(totalEnemiesAvailable);

                packs[packIndex] = pack;
            }

            this.packDefinitions = packs;
        }

        private static List<MapWriteOperation> operations = new List<MapWriteOperation>();
        public MapWriteOperation[] GetWriteOperations()
        {
            operations.Clear();

            foreach (var packDef in this.packDefinitions)
            {
                var hex = packDef.GenerateHexBlocks(out var packWriteLength);
                var op = new MapWriteOperation(packDef.romAddress, hex);
                operations.Add(op);
            }

            var mapHex = this.mapData.GenerateHexBlocks();
            var mapOp = new MapWriteOperation(this.mapData.ROMAddress, mapHex);
            operations.Add(mapOp);

            foreach (var region in this.encounterRegions)
            {
                var regionHex = region.GenerateHexBlocks();
                var regionOp = new MapWriteOperation(region.ROMAddressForPresets, regionHex);
                operations.Add(regionOp);
            }

            return operations.ToArray();
        }
    }

    public struct MonsterPackDefinition
    {
        public uint romAddress;
        public int packCount;

        public EnemyPackMember enemy1;
        public EnemyPackMember enemy2;
        public EnemyPackMember enemy3;
        public EnemyPackMember enemy4;

        public MonsterPackDefinition(uint romAddress, params EnemyPackMember[] enemies)
        {
            this.romAddress = romAddress;
            this.packCount = enemies.Length;

            this.enemy1 = new EnemyPackMember();
            this.enemy2 = new EnemyPackMember();
            this.enemy3 = new EnemyPackMember();
            this.enemy4 = new EnemyPackMember();

            if (enemies.Length > 0) this.enemy1 = enemies[0];
            if (enemies.Length > 1) this.enemy2 = enemies[1];
            if (enemies.Length > 2) this.enemy3 = enemies[2];
            if (enemies.Length > 3) this.enemy4 = enemies[3];
        }

        public void CapEnemyIDs(int uniqueEnemiesInCurrentTable)
        {
            this.enemy1.enemyID %= uniqueEnemiesInCurrentTable;
            this.enemy2.enemyID %= uniqueEnemiesInCurrentTable;
            this.enemy3.enemyID %= uniqueEnemiesInCurrentTable;
            this.enemy4.enemyID %= uniqueEnemiesInCurrentTable;
        }

        private static List<int> shuffleCache = new List<int>();
        public void RandomizeEnemyIDs(int uniqueEnemiesInCurrentTable)
        {
            shuffleCache.Clear();
            for (int k=0; k< uniqueEnemiesInCurrentTable; k++)
            {
                shuffleCache.Add(k);
            }
            shuffleCache.Shuffle();

            //Console.WriteLine("Unique Enemies: {0}, Pack Count: {1}, IDs: {2}", uniqueEnemiesInCurrentTable, this.packCount, string.Join(",", shuffleCache));

            // The actual usage of enemy blocks is controlled
            // elsewhere in memory, so assigning all values here 
            // does not matter etc.
            //
            this.enemy1.enemyID = shuffleCache[0];
            this.enemy2.enemyID = shuffleCache[1];
            this.enemy3.enemyID = shuffleCache[2];
            this.enemy4.enemyID = shuffleCache[3];
        }

        public struct EnemyPackMember
        {
            public int enemyID;
            public int minCount;
            public int extraCount;

            public EnemyPackMember(int id, int minCount, int extraCount)
            {
                this.enemyID = id;
                this.minCount = minCount;
                this.extraCount = extraCount;
            }

            public string[] GenerateHexBlocks(out int writeLength)
            {
                writeLength = 4 * 3;

                return new string[]
                {
                    $"{this.enemyID:X08}",
                    $"{this.minCount:X08}",
                    $"{this.extraCount:X08}"
                };
            }
        }

        private static List<string> blocks = new List<string>();
        public string[] GenerateHexBlocks(out int writeLength)
        {
            blocks.Clear();

            var writeSize1 = 0;
            var writeSize2 = 0;
            var writeSize3 = 0;
            var writeSize4 = 0;

            if (this.packCount > 0) blocks.AddRange(this.enemy1.GenerateHexBlocks(out writeSize1));
            if (this.packCount > 1) blocks.AddRange(this.enemy2.GenerateHexBlocks(out writeSize2));
            if (this.packCount > 2) blocks.AddRange(this.enemy3.GenerateHexBlocks(out writeSize3));
            if (this.packCount > 3) blocks.AddRange(this.enemy4.GenerateHexBlocks(out writeSize4));

            writeLength = writeSize1 + writeSize2 + writeSize3 + writeSize4;

            return blocks.ToArray();
        }
    }

    public class AreaMapData
    {
        public string areaName = "UNASSIGNED";
        public uint ROMAddress { get; private set; }

        public uint unk0;
        public uint ptrDoorData;
        public uint doorCount;
        public uint unk8;
        public ushort unk10;
        public ushort tableIndex;
        public uint unk14;

        public AreaMapData(uint rOMAddress, string areaName, uint unk0, uint ptrDoorData, uint doorCount, uint unk8, ushort unk10, ushort tableIndex, uint unk14)
        {
            this.ROMAddress = rOMAddress;
            this.areaName = areaName;
            this.unk0 = unk0;
            this.ptrDoorData = ptrDoorData;
            this.doorCount = doorCount;
            this.unk8 = unk8;
            this.unk10 = unk10;
            this.tableIndex = tableIndex;
            this.unk14 = unk14;
        }

        public AreaMapData Copy()
        {
            return new AreaMapData(this.ROMAddress, this.areaName, this.unk0, this.ptrDoorData, this.doorCount, this.unk8, this.unk10, this.tableIndex, this.unk14);
        }

        public void AssignTableIndex(int assignedIndex)
        {
            this.tableIndex = (ushort)assignedIndex;
        }

        public void RandomizeTableIndex(int maxIndexInclusive = 5)
        {
            this.tableIndex = (ushort)RandomGen.RandomInt(maxIndexInclusive + 1);
        }

        public string[] GenerateHexBlocks()
        {
            return new string[]
            {
                $"{this.unk0:X08}",
                $"{this.ptrDoorData:X08}",
                $"{this.doorCount:X08}",
                $"{this.unk8:X08}",
                $"{this.unk10:X04}{this.tableIndex:X04}",
                $"{this.unk14:X08}",
            };
        }
    }

    public class EncounterRegion
    {
        private uint ROMAddress;
        public uint ROMAddressForPresets { get; private set; }

        public int xStart;
        public int xEnd;
        public int zStart;
        public int zEnd;
        public int presetCount;

        public List<int> encounterPresetIndices = new List<int>();

        public EncounterRegion(uint romStartAddress, int xStart, int xEnd, int zStart, int zEnd, int presetCount, params int[] defaultIndices)
        {
            // Addresses for the Presets will start 8 bytes after the 
            this.ROMAddress = romStartAddress;
            this.ROMAddressForPresets = romStartAddress + 0x8;
            this.xStart = xStart;
            this.xEnd = xEnd;
            this.zStart = zStart;
            this.zEnd = zEnd;
            this.presetCount = presetCount;
            this.encounterPresetIndices = new List<int>(defaultIndices);
        }

        private static List<int> shuffleCache = new List<int>();
        public void RandomizeEncounterPresets(int maxPresetsAvailable, int presetCount = 7)
        {
            shuffleCache.Clear();
            for (int k=0; k<maxPresetsAvailable; k++)
            {
                shuffleCache.Add(k);
            }
            shuffleCache.Shuffle();

            this.encounterPresetIndices.Clear();
            for (int k=0; k<presetCount; k++)
            {
                if (k < shuffleCache.Count)
                    this.encounterPresetIndices.Add(shuffleCache[k]);
                else
                {
                    var index = k % shuffleCache.Count;
                    this.encounterPresetIndices.Add(shuffleCache[index]);
                }
            }

            this.presetCount = presetCount;
        }

        private string GetHexForPresetIndex(int index)
        {
            if (index >= this.encounterPresetIndices.Count)
                return "0000";
                
            return $"{this.encounterPresetIndices[index]:X04}";
        }

        public string[] GenerateHexBlocks()
        {
            var count = $"{this.presetCount:X04}";
            var p0Hex = this.GetHexForPresetIndex(0);
            var p1Hex = this.GetHexForPresetIndex(1);
            var p2Hex = this.GetHexForPresetIndex(2);
            var p3Hex = this.GetHexForPresetIndex(3);
            var p4Hex = this.GetHexForPresetIndex(4);
            var p5Hex = this.GetHexForPresetIndex(5);
            var p6Hex = this.GetHexForPresetIndex(6);

            return new string[] {
                $"{count}{p0Hex}",
                $"{p1Hex}{p2Hex}",
                $"{p3Hex}{p4Hex}",
                $"{p5Hex}{p6Hex}",
            };
        }

        public EncounterRegion Copy()
        {
            var indices = this.encounterPresetIndices;
            return new EncounterRegion(this.ROMAddress, this.xStart, this.xEnd, this.zStart, this.zEnd, this.presetCount, indices[0], indices[1], indices[2], indices[3], indices[4], indices[5], indices[6]);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var blocks = this.GenerateHexBlocks();

            foreach (var block in blocks) { builder.Append(block); }

            return builder.ToString();
        }
    }

    public class EnemyTable
    {
        public int ROMAddressStart;
        public EnemyEntry[] enemies;

        public EnemyTable(int ROMAddressStart, params EnemyEntry[] enemies)
        {
            this.ROMAddressStart = ROMAddressStart;
            this.enemies = enemies;
        }
    }

    public class EnemyEntry
    {
        public int ROMAddress;
        public string name;

        public EnemyEntry(int ROMAddress, string name)
        {
            this.ROMAddress = ROMAddress;
            this.name = name;
        }
    }

    public class MapWriteOperation
    {
        public uint romAddress;
        public string[] hex32BitStrings;

        public string GetMerrowROMAddress() => this.romAddress.ToString("X8");
        public string GetMerrowWriteLength() => (this.hex32BitStrings.Length * 4).ToString("X04");

        public string GetMerrowWriteBlock()
        {
            var builder = new StringBuilder();

            foreach (var block in this.hex32BitStrings)
            {
                builder.Append(block);
            }

            return builder.ToString();
        }

        public MapWriteOperation(uint romAddress, string[] hex32BitStrings)
        {
            this.romAddress = romAddress;
            this.hex32BitStrings = hex32BitStrings;
        }

        public int ByteLength => this.hex32BitStrings.Length * 4;
    }
}
