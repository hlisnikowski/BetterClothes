using Vintagestory.API.Common;

namespace BetterClothes;

public static class Extensions
{
    public static bool IsEmpty(this string value)
        => string.IsNullOrEmpty(value);

    public static bool IsNotEmpty(this string value)
        => !IsEmpty(value);

    public static void Log(this ICoreAPI api, string message)
        => api.Logger.Notification(message);
}
