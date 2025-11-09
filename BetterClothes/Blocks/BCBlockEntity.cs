using BetterClothes.Guis;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace BetterClothes.Blocks;

public class BCBlockEntity : BlockEntityOpenableContainer
{
    private InventoryGeneric _inventory;
    private BuffGuiDialog buffTableGui;
    public static SimpleParticleProperties bigMetalSparks;

    public override InventoryBase Inventory => _inventory;

    public override string InventoryClassName
    {
        get
        {
            return Block?.FirstCodePart(0) ?? "bufftable";
        }
    }

    public BCBlockEntity()
    {
        if (_inventory == null)
        {
            _inventory = new InventoryGeneric(2, InventoryClassName + "-0", null);
        }

        bigMetalSparks = new SimpleParticleProperties(
    20, 25,
    ColorUtil.ToRgba(255, 255, 233, 83),
    new Vec3d(), new Vec3d(),
    new Vec3f(-1f, 1f, -1f),
    new Vec3f(2f, 4f, 2f),
    0.4f,
    1f,
    0.25f, 0.8f
);
        bigMetalSparks.VertexFlags = 128;
        bigMetalSparks.AddPos.Set(1 / 16f, 0, 1 / 16f);
        bigMetalSparks.SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -0.25f);
        bigMetalSparks.Bounciness = 1f;
        bigMetalSparks.addLifeLength = 2f;
        bigMetalSparks.GreenEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -233f);
        bigMetalSparks.BlueEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -83f);
    }

    public override void OnReceivedClientPacket(IPlayer player, int packetid, byte[] data)
    {
        if (packetid == BCData.BUFF_PACKET_ID)
        {
            ItemSlot slot1 = Inventory[0];
            ItemSlot slot2 = Inventory[1];
            var (buff, val) = BCMethods.Rebuff(slot1.Itemstack);
            if (buff != null && BCData.Config.MessageNotifyOnSuccess)
            {
               ((IServerPlayer)player).SendMessage(GlobalConstants.GeneralChatGroup, $"{BCMethods.Translate("message-buff-add-success")} {buff.GetName()} ", EnumChatType.CommandSuccess);
            }
            slot1.MarkDirty();
            slot2.TakeOut(1);
            slot2.MarkDirty();
            AssetLocation sound = new AssetLocation("sounds/tool/reinforce");
            Api.World.PlaySoundAt(sound, Pos.X, Pos.Y, Pos.Z, null);
            Api.Log("Packet received");
            SpawnParticles();
            MarkDirty(true);
            return;
        }

        MarkDirty(true);
        base.OnReceivedClientPacket(player, packetid, data);
    }


    private void SpawnParticles()
    {
        Vec3d center = new Vec3d(Pos.X + 0.5, Pos.Y + 0, Pos.Z + 0.5);
        Random rand = Api.World.Rand;
        bigMetalSparks.MinPos = center.AddCopy(0, 1f, 0);
        bigMetalSparks.AddPos.Set(1 / 16f, 0, 1 / 16f);
        bigMetalSparks.VertexFlags = (byte)GameMath.Clamp((int)(800 - 700) / 2, 32, 128);
        Api.World.SpawnParticles(bigMetalSparks);
    }

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        if (_inventory != null)
        {
            _inventory.Pos = Pos;
            _inventory.LateInitialize(InventoryClassName + "-" + Pos.X + "/" + Pos.Y + "/" + Pos.Z, api);
        }
    }

    public override bool OnPlayerRightClick(IPlayer byPlayer, BlockSelection blockSel)
    {
        if (Api.Side == EnumAppSide.Client)
        {
            toggleInventoryDialogClient(byPlayer, () =>
            {
                buffTableGui = new BuffGuiDialog("Buff Table", Inventory, Pos, Api as ICoreClientAPI);
                return buffTableGui;
            });
        }

        return true;
    }
}
