using ReLogic.Content;
using System.Collections.Generic;
namespace LogSpiralLibrary;

partial class LogSpiralLibraryMod
{
    public static List<Asset<Texture2D>> BaseTex { get; private set; }
    public static List<Asset<Texture2D>> AniTex { get; private set; }
    public static List<Asset<Texture2D>> BaseTex_Swoosh { get; private set; }
    public static List<Asset<Texture2D>> AniTex_Swoosh { get; private set; }
    public static List<Asset<Texture2D>> BaseTex_Stab { get; private set; }
    public static List<Asset<Texture2D>> AniTex_Stab { get; private set; }
    public static List<Asset<Texture2D>> HeatMap { get; private set; }
    public static List<Asset<Texture2D>> MagicZone { get; private set; }
    /// <summary>
    /// 杂图，以下是内容表(0-17)
    /// <br>0-2:也许是给物品附魔光泽用的贴图</br>
    /// <br>3:刀光的灰度图，为什么会在这里有一张？？</br>
    /// <br>4-5:箭头和磁场点叉</br>
    /// <br>6:符文条带</br>
    /// <br>7-10:闪电激光</br>
    /// <br>11:车万激光</br>
    /// <br>12:压扁的白色箭头？？</br>
    /// <br>13-17:有些来着原版的Extra，有些是我自己瞎画，给最终分形那些用</br>
    /// <br>18:高斯模糊用加权贴图</br>
    /// <br>19:光玉</br>
    /// <br>20:星空</br>
    /// <br>21:星空2</br>
    /// </summary>
    public static List<Asset<Texture2D>> Misc { get; private set; }
    public static List<Asset<Texture2D>> Fractal { get; private set; }
    public static List<Asset<Texture2D>> Mask { get; private set; }

    static void LoadAllTextures()
    {
        BaseTex = LoadTextures(nameof(BaseTex));
        AniTex = LoadTextures(nameof(AniTex));
        BaseTex_Swoosh = LoadTextures($"Swoosh/{nameof(BaseTex)}", nameof(BaseTex));
        AniTex_Swoosh = LoadTextures($"Swoosh/{nameof(AniTex)}", nameof(AniTex));
        BaseTex_Stab = LoadTextures($"Stab/{nameof(BaseTex)}", nameof(BaseTex));
        AniTex_Stab = LoadTextures($"Stab/{nameof(AniTex)}", nameof(AniTex));
        HeatMap = LoadTextures(nameof(HeatMap));
        MagicZone = LoadTextures(nameof(MagicZone));
        Misc = LoadTextures(nameof(Misc));
        Fractal = LoadTextures(nameof(Fractal));
        Mask = LoadTextures(nameof(Mask));
    }
    private static List<Asset<Texture2D>> LoadTextures(string folderName, string textureName)
    {
        string basePath = $"Images/{folderName}/{textureName}_";
        List<Asset<Texture2D>> assets = [];
        for (int i = 0; ; i++)
        {
            string path = $"{basePath}{i}";
            if (!Instance.RootContentSource.HasAsset(path))
            {
                break;
            }
            assets.Add(Instance.Assets.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad));
        }
        return assets;
    }
    private static List<Asset<Texture2D>> LoadTextures(string textureName) => LoadTextures(textureName, textureName);
}
