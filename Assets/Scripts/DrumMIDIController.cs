using UnityEngine;
using MidiJack;
using System.Collections.Generic;
using System;
using AlertDialog;

public class DrumMIDIController : MonoBehaviour
{
    public DrumController drumController;
    [SerializeField] public AlertDialog.Popup_Setup alertDialog;

    List<Tuple<float, int, float>> recordingList = new List<Tuple<float, int, float>>();
    bool isRecording = false;
    float recordingTime;
    float startingLoopTime;
    float percentScore;
    float score = 0;
    float tol = 0.125f;
    float errorMargin = 0;
    float endingLoopTime;
    void Start()
    {
        MidiMaster.noteOnDelegate += NoteOn;
    }
    void Update()
    {

    }
    void NoteOn(MidiChannel channel, int note, float velocity)
    {
        Debug.Log(note);
        if (isRecording) 
        {
            Tuple<float, int, float> tuple = new Tuple<float, int, float>(Time.time - recordingTime + 2f + startingLoopTime, note, velocity);
            recordingList.Add(tuple);
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
            Debug.Log("Tiempo: "+tuple.Item1);
            if (tuple.Item1 >= startingLoopTime && tuple.Item1 <= endingLoopTime + 2f) 
            { 
                filterList.Add(tuple);
                Debug.Log("Agregue una tupla...");
            }
        }
        foreach (var tupleOriginal in filterList) 
        {
            foreach (var tupleRecording in recordingList) 
            {
                if (Math.Abs(tupleRecording.Item1 - tupleOriginal.Item1) < tol) 
                {
                    Debug.Log(Math.Abs(tupleRecording.Item1 - tupleOriginal.Item1));
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
                                if (tupleRecording.Item2 == 43 || tupleRecording.Item2 == 45 || tupleRecording.Item2 == 47 || tupleRecording.Item2 == 48 || tupleRecording.Item2 == 50) 
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
}
