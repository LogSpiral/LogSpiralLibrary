using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using PropertyPanelLibrary.EntityDefinition;
using PropertyPanelLibrary.PropertyPanelComponents.Core;
using ReLogic.Content;
using SilkyUIFramework.BasicComponents;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;

[TypeConverter(typeof(ToFromStringConverter<ModDefinition>))]
public class ModDefinition : EntityDefinition
{
    public override int Type
    {
        get
        {
            var mods = ModLoader.Mods;
            for (int n = 0; n < mods.Length; n++)
            {
                if (Name == mods[n].Name)
                    return n;
            }
            return -1;
        }
    }

    public override bool IsUnloaded => Type < 0;

    public ModDefinition(string mod) : base(mod, mod)
    {
    }

    public override string DisplayName =>
        IsUnloaded
        ? Language.GetTextValue("LegacyInterface.23")
        : ModLoader.GetMod(Name).DisplayName;
}

public class ModDefinitionOption : SUIDefinitionIconOption 
{
    public override void OnSetDefinition(EntityDefinition current, EntityDefinition previous)
    {
        base.OnSetDefinition(current, previous);
        try
        {
            var Mod = ModLoader.Mods[Type];
            var file = Mod.File;
            var modIcon = Main.Assets.Request<Texture2D>("Images/UI/DefaultResourcePackIcon", AssetRequestMode.ImmediateLoad);

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
                            modIcon = iconTexture;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logging.tML.Error("Unknown error", e);
                }
            }

            Icon.Texture2D = modIcon;
        }
        catch { }
    }
}

public class ModDefinitionHandler : EntityDefinitionCommonHandler
{
    public override UIView CreateChoiceView(PropertyOption.IMetaDataHandler metaData)
    {
        var proxyOptionChoice = OptionChoice = new ModDefinitionOption() { Definition = metaData.GetValue() as EntityDefinition };
        proxyOptionChoice.SetTop(2);
        proxyOptionChoice.SetLeft(-4, 0, 0);
        proxyOptionChoice.SetPadding(0);
        return proxyOptionChoice;
    }
    protected override void FillingOptionList(List<SUIEntityDefinitionOption> options)
    {
        foreach (var mod in ModLoader.Mods)
            options.Add(new ModDefinitionOption() { Definition = new ModDefinition(mod.Name) });
    }
}

public class LSLRefedModDefinitionHandler : ModDefinitionHandler
{
    protected override bool CheckPassOption(SUIEntityDefinitionOption view)
    {
        if (!base.CheckPassOption(view))
            return false;

        var mod = ModLoader.Mods[view.Definition.Type];
        if (mod.Name == "ModLoader")
            return false;
        if (mod.Name == "LogSpiralLibrary")
            return true;

        var localMod = SequenceSystem.ModToLocal(mod);
        if (localMod != null)
        {
            foreach (var refMod in localMod.properties.modReferences)
            {
                if (refMod.mod == nameof(LogSpiralLibrary))
                    return true;
            }
        }
        return false;
    }
}