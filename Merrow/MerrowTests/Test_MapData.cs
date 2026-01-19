using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Merrow;
using Merrow.Util;
using System.Text;

namespace MerrowTests
{
    [TestClass]
    public class Test_MapData
    {
        [TestMethod]
        public void Test_EnemyPackIDsAreRandomizing()
        {
            var mapData = DataStore.GetMapData();

            var moddedMammon6 = mapData.allData[mapData.allData.Length - 1];
            var backupMammon6 = moddedMammon6.Copy();

            moddedMammon6.RandomizeEnemiesWithinPacks();

            var moddedBuilder = new StringBuilder();
            var backupBuilder = new StringBuilder();

            for (int k=0; k< moddedMammon6.packDefinitions.Length; k++)
            {
                var packModded = moddedMammon6.packDefinitions[k];
                var packBackup = backupMammon6.packDefinitions[k];

                var hexModded = string.Join(",", packModded.GenerateHexBlocks(out var baseLength));
                var hexBackup = string.Join(",", packBackup.GenerateHexBlocks(out var copyLength));

                moddedBuilder.Append(hexModded); 
                backupBuilder.Append(hexBackup);
            }

            var modded = moddedBuilder.ToString();
            var backup = backupBuilder.ToString();

            Assert.AreNotEqual(modded, backup);
        }

        [TestMethod]
        public void Test_EnemyRegionPresetsAreRandomizing()
        {
            var region = new EncounterRegion(0x0, 0, 0, 0, 0, 5, new int[]
            {
                0, 1, 2, 3, 4, 5, 6
            });

            var initialHex = region.GenerateHexBlocks();

            region.RandomizeEncounterPresets(4);

            var modifiedHex = region.GenerateHexBlocks();

            Assert.AreNotEqual(initialHex, modifiedHex);

            Console.WriteLine(String.Join(",", initialHex));
            Console.WriteLine(String.Join(",", modifiedHex));
        }

        [TestMethod]
        public void Test_GivesDifferentRegionDefsAfterShuffle()
        {
            var mapData = DataStore.GetMapData();
            var mapCopy = mapData.Copy();

            var firstMapBase = mapData.allData[0];
            var firstMapCopy = mapCopy.allData[0];

            firstMapBase.RandomizeMonsterPresets();

            for (int i = 0; i < firstMapBase.encounterRegions.Length; i++)
            {
                var regionBase = firstMapBase.encounterRegions[i];
                var regionCopy = firstMapCopy.encounterRegions[i];

                //Console.WriteLine(regionBase.ToString());
                //Console.WriteLine(regionCopy.ToString());

                Assert.AreNotEqual(regionBase.ToString(), regionCopy.ToString());
            }
        }

        [TestMethod]
        public void Test_CopyFunctionGivesSameHexOperationsAfterShuffle()
        {
            var mapData = DataStore.GetMapData();

            mapData.RandomizeMonsterTables();
            mapData.RandomizeAllMonsterPresets();

            var copiedData = mapData.Copy();

            mapData.isDirty = true;
            copiedData.isDirty = true;

            var normalOps = mapData.GetMapWriteOperations();
            var copiedOps = mapData.GetMapWriteOperations();

            Console.WriteLine("{0} vs. {1}", normalOps.Length, copiedOps.Length);

            for (int i = 0; i < copiedOps.Length; i++)
            {
                var normalOp = normalOps[i];
                var copiedOp = copiedOps[i];

                Assert.AreEqual(normalOp.GetMerrowROMAddress(), copiedOp.GetMerrowROMAddress());
                Assert.AreEqual(normalOp.GetMerrowWriteLength(), copiedOp.GetMerrowWriteLength());
                Assert.AreEqual(normalOp.GetMerrowWriteBlock(), copiedOp.GetMerrowWriteBlock());
            }
        }


        [TestMethod]
        public void Test_EnemyIndicesDoNotExceedTableRanges()
        {
            var mapData = DataStore.GetMapData();

            for (int run=0; run < 100_000; run++)
            {
                var runData = mapData.Copy();

                runData.RandomizeMonsterTables();
                runData.RandomizeAllMonsterPresets();
                runData.FixBaragoonMoor();
                runData.FixBrannochCastle();
                runData.FixMammonsWorld();

                for (int a = 0; a<runData.allData.Length; a++)
                {
                    var area = runData.allData[a];
                    var tableIndex = area.mapData.tableIndex;
                    var enemyCount = area.globalEnemyTables[tableIndex].enemies.Length;

                    for (int p=0; p < area.packDefinitions.Length; p++)
                    {
                        var pack = area.packDefinitions[p];

                        Assert.IsTrue(pack.enemy1.enemyID <  enemyCount);
                        Assert.IsTrue(pack.enemy2.enemyID <  enemyCount);
                        Assert.IsTrue(pack.enemy3.enemyID <  enemyCount);
                        Assert.IsTrue(pack.enemy4.enemyID <  enemyCount);
                    }
                }
            }
        }
    }
}
