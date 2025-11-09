using System.Collections.Generic;

namespace BetterClothes.Config;

public sealed class ModConfig
{
    /// <summary>
    /// New items will automaticaly gets buffs.
    /// </summary>
    public bool AddBuffAfterItemSpawn { get; set; }

    /// <summary>
    /// Code of item required for buff.
    /// </summary>
    public string BuffCostCode { get; set; } = "gear-rusty";

    /// <summary>
    /// Item quantity cost for buff.
    /// </summary>
    public int BuffCostQty { get; set; } = 1;

    /// <summary>
    /// When buff have been added, show chat message.
    /// </summary>
    public bool MessageNotifyOnSuccess { get; set; } = true;

    /// <summary>
    /// Buff pool.
    /// </summary>
    public List<BuffConfig> Buffs { get; set; } = [];

    /// <summary>
    /// ItemWearable > Attributes.clothescategory codes and which <see cref="Buffs"/> can be applied based on <see cref="BuffType"/>.
    /// </summary>
    public List<ClothesCategory> ClothesCategories { get; set; } = [];

    /// <summary>
    /// Data seed with default values.
    /// </summary>
    /// <returns>ModConfig with seeded data.</returns>
    public ModConfig Seed()
    {
        AddBuffAfterItemSpawn = true;
        MessageNotifyOnSuccess = true;
        BuffCostCode = "gear-rusty";
        BuffCostQty = 1;

        Buffs = [
        new BuffConfig
        {
            Type = BuffType.WalkSpeed,
            Values = [2, 3], // 18
            IsPercent = true
        },
        new BuffConfig
        {
            Type = BuffType.Hunger,
            Values = [-4, -5], // 36
            IsPercent = true
        },
        new BuffConfig
        {
            Type = BuffType.Luck,
            Values = [2, 3], // 18
            IsPercent = true
        },
        new BuffConfig
        {
            Type = BuffType.Damage,
            Values = [4, 5], // 30
            IsPercent = true
        },
        new BuffConfig
        {
            Type = BuffType.Mining,
            Values = [2, 3], // 18
            IsPercent = true
        },
        new BuffConfig
        {
            Type = BuffType.Health,
            Values = [.75f, 1f], // 6
            IsPercent = false
        },
        new BuffConfig
        {
            Type = BuffType.Farming,
            Values = [1.5f, 2.5f], // 15
            IsPercent = true
        },
        new BuffConfig
        {
            Type = BuffType.Durability,
            Values = [2f, 3f], // 18
            IsPercent = true
        }
        ];

        ClothesCategories = [
        // ----- RIGHT SIDE OF CHARACTER PANEL -----
        new ClothesCategory
        {
            Name = "neck",
            BuffsByTypes = BCDataSeed.CharacterRightSideBuffs
        },
        new ClothesCategory
        {
            Name = "emblem",
            BuffsByTypes = BCDataSeed.CharacterRightSideBuffs
        },
        new ClothesCategory
        {
            Name = "face",
            BuffsByTypes = BCDataSeed.CharacterRightSideBuffs
        },
        new ClothesCategory
        {
            Name = "arm",
            BuffsByTypes = BCDataSeed.CharacterRightSideBuffs
        },
        new ClothesCategory
        {
            Name = "hand",
            BuffsByTypes = BCDataSeed.CharacterRightSideBuffs
        },
        new ClothesCategory
        {
            Name = "waist",
            BuffsByTypes = BCDataSeed.CharacterRightSideBuffs
        },
        // -----  LEFT SIDE OF CHARACTER PANEL -----
        new ClothesCategory
        {
            Name = "head",
            BuffsByTypes = BCDataSeed.CharacterLeftSideBuffs
        },
        new ClothesCategory
        {
            Name = "shoulder",
            BuffsByTypes = BCDataSeed.CharacterLeftSideBuffs
        },
        new ClothesCategory
        {
            Name = "upperbody",
            BuffsByTypes = BCDataSeed.CharacterLeftSideBuffs
        },
        new ClothesCategory
        {
            Name = "upperbodyover",
            BuffsByTypes = BCDataSeed.CharacterLeftSideBuffs
        },
        new ClothesCategory
        {
            Name = "lowerbody",
            BuffsByTypes = BCDataSeed.CharacterLeftSideBuffs
        },
        new ClothesCategory
        {
            Name = "foot",
            BuffsByTypes = BCDataSeed.CharacterLeftSideBuffs
        }
        ];

        return this;
    }
}

internal static class BCDataSeed
{
    public static readonly List<BuffType> CharacterRightSideBuffs =
        [BuffType.Luck, BuffType.Farming, BuffType.Damage, BuffType.WalkSpeed];

    public static readonly List<BuffType> CharacterLeftSideBuffs =
        [BuffType.Health, BuffType.Hunger, BuffType.Mining, BuffType.Durability];
}


public class BuffConfig
{
    /// <summary>
    /// Type of buff.
    /// </summary>
    public BuffType Type { get; set; }

    /// <summary>
    /// Value that will be applied to item.
    /// </summary>
    public List<float> Values { get; set; } = [];

    /// <summary>
    /// Is value percent type. <see cref="BuffType.WalkSpeed"/> is for example percent value, but <see cref="BuffType.Health"/> is not.
    /// </summary>
    public bool IsPercent { get; set; } = true; 
}

public class ClothesCategory
{
    /// <summary>
    /// This is a code of ItemWearable > attributes.clothescategory
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Which buffs can be applied to this item.
    /// </summary>
    public List<BuffType> BuffsByTypes { get; set; } = [];
}
