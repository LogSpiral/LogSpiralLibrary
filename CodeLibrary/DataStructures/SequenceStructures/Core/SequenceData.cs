using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Helpers;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using PropertyPanelLibrary.EntityDefinition;
using System.IO;
using System.Xml;
using Terraria.ModLoader;
using static Terraria.Localization.NetworkText;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;

public class SequenceData : ILoadable
{
    public string FileName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string Description { get; set; } = "";

    [CustomEntityDefinitionHandler<LSLRefedModDefinitionHandler>]
    public ModDefinition ModDefinition { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime ModifyTime { get; set; }
    public bool Finished { get; set; } = true;
    public string GetSequenceKeyName(string elementTypeName)
    {
        return $"{ModDefinition.Name}/{elementTypeName}/{FileName}";
    }
    public static bool ParseKeyName(string keyName, out string modName, out string elementTypeName, out string fileName)
    {
        var subs = keyName.Split('/');
        if (subs.Length < 3)
        {
            modName = elementTypeName = fileName = "";
            return false;
        }
        modName = subs[0];
        elementTypeName = subs[1];
        fileName = Path.Combine(subs[2..]).Replace("\\", "/");
        return true;
    }
    public override string ToString() => $"DisplayName{DisplayName},AuthorName{AuthorName},Description{Description},CreateTime{CreateTime},ModifyTime{ModifyTime},Finished{Finished}";

    public void ReadXml(XmlReader reader)
    {
        XmlElementReader elementReader = new(reader);
        DisplayName = elementReader[nameof(DisplayName)]?.Value ?? string.Empty;
        AuthorName = elementReader[nameof(AuthorName)]?.Value ?? string.Empty;
        Description = elementReader[nameof(Description)]?.Value ?? string.Empty;
        CreateTime = DateTime.TryParse(elementReader[nameof(CreateTime)]?.Value ?? string.Empty, out DateTime dateTime) ? dateTime : DateTime.MinValue;
        ModifyTime = DateTime.TryParse(elementReader[nameof(ModifyTime)]?.Value ?? string.Empty, out dateTime) ? dateTime : DateTime.MinValue;
        Finished = !bool.TryParse(elementReader[nameof(ModifyTime)]?.Value ?? string.Empty, out bool finished) || finished;
        Load(elementReader);
    }

    public virtual string GetFullName(Mod mod) => mod.Name == nameof(LogSpiralLibrary) ? GetType().Name : $"{mod.Name}/{GetType().Name}";

    public void WriteXml(XmlWriter writer)
    {
        var type = GetType();
        if (type != typeof(SequenceData))
            writer.WriteAttributeString("FullName", GetFullName((MiscMethods.GetInstanceViaType(GetType()) as SequenceData).Mod));

        writer.WriteElementString(nameof(DisplayName), DisplayName);
        writer.WriteElementString(nameof(AuthorName), AuthorName);
        writer.WriteElementString(nameof(Description), Description);
        writer.WriteElementString(nameof(CreateTime), CreateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        writer.WriteElementString(nameof(ModifyTime), ModifyTime.ToString("yyyy-MM-dd HH:mm:ss"));
        if (!Finished)
            writer.WriteElementString(nameof(Finished), Finished.ToString());
        Save(writer);
    }

    protected virtual void Load(XmlElementReader elementReader)
    {
    }

    protected virtual void Save(XmlWriter writer)
    {
    }
    private Mod Mod { get; set; }
    void ILoadable.Load(Mod mod)
    {
        Mod = mod;
        var type = GetType();
        //var key = mod.Name == nameof(LogSpiralLibrary) ? type.Name : $"{mod.Name}/{type.Name}";
        SequenceGlobalManager.DataTypeLookup.Add(GetFullName(mod), type);
    }

    void ILoadable.Unload()
    {
    }

    protected virtual void HandleClone(SequenceData target)
    {

    }
    public SequenceData Clone()
    {
        var result = MemberwiseClone() as SequenceData;
        result.ModDefinition = new(ModDefinition.Name);
        HandleClone(result);
        return result;
    }
}