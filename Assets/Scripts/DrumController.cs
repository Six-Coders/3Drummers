using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    [SerializeField] public GameObject noteContainer;
    [SerializeField] public GameObject notePrefab;

    [SerializeField] public Image kickImage;
    [SerializeField] public Image snareImage;
    [SerializeField] public Image hihatImage;
    [SerializeField] public Image tomImage;
    [SerializeField] public Image rideImage;
    [SerializeField] public Image crashImage;

    public Dictionary<int,Vector3> drumPositions = new Dictionary<int,Vector3>();
    public Dictionary<int, Color> drumColors = new Dictionary<int, Color>();

    private Color kickColor = new Color(255/255f, 71 / 255f, 71 / 255f);
    private Color snareColor = new Color(88 / 255f, 88 / 255f, 255 / 255f);
    private Color hihatColor = new Color(115 / 255f, 255 / 255f, 115 / 255f);
    private Color tomColor = new Color(255 / 255f, 107 / 255f, 245 / 255f);
    private Color rideColor = new Color(134 / 255f, 255 / 255f, 255 / 255f);
    private Color crashColor = new Color(253 / 255f, 253 / 255f, 123 / 255f);

    //[SerializeField] public Generador3 intentoPrimero;

    private List<Vector3> drumElements = new List<Vector3> ();
    public UIMenuController menuController;

    [SerializeField] public List<Material> overlineMaterials = new List<Material>();

    private float outlineThick = 0.015f;
    public float currentAlpha = 0f;
    //public AudioSource audioPlayer;
    public AudioController audioController;

    private string midifilePath;
    private List<Tuple<float, int, float>> noteList = new List<Tuple<float, int, float>>();
    private float audioStartTime;

    public float tolerance = 3f;

    public float noteSpeed = 2f;
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
    private void OnEnable()
    {
        drumPositions.Add(36, kickImage.rectTransform.localPosition);
        drumPositions.Add(38, snareImage.rectTransform.localPosition);
        drumPositions.Add(46, hihatImage.rectTransform.localPosition);
        drumPositions.Add(48, tomImage.rectTransform.localPosition);
        drumPositions.Add(51, rideImage.rectTransform.localPosition);
        drumPositions.Add(53, rideImage.rectTransform.localPosition);
        drumPositions.Add(59, rideImage.rectTransform.localPosition);
        drumPositions.Add(49, crashImage.rectTransform.localPosition);
        drumPositions.Add(52, crashImage.rectTransform.localPosition);
        drumPositions.Add(55, crashImage.rectTransform.localPosition);
        drumPositions.Add(57, crashImage.rectTransform.localPosition);

        drumColors.Add(36, kickColor);
        drumColors.Add(38, snareColor);
        drumColors.Add(46, hihatColor);
        drumColors.Add(48, tomColor);
        drumColors.Add(51, rideColor);
        drumColors.Add(53, rideColor);
        drumColors.Add(59, rideColor);
        drumColors.Add(49, crashColor);
        drumColors.Add(52, crashColor);
        drumColors.Add(55, crashColor);
        drumColors.Add(57, crashColor);
    }
    private void Start()
    {
        //Set Colors for every drum element
        kickHitController.SetColor(kickColor);
        snareHitController.SetColor(snareColor);
        tom1HitController.SetColor(tomColor);
        hihatHitController.SetColor(hihatColor);
        crashHitController.SetColor(crashColor);
        rideHitController.SetColor(rideColor);

        kickHitController2.SetColor(kickColor);
        snareHitController2.SetColor(snareColor);
        tom1HitController2.SetColor(tomColor);
        hihatHitController2.SetColor(hihatColor);
        crashHitController2.SetColor(crashColor);
        rideHitController2.SetColor(rideColor);

    }
    void FixedUpdate()
    {
        kickHitController.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        if (audioController.AudioIsPlaying()) 
        {
            DeactivateOutline();
        }
        if (midifilePath != null) 
        {
            for (int i = 0; i < noteList.Count; i++)
            {
                var tuple = noteList[i];
                if (Mathf.Abs(tuple.Item1 - audioController.audioPlayerTime - 2f) < tolerance+0.0125 && audioController.AudioIsPlaying()) 
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
                            break;
                        case 38:
                            snareHitController.SetAlpha(alpha);
                            snareHitController2.SetAlpha(alpha);
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
        CreateAllNotes();
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

    public void CreateAllNotes(float loopStartTime = 0)
    {
        GameObject[] notas = GameObject.FindGameObjectsWithTag("Note");
        foreach (var nota in notas) 
        {
            Destroy(nota);
        }
        foreach (var note in noteList) 
        {
            if (drumPositions.ContainsKey(note.Item2)) 
            {
                var newNote = Instantiate(notePrefab, noteContainer.transform);
                float x = (noteSpeed * 250f * note.Item1) - 1100f;
                Vector3 posFixed = new Vector3(x, drumPositions[note.Item2].y, drumPositions[note.Item2].z);
                Image noteImage = newNote.GetComponent<Image>();
                var fixedColor = drumColors[note.Item2];
                fixedColor.a = note.Item3;
                if (!menuController.isIntensitySet)
                {
                    fixedColor.a = 1f;
                }
                noteImage.color = fixedColor;
                newNote.transform.localPosition = posFixed;
            }
        }
    }
    public List<Tuple<float, int, float>> GetMidiList() 
    {
        return noteList;
    }
}
