using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public AudioSource drumTrackPlayer;
    public AudioSource drumlessTrackPlayer;
    public float audioPlayerTime;

    private void Update()
    {
        if (AudioIsPlaying()) 
        {
            if (drumlessTrackPlayer.time > drumTrackPlayer.time)
            {
                audioPlayerTime = drumlessTrackPlayer.time;
            }
            else if (drumlessTrackPlayer.time < drumTrackPlayer.time)
            {
                audioPlayerTime = drumTrackPlayer.time;
            }
            else
            {
                audioPlayerTime = drumTrackPlayer.time;
            }
        }
    }
    public void SetAudioPlayerTime(float time) 
    {
        audioPlayerTime += time;
        drumTrackPlayer.time += time;
        drumlessTrackPlayer.time += time;
    }
    public void ResetPlayerTime() 
    {
        audioPlayerTime = 0f;
        drumTrackPlayer.time = 0f;
        drumlessTrackPlayer.time = 0f;
    }
    public void SetAudioClip(AudioClip drumTrackClip, AudioClip drumlessTrackClip) 
    { 
        drumTrackPlayer.clip = drumTrackClip;
        drumlessTrackPlayer.clip = drumlessTrackClip;
    }
    public void PlayTrack() 
    {
        drumTrackPlayer.time = audioPlayerTime;
        drumlessTrackPlayer.time = audioPlayerTime;
        drumTrackPlayer.Play();
        drumlessTrackPlayer.Play();
    }

    public void PauseTrack(int option = 0)
    {
        if (option == 0)
        {
            drumTrackPlayer.Pause();
        }
        else if (option == 1)
        {
            drumlessTrackPlayer.Pause();
        }
        else 
        {
            drumlessTrackPlayer.Pause();
            drumTrackPlayer.Pause();
        }
    }
    public void StopTrack() 
    {
        drumTrackPlayer.Stop();
        drumlessTrackPlayer.Stop();
    }
    public void ResetPlayerTimer(float time = 0) 
    {
        audioPlayerTime = time;
    }
    public bool AudioIsPlaying() 
    {
        return drumlessTrackPlayer.isPlaying || drumTrackPlayer.isPlaying;
    }
}
