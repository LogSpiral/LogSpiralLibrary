using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace LogSpiralLibrary.CodeLibrary;

[System.ComponentModel.TypeConverter(typeof(ToFromStringConverter<ModDefinition>))]
public class ModDefinition : EntityDefinition
{
    public override int Type
    {
        get
        {
            var list = ModLoader.Mods.ToList();
            for (int n = 0; n < list.Count; n++)
            {
                if (Name == list[n].Name)
                    return n;
            }
            return -1;
        }
    }
    public override bool IsUnloaded => Type < 0;
    public static readonly Func<TagCompound, ModDefinition> DESERIALIZER = Load;
    public ModDefinition() : base() { }
    public ModDefinition(int type) : base(ModLoader.Mods[type].Name) { }
    public ModDefinition(string mod) : base(mod, mod) { }
    public static ModDefinition FromString(string s) => new(s);
    public static ModDefinition Load(TagCompound tag) => new(tag.GetString("mod"));
    public override string DisplayName => IsUnloaded ? Language.GetTextValue("LegacyInterface.23") : ModLoader.GetMod(Name).DisplayName;//SequenceSystem.conditions[Name].Description.ToString();
}

public class ModDefinitionElement : DefinitionElement<ModDefinition>
{
    public bool resetted;
    public override void Update(GameTime gameTime)
    {
        if (!resetted)
        {
            resetted = true;
            TextDisplayFunction = () => FontAssets.MouseText.Value.CreateWrappedText(Label + ": " + OptionChoice.Tooltip, GetDimensions().Width - 64 * OptionScale);
            if (List != null)
            {
                TextDisplayFunction = () => FontAssets.MouseText.Value.CreateWrappedText(Index + ": " + OptionChoice.Tooltip, GetDimensions().Width - 64 * OptionScale);
            }
            string str = TextDisplayFunction.Invoke();
            Height = MinHeight = new StyleDimension(Math.Max(184 * OptionScale, FontAssets.MouseText.Value.MeasureString(str).Y), 0);
            if (Parent?.Parent?.Parent is UIList list)
            {
                Parent.MinHeight = MinHeight;
                Parent.Height = Height;
                list.Recalculate();
            }
            else
                Recalculate();
        }

        base.Update(gameTime);
    }

    public override DefinitionOptionElement<ModDefinition> CreateDefinitionOptionElement() => new ModDefinitionOptionElement(Value, 0.5f);

    public override List<DefinitionOptionElement<ModDefinition>> CreateDefinitionOptionElementList()
    {
        var options = new List<DefinitionOptionElement<ModDefinition>>();

        for (int i = 0; i < ModLoader.Mods.Length; i++)
        {
            var optionElement = new ModDefinitionOptionElement(new ModDefinition(i), OptionScale);
            optionElement.OnLeftClick += (a, b) =>
            {
                Value = optionElement.Definition;
                UpdateNeeded = true;
                SelectionExpanded = false;
                // InternalOnSetObject();
                // SequenceSystem.SetSequenceUIPending();
                ConfigManager.SetPendingChanges();
            };
            options.Add(optionElement);
        }

        return options;
    }

