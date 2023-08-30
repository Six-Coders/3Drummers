using System.Collections;
using System.Collections.Generic;
//using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;

public class ConvertToMidi : MonoBehaviour
{
    public Button myButton;
    public PythonServerAPI pythonServerAPI;
    public LoadingIndicator loadingIndicator;
    public LibSongList songList;
    private string songDirectory = null;
    private bool trackProcessed = false;
    void Start()
    {
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(StartConvertionToMidi);
        myButton.interactable = false;
    }
    public void TrackProcessed(bool activate)
    {
        trackProcessed = activate;
    }
    public void ActivateButton(bool drumTrackIsSelected) 
    {
        Debug.Log("Track processed: " + trackProcessed);
        Debug.Log("DrumTrackSelected: " + drumTrackIsSelected);
        myButton.interactable = trackProcessed && drumTrackIsSelected;
    }
    private void StartConvertionToMidi()
    {
        if (songDirectory == null)
        {
            Debug.LogError("No Drums track detected.");
        }
        else 
        {
            SendCommand("transform_to_midi");
        }
    }
    public void ResetSongDirectory() 
    {
        songDirectory = null;
    }
    public void SetDrumsPath(string songName) 
    {
        songDirectory = Application.persistentDataPath + "/Lib/" + songName;
        Debug.Log("Song Directory: " + songDirectory);
    }
    private async void SendCommand(string command) 
    {
        loadingIndicator.ShowLoadingIndicator("Transforming drums to MIDI...");
        string modelDirectorie = Application.dataPath.Replace('\\', '/') + "/PythonServer/onsets_frames_transcription/model_checkpoint/model.ckpt-569400";
        Debug.Log("Directorio del Checkpoint: " + modelDirectorie);

        Dictionary<string,string> parameter = new Dictionary<string, string>() 
        {
            {"parameter1",songDirectory},
            { "parameter2",modelDirectorie}
        };
        if (songDirectory != null)
        {
            await pythonServerAPI.CreateCommand(command, parameter);
            loadingIndicator.HideLoadingIndicator();
            songList.UpdateSongs(songDirectory);
        }
        else 
        {
            Debug.LogError("No Song Selected.");
        }
    
    }
}
