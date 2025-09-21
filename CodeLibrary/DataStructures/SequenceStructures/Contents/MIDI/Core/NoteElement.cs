using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.CommonElement;
using Terraria.Audio;
using Terraria.ModLoader.Config;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.MIDI.Core;

public class NoteElement : CommonElement
{
    public override bool IsCompleted => _timer >= _timeMax;

    public override void Initialize()
    {
        _timer = 0;
        if (TimeValue == NoteTimeValue.Instanced)
            _timeMax = 1;
        else 
        {
            _timeMax = TimeValue switch
            {
                NoteTimeValue.Whole => 240,
                NoteTimeValue.Half => 120,
                NoteTimeValue.Quarter => 60,
                NoteTimeValue.Eighth => 30,
                NoteTimeValue.Sixteenth => 15,
                _ => 0
            };
            if (Dot) _timeMax = _timeMax * 3 / 2;
            _timeMax /= 3;
        }

        _pitch = Octave + (int)Pitch / 12f;
    }

    private int _timer;

    private int _timeMax;

    private float _pitch;

    [ElementCustomData]
    public NoteTimeValue TimeValue {  get;  set; } = NoteTimeValue.Eighth;

    [ElementCustomData]
    public NotePitch Pitch { get; set; }

    [ElementCustomData]
    [Range(-3, 3)]
    public int Octave { get; set; }

    [ElementCustomData]
    [Range(0, 2f)]
    public float Volume { get; set; } = 1;

    [ElementCustomData]
    public bool Dot { get; set; }

    protected virtual SoundStyle Instrument { get; }

    public override void Update()
    {
        if (_timer == 0)
        {
            var instrument = Instrument with
            {
                MaxInstances = -1,
                Pitch = _pitch,
                Volume = Volume
            };
            SoundEngine.PlaySound(instrument);
        }
        _timer++;
    }

    public override string Category => "";
    public override string LocalizationCategory => $"Sequence.{nameof(NoteElement)}";

    protected override sealed void RootRegister() => CommonRegister(this);
}