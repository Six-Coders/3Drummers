using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LibSongList : MonoBehaviour
{
    static string folderName = "Lib";
    public string libPath;
    public GameObject foldersNamePrefab;
    public GameObject songCellPrefab;
    public Transform content;

    //Files to Verify
    private string[] fileNames = {"drums.wav","drums.midi","data.json"}; //Agregar no_drums.wav cuando tenga el codigo del Alvaro Churrasco
    private Color32 invalidSongColor = Color.red;

    void Start() {
        libPath = Path.Combine(Application.persistentDataPath, folderName);
        if (!Directory.Exists(libPath))
        {
            Directory.CreateDirectory(libPath);
            Debug.Log("Carpeta creada en " + libPath);
        }
        SearchSongs();
    }

    public void SearchSongs() {
        foreach (Transform child in content) 
        { 
            Destroy(child.gameObject);
        }
        content.DetachChildren();

        string[] subdirectories = Directory.GetDirectories(libPath);
        foreach (string subdir in subdirectories)
        {
            string subdirName = Path.GetFileName(subdir);
            GameObject subdirObject = Instantiate(songCellPrefab, content);
            
            TextMeshProUGUI [] components = subdirObject.GetComponentsInChildren<TextMeshProUGUI>();
            Debug.Log(components.Length);
            Debug.Log(subdir);
            string dataJsonPath = Path.Combine(subdir, "data.json");

            if (VerifyFiles(subdir)) 
            {
                string jsonContent = File.ReadAllText(dataJsonPath);
                SongData songData = JsonUtility.FromJson<SongData>(jsonContent);

                const int maxNameSize = 10;
                string truncatedName = subdirName.Substring(0, Mathf.Min(subdirName.Length, maxNameSize));
                components[0].text = truncatedName;
                components[1].text = songData.difficulty;
                components[2].text = songData.tempo;
            }
            
            //subdirObject.GetComponentInChildren<TextMeshProUGUI>().text = subdirName;
            if (!VerifyFiles(subdir)) {
                subdirObject.GetComponentInChildren<TextMeshProUGUI>().color = invalidSongColor;
            }
            
        }
    }
    private bool VerifyFiles(string subdirPath) {
        foreach (string fileName in fileNames) { 
            string filePath = Path.Combine(subdirPath, fileName);
            if (!File.Exists(filePath)) {
                return false;
            }
        }
        return true;
    }
}
[System.Serializable]
public class SongData 
{
    public string tempo;
    public string difficulty;
}