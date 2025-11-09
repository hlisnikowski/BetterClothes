using BetterClothes.Config;
using System;
using Vintagestory.API.Common;

namespace BetterClothes;

public class BCOrchestrator
{
    public static ModConfig Config;
    public static ICoreAPI Api;

    public static void Orchestrate(ICoreAPI api, string modinfo)
    {
        Api = api;
        // if ModConfig().WithDefaults is edited, dont forget to delete betterclothes.json.
        Config = api.LoadModConfig<ModConfig>("betterclothes.json");

        // if not exists, it will create file in %appdata% modconfig folder.
        if (Config == null)
        {
            Config = new ModConfig().Seed();
            api.StoreModConfig(Config, "betterclothes.json");
        }

        BCData.ModID = modinfo;
        BCData.ModIDPrefix = modinfo + ":";
        BCData.Config = Config;
        BCData.Random = api.World.Rand;
        BCData.Buffs = BCMapper.Map(Config);
    }
}
