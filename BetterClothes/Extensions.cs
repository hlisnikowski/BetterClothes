using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace BetterClothes;

public static class Extensions
{
    public static bool IsEmpty(this string value)
        => string.IsNullOrEmpty(value);

    public static bool IsNotEmpty(this string value)
        => !IsEmpty(value);

    public static void Log(this ICoreAPI api, string message)
        => api.Logger.Notification(message);

    /// <summary>
    /// ItemWearable no longer supported so we need to check if item have CollectibleBehaviorWearable behavior in collectible list.
    /// </summary>
    /// <param name="itemSlot">current slot.</param>
    /// <returns>Has item behavior Wearable. Can we equip this item ?</returns>
    public static bool IsWearable(this ItemSlot itemSlot)
    {
        if (itemSlot == null) return false;
        if (itemSlot.Itemstack == null) return false;
        return itemSlot.Itemstack.Collectible?.HasBehavior<CollectibleBehaviorWearable>() == true;
    }
}
