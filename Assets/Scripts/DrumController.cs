using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DrumController : MonoBehaviour
{
    public HitController kickHitController;
    public HitController snareHitController;
    public HitController tom1HitController;
    public HitController hihatHitController = null;
    public HitController crashHitController = null;
    public HitController rideHitController;

    public float currentAlpha = 0f;
    public AudioSource audioPlayer;

    private string midifilePath;
    private List<Tuple<Double, int, int>> noteList = new List<Tuple<Double, int, int>>();
    private float audioStartTime;

    public float tolerance = 0.01f;
    private void Start()
    {
        //Set Colors for every drum element
        kickHitController.SetColor(Color.red);
        snareHitController.SetColor(Color.blue);
        tom1HitController.SetColor(Color.magenta);
        hihatHitController.SetColor(Color.green);
        //crashHitController.SetColor(Color.yellow);
        rideHitController.SetColor(Color.cyan);
    }
    void FixedUpdate()
    {
        if (midifilePath != null) 
        {
            float currentTime = Time.time - audioStartTime;
            for (int i = 0; i < noteList.Count; i++)
            {
                var tuple = noteList[i];
                if (Mathf.Abs((float)tuple.Item1 - audioPlayer.time) < tolerance) 
                {
                    switch(tuple.Item2){
                        case 36:
                            kickHitController.SetAlpha(1f);
                            break;
                        case 38:
                            snareHitController.SetAlpha(1f);
                            break;
                        case 48:
                            tom1HitController.SetAlpha(1f);
                            break;
                        case 46:
                            hihatHitController.SetAlpha(1f);
                            break;

                        case 49 or 52 or 55 or 57:
                            crashHitController.SetAlpha(1f);
                            break;
                        
                        case 51:
                            rideHitController.SetAlpha(1f);
                            break;
                    }
                }
            }
        }

    }
    public void OpenMidiFile() 
    {
        noteList.Clear();
        if (midifilePath == null) 
        {
            return;
        }
        var midi = MidiFile.Read(midifilePath);
        var notes = midi.GetNotes();

        foreach (Note note in notes) 
        {
            long startTime = note.Time;
            var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, midi.GetTempoMap());
            double fixStartTime = (double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds + (double)metricTimeSpan.Milliseconds / 1000f;
            int noteName = note.NoteNumber;
            int noteVelocity = note.Velocity;
            Tuple<Double,int,int> tuple = new Tuple<Double, int, int> ( fixStartTime, noteName, noteVelocity );
            noteList.Add( tuple );
        }
    }

    public void StartInterpretation() 
    {
        audioStartTime = Time.time;
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
