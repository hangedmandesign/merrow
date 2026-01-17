using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merrow.Util
{
    public class MonsterPackDefinitions
    {
        public int ROMAddress { get; private set; }

        public int packCount;

        public EnemyPackMember enemy1;
        public EnemyPackMember enemy2;
        public EnemyPackMember enemy3;
        public EnemyPackMember enemy4;

        public void CapEnemyIDs(int uniqueEnemyCount)
        {
            if (this.packCount > 0) this.enemy1.enemyID = this.enemy1.enemyID % uniqueEnemyCount;
            if (this.packCount > 1) this.enemy2.enemyID = this.enemy2.enemyID % uniqueEnemyCount;
            if (this.packCount > 2) this.enemy3.enemyID = this.enemy3.enemyID % uniqueEnemyCount;
            if (this.packCount > 3) this.enemy4.enemyID = this.enemy4.enemyID % uniqueEnemyCount;
        }

        public void RandomizeEnemyIDs(int maxIDInclusive)
        {
            var rng = new Random();

            if (this.packCount > 0) this.enemy1.enemyID = rng.Next(maxIDInclusive + 1);
            if (this.packCount > 1) this.enemy2.enemyID = rng.Next(maxIDInclusive + 1);
            if (this.packCount > 2) this.enemy3.enemyID = rng.Next(maxIDInclusive + 1);
            if (this.packCount > 3) this.enemy4.enemyID = rng.Next(maxIDInclusive + 1);
        }

        public struct EnemyPackMember
        {
            public int enemyID;
            public int minCount;
            public int extraCount;

            public string[] GenerateHexBlocks()
            {
                return new string[]
                {
                    $"{this.enemyID:8X}",
                    $"{this.minCount:8X}",
                    $"{this.extraCount:8X}"
                };
            }
        }

        public MonsterPackDefinitions(int rOMAddress, params EnemyPackMember[] enemies)
        {
            this.ROMAddress = rOMAddress;
            this.packCount = enemies.Length;

            if (enemies.Length > 0) this.enemy1 = enemies[0];
            if (enemies.Length > 1) this.enemy2 = enemies[1];
            if (enemies.Length > 2) this.enemy3 = enemies[2];
            if (enemies.Length > 3) this.enemy4 = enemies[3];
        }

        private static List<string> blocks = new List<string>();
        public string[] GenerateHexBlocks()
        {
            blocks.Clear();

            if (this.packCount > 0) blocks.AddRange(this.enemy1.GenerateHexBlocks());
            if (this.packCount > 1) blocks.AddRange(this.enemy2.GenerateHexBlocks());
            if (this.packCount > 2) blocks.AddRange(this.enemy3.GenerateHexBlocks());
            if (this.packCount > 3) blocks.AddRange(this.enemy4.GenerateHexBlocks());

            return blocks.ToArray();
        }
    }

    public class AreaMapData
    {
        public int ROMAddress { get; private set; }

        public int unk0;
        public int ptrDoorData;
        public int doorCount;
        public int unk8;
        public short unk10;
        public short tableIndex;
        public int unk14;

        public AreaMapData(int rOMAddress, int unk0, int ptrDoorData, int doorCount, int unk8, short unk10, short tableIndex, int unk14)
        {
            ROMAddress = rOMAddress;
            this.unk0 = unk0;
            this.ptrDoorData = ptrDoorData;
            this.doorCount = doorCount;
            this.unk8 = unk8;
            this.unk10 = unk10;
            this.tableIndex = tableIndex;
            this.unk14 = unk14;
        }

        public void AssignTableIndex(int assignedIndex)
        {
            this.tableIndex = (short)assignedIndex;
        }

        public void RandomizeTableIndex(int maxIndexInclusive = 5)
        {
            var rng = new System.Random();
            this.tableIndex = (short)rng.Next(maxIndexInclusive + 1);
        }

        public string[] GenerateHexBlock()
        {
            return new string[]
            {
                $"{this.unk0}:8X",
                $"{this.ptrDoorData}:8X",
                $"{this.doorCount}:8X",
                $"{this.unk8}:8X",
                $"{this.unk10}:4X",
                $"{this.tableIndex}:4X",
                $"{this.unk14}:8X",
            };
        }
    }

    internal class EncounterRegion
    {
        public int ROMAddress { get; private set; }

        public int xStart;
        public int xEnd;
        public int zStart;
        public int zEnd;

        public List<int> encounterPresetIndices = new List<int>();

        public EncounterRegion(int romAddress, int xStart, int xEnd, int zStart, int zEnd, params int[] defaultIndices)
        {
            this.ROMAddress = romAddress;
            this.xStart = xStart;
            this.xEnd = xEnd;
            this.zStart = zStart;
            this.zEnd = zEnd;
            this.encounterPresetIndices = new List<int>(defaultIndices);
        }
        public void RandomizeEncounterPresets(int maxPresetIndex, int presetCount = 7)
        {
            var rng = new System.Random();
            this.encounterPresetIndices.Clear();
            for (int k=0; k<presetCount; k++)
            {
                var randomIndex = rng.Next(maxPresetIndex);
                this.encounterPresetIndices.Add(randomIndex);
            }
        }

        private string GetHexForPresetIndex(int index)
        {
            if (index >= this.encounterPresetIndices.Count)
                return "0000";
                
            return $"{this.encounterPresetIndices[index]:4X}";
        }

        public string[] GenerateHexBlocks()
        {
            var indices = this.encounterPresetIndices.ToArray();

            var p0Hex = this.GetHexForPresetIndex(0);
            var p1Hex = this.GetHexForPresetIndex(1);
            var p2Hex = this.GetHexForPresetIndex(2);
            var p3Hex = this.GetHexForPresetIndex(3);
            var p4Hex = this.GetHexForPresetIndex(4);
            var p5Hex = this.GetHexForPresetIndex(5);
            var p6Hex = this.GetHexForPresetIndex(6);
            var p7Hex = "0000";

            return new string[] {
                $"{this.xStart:4X}{this.xEnd:4X}",
                $"{this.zStart:4X}{this.zEnd:4X}",
                $"{p0Hex}{p1Hex}",
                $"{p2Hex}{p3Hex}",
                $"{p4Hex}{p5Hex}",
                $"{p6Hex}{p7Hex}",
            };
        }
    }
}
