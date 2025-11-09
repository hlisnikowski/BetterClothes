using HarmonyLib;
using System;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace BetterClothes;

/// <summary>
/// Better Clothes Helper
/// </summary>
public static class BCMethods
{
    public static string Translate(string code, params object[] args)
    {
        return Lang.Get(BCData.ModIDPrefix + code, args);
    }

    /// <summary>
    /// Gets <see cref="BuffType"/> from name.
    /// </summary>
    /// <param name="buffType">text represation of enum</param>
    public static BuffType? ParseType(string buffType)
    {
        if(Enum.TryParse(buffType, true, out BuffType result))
            return result;
        else return null;
    }

    /// <summary>
    /// Converts enum to string.
    /// </summary>
    public static string GetTreeKey(BuffType type)
       => type.ToString();

     /// <summary>
     /// Gets value of buff by <see cref="BuffType"/>
     /// </summary>
    public static float GetBuffByType(ItemStack itemstack, BuffType type)
       => GetBuffByType(itemstack, GetTreeKey(type));

    /// <summary>
    /// Gets value of buff by <see cref="BuffType"/> as text.
    /// </summary>
    public static float GetBuffByType(ItemStack itemstack, string type)
    {
        if (itemstack?.Attributes == null) return 0;

        ITreeAttribute buffAttr = itemstack.Attributes.GetTreeAttribute(BCData.BUFF_ATTRIBUTE_KEY);
        if (buffAttr == null) return 0;

        return buffAttr.GetFloat(type, 0);
    }

    /// <summary>
    /// Gets buff and its value by type. If type is not present then it will asign random type.
    /// If item already has buff then its returned.
    /// </summary>
    public static (BCBuff?, float) GetOrAdd(ItemStack itemstack, BuffType? type = null)
    {
        //  emblem, face etc.
        if (itemstack.IsNotValid(out string categoryCode)) return (null, 0);

        ITreeAttribute? buffAttr = itemstack.Attributes.GetTreeAttribute(BCData.BUFF_ATTRIBUTE_KEY);

        var buff = BCData.GetBuff(categoryCode, type);
        if (buff == null)
        {
            BCOrchestrator.Api.Logger.Notification($"Tried to add buff to {itemstack.GetName()} but settings with type '{type}' does not exists.");
            return (null, 0);
        }
        // null == buff not has been added yet.
        if (buffAttr == null)
        {
            buffAttr = new TreeAttribute();
            var val = buff.GetRandomValue();
            buffAttr.SetFloat(buff.TreeKey, val);

            itemstack.Attributes[BCData.BUFF_ATTRIBUTE_KEY] = buffAttr;
            return (buff, buff.Value);
        }

        return (buff, buffAttr.GetFloat(buff.TreeKey, 0));
    }

    /// <summary>
    /// Gets buff from itemstack. If not present, then <c>null,0</c> will be returned.
    /// </summary>
    public static (BCBuff?, float) GetBuffAndValue(ItemStack itemstack)
    {
        if (itemstack.IsNotValid(out string categoryCode)) return (null, 0);

        ITreeAttribute? buffAttr = itemstack.Attributes.GetTreeAttribute(BCData.BUFF_ATTRIBUTE_KEY);
        if (buffAttr == null) return (null, 0);
        var type = buffAttr.FirstOrDefault().Key;
        if (type == null) return (null, 0);

       var buff = BCData.GetBuff(categoryCode, ParseType(type));
       return (buff, buffAttr.GetFloat(type, 0));
    }

    public static bool HasBuffMaxValue(BCBuff buff, float current)
    {
        var data = BCData.Config.Buffs.Where(x => x.Type == buff.Type).FirstOrDefault();
        if (data == null)
        {
            BCOrchestrator.Api.Log("This item has buff that does not exist in config.");
            return false;
        }
        return data.Values.LastOrDefault(0) == current;
    }

    /// <summary>
    /// Can we add buff to this itemstack ? Checks if it has <c>clothescategory</c> and tree attribute.
    /// </summary>
    public static bool CanAddBuff(ItemStack itemstack)
    {
        if (itemstack.IsNotValid(out string categoryCode)) return false;
        ITreeAttribute? buffAttr = itemstack.Attributes.GetTreeAttribute(BCData.BUFF_ATTRIBUTE_KEY);
        if (buffAttr != null) return false;
        var buff = BCData.GetBuff(categoryCode);
        return buff != null;
    }

    /// <summary>
    /// Removes buff and apply new one. <seealso cref="RemoveBuff(ItemStack)"/>
    /// </summary>
    public static (BCBuff?, float) Rebuff(ItemStack itemstack, BuffType? type = null)
    {
        RemoveBuff(itemstack);
        return GetOrAdd(itemstack, type);
    }

