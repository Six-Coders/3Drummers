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
    [SerializeField] public DrumController drumController;
    [SerializeField] public List<SongData> songInfo = new List<SongData>();  // List to store song data
    [SerializeField] public AlertDialog.Popup_Setup alertDialog; //Alert system!
    [SerializeField] public UISettings.SettingsSetup settingsSetup; //Settings system!
    [SerializeField] public DrumMIDIController drumMIDIController;
    [SerializeField] public UIPerso.PersoSetup persoSetup;
    [SerializeField] public GameObject UIProcessIndicator;
    [SerializeField] public GameObject notesContainer;
    [SerializeField] public AudioController audioController;

    [SerializeField] private Button exercisesButton;
    [SerializeField] private Button songsButton;
    

    public Sprite iconEnable;
    public Sprite iconDisable;

    public AudioMixerGroup audioMixerGroupDrumTrack;
    public AudioMixerGroup audioMixerGroupNoDrumsTrack;

    // Paths and variables for managing audio files and libraries
    private string audioFilePath;
    private string libPath;
    private string unityPath;
    private string dataPersistentPath;
    private string exercisesPath;
    private float moveTime = 3;
    private int songSelectedIndex = 0;

    private string songSelected; // Currently selected song
    private string trackSelected; // Currently selected track (drums or no_drums)

    [SerializeField] private List<string> songsLibrary = new List<string>();  // List of song names in the library
    [SerializeField] private List<string> songDirectories = new List<string>();  // List of song directories
    [SerializeField] private List<string> requiredFiles = new List<string>() { "drums.wav", "no_drums.wav", "drums.midi", "data.json" };  // Required files for each song
    public List<string> trackList = new List<string>() { "drums", "no drums", "full track"};  // List of available tracks

    private AudioClip track = null;  // Reference to the currently selected audio track
    private AudioClip[] tracks;

    public bool isIntensitySet = true;

    // Unity UI elements
    [SerializeField] private VisualElement root;
    [SerializeField] private DropdownField trackDropdown;
    [SerializeField] private Button uploadButton;
    [SerializeField] private Button processButton;
    [SerializeField] private Button testButton;
    [SerializeField] private Button persoButton;
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
    [SerializeField] private MinMaxSlider multimediaLine;
    [SerializeField] private ProgressBar multimediaProgress;

    public Sprite playIcon;
    public Sprite pauseIcon;

    private StyleBackground playIconBackground;
    private StyleBackground pauseIconBackground;

    // Colors!
    private Color selectedSongColor = new Color32(130, 127, 160, 255);
    private Color unselectedSongColor = new Color32(29, 29, 29, 255);
    private Color selectedColor = new Color32(113,113,113,255);
    private Color unSelectedColor = new Color32(22, 22, 22,255);

    // Labels
    private Label fileName;

    //Multimedia System
    private float loopStartTime;
    private float loopEndTime;
    private float audioLength;

    bool isRecording = false;

    //GameObject
    public GameObject DrumZurda;
    private void Start()
    {
        string LibraryName = "/Lib";
        string exercisesName = "/Exercises";
        exercisesPath = Application.persistentDataPath + exercisesName;
        unityPath = Application.streamingAssetsPath;
        dataPersistentPath = Application.persistentDataPath;
        libPath = Application.persistentDataPath + LibraryName;
        playIconBackground = new StyleBackground(playIcon);
        pauseIconBackground = new StyleBackground(pauseIcon);

        RefreshLibrary(libPath);  // Initialize the song library
        songsButton.style.backgroundColor = selectedColor;
        exercisesButton.style.backgroundColor = unSelectedColor;
        multimediaLine.lowLimit = 0;
        DrumZurda.SetActive(false);
        UIProcessIndicator.SetActive(false);
        intensityButton.style.backgroundColor = selectedColor;
    }
    private void Update()
    {
        if (audioController.AudioIsPlaying())
        {
            playIconElement.style.backgroundImage = pauseIconBackground;
            exportMIDIButton.SetEnabled(false);
        }
        else
        {
            playIconElement.style.backgroundImage = playIconBackground;
            exportMIDIButton.SetEnabled(true);
        }

        if (audioController.audioPlayerTime > loopEndTime) 
        {
            LoopTrack();
            if (isRecording)
            {
                StopRecording();
            }
        }
        testButton.SetEnabled(drumMIDIController.IsConnected());
        multimediaProgress.value = audioController.audioPlayerTime;
    }

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        

        // Get references to UI elements and set up event handlers
        persoButton = root.Q<Button>("buttonPerso");
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
        exercisesButton = root.Q<Button>("exercisesButton");
        songsButton = root.Q<Button>("songsButton");

        fileName = root.Q<Label>("textFilename");
        playIconElement = root.Q<VisualElement>("iconPlay");
        playIconElement.style.backgroundImage = playIconBackground;

        multimediaLine = root.Q<MinMaxSlider>("multimediaLine");
        multimediaProgress = root.Q<ProgressBar>("multimediaBar");
        libraryListView = root.Q<ListView>("libraryListView");
        trackDropdown = root.Q<DropdownField>("trackSelection");
        trackDropdown.choices.Clear();
        trackDropdown.choices = trackList;

        trackDropdown.index = 0;
        trackSelected = trackDropdown.choices[0];
        trackDropdown.RegisterValueChangedCallback(async v => {
            trackSelected = v.newValue;
            StopTrack();
            if (trackSelected == "drums")
            {
                audioController.PauseTrack(1);
            }
            else if (trackSelected == "no drums") 
            {
                audioController.PauseTrack(0);
            }
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
            GameObject[] notas = GameObject.FindGameObjectsWithTag("Note");
            foreach (var n in notas)
            {
                Destroy(n);
            }
            if (isIntensitySet)
            {
                isIntensitySet = false;
                intensityButton.style.backgroundColor = unselectedSongColor;
                StopTrack();
            }
            else 
            {
                isIntensitySet = true;
                intensityButton.style.backgroundColor = selectedColor;
                StopTrack();
            }
            drumController.CreateAllNotes();
        };
        persoButton.clicked += () => {
            persoSetup.CreatePersoWindow();
        };
        exercisesButton.clicked += () => {

            RefreshLibrary(exercisesPath);
            processButton.SetEnabled(false);
            uploadButton.SetEnabled(false);
            trackDropdown.SetEnabled(false);
            exercisesButton.style.backgroundColor = selectedColor;
            songsButton.style.backgroundColor = unselectedSongColor;

        };

        songsButton.clicked += () => {

            RefreshLibrary(libPath);
            processButton.SetEnabled(true);
            uploadButton.SetEnabled(true);
            trackDropdown.SetEnabled(true);
            songsButton.style.backgroundColor = selectedColor;
            exercisesButton.style.backgroundColor = unSelectedColor;
        };

        // Set up event handler for selecting a song in the library
        libraryListView.itemsChosen += async (evt) =>
        {
            
            libraryListView.SetEnabled(false);
            mediaPlayButton.SetEnabled(false);
            songSelected = songDirectories[libraryListView.selectedIndex];
            if (audioController.AudioIsPlaying()) 
            {
                StopTrack();
                drumController.SetMidiPathFile(songSelected+ "/drums.midi");

            }
            
            tracks = await LoadAudioClip();  // Load the selected audio clip
            if (tracks == null) 
            {
                return;
            }
            audioController.SetAudioClip(tracks[0], tracks[1]);
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
            StopTrack();
            MoveNoteContainer();
            audioController.ResetPlayerTimer(loopStartTime);
        });
    }

    //Función para "desplazar" las notas segun el startLoop
    private void MoveNoteContainer()
    {
        notesContainer.transform.localPosition = Vector3.left * ((2f * loopStartTime * 250f));
    }

    //Funcion para grabar retroalimentación
    private void StartRecordMidi() 
    {
        if (!isRecording)
        {
            mediaPlayButton.SetEnabled(false);
            mediaDecreaseButton.SetEnabled(false);
            mediaIncreaseButton.SetEnabled(false);
            mediaStopButton.SetEnabled(false);
            isRecording = true;
            audioController.StopTrack();
            audioController.PlayTrack();
            if (trackSelected == "drums")
            {
                audioController.PauseTrack(1);
            }
            else if (trackSelected == "no drums")
            {
                audioController.PauseTrack(0);
            }
            drumMIDIController.Recording(loopStartTime,loopEndTime);
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
        audioController.StopTrack();
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
                UIProcessIndicator.SetActive(true);
                await Task.Run(() => { ProcessSong(); }) ;  // Start a background task to process the song
                bussy = false;
                processButton.SetEnabled(true);
                alertDialog.CreateDialog("The file is processed", "The song is ready and loaded in the library.");
                UIProcessIndicator.SetActive(false);
                RefreshLibrary(libPath);
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
    private async void RefreshLibrary(string path) 
    {
        SearchSongs(path);
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
            tracks = await LoadAudioClip();
            audioController.SetAudioClip(tracks[0], tracks[1]);
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
        if (audioController.AudioIsPlaying()) 
        {
            audioController.StopTrack();
            audioController.audioPlayerTime = 0f;
            multimediaLine.minValue = 0;
            multimediaLine.maxValue = audioLength;
            loopStartTime = multimediaLine.minValue;
            loopEndTime = multimediaLine.maxValue;
            notesContainer.transform.localPosition = Vector3.zero;
        }
    }

    private void LoopTrack() 
    {
        audioController.StopTrack();
        audioController.audioPlayerTime = loopStartTime;
        MoveNoteContainer();
        Debug.Log("Pase por aqui...");
        audioController.PlayTrack();
        if (trackSelected == "drums")
        {
            audioController.PauseTrack(1);
        }
        else if (trackSelected == "no drums")
        {
            audioController.PauseTrack(0);
        }
        else 
        {
            audioController.PauseTrack(2);
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
                fileName.text = "File: " + Path.GetFileName(audiofileName);
            }
            else
            {
                processButton.SetEnabled(false);
                alertDialog.CreateDialog("Import Error", "The file format is invalid.");
                fileName.text = "File:";
            }
        }
        else
        {
            processButton.SetEnabled(false);
            alertDialog.CreateDialog("Import Error", "Not file selected.");
            fileName.text = "File";
        }
    }
    //Función para buscar canciones en un directorio (el default)
    private void SearchSongs(string path) 
    {
        songDirectories.Clear();
        songsLibrary.Clear();
        songInfo.Clear();

        string[] subdirectories = Directory.GetDirectories(path);
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
        float[] originalData = new float[audioController.drumTrackPlayer.clip.samples * audioController.drumTrackPlayer.clip.channels];
        audioController.drumTrackPlayer.clip.GetData(originalData, 0);
        float[] combinedData = new float[silenceData.Length + originalData.Length];
        Array.Copy(silenceData, combinedData, silenceData.Length);
        Array.Copy(originalData, 0, combinedData, silenceData.Length, originalData.Length);
        AudioClip combinedClip = AudioClip.Create("fixedDrumTrack", combinedData.Length, audioController.drumTrackPlayer.clip.channels, audioController.drumTrackPlayer.clip.frequency, false);
        combinedClip.SetData(combinedData, 0);
        //AudioPlayer.clip = combinedClip;

        float[] originalData2 = new float[audioController.drumlessTrackPlayer.clip.samples * audioController.drumlessTrackPlayer.clip.channels];
        audioController.drumlessTrackPlayer.clip.GetData(originalData2, 0);
        float[] combinedData2 = new float[silenceData.Length + originalData2.Length];
        Array.Copy(silenceData, combinedData2, silenceData.Length);
        Array.Copy(originalData2, 0, combinedData2, silenceData.Length, originalData2.Length);
        AudioClip combinedClip2 = AudioClip.Create("fixedDrumlessTrack", combinedData2.Length, audioController.drumlessTrackPlayer.clip.channels, audioController.drumlessTrackPlayer.clip.frequency, false);
        combinedClip2.SetData(combinedData2, 0);

        audioController.SetAudioClip(combinedClip, combinedClip2);

        //Linea MULTIMEDIA
        audioLength = audioController.drumTrackPlayer.clip.length/2;
        multimediaLine.highLimit = audioLength;
        multimediaLine.minValue = 0;
        multimediaLine.maxValue = audioLength;
        loopStartTime = multimediaLine.minValue;
        loopEndTime = multimediaLine.maxValue;
        multimediaProgress.highValue = audioLength;
        multimediaProgress.lowValue = 0;
    }

    //Adelanta la canción (MODIFICAR ESTA WEAAAA CTMMM)
    private void IncreaseTrack()
    {
        if (audioController.audioPlayerTime < loopEndTime)
        {
            audioController.SetAudioPlayerTime(moveTime);
            notesContainer.transform.Translate(Vector3.left * moveTime * 2f * 250f);
        }
    }
    //Retrocede la canción (MODIFICAR ESTA WEAAA CTMMMM)
    private void DecreaseTrack()
    {
        if (track != null)
        {
            if (audioController.audioPlayerTime > moveTime)
            {
                audioController.SetAudioPlayerTime(-moveTime);
                notesContainer.transform.Translate(Vector3.right * moveTime * 2f * 250f);
            }
            else 
            {
                audioController.ResetPlayerTime();
            }
        }

    }
    //Reproduce la canción
    private void PlayTrack()
    {
        if (tracks != null)
        {
            if (audioController.AudioIsPlaying())
            {
                audioController.PauseTrack(2);
            }
            else 
            {
                
                audioController.PlayTrack();
                if (trackSelected == "drums")
                {
                    audioController.PauseTrack(1);
                }
                else if (trackSelected == "no drums") 
                {
                    audioController.PauseTrack(0);
                }
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
    async Task<AudioClip[]> LoadAudioClip()
    {

        string drumTrackDir = null;
        string drumlessTrackDir = null;

        if (songSelected != null)
        {
            drumTrackDir = songSelected + "/" + "drums" + ".wav";
            drumlessTrackDir = songSelected + "/" + "no_drums" + ".wav";
             
        }
        else 
        {
            return null;
        }
        string drumDirAuxPath = drumTrackDir.Replace('\\', '/');
        string drumDir = drumDirAuxPath.Replace("/", "//");

        string drumlessDirAuxPath = drumlessTrackDir.Replace('\\', '/');
        string drumlessDir = drumlessDirAuxPath.Replace("/", "//");

        AudioClip audioClip1 = null;
        AudioClip audioClip2 = null;

        if (drumDir != null)
        {
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(drumDir, AudioType.WAV))
            {
                uwr.SendWebRequest();
                try
                {
                    while (!uwr.isDone) await Task.Delay(5);
                    if (uwr.isNetworkError || uwr.isHttpError) Debug.Log($"{uwr.error}");
                    else
                    {
                        audioClip1 = DownloadHandlerAudioClip.GetContent(uwr);
                    }
                }
                catch (Exception err)
                {
                    // Handle any exceptions
                }
            };
            audioClip1.name = drumDir;
        }
        else
        {
            return null;
        }

        if (drumlessDir != null)
        {
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(drumlessDir, AudioType.WAV))
            {
                uwr.SendWebRequest();
                try
                {
                    while (!uwr.isDone) await Task.Delay(5);
                    if (uwr.isNetworkError || uwr.isHttpError) Debug.Log($"{uwr.error}");
                    else
                    {
                        audioClip2 = DownloadHandlerAudioClip.GetContent(uwr);
                    }
                }
                catch (Exception err)
                {
                    // Handle any exceptions
                }
            };
            audioClip2.name = drumlessDir;
        }
        else
        {
            return null;
        }
        AudioClip[] clips = { audioClip1, audioClip2 };
        return clips;
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
