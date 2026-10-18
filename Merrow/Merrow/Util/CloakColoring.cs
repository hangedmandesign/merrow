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
    public static class CloakColoring
    {
        public enum BrianColorPreset
        {
            // ROYGBIV Colors
            Red,
            Orange,
            Yellow,
            Green,
            Blue,
            Indigo,
            Violet,

            // Neutral Colors
            White,
            LightGrey,
            DarkGrey,
            Black,

            // Flags
            Canada,

            // Special Presets
            Rainbow,
            Lesbian,
            Trans,
        }

        public enum CommonColorCodes
        {
            Red =    0xFF0000,
            Orange = 0xFF7F00,
            Yellow = 0xFFFF00,
            Green =  0x00FF00,
            Blue =   0x0000FF,
            Indigo = 0x4B0082,
            Violet = 0x9400D3,

            White =     0xFFFFFF,
            LightGrey = 0x919191,
            DarkGrey =  0x3D3D3D,
            Black =     0x121212,

            TransBlue = 0x5BCEFA,
            TransPink = 0xF5A9B8,

            Les1 = 0xD62900,
            Les2 = 0xFF9B55,
            Les3 = 0xD462A5,
            Les4 = 0xA50062,

            Clear = 0x00000000,
        }

        public class CloakColorApplicator
        {
            private const int INDEX_TOP_LEFT= 0;
            private const int INDEX_BOTTOM_LEFT = 1;
            private const int INDEX_FAR_LEFT = 2;
            private const int INDEX_TOP_RIGHT= 3;
            private const int INDEX_BOTTOM_RIGHT= 4;
            private const int INDEX_FAR_RIGHT = 5;

            private readonly static int[] INDICES_LEFT_TO_RIGHT = new int[]
            {
                INDEX_FAR_LEFT,
                INDEX_BOTTOM_LEFT,
                INDEX_TOP_LEFT,
                INDEX_TOP_RIGHT,
                INDEX_BOTTOM_RIGHT,
                INDEX_FAR_RIGHT
            };

            private readonly static uint[] ROM_CLOAK_ADDRS = {
                0x8713DC,
                0x87148C,
                0x87153C,
                0x8715EC,
                0x87169C,
                0x87174C
            };

            private const uint ROM_SHAWL_ADDR = 0x870184;

            private Dictionary<uint, int> colorWrites = new Dictionary<uint, int>();

            public static CloakColorApplicator FromSpecialCloakPreset(BrianColorPreset colorPreset)
            {
                switch (colorPreset)
                {

                    default:
                        return null;

                    case BrianColorPreset.Red: return FromSingleColor(CommonColorCodes.Red);
                    case BrianColorPreset.Orange: return FromSingleColor(CommonColorCodes.Orange);
                    case BrianColorPreset.Yellow: return FromSingleColor(CommonColorCodes.Yellow);
                    case BrianColorPreset.Green: return FromSingleColor(CommonColorCodes.Green);
                    case BrianColorPreset.Blue: return FromSingleColor(CommonColorCodes.Blue);
                    case BrianColorPreset.Indigo: return FromSingleColor(CommonColorCodes.Indigo);
                    case BrianColorPreset.Violet: return FromSingleColor(CommonColorCodes.Violet);

                    case BrianColorPreset.White: return FromSingleColor(CommonColorCodes.White);
                    case BrianColorPreset.LightGrey: return FromSingleColor(CommonColorCodes.LightGrey);
                    case BrianColorPreset.DarkGrey: return FromSingleColor(CommonColorCodes.DarkGrey);
                    case BrianColorPreset.Black: return FromSingleColor(CommonColorCodes.Black);

                    case BrianColorPreset.Canada:
                        return FromTwoSolidColors(CommonColorCodes.White, CommonColorCodes.Red);

                    case BrianColorPreset.Rainbow:
                        return LeftToRight(shawlColor: (int)CommonColorCodes.Red, new int[]
                        {
                            (int)CommonColorCodes.Orange,
                            (int)CommonColorCodes.Yellow,
                            (int)CommonColorCodes.Green,
                            (int)CommonColorCodes.Blue,
                            (int)CommonColorCodes.Indigo,
                            (int)CommonColorCodes.Violet,
                        });

                    case BrianColorPreset.Lesbian:
                        return LeftToRight(shawlColor: (int)CommonColorCodes.White, new int[]
                        {
                            (int)CommonColorCodes.Les1,
                            (int)CommonColorCodes.Les2,
                            (int)CommonColorCodes.White,
                            (int)CommonColorCodes.White,
                            (int)CommonColorCodes.Les3,
                            (int)CommonColorCodes.Les4,
                        });

                    case BrianColorPreset.Trans:
                        return LeftToRight(shawlColor: (int)CommonColorCodes.TransBlue, new int[]
                        {
                            (int)CommonColorCodes.TransBlue,
                            (int)CommonColorCodes.TransPink,
                            (int)CommonColorCodes.White,
                            (int)CommonColorCodes.White,
                            (int)CommonColorCodes.TransPink,
                            (int)CommonColorCodes.TransBlue,
                        });
                }
            }

            public static CloakColorApplicator FromShawlExplicit(int explicitColor)
            {
                var applicator = new CloakColorApplicator();
                applicator.colorWrites[ROM_SHAWL_ADDR] = explicitColor;

                return applicator; ;
            }

            public static CloakColorApplicator FromSingleColor(CommonColorCodes color)
            {
                return TwoSolids((int)color, (int)color);
            }

            public static CloakColorApplicator FromTwoSolidColors(CommonColorCodes shawl, CommonColorCodes cloak)
            {
                return TwoSolids((int)shawl, (int)cloak);
            }

            private static CloakColorApplicator LeftToRight(int shawlColor, int[] cloakColors)
            {
                var applicator = new CloakColorApplicator();
                applicator.colorWrites[ROM_SHAWL_ADDR] = shawlColor;

                for (int k=0; k<INDICES_LEFT_TO_RIGHT.Length; k++)
                {
                    if (k>= cloakColors.Length)
                        break;

                    var pieceIndex = INDICES_LEFT_TO_RIGHT[k];
                    var addr = ROM_CLOAK_ADDRS[pieceIndex];

                    applicator.colorWrites[addr] = cloakColors[k];
                }

                return applicator;
            }


            private static CloakColorApplicator TwoSolids(int shawlColor, int cloakColor)
            {
                var applicator = new CloakColorApplicator();
                applicator.colorWrites[ROM_SHAWL_ADDR] = shawlColor;

                for (int k = 0; k < INDICES_LEFT_TO_RIGHT.Length; k++)
                {
                    var pieceIndex = INDICES_LEFT_TO_RIGHT[k];
                    var addr = ROM_CLOAK_ADDRS[pieceIndex];

                    applicator.colorWrites[addr] = cloakColor;
                }

                return applicator;
            }

            public void ApplyToPatchStrings(List<string> patchStrings)
            {
                foreach (var pair in this.colorWrites)
                {
                    var addr = pair.Key;
                    var color = pair.Value;

                    var colorHex = color == (int)CommonColorCodes.Clear ? color.ToString("X08") : (color.ToString("X06") + "FF");

                    var operation = new MapWriteOperation(addr, colorHex);
                    operation.AddToPatchStrings(patchStrings);
                }
            }
        }
    }
}
