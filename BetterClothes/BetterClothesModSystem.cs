using BetterClothes.Blocks;
using BetterClothes.Config;
using HarmonyLib;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace BetterClothes
{
    public class BetterClothesModSystem : ModSystem
    {
        private ICoreServerAPI? sapi;
        private Harmony harmony = null!;

        public override void StartPre(ICoreAPI api)
        {
            base.StartPre(api);
            BCOrchestrator.Orchestrate(api, Mod.Info.ModID);
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            if (!Harmony.HasAnyPatches(Mod.Info.ModID))
            {
                harmony = new Harmony(Mod.Info.ModID);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                api.Logger.Notification("Enchanting Mod: Harmony patches applied");
            }
            api.RegisterBlockEntityClass("BCBlockEntity", typeof(BCBlockEntity));
            api.RegisterBlockClass("BCBlock", typeof(BCBlock));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;
            api.Event.PlayerJoin += Event_PlayerJoin;
            api.ChatCommands
                .Create("buff")
                .WithDescription("Manage buffs on equipment (Creative mode only)")
                .RequiresPrivilege(Privilege.controlserver)
                .BeginSubCommand("add")
                    .WithDescription("Add buff to the item in first hotbar slot")
                    .HandleWith(BuffAddCommand)
                .EndSubCommand()
                .BeginSubCommand("remove")
                    .WithDescription("Remove buff from item in first hotbar slot")
                    .HandleWith(BuffRemoveCommand)
                .EndSubCommand()
                .BeginSubCommand("reload")
                    .WithDescription("Reload all buffs in character inventory")
                    .HandleWith(BuffReloadCommand)
                .EndSubCommand();
        }

        private void Event_PlayerJoin(IServerPlayer byPlayer)
        {
            IInventory inv = byPlayer.InventoryManager.GetOwnInventory(GlobalConstants.characterInvClassName);
            BCMethods.UpdatePlayerStats(byPlayer.Entity, inv);
            inv.SlotModified += (slot) => OnSlotModified(byPlayer.Entity, inv);
        }

        private void OnSlotModified(EntityPlayer player, IInventory inventory) {

            BCMethods.UpdatePlayerStats(player, inventory);
        }

        private TextCommandResult BuffAddCommand(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;

            if (player.WorldData.CurrentGameMode != EnumGameMode.Creative)
            {
                return TextCommandResult.Error("This command requires Creative mode!");
            }

            IInventory hotbarInv = player.InventoryManager.GetOwnInventory(GlobalConstants.hotBarInvClassName);
            ItemSlot targetSlot = hotbarInv[0];

            if (targetSlot == null || targetSlot.Empty)
            {
                return TextCommandResult.Error("First hotbar slot is empty or cannot be buffed.");
            }

            var (buff, val) = BCMethods.Rebuff(targetSlot.Itemstack);
            targetSlot.MarkDirty();

            if (buff != null && val != 0)
            {
                PostBuff(player);
                return TextCommandResult.Success($"Added new {buff.Name} buff to item {targetSlot.Itemstack.GetName()}");
            }

            return TextCommandResult.Error("Failed to add buff to item.");
        }

        private TextCommandResult BuffRemoveCommand(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;

            if (player.WorldData.CurrentGameMode != EnumGameMode.Creative)
            {
                return TextCommandResult.Error("This command requires Creative mode!");
            }

            IInventory hotbarInv = player.InventoryManager.GetOwnInventory(GlobalConstants.hotBarInvClassName);
            ItemSlot targetSlot = hotbarInv[0];

            if (targetSlot == null || targetSlot.Empty)
            {
                return TextCommandResult.Error("Cannot remove buff from first slot - slot is empty.");
            }

            if (BCMethods.RemoveBuff(targetSlot.Itemstack))
            {
                targetSlot.MarkDirty();
                PostBuff(player);
                return TextCommandResult.Success("Buff has been removed");
            }

            return TextCommandResult.Error("Failed to remove buff.");
        }

        private TextCommandResult BuffReloadCommand(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;

            if (player.WorldData.CurrentGameMode != EnumGameMode.Creative)
            {
                return TextCommandResult.Error("This command requires Creative mode!");
            }

            IInventory charInv = player.InventoryManager.GetOwnInventory(GlobalConstants.characterInvClassName);
            int buffedCount = 0;

            foreach (var slot in charInv)
            {
                if (slot != null && !slot.Empty)
                {
                    var (buff, val) = BCMethods.Rebuff(slot.Itemstack);
                    if (buff != null && val != 0)
                    {
                        slot.MarkDirty();
                        buffedCount++;
                    }
                }
            }

            PostBuff(player);

            if (buffedCount > 0)
            {
                return TextCommandResult.Success($"Reloaded buffs on {buffedCount} item(s) in inventory");
            }

            return TextCommandResult.Error("No items were buffed - check if items support buffs.");
        }

        private void PostBuff(IServerPlayer player)
        {
            player.Entity.WatchedAttributes.MarkPathDirty("stats");
            player.Entity.WatchedAttributes.MarkAllDirty();
        }

        public override void Dispose()
        {
            harmony?.UnpatchAll(Mod.Info.ModID);
            base.Dispose();
        }
    }
}