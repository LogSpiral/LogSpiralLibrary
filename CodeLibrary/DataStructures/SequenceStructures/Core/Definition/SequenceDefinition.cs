using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using System.Linq;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;

//[System.ComponentModel.TypeConverter(typeof(ToFromStringConverter<SequenceDefinition<T>>))]
public class SequenceDefinition<T> : EntityDefinition where T : ISequenceElement
{
    public override int Type
    {
        get
        {
            // var list = from pair in SequenceManager<T>.Sequences select pair.Value;
            int n = 0;
            foreach (var pair in SequenceManager<T>.Instance.Sequences.Keys)
            {
                if ($"{Mod}/{Name}" == pair)
                    return n;
                n++;
            }
            return -1;
        }
    }

    public override bool IsUnloaded => Type < 0;

    public SequenceDefinition() : base()
    {
    }

    public SequenceDefinition(int type) : base(SequenceManager<T>.Instance.Sequences.ToList()[type].Key)
    {
    }

    public SequenceDefinition(string key) : base(key)
    {
    }

    public SequenceDefinition(string mod, string name) : base(mod, name)
    {
    }

    public static SequenceDefinition<T> FromString(string s) => new(s);

    public static SequenceDefinition<T> Load(TagCompound tag) => new(tag.GetString("mod"), tag.GetString("name"));

    public static readonly Func<TagCompound, SequenceDefinition<T>> DESERIALIZER = Load;
    public override string DisplayName => IsUnloaded ? Language.GetTextValue("LegacyInterface.23") : (Name == "None" ? "None" : SequenceManager<T>.Instance.Sequences[$"{Mod}/{Name}"].Data.DisplayName);
}

public class SequenceDefinitionOptionElement<T> : DefinitionOptionElement<SequenceDefinition<T>> where T : ISequenceElement
{
    private readonly UIAutoScaleTextTextPanel<string> text;

    public SequenceDefinitionOptionElement(SequenceDefinition<T> definition, float scale = .75f) : base(definition, scale)
    {
        NullID = -1;
        SetItem(definition);
        Scale = scale;
        Width.Set(150 * scale, 0f);
        Height.Set(40 * scale, 0f);
        if (definition == null)
        {
            Main.NewText("定义null了");
            return;
        }
        text = new UIAutoScaleTextTextPanel<string>(Definition.DisplayName)
        {
            Width = { Percent = 1f },
            Height = { Percent = 1f },
        };
        Append(text);
    }

    public override void SetItem(SequenceDefinition<T> item)
    {
        base.SetItem(item);

        text?.SetText(item.DisplayName);
    }

    public override void SetScale(float scale)
    {
        base.SetScale(scale);

        Width.Set(150 * scale, 0f);
        Height.Set(40 * scale, 0f);
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (IsMouseHovering)
            UIModConfig.Tooltip = Tooltip;
    }
}