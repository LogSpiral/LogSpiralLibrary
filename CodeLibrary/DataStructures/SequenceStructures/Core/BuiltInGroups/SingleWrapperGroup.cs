using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using System.Collections.Generic;
using System.Xml;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups;
public class SingleWrapperGroup(Wrapper wrapper) : IGroup
{
    #region Core
    public SingleWrapperGroup() : this(null!)
    {

    }
    public Wrapper GetWrapper() => _wrapper;
    #endregion


    #region IO
    bool IGroup.ReadSingleWrapper => true;
    private Wrapper _wrapper = wrapper;

    public void AppendWrapper(Wrapper wrapper, Dictionary<string, string> attributes)
    {
        _wrapper = wrapper;
    }

    public void WriteXml(XmlWriter writer)
    {
        _wrapper.WriteXml(writer, new Dictionary<string, string>());
    }
    #endregion
}
