using UnityEngine;
using MidiJack;
using System.Collections.Generic;
using System;
using System.Linq;

public class DrumMIDIController : MonoBehaviour
{
    public DrumController drumController;

    List<Tuple<float, int, float>> recordingList = new List<Tuple<float, int, float>>();
    bool isRecording = false;
    float recordingTime;
    float startingLoopTime;
    int score = 0;
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
        Debug.Log("Puntaje: " + score);
    }
    private void Evaluation() 
    {
        List <Tuple<float,int,float>> originalMidi = drumController.GetMidiList();

        //Filter Original MIDI
        List < Tuple<float, int, float> > filterList = originalMidi
            .Where(tuple => tuple.Item1 >= startingLoopTime && tuple.Item1 <= endingLoopTime)
            .ToList();

        Debug.Log("filterList: " + filterList.Count.ToString());
        Debug.Log("recordingList: " + recordingList.Count.ToString());

        foreach (var tupleOriginal in filterList) 
        {
            foreach (var tupleRecording in recordingList) 
            {
                if (Math.Abs(tupleRecording.Item1 - tupleOriginal.Item1) < tol) 
                {
                    //MIDI Mapping Dictionary
                    Dictionary<int, List<int>> noteMappings = new Dictionary<int, List<int>>()
                    {

                        { 36, new List<int> { 35, 36 } },               // Kick
                        { 38, new List<int> { 38, 40 } },               // Snare
                        { 48, new List<int> { 43, 45, 47, 48, 50 } },   // Toms
                        { 46, new List<int> { 26, 42, 46 } },           // Hi-hat
                        { 49, new List<int> { 49, 52, 55, 57 } },       // Crash
                        { 51, new List<int> { 51, 53, 59 } },           // Ride
                    };

                    Debug.Log(Math.Abs(tupleRecording.Item1 - tupleOriginal.Item1));

                    // Check if original note and recording note match
                    if (noteMappings.ContainsKey(tupleOriginal.Item2) && noteMappings[tupleOriginal.Item2].Contains(tupleRecording.Item2)) 
                    {
                        score += 1;
                    }
                } 
            }
        }
        // Adjust score base on the difference in note count
        score -= Math.Abs(filterList.Count - recordingList.Count);
    }
}
