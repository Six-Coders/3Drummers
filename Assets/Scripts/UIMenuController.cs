using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

public class UIMenuController : MonoBehaviour
{
    public AudioSource AudioPlayer;
    public DrumController drumController;
    public List<SongData> songInfo = new List<SongData>();

    private string audioFilePath;
    private string libPath;
    private string unityPath;
    private string dataPersistentPath;

    private string songSelected; //Lista de canciones
    private string trackSelected; // Drum; no_drums

    private List<string> songsLibrary = new List<string>();
    private List<string> songDirectories = new List<string>();
    private List<string> requiredFiles = new List<string>() { "drums.wav", "no_drums.wav", "drums.midi", "data.json" };
    private List<string> trackList = new List<string>() { "drums", "no_drums" };

    private AudioClip track = null;

    private VisualElement root;
    private DropdownField trackDropdown;
    private Button uploadButton;
    private Button processButton;

    private ListView libraryListView;

    private bool bussy = false;

    //Labels
    private Label fileName;

    private void Start()
    {
        string LibraryName = "/Lib";
        unityPath = Application.streamingAssetsPath;
        dataPersistentPath = Application.persistentDataPath;
        libPath = Application.persistentDataPath + LibraryName;
        RefreshLibrary();
    }

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        uploadButton = root.Q<Button>("buttonUpload");
        processButton = root.Q<Button>("buttonProcess");
        libraryListView = root.Q<ListView>("libraryListView");
        trackDropdown = root.Q<DropdownField>("trackSelection");
        trackDropdown.choices.Clear();
        trackDropdown.choices = trackList;
        trackDropdown.index = 0;

        uploadButton.clicked += () => UploadAudiofile();
        processButton.SetEnabled(false);
        processButton.clicked += () => StartProcessing();
        libraryListView.itemsChosen += (evt) => Debug.Log(evt.First().ToString());
    }

    private async void StartProcessing() 
    {
        if (audioFilePath != null) 
        {
            if (!bussy) 
            {
                bussy = true;
                Debug.Log("Empezo el webeo");
                await Task.Run(() => { ProcessSong(); }) ;
                bussy = false;
                
            }
            else 
            {
                Debug.Log("Estoy ocupao");
            }

        }
    }


    public void ProcessSong()
    {
        bussy = true;
        string command = "python \"";
        command += unityPath + "/Python/main.py\" " + "\""+audioFilePath+"\"";
        ProcessStartInfo processStartInfo = new ProcessStartInfo();
        processStartInfo.FileName = "cmd.exe";
        processStartInfo.RedirectStandardInput = true;
        processStartInfo.RedirectStandardOutput = true;
        processStartInfo.UseShellExecute = false;
        processStartInfo.CreateNoWindow = true;
        Process pythonProcess = new Process();
        pythonProcess.StartInfo = processStartInfo;
        pythonProcess.Start();
        pythonProcess.StandardInput.WriteLine("conda activate 3Drummers");
        command += " \"" + dataPersistentPath + "/Lib\"";
        string modelDirectorie = unityPath.Replace('\\', '/') + "\"/Python/onsets_frames_transcription/model_checkpoint/model.ckpt-569400\"";
        command += " \"" + modelDirectorie + "\"";
        pythonProcess.StandardInput.WriteLine(command);
        pythonProcess.WaitForExit();
    }
    private void RefreshLibrary() 
    {
        SearchSongs();
        libraryListView.Clear();
        

        var listItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/SongUI.uxml");


        Func<VisualElement> makeItem = () => listItem.Instantiate();

        Action<VisualElement, int> bindItem = (e, i) => 
        {
            //(e as Label).text = songsLibrary[i];
            var name = e.Q<Label>("name");
            var difficulty = e.Q<Label>("difficulty");
            var tempo = e.Q<Label>("tempo");

            name.text = songsLibrary[i];
            difficulty.text = songInfo[i].difficulty;
            tempo.text = songInfo[i].tempo;

            e.style.backgroundColor = Color.gray;
        };

        libraryListView.makeItem = makeItem;
        libraryListView.bindItem = bindItem;
        libraryListView.itemsSource = songsLibrary;
        libraryListView.selectionType = SelectionType.Single;
    }


    private void StopTrack() 
    {
        if (AudioPlayer.isPlaying) 
        {
            AudioPlayer.Stop();
        }
    }

    private void SetDifficultyAndTempo() 
    {
        if (CheckFiles(songSelected))
        {
            string jsonPath = Path.Combine(songSelected, "data.json");
            string jsonContent = File.ReadAllText(jsonPath);
            SongData songData = JsonUtility.FromJson<SongData>(jsonContent);
        }
        else
        {
        }

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

    private void UploadAudiofile() 
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false);
        if (paths.Length > 0)
        {
            //Verificar el directorio
            Match match = Regex.Match(paths[0], pattern: @".+\\(.*)\.(?:mp3|wav|flac|ogg)$");
            if (match.Success)
            {
                string audiofileName = match.Groups[1].Value;
                audiofileName = paths[0].Replace('\\', '/');
                audioFilePath = audiofileName;
                processButton.SetEnabled(true);
            }
            else
            {
                processButton.SetEnabled(false);
            }
        }
        else
        {
            processButton.SetEnabled(false);
        }

    }
    private void SearchSongs() 
    {
        songDirectories.Clear();
        songsLibrary.Clear();
        songInfo.Clear();

        string[] subdirectories = Directory.GetDirectories(libPath);
        foreach (string subdirectory in subdirectories) 
        {
            string subDirName = Path.GetFileName(subdirectory);
            songsLibrary.Add(subDirName);
            songDirectories.Add(subdirectory);
            if (CheckFiles(subdirectory))
            {
                string jsonPath = Path.Combine(subdirectory, "data.json");
                string jsonContent = File.ReadAllText(jsonPath);
                SongData songData = JsonUtility.FromJson<SongData>(jsonContent);
                songInfo.Add(songData);
            }
        }
        songSelected = songDirectories[0];
        string firstSong = songsLibrary[0];
        SetDifficultyAndTempo();
    }

    private async void PlayTrack()
    {
        track = await LoadAudioClip();
        if (track != null)
        {
            AudioPlayer.clip = track;
            AudioPlayer.Play();
        }
        else 
        {
            return;
        }
    }

    async Task<AudioClip> LoadAudioClip()
    {
        string songDirectory;
        if (songSelected != null)
        {
            if (trackSelected != null)
            {
                songDirectory = songSelected + "/" + trackSelected + ".wav";
            }
            else 
            {
                return null;
            }
            
        }
        else 
        {
            return null;
        }
        string songAuxPath = songDirectory.Replace('\\', '/');
        string songDir = songAuxPath.Replace("/", "//");
        if (songDir != null)
        {
            AudioClip audioClip = null;
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(songDir, AudioType.WAV))
            {
                uwr.SendWebRequest();
                try
                {
                    while (!uwr.isDone) await Task.Delay(5);
                    if (uwr.isNetworkError || uwr.isHttpError) Debug.Log($"{uwr.error}");
                    else
                    {
                        audioClip = DownloadHandlerAudioClip.GetContent(uwr);
                    }
                }
                catch (Exception err)
                {
                }
            };
            audioClip.name = songDir;
            return audioClip;
        }
        else
        {
            return null;
        }
    }
}

[Serializable]
public class SongData 
{
    public string difficulty;
    public string tempo;
}