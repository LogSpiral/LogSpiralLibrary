using LogSpiralLibrary.CodeLibrary.Utilties.BaseClasses;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.Default;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingEffects;

public class DyeEffect(int dyeType = 0) : IRenderEffect
{
    #region 参数属性

    public int Type { get; set; } = dyeType;

    #endregion 参数属性

    #region 接口实现

    public bool Active => GameShaders.Armor._shaderLookupDictionary.ContainsKey(Type);

    public bool DoRealDraw => true;

    public void ProcessRender(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, ref RenderTarget2D contentRender, ref RenderTarget2D assistRender)
    {
        #region 准备状态

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

        #endregion 准备状态

        #region 切换绘制目标至备用画布

        graphicsDevice.SetRenderTarget(assistRender);
        graphicsDevice.Clear(Color.Transparent);

        #endregion 切换绘制目标至备用画布

        #region 设置参数

        var shaderData = GameShaders.Armor.GetShaderFromItemId(Type);
        if (shaderData != null)
        {
            // TODO 深入研究盔甲染料的shader参数
            shaderData.Apply(null);
            Vector4 value3 = new(0f, 0f, contentRender.Width, contentRender.Height);
            shaderData.Shader.Parameters["uSourceRect"]?.SetValue(value3);
            shaderData.Shader.Parameters["uLegacyArmorSourceRect"]?.SetValue(value3);
            shaderData.Shader.Parameters["uLegacyArmorSheetSize"]?.SetValue(new Vector2(contentRender.Width, contentRender.Height));
            shaderData.Apply();
        }

        #endregion 设置参数

        #region 绘制内容

        spriteBatch.Draw(contentRender, Vector2.Zero, Color.White);

        Utils.Swap(ref contentRender, ref assistRender);

        #endregion 绘制内容

        #region 恢复状态

        spriteBatch.End();

        #endregion 恢复状态
    }

    #endregion 接口实现
}

public class DyeConfigs : IAvailabilityChangableConfig
{
    public bool Available { get; set; } = false;

    [CustomModConfigItem(typeof(DyeDefinitionElement))]
    public ItemDefinition Dye
    {
        get;
        set;
    } = new();

    [JsonIgnore]
    public DyeEffect EffectInstance => !Available ? new DyeEffect() : new DyeEffect(Dye.Type);

    //  field ??=

    public void CopyToInstance(DyeEffect effect) => effect.Type = Available ? Dye.Type : 0;
}

file class DyeDefinitionElement : ItemDefinitionElement
{
    public override List<DefinitionOptionElement<ItemDefinition>> GetPassedOptionElements()
        => [.. (from elem in base.GetPassedOptionElements() where elem.Definition.Type == 0 || elem.Definition.Type == ModContent.ItemType<UnloadedItem>() || GameShaders.Armor._shaderLookupDictionary.ContainsKey(elem.Definition.Type) select elem)];
}