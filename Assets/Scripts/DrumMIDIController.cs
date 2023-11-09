using UnityEngine;
using MidiJack;
using System.Collections.Generic;
using System;
using Melanchall.DryWetMidi.Multimedia;

public class DrumMIDIController : MonoBehaviour
{
    public DrumController drumController;
    [SerializeField] public AlertDialog.Popup_Setup alertDialog;
    InputDevice inputDevice = null;

    List<Tuple<float, int, float>> recordingList = new List<Tuple<float, int, float>>();
    bool isRecording = false;
    float recordingTime;
    float startingLoopTime;
    float percentScore;
    float score = 0;
    float tol = 0.125f;
    float errorMargin = 0;
    float endingLoopTime;
    bool isConected;

    //Drum HUD elements! (for animation)
    public Transform kickTransform;
    public Transform snareTransform;
    public Transform hihatTransform;
    public Transform rideTransform;
    public Transform crashTransform;
    public Transform tomTransform;

    private Dictionary<int, Transform> drumMap = new Dictionary<int, Transform>();


    void Start()
    {
        MidiMaster.noteOnDelegate += NoteOn;
        drumMap.Add(36, kickTransform);
        drumMap.Add(38, snareTransform);
        drumMap.Add(40, snareTransform);

        drumMap.Add(26, hihatTransform);
        drumMap.Add(42, hihatTransform);
        drumMap.Add(46, hihatTransform);

        drumMap.Add(43, tomTransform);
        drumMap.Add(45, tomTransform);
        drumMap.Add(47, tomTransform);
        drumMap.Add(48, tomTransform);
        drumMap.Add(50, tomTransform);

        drumMap.Add(49, crashTransform);

        drumMap.Add(51, rideTransform);
        drumMap.Add(59, rideTransform);
    }
    void Update()
    {
        if (InputDevice.GetAll().Count > 0 && !isConected)
        {
            isConected = true;
            inputDevice = InputDevice.GetByIndex(0);
            alertDialog.CreateDialog("Notification","MIDI device "+inputDevice.Name+" is connected.");
        }
 
        if (isConected && InputDevice.GetAll().Count < 1) 
        {
            isConected = false;
            alertDialog.CreateDialog("Notification", "MIDI device " + inputDevice.Name + " is disconnected.");
            inputDevice = null;
        }
        foreach (var t in drumMap) 
        {
            Transform transformElement = t.Value;
            if (transformElement.localScale.x > 1 && transformElement.localScale.y > 1)
            {
                transformElement.localScale -= new Vector3(0.05f, 0.05f, 0);
            }
            else 
            {
                transformElement.localScale = new Vector3(1, 1, 0);
            }
            
        }
    }
    void NoteOn(MidiChannel channel, int note, float velocity)
    {
        Debug.Log(note);
        if (velocity > 0) 
        {
            if (isRecording)
            {
                Tuple<float, int, float> tuple = new Tuple<float, int, float>(Time.time - recordingTime + 2f + startingLoopTime, note, velocity);
                recordingList.Add(tuple);
            }
            if (drumMap.ContainsKey(note))
            {
                var t = drumMap[note];
                Debug.Log("Velocity: ");
                Debug.Log(velocity);
                t.localScale = new Vector3(1 + velocity * 2, 1 + velocity * 2, 0);
            }
        }
    }
    public void Recording(float startTime, float endTime) 
    {
        score = 0;
        isRecording = true;
        recordingTime = Time.time;
        startingLoopTime = startTime;
        endingLoopTime = endTime;
        recordingList.Clear();
    }
    public void StopRecording() 
    {
        isRecording = false;
        Evaluation();
    }
    private void Evaluation() 
    {
        List <Tuple<float,int,float>> originalMidi = drumController.GetMidiList();

        //Filter originalList
        List < Tuple<float, int, float> > filterList = new List<Tuple<float, int, float>> ();
        foreach (var tuple in originalMidi) 
        {
            if (tuple.Item1 >= startingLoopTime && tuple.Item1 <= endingLoopTime + 2f) 
            { 
                filterList.Add(tuple);
            }
        }
        foreach (var tupleOriginal in filterList) 
        {
            foreach (var tupleRecording in recordingList) 
            {
                if (Math.Abs(tupleRecording.Item1 - tupleOriginal.Item1) < tol) 
                {
                    switch (tupleOriginal.Item2) 
                    {
                        case 36: 
                            {
                                if (tupleRecording.Item2 == 35 || tupleRecording.Item2 == 36) 
                                {
                                    score += 1;
                                }
                                break;
                            }
                        case 38:
                            {
                                if (tupleRecording.Item2 == 38 || tupleRecording.Item2 == 40)
                                {
                                    score += 1;
                                }
                                break;
                            }
                        case 48:
                            {
                                if (tupleRecording.Item2 == 43 || tupleRecording.Item2 == 45 || tupleRecording.Item2 == 47 || tupleRecording.Item2 == 48 || tupleRecording.Item2 == 50 || tupleRecording.Item2 == 58) 
                                {
                                    score += 1;
                                }
                                break;
                            }
                        case 46: 
                            {
                                if (tupleRecording.Item2 == 42 || tupleRecording.Item2 == 46 || tupleRecording.Item2 == 26) 
                                {
                                    score += 1;
                                }
                                break;
                            }
                        case 49 or 52 or 55 or 57: 
                            {
                                if (tupleRecording.Item2 == 49 || tupleRecording.Item2 == 52 || tupleRecording.Item2 == 55 || tupleRecording.Item2 == 57) 
                                {
                                    score += 1;
                                }
                                break;
                            }
                        case 51 or 53 or 59: 
                            {
                                if (tupleRecording.Item2 == 51 || tupleRecording.Item2 == 53 || tupleRecording.Item2 == 59) 
                                {
                                    score += 1;
                                }
                                break;
                            }
                    }
                } 
            }
        }
        score -= Math.Abs(filterList.Count - recordingList.Count);
        var totalNotes = filterList.Count;
        if (totalNotes == 0) 
        {
            totalNotes = 1;
        }
        Debug.Log("Score: " + score.ToString());
        Debug.Log("TotalNotes: " + totalNotes.ToString());
        float division = score / totalNotes;
        Debug.Log("Division: " + division.ToString());
        percentScore = division * 100f;
        Debug.Log("PercentScore: "+percentScore.ToString());
        if (percentScore < 0) 
        {
            percentScore = 0;
        }
        var result = "";
        if (percentScore <= 40)
        {
            result = "Bad";
        }
        else if (percentScore <= 60)
        {
            result = "Good";
        }
        else if (percentScore <= 80)
        {
            result = "Very Good";
        }
        else 
        {
            result = "Excellent!";
        }
        alertDialog.CreateDialog("Result", "Your Score is: " + result+" with %"+percentScore.ToString()+" correct notes.");
    }
    public bool IsConnected() 
    {
        return isConected;
    }
}
