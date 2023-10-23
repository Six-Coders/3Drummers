using SFB;  // Using a file browser library
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using Melanchall.DryWetMidi.Core;
using Debug = UnityEngine.Debug;

public class UIMenuController : MonoBehaviour
{
    [SerializeField] public AudioSource AudioPlayer;
    [SerializeField] public DrumController drumController;
    [SerializeField] public List<SongData> songInfo = new List<SongData>();  // List to store song data
    [SerializeField] public AlertDialog.Popup_Setup alertDialog; //Alert system!
    [SerializeField] public UISettings.SettingsSetup settingsSetup; //Settings system!
    [SerializeField] public MultimediaLineController lineController;
    [SerializeField] public DrumMIDIController drumMIDIController;

    private StyleBackground intensityIconEnable;
    private StyleBackground intensityIconDisable;
    public Sprite iconEnable;
    public Sprite iconDisable;

    public AudioMixerGroup audioMixerGroupDrumTrack;
    public AudioMixerGroup audioMixerGroupNoDrumsTrack;

    // Paths and variables for managing audio files and libraries
    private string audioFilePath;
    private string libPath;
    private string unityPath;
    private string dataPersistentPath;
    private float moveTime = 3;
    private int songSelectedIndex = 0;

    private string songSelected; // Currently selected song
    private string trackSelected; // Currently selected track (drums or no_drums)

    [SerializeField] private List<string> songsLibrary = new List<string>();  // List of song names in the library
    [SerializeField] private List<string> songDirectories = new List<string>();  // List of song directories
    [SerializeField] private List<string> requiredFiles = new List<string>() { "drums.wav", "no_drums.wav", "drums.midi", "data.json" };  // Required files for each song
    [SerializeField] private List<string> trackList = new List<string>() { "drums", "no_drums" };  // List of available tracks

    private AudioClip track = null;  // Reference to the currently selected audio track
    private bool audioIsPlaying = false;
    public bool isIntensitySet = true;

    // Unity UI elements
    [SerializeField] private VisualElement root;
    [SerializeField] private DropdownField trackDropdown;
    [SerializeField] private Button uploadButton;
    [SerializeField] private Button processButton;
    [SerializeField] private Button testButton;
    [SerializeField] private Button mediaPlayButton;
    [SerializeField] private Button mediaStopButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mediaIncreaseButton;
    [SerializeField] private Button mediaDecreaseButton;
    [SerializeField] private Button exportMIDIButton;
    [SerializeField] private Button intensityButton;
    [SerializeField] private VisualElement playIconElement;
    [SerializeField] private ListView libraryListView;
    [SerializeField] private bool bussy = false;  // Indicates if a background process is busy
    [SerializeField] private VisualElement intensityIcon;
    [SerializeField] private MinMaxSlider multimediaLine;

    public Sprite playIcon;
    public Sprite pauseIcon;

    private StyleBackground playIconBackground;
    private StyleBackground pauseIconBackground;

    // Colors!
    private Color selectedSongColor = new Color32(130, 127, 160, 255);
    private Color unselectedSongColor = new Color32(29, 29, 29, 255);

    // Labels
    private Label fileName;

    //Multimedia System
    private float loopStartTime;
    private float loopEndTime;
    private float audioLength;

    bool isRecording = false;
    private void Start()
    {
        string LibraryName = "/Lib";
        unityPath = Application.streamingAssetsPath;
        dataPersistentPath = Application.persistentDataPath;
        libPath = Application.persistentDataPath + LibraryName;
        playIconBackground = new StyleBackground(playIcon);
        pauseIconBackground = new StyleBackground(pauseIcon);
        intensityIconEnable = new StyleBackground(iconEnable);
        intensityIconDisable = new StyleBackground(iconDisable);
        RefreshLibrary();  // Initialize the song library
        multimediaLine.lowLimit = 0;
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

        if (AudioPlayer.time > loopEndTime) 
        {
            AudioPlayer.time = loopStartTime;
            if (isRecording)
            {
                StopRecording();
            }
        }
    }

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        AudioPlayer.outputAudioMixerGroup = audioMixerGroupDrumTrack;

        // Get references to UI elements and set up event handlers
        uploadButton = root.Q<Button>("buttonUpload");
        processButton = root.Q<Button>("buttonProcess");
        mediaPlayButton = root.Q<Button>("buttonPlay");
        mediaStopButton = root.Q<Button>("buttonStop");
        settingsButton = root.Q<Button>("buttonSettings");
        mediaIncreaseButton = root.Q<Button>("buttonIncrease");
        mediaDecreaseButton = root.Q<Button>("buttonDecrease");
        intensityButton = root.Q<Button>("buttonIntensity");
        exportMIDIButton = root.Q<Button>("buttonExportMIDI");
        testButton = root.Q<Button>("buttonTest");

