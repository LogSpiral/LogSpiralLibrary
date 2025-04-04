using Newtonsoft.Json;
using System.Xml;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
public class SequenceBasicInfo
{
    [JsonIgnore]
    public string KeyName => $"{ModName}/{FileName}";
    public string AuthorName;
    public string Description;
    public string FileName;
    public string DisplayName;
    public ModDefinition ModDefinition = new("LogSpiralLibrary");
    [JsonIgnore]
    public string ModName => ModDefinition?.Name ?? "UnknownMod";
    public DateTime createDate;
    public DateTime lastModifyDate;

    //这个密码有存在的意义吗...?
    //之前原本是想仿照YSM那样，序列的制作者可以加密制作内容啥的
    //但是现在还是采取完全公开xml的形式了不是吗
    //才不是因为有笨蛋没写加密算法，而且感觉意义不大
    public string passWord;
    public bool Finished;

    //TODO 给序列分级
    //public enum SequenceMode
    //{
    //    balanced = 0,
    //    expansion = 1,
    //    developer = 2,
    //    lunatic = 3
    //}
    public void Save(XmlWriter writer)
    {
        writer.WriteAttributeString("AuthorName", AuthorName);
        writer.WriteAttributeString("Description", Description);
        writer.WriteAttributeString("FileName", FileName);
        writer.WriteAttributeString("DisplayName", DisplayName);
        writer.WriteAttributeString("ModName", ModName);
        writer.WriteAttributeString("createTime", createDate.Ticks.ToString());
        writer.WriteAttributeString("lastModifyDate", lastModifyDate.Ticks.ToString());
        writer.WriteAttributeString("passWord", passWord);
        writer.WriteAttributeString("Finished", Finished.ToString());
    }
    public SequenceBasicInfo Load(XmlReader reader)
    {
        AuthorName = reader["AuthorName"] ?? "不认识的孩子呢";
        Description = reader["Description"] ?? "绝赞的描述";
        FileName = reader["FileName"] ?? "Error";
        DisplayName = reader["DisplayName"] ?? "出错的序列";
        ModDefinition = new ModDefinition(reader["ModName"]);
        createDate = new DateTime(long.Parse(reader["createTime"] ?? "0"));
        lastModifyDate = new DateTime(long.Parse(reader["lastModifyDate"] ?? "0"));
        passWord = reader["passWord"] ?? "";
        Finished = bool.Parse(reader["Finished"] ?? "false");
        return this;
    }
    public SequenceBasicInfo Clone() => MemberwiseClone() as SequenceBasicInfo;
}
