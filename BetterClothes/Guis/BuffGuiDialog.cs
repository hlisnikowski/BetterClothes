using Cairo;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace BetterClothes.Guis;

internal class BuffGuiDialog : GuiDialogBlockEntity
{
    public BuffGuiDialog(string dialogTitle, InventoryBase inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, inventory, blockEntityPos, capi)
    {
        capi.Event.RegisterGameTickListener((dt) =>
        {
            if (!IsOpened()) return;

            double distance = capi.World.Player.Entity.Pos.DistanceTo(
                BlockEntityPosition.ToVec3d().Add(0.5, 0.5, 0.5)
            );

            if (distance > 4)
            {
                TryClose();
                return;
            }

            BlockEntity be = capi.World.BlockAccessor.GetBlockEntity(BlockEntityPosition);
            if (be == null)
            {
                TryClose();
            }
        }, 200);
    }

    public override void OnGuiOpened()
    {
        base.OnGuiOpened();
        SetupDialog();
    }

    public void SetupDialog()
    {
        int width = 250;
        ElementBounds dialogBounds = ElementBounds.Fixed(0, 0, width, width).WithAlignment(EnumDialogArea.CenterMiddle);
        ElementBounds bgBounds = ElementBounds.Fixed(0, 0, width, width - 40);
        ElementBounds slot1Bounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 100 - 60 , 75, 1, 1); // clothes
        ElementBounds slot2Bounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 155, 75, 1, 1); // rusty-gear
        var buffBound = ElementBounds.Fixed(20, 100, 32, 32);

        SingleComposer = capi.Gui
            .CreateCompo("buffDialog", dialogBounds)
            .AddShadedDialogBG(bgBounds)
            .AddDialogTitleBar(BCMethods.Translate("block-bufftable-north"), () => { TryClose(); })
            .BeginChildElements(bgBounds) 
               .AddButton(BCMethods.Translate("change-btn") , OnClicked, ElementBounds.Fixed(10, 160, 210, 20).WithFixedPadding(5), CairoFont.WhiteSmallishText().WithOrientation(EnumTextOrientation.Center))
               .AddStaticCustomDraw(buffBound, (ctx, s, b) => DrawBuffIcon(ctx, 60, 40 , 120, 120, [1,1,1,1])) // ICON
                .AddItemSlotGrid(Inventory, (p) => capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, p), 1, new int[] { 0 }, slot1Bounds, "slot1")
                .AddItemSlotGrid(Inventory, (p) => capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, p), 1, new int[] { 1 }, slot2Bounds, "slot2")
            .EndChildElements() 
            .Compose();
    }

    public bool OnClicked()
    {
        ItemSlot slot1 = Inventory[0];
        ItemSlot slot2 = Inventory[1];

        if (slot2.Empty) return true;
        if (slot1.Empty || slot1.Itemstack.Collectible is not ItemWearable) return true;
        if (slot2.Itemstack.Collectible.Code.Path != BCData.Config.BuffCostCode) return true;
        capi.Network.SendBlockEntityPacket(BlockEntityPosition, BCData.BUFF_PACKET_ID);
        return true;
    }

    public override void OnGuiClosed()
    {
        base.OnGuiClosed();
    }


    public void DrawBuffIcon(Context ctx, double x, double y, double width, double height,
      double[] fillColor = null, double[] strokeColor = null, double strokeWidth = 2.0)
    {
        ctx.Save();

        ctx.Translate(x, y);
        double scale = Math.Min(width / 512.0, height / 512.0);
        ctx.Scale(scale, scale);

        ctx.NewPath();
        ctx.MoveTo(247.79, 18.734);
        ctx.CurveTo(137.967, 17.596, 19.874, 96.94, 19.73, 244.53);
        ctx.LineTo(41.133, 193.135);
        ctx.CurveTo(31.648, 265.415, 33.383, 340.371, 79.923, 395.637);
        ctx.LineTo(38.2, 377.355);
        ctx.CurveTo(77.44, 447.129, 164.533, 468.331, 239.055, 469.865);
        ctx.CurveTo(124.11, 429.9, 67.87, 342.277, 63.912, 246.492);
        ctx.CurveTo(57.19, 34.712, 324.57, 28.798, 404.692, 170.722);
        ctx.CurveTo(401.275, 151.23, 396.069, 132.296, 389.074, 114.612);
        ctx.CurveTo(466.48, 203.767, 448.367, 329.487, 367.784, 367.648);
        ctx.CurveTo(343.534, 371.598, 318.854, 379.708, 306.83, 386.648);
        ctx.CurveTo(248.282, 420.45, 300.56, 513.184, 360.055, 478.836);
        ctx.CurveTo(369.495, 473.386, 383.459, 461.533, 396.549, 447.484);
        ctx.CurveTo(460.909, 387.964, 494.649, 329.244, 489.657, 258.544);
        ctx.CurveTo(483.137, 287.644, 470.482, 316.448, 454.034, 343.227);
        ctx.CurveTo(517.192, 196.405, 461.99, 79.337, 309.196, 41.873);
        ctx.CurveTo(321.293, 47.708, 332.699, 55.503, 343.069, 65.233);
        ctx.CurveTo(285.654, 41.481, 211.946, 42.613, 156.185, 68.738);
        ctx.CurveTo(184.251, 42.538, 220.961, 25.008, 258.385, 19.096);
        ctx.CurveTo(254.865, 18.891, 251.331, 18.771, 247.788, 18.734);
        ctx.ClosePath();

        ctx.MoveTo(228.05, 178.936);
        ctx.LineTo(208.207, 279.502);
        ctx.CurveTo(205.249, 283.312, 202.567, 286.354, 199.174, 289.442);
        ctx.LineTo(173.486, 240.346);
        ctx.LineTo(150.781, 252.276);
        ctx.LineTo(182.151, 313.221);
        ctx.CurveTo(186.631, 324.695, 192.171, 333.901, 197.313, 341.745);
        ctx.CurveTo(225.376, 384.548, 261.86, 376.997, 292.616, 351.3);
        ctx.LineTo(379.896, 302.848);
        ctx.LineTo(367.186, 280.35);
        ctx.LineTo(301.05, 317.29);
        ctx.CurveTo(299.533, 314.136, 297.784, 310.738, 295.994, 307.78);
        ctx.LineTo(363.812, 242.82);
        ctx.LineTo(346.272, 224.125);
        ctx.LineTo(279.802, 287.887);
        ctx.CurveTo(277.446, 285.569, 275.564, 283.36, 273.037, 281.347);
        ctx.LineTo(318.121, 203.262);
        ctx.LineTo(295.388, 190.135);
        ctx.LineTo(249.524, 268.432);
        ctx.CurveTo(245.734, 267.122, 241.804, 266.232, 237.929, 265.687);
        ctx.LineTo(253.585, 183.791);
        ctx.LineTo(228.052, 178.937);
        ctx.ClosePath();

        if (fillColor != null && fillColor.Length >= 3)
        {
            ctx.SetSourceRGBA(fillColor[0], fillColor[1], fillColor[2],
                fillColor.Length > 3 ? fillColor[3] : 1.0);
            if (strokeColor != null)
                ctx.FillPreserve();
            else
                ctx.Fill();
        }

        if (strokeColor != null && strokeColor.Length >= 3)
        {
            ctx.SetSourceRGBA(strokeColor[0], strokeColor[1], strokeColor[2],
                strokeColor.Length > 3 ? strokeColor[3] : 1.0);
            ctx.LineWidth = strokeWidth;
            ctx.Stroke();
        }

        ctx.Restore();
    }

}