    public override void TweakDefinitionOptionElement(DefinitionOptionElement<ModDefinition> optionElement)
    {
        optionElement.Left.Set(-184 * OptionScale, 1f);
    }
    public static LocalMod[] locals;
    public static LocalMod ModToLocal(Mod mod)
    {
        string fileName = $"{mod.Name}.tmod";
        List<LocalMod> targetLocals = [];
        foreach (var localsItem in locals)
        {
            if (localsItem.Name == mod.Name)
                targetLocals.Add(localsItem);
        }
        LocalMod localMod = targetLocals[^1];
        //Main.NewText((targetLocals.Count,localMod.DisplayName,localMod.lastModified));
        //bool success =
        //    ModOrganizer.TryReadLocalMod(ModLocation.Modpack, ModOrganizer.ModPackActive+fileName, out localMod) ||
        //    ModOrganizer.TryReadLocalMod(ModLocation.Local, ModOrganizer.modPath+fileName, out localMod) ||
        //    ModOrganizer.TryReadLocalMod(ModLocation.Workshop, fileName, out localMod);
        return localMod;
    }
    public override List<DefinitionOptionElement<ModDefinition>> GetPassedOptionElements()
    {
        var passed = new List<DefinitionOptionElement<ModDefinition>>();

        foreach (var option in Options)
        {
            string modname = "Terraria";

            if (modname.IndexOf(ChooserFilterMod.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
                continue;


            var mod = ModLoader.Mods[option.Type];
            if (mod.Name == "ModLoader")
                continue;
            if (mod.Name == "LogSpiralLibrary")
            {
                passed.Add(option);
                continue;
            }

            var localMod = ModToLocal(mod);
            if (localMod != null)
            {
                var refMods = from refMod in localMod.properties.modReferences where refMod.mod == nameof(LogSpiralLibrary) select refMod;
                if (refMods.Count() != 0)
                    passed.Add(option);
            }
            else
            {
                Main.NewText("获取LocalMod失败!!");
            }
        }
        return passed;
    }

}

public class ModDefinitionOptionElement : DefinitionOptionElement<ModDefinition>
{
    public Mod Mod { get; set; }
    public Texture2D modIcon;
    public ModDefinitionOptionElement(ModDefinition definition, float scale = .75f) : base(definition, scale)
    {
        NullID = -1;
        SetItem(definition);
        BackgroundTexture = TextureAssets.InventoryBack10;
        Width = Height = new StyleDimension(180 * scale, 0f);
    }

    public override void SetItem(ModDefinition definition)
    {
        base.SetItem(definition);
        Tooltip = definition.DisplayName;

        try
        {
            Mod = ModLoader.Mods[Type];
            var file = Mod.File;
            modIcon = Main.Assets.Request<Texture2D>("Images/UI/DefaultResourcePackIcon", AssetRequestMode.ImmediateLoad).Value;

            if (file != null && file.HasFile("icon.png"))
            {
                try
                {
                    using (file.Open())
                    using (var s = file.GetStream("icon.png"))
                    {
                        var iconTexture = Main.Assets.CreateUntracked<Texture2D>(s, ".png");

                        if (iconTexture.Width() == 80 && iconTexture.Height() == 80)
                        {
                            modIcon = iconTexture.Value;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logging.tML.Error("Unknown error", e);
                }
            }
        }
        catch { }
    }
    public override void SetScale(float scale)
    {
        Width = Height = new StyleDimension(180 * scale, 0f);

        base.SetScale(scale);
    }
    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (Mod != null)
        {
            CalculatedStyle dimensions = base.GetInnerDimensions();
            spriteBatch.Draw(BackgroundTexture.Value, dimensions.Center(), null, Color.White, 0f, new Vector2(26), Scale / 52f * 180f, SpriteEffects.None, 0f);

            spriteBatch.Draw(modIcon, dimensions.Center(), null, Color.White with { A = (byte)(IsMouseHovering ? 0 : 255) }, 0f, new Vector2(modIcon.Width / 2), Scale * 160f / modIcon.Width, 0, 0);
            if (IsMouseHovering)
                Main.instance.MouseText(Tooltip);
        }
    }
}


[System.ComponentModel.TypeConverter(typeof(ToFromStringConverter<ConditionDefinition>))]
public class ConditionDefinition : EntityDefinition
{
    public override int Type
    {
        get
        {
            var list = SequenceSystem.Conditions.ToList();
            for (int n = 0; n < list.Count; n++)
            {
                if (Name == list[n].Key)
                    return n;
            }
            return -1;
        }
    }
    public override bool IsUnloaded => Type < 0;
    public ConditionDefinition() : base() { }
    public ConditionDefinition(int type) : base(SequenceSystem.Conditions.ToList()[type].Key) { }
    public ConditionDefinition(string key) : base(key) { }
    public ConditionDefinition(string mod, string name) : base(mod, name) { }
    public static ConditionDefinition FromString(string s) => new(s);
    public static ConditionDefinition Load(TagCompound tag) => new(tag.GetString("mod"), tag.GetString("name"));
    public static readonly Func<TagCompound, ConditionDefinition> DESERIALIZER = Load;

    public override string DisplayName => IsUnloaded ? Language.GetTextValue("LegacyInterface.23") : (Name == "None" ? "None" : SequenceSystem.Conditions[Name].Description.ToString());
}

/*
public class ConditionDefinitionElement : DefinitionElement<ConditionDefinition>
{
    public bool resetted;
    public override void Update(GameTime gameTime)
    {
        if (!resetted)
        {
            resetted = true;
            TextDisplayFunction = () => FontAssets.MouseText.Value.CreateWrappedText(Label + ": " + OptionChoice.Tooltip, GetDimensions().Width);
            if (List != null)
            {
                TextDisplayFunction = () => FontAssets.MouseText.Value.CreateWrappedText(Index + ": " + OptionChoice.Tooltip, GetDimensions().Width);
            }
            var str = TextDisplayFunction.Invoke();

            Height = MinHeight = new StyleDimension(Math.Max(Height.Pixels, FontAssets.MouseText.Value.MeasureString(str).Y + 80 * OptionScale), 0);
            if (Parent?.Parent?.Parent is UIList list)
            {
                Parent.MinHeight = MinHeight;
                Parent.Height = Height;
                list.Recalculate();
            }
            else
                Recalculate();
        }

        base.Update(gameTime);
    }
    public override DefinitionOptionElement<ConditionDefinition> CreateDefinitionOptionElement() => new ConditionDefinitionOptionElement(Value, .8f);

    public override void TweakDefinitionOptionElement(DefinitionOptionElement<ConditionDefinition> optionElement)
    {
        optionElement.Top.Set(-40f, 1f);
        //optionElement.Left.Set(-124, 1f);
        optionElement.HAlign = 0.5f;
        optionElement.Left.Set(0f, 0f);

    }

    public override List<DefinitionOptionElement<ConditionDefinition>> CreateDefinitionOptionElementList()
    {
        OptionScale = 0.8f;

        var options = new List<DefinitionOptionElement<ConditionDefinition>>();

        for (int i = 0; i < SequenceSystem.Conditions.Count; i++)
        {
            ConditionDefinitionOptionElement optionElement;

            //if (i == 0)
            //    optionElement = new ConditionDefinitionOptionElement(new ConditionDefinition("Terraria", "None"), OptionScale);
            //else
            optionElement = new ConditionDefinitionOptionElement(new ConditionDefinition(i), OptionScale);

            optionElement.OnLeftClick += (a, b) =>
            {
                Value = optionElement.Definition;
                UpdateNeeded = true;
                SelectionExpanded = false;
                InternalOnSetObject();

            };

            options.Add(optionElement);
        }

        return options;
    }

    public override List<DefinitionOptionElement<ConditionDefinition>> GetPassedOptionElements()
    {
        var passed = new List<DefinitionOptionElement<ConditionDefinition>>();

        foreach (var option in Options)
        {
            if (option.Definition.DisplayName.IndexOf(ChooserFilter.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
                continue;

            string modname = option.Definition.Mod;

            if (modname.IndexOf(ChooserFilterMod.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
                continue;

            passed.Add(option);
        }

        return passed;
    }
}
*/
public class ConditionDefinitionOptionElement : DefinitionOptionElement<ConditionDefinition>
{
    private readonly UIAutoScaleTextTextPanel<string> text;

    public ConditionDefinitionOptionElement(ConditionDefinition definition, float scale = .75f) : base(definition, scale)
    {
        NullID = -1;
        SetItem(definition);
        Scale = scale;
        Width.Set(280 * scale, 0f);
        Height.Set(40 * scale, 0f);
        text = new UIAutoScaleTextTextPanel<string>(Definition.DisplayName)
        {
            Width = { Percent = 1f },
            Height = { Percent = 1f },
        };

        Append(text);
    }

    public override void SetItem(ConditionDefinition item)
    {
        base.SetItem(item);

        text?.SetText(item.DisplayName);
        Tooltip = item.DisplayName;
    }

    public override void SetScale(float scale)
    {
        base.SetScale(scale);

        Width.Set(280 * scale, 0f);
        Height.Set(40 * scale, 0f);
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (IsMouseHovering)
            UIModConfig.Tooltip = Tooltip;
    }
}

//[System.ComponentModel.TypeConverter(typeof(ToFromStringConverter<SequenceDefinition<T>>))]
public class SequenceDefinition<T> : EntityDefinition where T : ISequenceElement
{
    public override int Type
    {
        get
        {
            // var list = from pair in SequenceManager<T>.Sequences select pair.Value;
            int n = 0;
            foreach (var pair in SequenceManager<T>.Sequences.Keys)
            {
                if ($"{Mod}/{Name}" == pair)
                    return n;
                n++;
            }
            return -1;
        }
    }
    public override bool IsUnloaded => Type < 0;
    public SequenceDefinition() : base() { }
    public SequenceDefinition(int type) : base(SequenceManager<T>.Sequences.ToList()[type].Key) { }
    public SequenceDefinition(string key) : base(key) { }
    public SequenceDefinition(string mod, string name) : base(mod, name) { }
    public static ConditionDefinition FromString(string s) => new(s);
    public static ConditionDefinition Load(TagCompound tag) => new(tag.GetString("mod"), tag.GetString("name"));
    public static readonly Func<TagCompound, ConditionDefinition> DESERIALIZER = Load;
    public override string DisplayName => IsUnloaded ? Language.GetTextValue("LegacyInterface.23") : (Name == "None" ? "None" : SequenceManager<T>.Sequences[$"{Mod}/{Name}"].Data.DisplayName);
}

/*
public class SequenceDefinitionElement<T> : DefinitionElement<SequenceDefinition<T>> where T : ISequenceElement
{
    public bool resetted;
    public override void Update(GameTime gameTime)
    {
        if (!resetted)
        {
            resetted = true;
            TextDisplayFunction = () => FontAssets.MouseText.Value.CreateWrappedText(Label + ": " + OptionChoice.Tooltip, GetDimensions().Width - 130 * OptionScale);
            if (List != null)
            {
                TextDisplayFunction = () => FontAssets.MouseText.Value.CreateWrappedText(Index + ": " + OptionChoice.Tooltip, GetDimensions().Width - 130 * OptionScale);
            }
            var str = TextDisplayFunction.Invoke();

            Height = MinHeight = new StyleDimension(Math.Max(Height.Pixels, FontAssets.MouseText.Value.MeasureString(str).Y), 0);
            if (Parent?.Parent?.Parent is UIList list)
            {
                Parent.MinHeight = MinHeight;
                Parent.Height = Height;
                list.Recalculate();
            }
            else
                Recalculate();
        }

        base.Update(gameTime);
    }
    public override DefinitionOptionElement<SequenceDefinition<T>> CreateDefinitionOptionElement() => new SequenceDefinitionOptionElement<T>(Value, .8f);

    public override void TweakDefinitionOptionElement(DefinitionOptionElement<SequenceDefinition<T>> optionElement)
    {
        optionElement.Top.Set(0f, 0f);
        optionElement.Left.Set(-124, 1f);
    }

    public override List<DefinitionOptionElement<SequenceDefinition<T>>> CreateDefinitionOptionElementList()
    {
        OptionScale = 0.8f;

        var options = new List<DefinitionOptionElement<SequenceDefinition<T>>>();

        for (int i = 0; i < SequenceManager<T>.Sequences.Count; i++)
        {
            SequenceDefinitionOptionElement<T> optionElement;

            optionElement = new SequenceDefinitionOptionElement<T>(new SequenceDefinition<T>(i), OptionScale);

            optionElement.OnLeftClick += (a, b) =>
            {
                Value = optionElement.Definition;
                UpdateNeeded = true;
                SelectionExpanded = false;
                Interface.modConfig.SetPendingChanges();
            };

            options.Add(optionElement);
        }

        return options;
    }

    public override List<DefinitionOptionElement<SequenceDefinition<T>>> GetPassedOptionElements()
    {
        var passed = new List<DefinitionOptionElement<SequenceDefinition<T>>>();

        foreach (var option in Options)
        {
            if (option.Definition.DisplayName.IndexOf(ChooserFilter.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
                continue;

            string modname = option.Definition.Mod;

            if (modname.IndexOf(ChooserFilterMod.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
                continue;

            passed.Add(option);
        }

        return passed;
    }
}
*/
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

[System.ComponentModel.TypeConverter(typeof(ToFromStringConverter<SequenceDelegateDefinition>))]
public class SequenceDelegateDefinition : EntityDefinition
{
    public override int Type
    {
        get
        {
            var list = SequenceSystem.elementDelegates.ToList();
            for (int n = 0; n < list.Count; n++)
            {
                if (Key == list[n].Key)
                    return n;
            }
            return -1;
        }
    }
    public override bool IsUnloaded => Type < 0;
    public string Key => $"{Mod}/{Name}";
    public SequenceDelegateDefinition() : base(SequenceSystem.NoneDelegateKey) { }
    public SequenceDelegateDefinition(int type) : base(SequenceSystem.elementDelegates.ToList()[type].Key) { }
    public SequenceDelegateDefinition(string key) : base(key) { }
    public SequenceDelegateDefinition(string mod, string name) : base(mod, name) { }
    public static SequenceDelegateDefinition FromString(string s) => new(s);
    public static SequenceDelegateDefinition Load(TagCompound tag) => new(tag.GetString("mod"), tag.GetString("name"));
    public static readonly Func<TagCompound, SequenceDelegateDefinition> DESERIALIZER = Load;

    public override string DisplayName => IsUnloaded ? Language.GetTextValue("LegacyInterface.23") : (Name == "None" ? "None" : Name);
}
/*
public class SeqDelegateDefinitionElement : DefinitionElement<SequenceDelegateDefinition>
{
    public bool resetted;
    public override void Update(GameTime gameTime)
    {
        if (!resetted)
        {
            resetted = true;
            TextDisplayFunction = () => FontAssets.MouseText.Value.CreateWrappedText(Label + ": " + OptionChoice.Tooltip, GetDimensions().Width - 130 * OptionScale);
            if (List != null)
            {
                TextDisplayFunction = () => FontAssets.MouseText.Value.CreateWrappedText(Index + ": " + OptionChoice.Tooltip, GetDimensions().Width - 130 * OptionScale);
            }
            var str = TextDisplayFunction.Invoke();

            Height = MinHeight = new StyleDimension(Math.Max(Height.Pixels, FontAssets.MouseText.Value.MeasureString(str).Y), 0);
            if (Parent?.Parent?.Parent is UIList list)
            {
                Parent.MinHeight = MinHeight;
                Parent.Height = Height;
                list.Recalculate();
            }
            else
                Recalculate();
        }

        base.Update(gameTime);
    }
    public override DefinitionOptionElement<SequenceDelegateDefinition> CreateDefinitionOptionElement() => new GenericDelegateDefinitionOptionElement(Value, .8f);

    public override void TweakDefinitionOptionElement(DefinitionOptionElement<SequenceDelegateDefinition> optionElement)
    {
        optionElement.Top.Set(0f, 0f);
        optionElement.Left.Set(-124, 1f);
    }

    public override List<DefinitionOptionElement<SequenceDelegateDefinition>> CreateDefinitionOptionElementList()
    {
        OptionScale = 0.8f;

        var options = new List<DefinitionOptionElement<SequenceDelegateDefinition>>();

        for (int i = 0; i < SequenceSystem.elementDelegates.Count; i++)
        {
            GenericDelegateDefinitionOptionElement optionElement;

            //if (i == 0)
            //    optionElement = new SeqDelegateDefinitionOptionElement(new SeqDelegateDefinition("Terraria", "None"), OptionScale);
            //else
            optionElement = new GenericDelegateDefinitionOptionElement(new SequenceDelegateDefinition(i), OptionScale);

            optionElement.OnLeftClick += (a, b) =>
            {
                Value = optionElement.Definition;
                UpdateNeeded = true;
                SelectionExpanded = false;
                InternalOnSetObject();
                //SequenceSystem.SetSequenceUIPending();

            };

            options.Add(optionElement);
        }

        return options;
    }

    public override List<DefinitionOptionElement<SequenceDelegateDefinition>> GetPassedOptionElements()
    {
        var passed = new List<DefinitionOptionElement<SequenceDelegateDefinition>>();

        foreach (var option in Options)
        {
            if (option.Definition.DisplayName.IndexOf(ChooserFilter.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
                continue;

            string modname = option.Definition.Mod;

            if (modname.IndexOf(ChooserFilterMod.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
                continue;

            passed.Add(option);
        }

        return passed;
    }
}
*/
public class GenericDelegateDefinitionOptionElement : DefinitionOptionElement<SequenceDelegateDefinition>
{
    private readonly UIAutoScaleTextTextPanel<string> text;

    public GenericDelegateDefinitionOptionElement(SequenceDelegateDefinition definition, float scale = .75f) : base(definition, scale)
    {
        Width.Set(150 * scale, 0f);
        Height.Set(40 * scale, 0f);

        text = new UIAutoScaleTextTextPanel<string>(Definition.DisplayName)
        {
            Width = { Percent = 1f },
            Height = { Percent = 1f },
        };
        Append(text);
    }

    public override void SetItem(SequenceDelegateDefinition item)
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
public class DyeDefinitionElement : ItemDefinitionElement
{
    public override List<DefinitionOptionElement<ItemDefinition>> GetPassedOptionElements()
        => [.. (from elem in base.GetPassedOptionElements() where elem.Definition.Type == 0 || elem.Definition.Type == ModContent.ItemType<UnloadedItem>() || GameShaders.Armor._shaderLookupDictionary.ContainsKey(elem.Definition.Type) select elem)];
}
