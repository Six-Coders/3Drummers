using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using System;

public class NewSongButtonController : MonoBehaviour
{
    public LoadingIndicator loadingIndicator;
    public PythonServerAPI serverAPI;
    public LibSongList libSongList;
    public ConvertToMidi convertToMidi;
    public DropdownTrackSelect dropDown;
    public string libPath;

    
    private string audioPathFile = null;
    Button button = null;
    void Start()
    {
        string folderName = "Lib";
        libPath = Application.persistentDataPath + "/" + folderName;
        button = GetComponent<Button>();
        button.onClick.AddListener(SplitSong);
        button.interactable = false;
        dropDown.GetComponent<TMPro.TMP_Dropdown>().interactable = false;
    }
    public void SetAudioFilePath(string path) 
    {
        audioPathFile = path;
    }
    public void ActivateButton(bool activate) 
    {
        button.interactable = activate;
    }
    private void SplitSong() 
    {
        if (audioPathFile == null)
        {
            Debug.LogError("No File Selected");
        }
        else 
        { 
            ProcessSong(audioPathFile);
        }
    }
    private async void ProcessSong(string soundfilePath) {
        //Crear Pruebas Despues!
        loadingIndicator.ShowLoadingIndicator("The song is separating into tracks...");
        Dictionary<string, string> parameters = new Dictionary<string, string>()
        { 
            {"parameter1",soundfilePath},
            { "parameter2",libPath}
        };
        await serverAPI.CreateCommand("separate_tracks",parameters);
        loadingIndicator.HideLoadingIndicator();
        libSongList.UpdateSongs(soundfilePath);
        convertToMidi.TrackProcessed(true);
        dropDown.GetComponent<TMPro.TMP_Dropdown>().interactable = true;
    }
}
