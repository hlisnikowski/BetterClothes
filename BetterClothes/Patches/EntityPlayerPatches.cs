using HarmonyLib;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace BetterClothes.Patches
{
    [HarmonyPatch(typeof(EntityPlayer))]
    public class EntityPlayerPatches
    {
        [HarmonyPatch("GetWalkSpeedMultiplier")]
        [HarmonyPostfix]
        public static void GetWalkSpeedMultiplier_Postfix(EntityPlayer __instance, ref double __result)
        {
            try
            {
                float totalWalkSpeedBonus = 0f;
                
                IPlayer player = __instance.Player;
                if (player == null) return;

                if (player.InventoryManager != null)
                {
                    IInventory charInv = player.InventoryManager.GetOwnInventory(GlobalConstants.characterInvClassName);
                    if (charInv == null) return;
                    foreach (ItemSlot slot in charInv)
                    {
                        if (slot?.Itemstack?.Collectible is ItemWearable i)
                        {
                            float walkSpeedLevel = BCMethods.GetBuffByType(slot.Itemstack, BuffType.WalkSpeed);
                            if (walkSpeedLevel > 0)
                            {
                                totalWalkSpeedBonus += walkSpeedLevel * 0.01f;
                            }
                        }
                    }
                }

                if (totalWalkSpeedBonus > 0)
                {
                    __result += totalWalkSpeedBonus;
                }
            }
            catch (System.Exception e)
            {
                __instance.World.Logger.Error("Enchanting Mod: Error in walk speed calculation - " + e.Message);
            }
        }
    }

    [HarmonyPatch(typeof(CollectibleObject), "GetHeldItemInfo")]
    public class ItemTooltipPatch
    {
        [HarmonyPostfix]
        public static void GetHeldItemInfo_Postfix(CollectibleObject __instance,
            ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            if (inSlot?.Itemstack?.Collectible is ItemWearable && inSlot.Itemstack != null)
            {
                var (buff, val) = BCMethods.GetBuffAndValue(inSlot.Itemstack);
                if (val != 0 && buff != null)
                {
                    bool isMax = BCMethods.HasBuffMaxValue(buff, val);
                    bool isNegative = val < 0;
                    dsc.Append($"<font size=\"24\"><icon size=\"20\" path=\"betterclothes:textures/buff.svg\"></icon></font> ");
                    dsc.Append($"<font color=\"{(isMax ? "#FFD700" : "#88ff88")}\">{buff.GetName()} {(isNegative ? "" : "+")}{val}");
                    if (buff.IsPercent) dsc.Append('%');
                    if (isMax) dsc.Append($" <font size=\"24\"><icon size=\"20\" path=\"betterclothes:textures/star.svg\"></icon></font>");
                    dsc.Append(" </font>");
                    dsc.AppendLine();
                }
            }
        }
    }

    [HarmonyPatch(typeof(InventoryBase), "DidModifyItemSlot")]
    public class Patch_Inventory_DidModifyItemSlot
    {
        static void Postfix(InventoryBase __instance, ItemSlot slot)
        {
            var playerProp = __instance.GetType().GetProperty("Player");
            var player = playerProp?.GetValue(__instance) as IPlayer;
            if (player?.Entity is EntityPlayer entityPlayer && entityPlayer.World.Side == EnumAppSide.Server)
            {
                if (BCData.Config.AddBuffAfterItemSpawn && BCMethods.CanAddBuff(slot.Itemstack))
                {
                    BCMethods.GetOrAdd(slot.Itemstack);
                    slot.MarkDirty();
                }
            }
        }
    }
}
