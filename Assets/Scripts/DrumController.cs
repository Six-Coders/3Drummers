using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


public class DrumController : MonoBehaviour
{
    private string midifilePath;
    private List<Tuple<long, int, int>> noteList = new List<Tuple<long, int, int>>();

     

    public void OpenMidiFile() 
    {
        if (midifilePath == null) 
        {
            return;
        }
        var midi = MidiFile.Read(midifilePath);
        IEnumerable<Note> notes = midi.GetNotes();
        foreach (Note note in notes) 
        {
            long startTime = note.Time;
            int noteName = note.NoteNumber;
            int noteVelocity = note.Velocity;
            Tuple<long,int,int> tuple = new Tuple<long, int, int> ( startTime, noteName, noteVelocity );
            noteList.Add( tuple );
        }
        noteList.Sort(Comparer<Tuple<long, int, int>>.Default);
        PrintListNotes();
    }

    public void SetMidiPathFile(string path) 
    {
        if (File.Exists(path))
        {
            midifilePath = path;
            OpenMidiFile();
        }
        else 
        {
            midifilePath = null;
        }
    }
    private void PrintListNotes() 
    {
        foreach (var tuple in noteList) 
        {
            Debug.Log(tuple);   
        }
    }
}
