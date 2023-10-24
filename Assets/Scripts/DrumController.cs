using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DrumController : MonoBehaviour
{
    [SerializeField] public HitController kickHitController;
    [SerializeField] public HitController snareHitController;
    [SerializeField] public HitController tom1HitController;
    [SerializeField] public HitController hihatHitController;
    [SerializeField] public HitController crashHitController;
    [SerializeField] public HitController rideHitController;

    [SerializeField] public HitController kickHitController2;
    [SerializeField] public HitController snareHitController2;
    [SerializeField] public HitController tom1HitController2;
    [SerializeField] public HitController hihatHitController2;
    [SerializeField] public HitController crashHitController2;
    [SerializeField] public HitController rideHitController2;

    [SerializeField] public Generador3 intentoPrimero;

    private List<Vector3> drumElements = new List<Vector3> ();
    public UIMenuController menuController;

    [SerializeField] public List<Material> overlineMaterials = new List<Material>();

    private float outlineThick = 0.015f;
    public float currentAlpha = 0f;
    public AudioSource audioPlayer;

    private string midifilePath;
    private List<Tuple<float, int, float>> noteList = new List<Tuple<float, int, float>>();
    private float audioStartTime;

    public float tolerance = 3f;

    private Quaternion snareRotation;
    private Quaternion rideRotation;
    private void ActivateOutline() 
    {
        foreach (Material material in overlineMaterials) 
        {
            
            if (material.name.Equals("Outline Material Hihat"))
            {
                material.SetFloat("_Outline_Thickness", 0.0015f);
            }
            else if (material.name.Equals("Outline Material Crash"))
            {
                material.SetFloat("_Outline_Thickness", 0.075f);
            }
            else if (material.name.Equals("Outline Material Ride"))
            {
                material.SetFloat("_Outline_Thickness", 0.00075f);
            }
            else if (material.name.Equals("Outline Material Tom")) 
            {
                material.SetFloat("_Outline_Thickness", 0.00025f);
            }
            else
            {
                material.SetFloat("_Outline_Thickness", outlineThick);
            }
            
        }
    }

    private void DeactivateOutline() 
    {
        foreach (Material material in overlineMaterials) 
        {
            material.SetFloat("_Outline_Thickness", 0f);
        }
    }
    private void Start()
    {
        //Set Colors for every drum element
        kickHitController.SetColor(Color.red);
        snareHitController.SetColor(Color.blue);
        tom1HitController.SetColor(Color.magenta);
        hihatHitController.SetColor(Color.green);
        crashHitController.SetColor(Color.yellow);
        rideHitController.SetColor(Color.cyan);

        kickHitController2.SetColor(Color.red);
        snareHitController2.SetColor(Color.blue);
        tom1HitController2.SetColor(Color.magenta);
        hihatHitController2.SetColor(Color.green);
        crashHitController2.SetColor(Color.yellow);
        rideHitController2.SetColor(Color.cyan);

        snareRotation = snareHitController.transform.rotation;
        rideRotation = rideHitController.transform.rotation;
    }
    void FixedUpdate()
    {
        kickHitController.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        snareHitController.transform.rotation = snareRotation;
        rideHitController.transform.rotation = rideRotation;
        if (audioPlayer.isPlaying) 
        {
            DeactivateOutline();
        }
        if (midifilePath != null) 
        {
            for (int i = 0; i < noteList.Count; i++)
            {
                var tuple = noteList[i];
                if (Mathf.Abs(tuple.Item1 - audioPlayer.time - 2f) < tolerance+0.0125 && audioPlayer.isPlaying) 
                {
                    float alpha = 1;
                    if (menuController.isIntensitySet)
                    {
                        alpha = tuple.Item3;
                    }
                    switch (tuple.Item2)
                    {
                        case 36:
                            kickHitController.SetAlpha(alpha);
                            kickHitController2.SetAlpha(alpha);
                            kickHitController.transform.localScale = new Vector3(1.55f, 1.55f, 1.55f);
                            break;
                        case 38:
                            snareHitController.SetAlpha(alpha);
                            snareHitController2.SetAlpha(alpha);
                            snareHitController.transform.Rotate(new Vector3(-2f, 0f, 0f));
                            break;
                        case 48:
                            tom1HitController.SetAlpha(alpha);
                            tom1HitController2.SetAlpha(alpha);
                            break;
                        case 46:
                            hihatHitController.SetAlpha(alpha);
                            hihatHitController2.SetAlpha(alpha);
                            break;

                        case 49 or 52 or 55 or 57:
                            crashHitController.SetAlpha(alpha);
                            crashHitController2.SetAlpha(alpha);
                            break;

                        case 51 or 53 or 59:
                            rideHitController.SetAlpha(alpha);
                            rideHitController2.SetAlpha(alpha);
                            rideHitController.transform.Rotate(new Vector3(0.25f, -10f, 1f));
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
        ActivateOutline();
        foreach (Note note in notes) 
        {
            var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, midi.GetTempoMap());
            float fixStartTime = (float)(metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds + (double)metricTimeSpan.Milliseconds / 1000f);
            int noteName = note.NoteNumber;
            float noteVelocity = note.Velocity/127.0f;
            Tuple<float,int,float> tuple = new Tuple<float, int, float> ( fixStartTime + 4f, noteName, noteVelocity );
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
    public List<Tuple<float, int, float>> GetMidiList() 
    {
        return noteList;
    }
}
