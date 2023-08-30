using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LibSongList : MonoBehaviour
{
    static string folderName = "Lib";
    public string libPath;
    public TMP_Dropdown dropdown;
    public PlaySongButton playSongButton;
    public TextMeshProUGUI difficultyText;
    public TextMeshProUGUI tempoText;

    private List<string> songNames = new List<string>();
    private List<string> songDirectories = new List<string>();

    private Color colorNotMidi = Color.red;
    private List<string> requiredFiles = new List<string>() { "drums.wav","no_drums.wav","drums.midi","data.json"};


    void Start()
    {
        libPath = Path.Combine(Application.persistentDataPath, folderName);
        if (!Directory.Exists(libPath))
        {
            Directory.CreateDirectory(libPath);
        }
        SearchSongs();
    }

    private void SearchSongs()
    {
        dropdown.ClearOptions();

        songNames.Clear();
        songDirectories.Clear();
        

        string[] subdirectories = Directory.GetDirectories(libPath);
        foreach (string subdir in subdirectories)
        {
            string subdirName = Path.GetFileName(subdir);
            songNames.Add(subdirName);
            songDirectories.Add(subdir);
            
        }
        dropdown.AddOptions(songNames);
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        string defaultSong = songDirectories[0];
        ShowSongInfo(defaultSong);
        playSongButton.AddSongDir(defaultSong);
    }
    public void UpdateSongs(string path) 
    {
        SearchSongs();
        int index = 0;
        for (int i = 0; i < songDirectories.Count; i++) 
        {
            string songName = Path.GetFileName(songDirectories[i]);
            string songNameFromPath = Path.GetFileNameWithoutExtension(path);
            Debug.Log("Nombres: " + songName + "-" + songNameFromPath);
            if (songName == songNameFromPath) 
            {
                Debug.Log("Pase por aca");
                index = i; break;
            }
        }
        dropdown.value = index;
        string defaultSong = songDirectories[index];
        ShowSongInfo(defaultSong);
        playSongButton.AddSongDir(defaultSong);

    }

    // Método que se llama cuando se cambia la selección en el Dropdown
    void OnDropdownValueChanged(int index)
    {
        string selectedSongDirectory = songDirectories[index];
        playSongButton.AddSongDir(selectedSongDirectory);
        ShowSongInfo(selectedSongDirectory);
    }

    private bool CheckFiles(string path) 
    {
        bool isValid = true;
        foreach (string requiredFile in requiredFiles) 
        {
            string filePath = Path.Combine(path, requiredFile);
            if (!File.Exists(filePath)) 
            {
                isValid = false;
            }
        }
        return isValid;
    }
    private void ShowSongInfo(string path) 
    {
        if (CheckFiles(path))
        {
            string jsonPath = Path.Combine(path, "data.json");
            string jsonContent = File.ReadAllText(jsonPath);
            SongData songData = JsonUtility.FromJson<SongData>(jsonContent);
            difficultyText.text = "Difficulty: " + songData.difficulty;
            tempoText.text = "Tempo: " + songData.tempo +" BPM";
            difficultyText.color = Color.white;
            tempoText.color = Color.white;
        }
        else 
        {
            difficultyText.text = "Not Analized";
            difficultyText.color = colorNotMidi;
            tempoText.text = "Not Analized";
            tempoText.color = colorNotMidi;
        }
    }


}

[System.Serializable]
public class SongData 
{
    public string tempo;
    public string difficulty;
}