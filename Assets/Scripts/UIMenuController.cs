using SFB;  // Using a file browser library
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

public class UIMenuController : MonoBehaviour
{
    [SerializeField] public AudioSource AudioPlayer;
    [SerializeField] public DrumController drumController;
    [SerializeField] public List<SongData> songInfo = new List<SongData>();  // List to store song data
    [SerializeField] public AlertDialog.Popup_Setup alertDialog; //Alert system!

    // Paths and variables for managing audio files and libraries
    private string audioFilePath;
    private string libPath;
    private string unityPath;
    private string dataPersistentPath;
    private int songSelectedIndex = 0;

    private string songSelected; // Currently selected song
    private string trackSelected; // Currently selected track (drums or no_drums)

    [SerializeField] private List<string> songsLibrary = new List<string>();  // List of song names in the library
    [SerializeField] private List<string> songDirectories = new List<string>();  // List of song directories
    [SerializeField] private List<string> requiredFiles = new List<string>() { "drums.wav", "no_drums.wav", "drums.midi", "data.json" };  // Required files for each song
    [SerializeField] private List<string> trackList = new List<string>() { "drums", "no_drums" };  // List of available tracks

    private AudioClip track = null;  // Reference to the currently selected audio track
    private bool audioIsPlaying = false;

    // Unity UI elements
    [SerializeField] private VisualElement root;
    [SerializeField] private DropdownField trackDropdown;
    [SerializeField] private Button uploadButton;
    [SerializeField] private Button processButton;
    [SerializeField] private Button mediaPlayButton;
    [SerializeField] private Button mediaStopButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private VisualElement playIconElement;
    [SerializeField] private ListView libraryListView;
    [SerializeField] private bool bussy = false;  // Indicates if a background process is busy

    public Sprite playIcon;
    public Sprite pauseIcon;
    private StyleBackground playIconBackground;
    private StyleBackground pauseIconBackground;

    // Colors!
    private Color selectedSongColor = new Color32(130, 127, 160, 255);
    private Color unselectedSongColor = new Color32(29, 29, 29, 255);

    // Labels
    private Label fileName;

    private void Start()
    {
        string LibraryName = "/Lib";
        unityPath = Application.streamingAssetsPath;
        dataPersistentPath = Application.persistentDataPath;
        libPath = Application.persistentDataPath + LibraryName;
        playIconBackground = new StyleBackground(playIcon);
        pauseIconBackground = new StyleBackground(pauseIcon);
        RefreshLibrary();  // Initialize the song library
    }
    private void Update()
    {
        if (AudioPlayer.isPlaying)
        {
            playIconElement.style.backgroundImage = pauseIconBackground;
        }
        else
        {
            playIconElement.style.backgroundImage = playIconBackground;
        }
    }

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        // Get references to UI elements and set up event handlers
        uploadButton = root.Q<Button>("buttonUpload");
        processButton = root.Q<Button>("buttonProcess");
        mediaPlayButton = root.Q<Button>("buttonPlay");
        mediaStopButton = root.Q<Button>("buttonStop");
        
        playIconElement = root.Q<VisualElement>("iconPlay");
        playIconElement.style.backgroundImage = playIconBackground;

        libraryListView = root.Q<ListView>("libraryListView");
        trackDropdown = root.Q<DropdownField>("trackSelection");
        trackDropdown.choices.Clear();
        trackDropdown.choices = trackList;
        trackDropdown.index = 0;
        trackSelected = trackDropdown.choices[0];
        trackDropdown.RegisterValueChangedCallback(async v => {
            trackSelected = v.newValue;
            AudioPlayer.Stop();
            track = await LoadAudioClip();
        });

        // Set up event handlers for UI buttons
        uploadButton.clicked += () => UploadAudiofile();
        processButton.SetEnabled(false);
        processButton.clicked += () => StartProcessing();
        mediaPlayButton.clicked += () => PlayTrack();
        mediaStopButton.clicked += () => StopTrack();

