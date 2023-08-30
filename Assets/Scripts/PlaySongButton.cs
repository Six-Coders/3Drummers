using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.IO;

public class PlaySongButton : MonoBehaviour
{
    public AudioSource player;
    private string songDir;
    public Button myButton;
    public AudioImporter importer;
    private AudioClip clip = null;

    private void Start()
    {
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(PlaySong);
    }

    async Task<AudioClip> LoadAudioClip()
    {
        if (songDir != null)
        {
            AudioClip audioClip = null;
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(songDir, AudioType.WAV)) 
            {
                uwr.SendWebRequest();
                try {
                    while (!uwr.isDone) await Task.Delay(5);
                    if (uwr.isNetworkError || uwr.isHttpError) Debug.Log($"{uwr.error}");
                    else
                    {
                        Debug.Log("Estoy aca");
                        audioClip = DownloadHandlerAudioClip.GetContent(uwr);
                    }
                }
                catch (Exception err)
                {
                    Debug.Log($"{err.Message}, {err.StackTrace}");
                }
            } ;
            audioClip.name = songDir;
            return audioClip;
        }
        else 
        {
            return null;
        }
    }


    public void AddSongDir(string dir) 
    {
        songDir = dir+"/drums.wav";
        string songAuxPath = songDir.Replace('\\','/');
        songDir = songAuxPath.Replace("/", "//");
    }
    async private void PlaySong() 
    {
        clip = await LoadAudioClip();
        Debug.Log(clip.ToString());
        if (clip != null) 
        {
            player.clip = clip;
            Debug.Log("PASE POR AQUI clip: " + player.clip);
            player.Play();
        }
    }
}
