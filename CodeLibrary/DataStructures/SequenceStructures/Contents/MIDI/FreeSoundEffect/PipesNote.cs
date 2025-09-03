using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.MIDI.Core;
using Terraria.Audio;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.MIDI.FreeSoundEffect;

internal class PipesNote:NoteElement
{
    protected override SoundStyle Instrument { get; } = new SoundStyle($"{nameof(LogSpiralLibrary)}/Sounds/Pipes");
}
