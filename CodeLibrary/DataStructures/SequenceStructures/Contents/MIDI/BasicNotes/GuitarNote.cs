using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.MIDI.Core;
using Terraria.Audio;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.MIDI.BasicNotes;

public class GuitarNote : NoteElement
{
    protected override SoundStyle Instrument { get; } = new SoundStyle($"{nameof(LogSpiralLibrary)}/Sounds/Guitar");
}