        // Set up event handler for selecting a song in the library
        libraryListView.itemsChosen += async (evt) =>
        {
            
            libraryListView.SetEnabled(false);
            mediaPlayButton.SetEnabled(false);
            songSelected = songDirectories[libraryListView.selectedIndex];
            if (AudioPlayer.isPlaying) 
            {
                StopTrack();
                drumController.SetMidiPathFile(songSelected+ "/drums.midi");

            }
            
            track = await LoadAudioClip();  // Load the selected audio clip
            songSelectedIndex = libraryListView.selectedIndex;

            Action<VisualElement, int> bindItem = (e, i) =>
            {
                var name = e.Q<Label>("name");
                var difficulty = e.Q<Label>("difficulty");
                var tempo = e.Q<Label>("tempo");
                var background = e.Q<VisualElement>("layerBackground");
                if (i == songSelectedIndex)
                {
                    background.style.backgroundColor = selectedSongColor;
                }
                else 
                {
                    background.style.backgroundColor = unselectedSongColor;
                }
                name.text = songsLibrary[i];
                difficulty.text = songInfo[i].difficulty;
                tempo.text = songInfo[i].tempo;
            };
            libraryListView.bindItem += bindItem;
            drumController.SetMidiPathFile(songSelected + "/drums.midi");
            mediaPlayButton.SetEnabled(true);
            libraryListView.SetEnabled(true);
        };
    }

    private async void StartProcessing() 
    {
        if (audioFilePath != null) 
        {
            if (!bussy)  // Check if a background process is not already running
            {
                bussy = true;
                processButton.SetEnabled(false);
                alertDialog.CreateDialog("The file is processing", "This may take a while.");
                await Task.Run(() => { ProcessSong(); }) ;  // Start a background task to process the song
                bussy = false;
                processButton.SetEnabled(true);
                alertDialog.CreateDialog("The file is processed", "The song is ready and loaded in the library.");
            }
            else 
            {
                return;  // Do nothing if a process is already running
            }
        }
    }

    // Method to process a song using a Python script
    public void ProcessSong()
    {
        bussy = true;
        string command = "python \"";
        command += unityPath + "/Python/main.py\" " + "\"" + audioFilePath + "\"";
        ProcessStartInfo processStartInfo = new ProcessStartInfo();
        processStartInfo.FileName = "cmd.exe";
        processStartInfo.RedirectStandardInput = true;
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
        pythonProcess.StandardInput.Close();
        pythonProcess.WaitForExit();
        pythonProcess.Close();
    }

    private async void RefreshLibrary() 
    {
        SearchSongs();
        libraryListView.Clear();
        var listItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/SongUI.uxml");
        Func<VisualElement> makeItem = () => listItem.Instantiate();
        Action<VisualElement, int> bindItem = (e, i) => 
        {
            var name = e.Q<Label>("name");
            var difficulty = e.Q<Label>("difficulty");
            var tempo = e.Q<Label>("tempo");
            var background = e.Q<VisualElement>("layerBackground");
            if (i == songSelectedIndex)
            {
                background.style.backgroundColor = selectedSongColor;
            }
            else 
            {
                background.style.backgroundColor = unselectedSongColor;
            }
            name.text = songsLibrary[i];
            difficulty.text = songInfo[i].difficulty;
            tempo.text = songInfo[i].tempo;
        };
        libraryListView.makeItem = makeItem;
        libraryListView.bindItem = bindItem;
        libraryListView.itemsSource = songsLibrary;
        libraryListView.selectionType = SelectionType.Single;
        libraryListView.selectedIndex = 0;
        if (songSelected != null) 
        {
            track = await LoadAudioClip();
            drumController.SetMidiPathFile(songSelected + "/drums.midi");
        }
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
            // Handle the case where required files are missing
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
                alertDialog.CreateDialog("Import Error", "The file format is invalid.");
            }
        }
        else
        {
            processButton.SetEnabled(false);
            alertDialog.CreateDialog("Import Error", "Not file selected.");
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

        SetDifficultyAndTempo();
    }

    private void PlayTrack()
    {
        if (track != null)
        {
            AudioPlayer.clip = track;
            if (AudioPlayer.isPlaying)
            {
                AudioPlayer.Pause();
            }
            else 
            {
                AudioPlayer.Play();
            }
        }
        else 
        {
            return;
        }
    }

    // Load an audio clip asynchronously
    async Task<AudioClip> LoadAudioClip()
    {
        string songDirectory = null;
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
        Debug.Log("Song Dir: " + songDir);
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
                    // Handle any exceptions
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