    /// <summary>
    /// Removes buff from itemstack.
    /// </summary>
    public static bool RemoveBuff(ItemStack itemstack)
    {
        if (itemstack.IsNotValid(out string categoryCode)) return false;
        itemstack.Attributes.RemoveAttribute(BCData.BUFF_ATTRIBUTE_KEY);
        return true;
    }

    /// <summary>
    /// Resets stats and checks all character inventory slots and calculates stats.
    /// Use <see cref="Vintagestory.API.Config.GlobalConstants.characterInvClassName"/> inventory.
    /// </summary>
    public static void UpdatePlayerStats(EntityPlayer player, IInventory inventory)
    {
        if (inventory == null) return;

        ResetStats(player);

        foreach(var slot in inventory)
        {
            if (slot?.Itemstack?.Attributes == null) continue;
            UpdateStatsByType(slot.Itemstack, player);
        }

        player.WatchedAttributes.MarkPathDirty("stats");
        player.GetBehavior<EntityBehaviorHealth>()?.MarkDirty(); // update health gui

    }

    /// <summary>
    /// Sets player stats with <see cref="BCData.STATS_BUFF_ATTRIBUTE_KEY"/> to <c>0</c>
    /// </summary>
    public static void ResetStats(EntityPlayer player)
    {
        foreach (var stat in BCData.StatsKeys)
        {
            foreach (var key in stat.Value)
                player.Stats.Set(key, BCData.STATS_BUFF_ATTRIBUTE_KEY, 0);
        }
    }


    private static bool IsNotValid(this ItemStack itemstack, out string categoryCode)
    {
        categoryCode = string.Empty;
        if (itemstack == null || itemstack?.ItemAttributes == null) return true;
        categoryCode = itemstack.ItemAttributes["clothescategory"]?.ToString() ?? string.Empty;
        if (categoryCode.IsEmpty()) return true;

        return false;
    }

    #region STATS SETTERS
    private static void UpdateStatsByType(ItemStack stack, EntityPlayer player)
    {
        UpdateWalking(stack, player);
        UpdateHunger(stack, player);
        UpdateMaxHealth(stack, player);
        UpdateDamage(stack, player);
        UpdateMining(stack, player);
        UpdateLuck(stack, player);
        UpdateDurability(stack, player);
        UpdateHarvest(stack, player);
    }

    private static void UpdateHarvest(ItemStack stack, EntityPlayer player)
    {
        SetStatsValue(stack, player, BuffType.Farming, (curr, buff, code) =>
        {
            if (code == "animalHarvestingTime") // is negative
                buff *= -1;
            return (buff / 100f) + curr;
        });
    }

    private static void UpdateDurability(ItemStack stack, EntityPlayer player)
    {
        SetStatsValue(stack, player, BuffType.Durability, (curr, buff, _) =>
        {
            return (buff / 100f) + curr;
        });
    }

    private static void UpdateWalking(ItemStack stack, EntityPlayer player)
    {
        SetStatsValue(stack, player, BuffType.WalkSpeed, (curr, buff, _) =>
        {
            return (buff / 100f) + curr;
        });
    }
    private static void UpdateHunger(ItemStack stack, EntityPlayer player) {
        SetStatsValue(stack, player, BuffType.Hunger, (curr, buff, _) =>
        {
            return (buff / 100f) + curr;
        });
    }
    private static void UpdateMaxHealth(ItemStack stack, EntityPlayer player) {
        SetStatsValue(stack, player, BuffType.Health, (curr, buff, _) =>
        {
            return buff + curr;
        });
    }

    private static void UpdateDamage(ItemStack stack, EntityPlayer player)
    {
        SetStatsValue(stack, player, BuffType.Damage, (curr, buff, _) =>
        {
            return (buff / 100) + curr;
        });
    }

    private static void UpdateMining(ItemStack stack, EntityPlayer player)
    {
        SetStatsValue(stack, player, BuffType.Mining, (curr, buff, _) =>
        {
            return (buff / 100) + curr;
        });
    }

    private static void UpdateLuck(ItemStack stack, EntityPlayer player)
    {
        SetStatsValue(stack, player, BuffType.Luck, (curr, buff, _) =>
        {
            return (buff / 100) + curr;
        });
    }

    public static void SetStatsValue(ItemStack stack, EntityPlayer player, BuffType type, System.Func<float, float, string, float> sum)
    {
        foreach (var key in BCData.StatsKeys[type])
        {
            ITreeAttribute playerStatsTree = player.WatchedAttributes.GetOrAddTreeAttribute("stats");
            ITreeAttribute buffTree = playerStatsTree.GetOrAddTreeAttribute(key);
            float currentValue = buffTree.GetFloat(BCData.STATS_BUFF_ATTRIBUTE_KEY, 0f);
            float buffValue = GetBuffByType(stack, type);
            player.Stats.Set(key, BCData.STATS_BUFF_ATTRIBUTE_KEY, sum(currentValue, buffValue, key));
        }
    }
    #endregion

}