        playIconElement = root.Q<VisualElement>("iconPlay");
        intensityIcon = root.Q<VisualElement>("iconIntensity");
        playIconElement.style.backgroundImage = playIconBackground;

        multimediaLine = root.Q<MinMaxSlider>("multimediaLine");
        libraryListView = root.Q<ListView>("libraryListView");
        trackDropdown = root.Q<DropdownField>("trackSelection");
        trackDropdown.choices.Clear();
        trackDropdown.choices = trackList;
        trackDropdown.index = 0;
        trackSelected = trackDropdown.choices[0];
        trackDropdown.RegisterValueChangedCallback(async v => {
            trackSelected = v.newValue;
            if (v.newValue == "drums")
            {
                AudioPlayer.outputAudioMixerGroup = audioMixerGroupDrumTrack;
            }
            else
            {
                AudioPlayer.outputAudioMixerGroup = audioMixerGroupNoDrumsTrack;
            }
            AudioPlayer.Stop();
            track = await LoadAudioClip();
            AudioPlayer.clip = track;
            AudioPlayer.time = 0f;
            AddDelayToSong();
        });

        // Set up event handlers for UI buttons
        uploadButton.clicked += () => UploadAudiofile();
        processButton.SetEnabled(false);
        processButton.clicked += () => StartProcessing();
        mediaPlayButton.clicked += () => PlayTrack();
        mediaStopButton.clicked += () => StopTrack();
        mediaIncreaseButton.clicked += () => IncreaseTrack();
        mediaDecreaseButton.clicked += () => DecreaseTrack();
        exportMIDIButton.clicked += () => SelectFolderButton();
        testButton.clicked += () => StartRecordMidi();
        settingsButton.clicked += () => {
            settingsSetup.CreateSettingsWindow();
        };
        intensityButton.clicked += () =>
        {
            if (isIntensitySet)
            {
                isIntensitySet = false;
                intensityIcon.style.backgroundImage = intensityIconDisable;
            }
            else 
            {
                isIntensitySet = true;
                intensityIcon.style.backgroundImage = intensityIconEnable;
            }
        };
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
            AudioPlayer.clip = track;
            AddDelayToSong();
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
        multimediaLine.RegisterValueChangedCallback(v => 
        {
            var newValues = v.newValue;
            var minValue = newValues[0];
            var maxValue = newValues[1];
            loopStartTime = minValue;
            loopEndTime = maxValue;
            AudioPlayer.Pause();
            AudioPlayer.time = minValue;
        });
    }
    private void StartRecordMidi() 
    {
        if (!isRecording)
        {
            mediaPlayButton.SetEnabled(false);
            mediaDecreaseButton.SetEnabled(false);
            mediaIncreaseButton.SetEnabled(false);
            mediaStopButton.SetEnabled(false);
            isRecording = true;
            AudioPlayer.Stop();
            AudioPlayer.Play();
            drumMIDIController.Recording(loopStartTime,loopEndTime);
            Debug.Log(AudioPlayer.time);
        }
        else 
        {
            StopRecording();
        }
    }
    private void StopRecording() 
    {
        mediaPlayButton.SetEnabled(true);
        mediaDecreaseButton.SetEnabled(true);
        mediaIncreaseButton.SetEnabled(true);
        mediaStopButton.SetEnabled(true);
        AudioPlayer.Stop();
        isRecording = false;
        drumMIDIController.StopRecording();
    }
    //Funcion para inicializar el proceso de separar y ADT de la canción
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
                RefreshLibrary();
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

    //Funcion para refrescar la libreria (se cargan las canciones a la listView)
    private async void RefreshLibrary() 
    {
        SearchSongs();
        libraryListView.Clear();
        var listItem = Resources.Load<VisualTreeAsset>("SongUI");
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
            AudioPlayer.clip = track;
            AddDelayToSong();
            drumController.SetMidiPathFile(songSelected + "/drums.midi");
        }
        if (audioFilePath != null) 
        { 
            string fileName = Path.GetFileNameWithoutExtension(audioFilePath);
            int index = -1;
            for (int i = 0; i < songsLibrary.Count; i++) 
            {
                string songLibraryName = songsLibrary[i];
                if (fileName.Equals(songLibraryName)) 
                {
                    index = i;
                    Debug.Log("Pase por aqui");
                    break;
                }
            }
            if (index != -1) 
            {
                songSelectedIndex = index;
                UpdateLibrary();
            }
        }
    }

    //Función para ACTUALIZAR la libreria
    private void UpdateLibrary() 
    {

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

    }

    //Funcion para DETENER la reproducción de la canción
    private void StopTrack() 
    {
        if (AudioPlayer.isPlaying) 
        {
            AudioPlayer.Stop();
            GameObject[] notas = GameObject.FindGameObjectsWithTag("Note");
            foreach (GameObject obj in notas) 
            {
                Destroy(obj);
            }
        }
    }

    // XDDD
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
    //Funcion para verificar los archivos requeridos al momento de cargar la libreria
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
    //Funcion para abrir un archivo de audio
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
    //Función para buscar canciones en un directorio (el default)
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

    //Función para agregar un Delay de 4 segundos a la canción (se crea un clip nuevo).
    private void AddDelayToSong() 
    {
        float silenceDuration = 4f;
        AudioClip silenceClip = CreateSilenceClip(silenceDuration);
        float[] silenceData = new float[silenceClip.samples * silenceClip.channels];
        silenceClip.GetData(silenceData, 0);
        float[] originalData = new float[AudioPlayer.clip.samples * AudioPlayer.clip.channels];
        AudioPlayer.clip.GetData(originalData, 0);
        float[] combinedData = new float[silenceData.Length + originalData.Length];
        Array.Copy(silenceData, combinedData, silenceData.Length);
        Array.Copy(originalData, 0, combinedData, silenceData.Length, originalData.Length);
        AudioClip combinedClip = AudioClip.Create("CombinedAudio", combinedData.Length, AudioPlayer.clip.channels, AudioPlayer.clip.frequency, false);
        combinedClip.SetData(combinedData, 0);
        AudioPlayer.clip = combinedClip;

        //Linea MULTIMEDIA
        audioLength = AudioPlayer.clip.length/2;
        multimediaLine.highLimit = audioLength;
        multimediaLine.minValue = 0;
        multimediaLine.maxValue = audioLength;
        loopStartTime = multimediaLine.minValue;
        loopEndTime = multimediaLine.maxValue;
        lineController.maximum = audioLength;
    }

    //Adelanta la canción
    private void IncreaseTrack()
    {
        if (track != null)
        {
            AudioPlayer.time += moveTime;
        }
    }
    //Retrocede la canción
    private void DecreaseTrack()
    {
        if (track != null)
        {
            if (AudioPlayer.time > moveTime)
            {
                AudioPlayer.time -= moveTime;
            }
            else 
            {
                AudioPlayer.time = 0;
            }
        }

    }
    //Reproduce la canción
    private void PlayTrack()
    {
        if (track != null)
        {
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
    //Crea un clip con silencio para agregarlo a la canción (4 segs)
    private AudioClip CreateSilenceClip(float durationInSeconds)
    {
        int sampleRate = 44100;
        int totalSamples = (int)(durationInSeconds * sampleRate);
        float[] samples = new float[totalSamples];

        AudioClip silenceClip = AudioClip.Create("Silence", totalSamples, 1, sampleRate, false);
        silenceClip.SetData(samples, 0);

        return silenceClip;
    }


    // Cargar el clip de audio de manera async
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

    //Dialog Folder Path
    public string GetFolderPath()
    {
        string folderPath = "";
        var paths = StandaloneFileBrowser.OpenFolderPanel("Choose Directory","",false);
        if (paths.Length >= 1)
        {
            folderPath = paths[0].Replace('\\', '/');
        }
        else 
        {
            alertDialog.CreateDialog("Error", "No Folder Selected");
        }
        return folderPath;
    }

    //Export Button
    public void SelectFolderButton()
    {
        string selectedFolder = GetFolderPath();
        var midiFile = MidiFile.Read(songSelected + "/drums.midi");
        var songNameWithoutExtension = Path.GetFileNameWithoutExtension(songSelected);

        if (selectedFolder != null)
        {
            if (midiFile != null)
            {
                midiFile.Write(selectedFolder+"/"+songNameWithoutExtension+".midi",overwriteFile: true);
            }
            else
            {
                alertDialog.CreateDialog("Error", "No Song Selected.");
            }

        }
        else 
        {
            alertDialog.CreateDialog("Error", "No Folder Selected.");
        }
    }
}


[Serializable]
public class SongData 
{
    public string difficulty;
    public string tempo;
}
