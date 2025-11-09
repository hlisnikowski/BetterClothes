using BetterClothes.Config;
using System.Collections.Generic;
using System.Linq;

namespace BetterClothes;

public static class BCMapper
{
    public static Dictionary<string, List<BCBuff>> Map(ModConfig source)
    {
        var dest = new Dictionary<string, List<BCBuff>>();

        foreach (var category in source.ClothesCategories)
        {
            dest.Add(category.Name, []);
            var buffs = source.Buffs.Where(x => category.BuffsByTypes.Any(categoryBuffType => x.Type == categoryBuffType)).ToList();

            dest[category.Name].AddRange(buffs.Select(x => new BCBuff(
                name: x.Type.ToString(),
                treeKey: BCMethods.GetTreeKey(x.Type),
                enchantType: x.Type,
                values: [.. x.Values],
                isPercent: x.IsPercent
                )));
        }

        return dest;
    }
}
