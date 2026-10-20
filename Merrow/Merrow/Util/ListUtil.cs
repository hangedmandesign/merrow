using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merrow.Util
{
    public static class ListUtil
    {
        private static Random rng = new Random();
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static bool Contains(this IList<SpellNameEnum> names, SpellNameEnum nameEnum)
        {
            for (int k=0; k<names.Count; k++)
            {
                if (names[k] == nameEnum)
                    return true;
            }

            return false;
        }
        

        public static SpellData GetSpell(this IList<SpellData> definitions, SpellNameEnum nameEnum)
        {
            var index = (int)nameEnum;
            return definitions[index];
        }

        public static SpellAnimationData GetSpellAnimation(this IList<SpellAnimationData> definitions, SpellNameEnum nameEnum)
        {
            var index = (int)nameEnum;
            return definitions[index];
        }

        public static SpellText GetSpellText(this IList<SpellText> definitions, SpellNameEnum nameEnum)
        {
            var index = (int)nameEnum;
            return definitions[index];
        }

        private static List<string> attributeCache = new List<string>();
        public static string[] CopyCurrentAttributeData(this IList<SpellData> definitions)
        {
            attributeCache.Clear();

            foreach (SpellData data in definitions)
            {
                var attributes = data.GetAttributeData();
                attributeCache.Add(attributes);
            }

            return attributeCache.ToArray();
        }
    }
}
