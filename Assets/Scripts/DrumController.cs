using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class DrumController : MonoBehaviour
{
    [SerializeField] public HitController kickHitController;
    [SerializeField] public HitController snareHitController;
    [SerializeField] public HitController tom1HitController;
    [SerializeField] public HitController hihatHitController = null;
    [SerializeField] public HitController crashHitController = null;
    [SerializeField] public HitController rideHitController;
    [SerializeField] public Generador3 intentoPrimero;



    public float currentAlpha = 0f;
    public AudioSource audioPlayer;

    private string midifilePath;
    private List<Tuple<Double, int, float>> noteList = new List<Tuple<Double, int, float>>();
    private float audioStartTime;

    public float tolerance = 0.01f;

    [SerializeField] private VisualElement root;
    [SerializeField] private VisualElement box_kick;
    private IResolvedStyle box_kickGlobalPosition;
    private IResolvedStyle box_kickGlobalBounds;



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
                if (Mathf.Abs(((float)tuple.Item1) - audioPlayer.time) < tolerance) 
                {
                    switch(tuple.Item2){
                        case 36:
                            kickHitController.SetAlpha(tuple.Item3);
                            break;
                        case 38:
                            snareHitController.SetAlpha(tuple.Item3);
                            break;
                        case 48:
                            tom1HitController.SetAlpha(tuple.Item3);
                            break;
                        case 46:
                            hihatHitController.SetAlpha(tuple.Item3);
                            break;

                        case 49 or 52 or 55 or 57:
                            crashHitController.SetAlpha(tuple.Item3);
                            break;
                        
                        case 51:
                            rideHitController.SetAlpha(tuple.Item3);
                            break;
                    }
                }

                if (Mathf.Abs(((float)tuple.Item1) - audioPlayer.time) < tolerance && audioPlayer.isPlaying)
                {
                    switch (tuple.Item2)
                    {
                        case 36:
                            Color kick_color = Color.red;
                            kick_color.a = tuple.Item3;
                            intentoPrimero.Generador("kick", kick_color);

                            break;
                        case 38:

                            Color snare_color = Color.blue;
                            snare_color.a = tuple.Item3;
                            intentoPrimero.Generador("snare", snare_color);
                            break;
                        
                        case 48:
                            Color tom1_color = Color.magenta;
                            tom1_color.a = tuple.Item3;
                            intentoPrimero.Generador("tom1", tom1_color);
                            break;
                        case 46:
                            Color hihat_color = Color.green;
                            hihat_color.a = tuple.Item3;
                            intentoPrimero.Generador("hihat", hihat_color);
                            break;

                        case 49 or 52 or 55 or 57:
                            Color crash_color = Color.yellow;
                            crash_color.a = tuple.Item3;
                            intentoPrimero.Generador("crash", crash_color);
                            break;

                        case 51:
                            Color ride_color = Color.cyan;
                            ride_color.a = tuple.Item3;
                            intentoPrimero.Generador("ride", ride_color);
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
            var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, midi.GetTempoMap());
            double fixStartTime = (double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds + (double)metricTimeSpan.Milliseconds / 1000f;
            int noteName = note.NoteNumber;
            float noteVelocity = note.Velocity/127.0f;
            Tuple<Double,int,float> tuple = new Tuple<Double, int, float> ( fixStartTime, noteName, noteVelocity );
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
}
