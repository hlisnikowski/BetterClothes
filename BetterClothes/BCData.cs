using BetterClothes.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterClothes
{
    public static class BCData
    {
        public const string BUFF_ATTRIBUTE_KEY = "betterclothesbuff";
        public const string STATS_WEARABLE_ATTRIBUTE_KEY = "wearableclothes";
        public const string STATS_BUFF_ATTRIBUTE_KEY = "betterclothesbuff";
        public const int BUFF_PACKET_ID = 799;
        public static ModConfig Config { get; set; }
        public static Random Random { get; set; }

        public static string ModID { get; set; }
        public static string ModIDPrefix { get; set; }

#pragma warning disable CA2211 // Cant be readonly...
        public static Dictionary<string, List<BCBuff>> Buffs = [];
#pragma warning restore CA2211 

        /// <summary>
        /// Gets buff data by category name. If <paramref name="type"/> is null, gets random buff. 
        /// </summary>
        /// <param name="key">ItemWearable clothescategory name.</param>
        public static BCBuff? GetBuff(string key, BuffType? type = null)
        {
            if (!Buffs.TryGetValue(key, out var buffs) || buffs == null || buffs.Count == 0) {
                return null;
            }

            // Random buff;
            if (type == null)
            {
                var shuffle = buffs.OrderBy(x => Guid.NewGuid()).ToList();
                return shuffle[0];
            }

            return buffs.FirstOrDefault(x => x.Type == type);
        }

        /// <summary>
        /// Keys for EntityPlayer.Stats by BuffType
        /// </summary>
        public readonly static Dictionary<BuffType, string[]> StatsKeys = new()
        {
            { BuffType.WalkSpeed, ["walkspeed"] },
            { BuffType.Hunger, ["hungerrate"]},
            { BuffType.Health, ["maxhealthExtraPoints"] },
            { BuffType.Farming, [ "forageDropRate", "animalLootDropRate", "animalHarvestingTime"] },
            { BuffType.Mining, ["miningSpeedMul"] },
            { BuffType.Damage, ["meleeWeaponsDamage", "rangedWeaponsDamage"] },
            { BuffType.Luck, ["vesselContentsDropRate", "rustyGearDropRate", "wildCropDropRate"] },
            { BuffType.Durability, ["armorDurabilityLoss", "armorWalkSpeedAffectedness"] },
        };
    }

    public enum BuffType
    {
        WalkSpeed,  // walkspeed
        Hunger,     // hungerrate
        Health,     // maxhealthExtraPoints
        Mining,     // miningSpeedMul
        Luck,       // vesselContentsDropRate, rustyGearDropRate, animalLootDropRate
        Damage,     // meeleWeaponsDamage, rangedWeaponsDamage 
        Farming,    // wildCropDropRate, forageDropRate, animalLootDropRate
        Durability, // armorDurabilityLoss, armorWalkSpeedAffectedness
        // NOTE: oreDropRate is useless since more than 100% != more ore drop...
    }
}
