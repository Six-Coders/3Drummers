using UnityEngine;
using MidiJack;
public class DrumMIDIController : MonoBehaviour
{
    void Start()
    {
        MidiMaster.noteOnDelegate += NoteOn;
    }
    void Update()
    {

    }
    void NoteOn(MidiChannel channel, int note, float velocity)
    {
        Debug.Log("Nota activada: Canal " + channel + ", Nota " + note + ", Velocidad " + velocity);
    }
}
