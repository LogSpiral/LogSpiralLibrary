using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSpiralLibrary.CodeLibrary.Utilties.BaseClasses;

/// <summary>
/// 简化使用的<see cref = "ModTileEntity"/>
/// </summary>
/// <typeparam name="T">对应绑定物块的类型</typeparam>
public abstract class LModTileEntity<T> : ModTileEntity where T : ModTile
{
    /// <summary>
    /// 必须和tileObjectData那边一样
    /// </summary>
    public abstract Point16 Origin { get; }
    public override bool IsTileValidForEntity(int x, int y)
    {
        Tile tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == ModContent.TileType<T>();
    }
    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            // Sync the entire multitile's area.  Modify "width" and "height" to the size of your multitile in tiles
            int width = 2;
            int height = 2;
            NetMessage.SendTileSquare(Main.myPlayer, i, j, width, height);

            // Sync the placement of the tile entity with other clients
            // The "type" parameter refers to the tile type which placed the tile entity, so "Type" (the type of the tile entity) needs to be used here instead
            NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j, number3: Type);
        }

        // ModTileEntity.Place() handles checking if the entity can be placed, then places it for you
        // Set "tileOrigin" to the same value you set TileObjectData.newTile.Origin to in the ModTile
        Point16 tileOrigin = Origin;
        int placedEntity = Place(i - tileOrigin.X, j - tileOrigin.Y);
        return placedEntity;
    }
    public override void OnNetPlace()
    {
        if (Main.netMode == NetmodeID.Server)
        {
            NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
        }
    }
}
