using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using PropertyPanelLibrary.PropertyPanelComponents.Interfaces;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.CommonElement;

public abstract partial class CommonElement : ModType, ISequenceElement, ILocalizedModType, IMemberLocalized
{
    public abstract bool IsCompleted { get; }

    public virtual ISequenceElement CloneInstance() => MemberwiseClone() as ISequenceElement;

    public abstract void Initialize();

    public abstract void Update();
}