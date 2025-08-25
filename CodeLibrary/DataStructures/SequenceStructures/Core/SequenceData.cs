using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Helpers;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.CodeLibrary.Utilties;
using System.Xml;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;

public class SequenceData : ILoadable
{
    public string FileName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string Description { get; set; } = "";

    [CustomModConfigItem<ModDefinitionElement>]
    public ModDefinition ModDefinition { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime ModifyTime { get; set; }
    public bool Finished { get; set; } = true;

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

    public virtual string GetFullName => GetType().Name;

    public void WriteXml(XmlWriter writer)
    {
        var type = GetType();
        if (type != typeof(SequenceData))
            writer.WriteAttributeString("FullName", GetFullName);

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

    void ILoadable.Load(Mod mod)
    {
        var type = GetType();
        //var key = mod.Name == nameof(LogSpiralLibrary) ? type.Name : $"{mod.Name}/{type.Name}";
        SequenceGlobalManager.DataTypeLookup.Add(GetFullName, type);
    }

    void ILoadable.Unload()
    {
    }
}