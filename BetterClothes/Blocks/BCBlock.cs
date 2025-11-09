using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace BetterClothes.Blocks;

public class BCBlock : Block
{
    public static SimpleParticleProperties myParticles = new SimpleParticleProperties(1, 1, ColorUtil.ColorFromRgba(220, 220, 220, 255), new Vec3d(), new Vec3d(), new Vec3f(), new Vec3f());
    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        if (world.Side == EnumAppSide.Client)
        {
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BCBlockEntity be)
            {
                be.OnPlayerRightClick(byPlayer, blockSel);
            }
        }
        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }

    private Item? item;
    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        item = api.World.GetItem(new AssetLocation(BCData.Config.BuffCostCode));
        if (item != null)
        {
            dsc.AppendLine("<font color=\"#8888ff\"><i>");
            dsc.Append(BCMethods.Translate("block-bufftable-north-desc", item.GetHeldItemName(new ItemStack(item)).ToLower()));
            dsc.Append("</i></font>");
        }
    }

}
