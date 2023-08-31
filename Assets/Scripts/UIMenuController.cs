using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class UIMenuController : MonoBehaviour
{
    public PythonServerAPI pythonServerAPI;
    public AudioSource AudioPlayer;
    
    private string audioFilePath;
    private string libPath;
    private string songSelected;
    private string trackSelected;

    private List<string> songsLibrary = new List<string>();
    private List<string> songDirectories = new List<string>();
    private List<string> requiredFiles = new List<string>() { "drums.wav", "no_drums.wav", "drums.midi", "data.json" };
    private List<string> trackList = new List<string>() { "drums", "no_drums" };

    private AudioClip track = null;

    VisualElement root;
    private VisualElement UIblocker;
    private VisualElement dialogBackground;
    private VisualElement dialogPanel;

    //Buttons Init
    Button buttonUploadSong;
    Button buttonSplitTracks;
    Button buttonTransformToMidi;
    Button buttonMediaPlay;
    Button buttonMediaStop;
    Button buttonAcceptDialog;

    //Dropdowns
    DropdownField dropdownLibrary;
    DropdownField dropdownTrack;

    //ProgressBar
    ProgressBar progressBar;
    private float speedBar = 0.1f;

    //Labels
    private Label fileName;
    private Label difficulty;
    private Label tempo;
    private Label indicatorText;
    private Label textDialog;

    private void Start()
    {
        string LibraryName = "/Lib";
        libPath = Application.persistentDataPath + LibraryName;
        SearchSongs();

        dropdownTrack.choices.Clear();
        foreach (string track in trackList) 
        { 
            dropdownTrack.choices.Add(track);
        }
        trackSelected = trackList[0];
        dropdownTrack.value = trackSelected;
        dropdownTrack.label = trackSelected;

        dialogBackground.visible = false;
        dialogPanel.visible = false;
        dialogBackground.SetEnabled(false);
        dialogPanel.SetEnabled(false);

    }
    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        UIblocker = root.Q<VisualElement>("block");
        dialogBackground = root.Q<VisualElement>("dialogBackground");

        dialogPanel = root.Q<VisualElement>("dialogPanel");

        //Buttons:
        buttonUploadSong = root.Q<Button>("buttonUploadSong");
        buttonSplitTracks = root.Q<Button>("buttonSplitTracks");
        buttonTransformToMidi = root.Q<Button>("buttonTransform");
        buttonMediaPlay = root.Q<Button>("buttonPlay");
        buttonMediaStop = root.Q<Button>("buttonStop");
        buttonAcceptDialog = root.Q<Button>("buttonAccept");

        buttonUploadSong.clicked += () => UploadAudiofile();
        buttonSplitTracks.clicked += () => SplitTracks();
        buttonTransformToMidi.clicked += () => TransformToMidi();
        buttonMediaPlay.clicked += () => PlayTrack();
        buttonMediaStop.clicked += () => StopTrack();
        buttonAcceptDialog.clicked += () => AcceptDialog();

        //Deactivate Buttons!
        buttonSplitTracks.SetEnabled(false);

        //Text Labels
        fileName = root.Q<Label>("textFilename");
        difficulty = root.Q<Label>("textDifficulty");
        tempo = root.Q<Label>("textTempo");
        textDialog = root.Q<Label>("textDialog");

        //Dropdowns
        dropdownLibrary = root.Q<DropdownField>("dropdownSongs");
        dropdownLibrary.RegisterValueChangedCallback(evt => UpdateDropdownValue(dropdownLibrary.index));
        dropdownTrack = root.Q<DropdownField>("dropdownTrack");
        dropdownTrack.RegisterValueChangedCallback(evt => UpdateTrackValue(dropdownTrack.index));

        //UIBlocker
        indicatorText = root.Q<Label>("status");
        progressBar = root.Q < ProgressBar > ("indicator");
    }
    private void AcceptDialog() 
    {
        dialogBackground.SetEnabled(false);
        dialogPanel.SetEnabled(false);
        dialogBackground.visible = false;
        dialogPanel.visible = false;
    }
    public void ShowDialog(string text) 
    {
        dialogBackground.SetEnabled(true);
        dialogPanel.SetEnabled(true);
        dialogBackground.visible = true;
        dialogPanel.visible = true;
        textDialog.text = text;
    }

    private void FixedUpdate()
    {
        if (progressBar != null) 
        {
            if (progressBar.value < 100) 
            {
                progressBar.value += speedBar;
            }
        }
    }

    public void SetIndicatorText(string text) 
    { 
        indicatorText.text = text;
    }
    private void StopTrack() 
    {
        if (AudioPlayer.isPlaying) 
        {
            AudioPlayer.Stop();
        }
    }

    public void SetProgresBarSpeed(int speed) 
    {
        speedBar = speed;
    }
    public void BlockUI(bool activate, string nameIndicator="System is loading...")
    {
        if (activate)
        {
            progressBar.value = 0;
            UIblocker.SetEnabled(activate);
            UIblocker.visible = activate;
            indicatorText.text = nameIndicator;
        }
        else 
        {
            indicatorText.text = "Done.";
            progressBar.value = 100;
            Invoke("ResetUI", 3);
        }
    }
    private void ResetUI() 
    {
        UIblocker.SetEnabled(false);
        UIblocker.visible = false;
    }
    public void FinishProgress() 
    {
        progressBar.value = 100;
        indicatorText.text = "Done.";
    }
    private async void TransformToMidi() 
    {
        if (trackSelected != "drums") 
        {
            ShowDialog("Drum track is NOT selected.");
            return;
        }
        if (songSelected != null)
        {
            string songDir = songSelected.Replace("\\", "/");
            string modelDirectorie = Application.streamingAssetsPath.Replace('\\', '/') + "/PythonServer/onsets_frames_transcription/model_checkpoint/model.ckpt-569400";
            Dictionary<string, string> parameter = new Dictionary<string, string>()
            {
                {"parameter1",songDir},
                { "parameter2",modelDirectorie}
            };
            speedBar = 0.2f;
            BlockUI(true,"Creating the MIDI");
            await pythonServerAPI.CreateCommand("transform_to_midi", parameter);
            BlockUI(false);
            UpdateInfoSong();
        }
        else 
        {
            return;
        }
    }
    private void UpdateInfoSong() 
    {
        Debug.Log("Song Selection: " + songSelected);
        string jsonPath = Path.Combine(songSelected, "data.json");
        string jsonContent = File.ReadAllText(jsonPath);
        SongData songData = JsonUtility.FromJson<SongData>(jsonContent);
        difficulty.text = "Difficulty: " + songData.difficulty;
        tempo.text = "Tempo: " + songData.tempo + " BPM";
    }
    private void UpdateTrackValue(int index) 
    {
        trackSelected = trackList[index];

        dropdownTrack.label = trackSelected;
        dropdownTrack.value = trackSelected;
    }
    private void UpdateDropdownValue(int index) 
    {
        string songName = dropdownLibrary.value;
        dropdownLibrary.label = songName;
        songSelected = songDirectories[index];
        SetDifficultyAndTempo();

    }
    private void SetDifficultyAndTempo() 
    {
        if (CheckFiles(songSelected))
        {
            string jsonPath = Path.Combine(songSelected, "data.json");
            string jsonContent = File.ReadAllText(jsonPath);
            SongData songData = JsonUtility.FromJson<SongData>(jsonContent);
            difficulty.text = "Difficulty: " + songData.difficulty;
            tempo.text = "Tempo: " + songData.tempo + " BPM";
        }
        else
        {
            difficulty.text = "Not Defined";
            tempo.text = "Not Defined";
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
            Match match = Regex.Match(paths[0], pattern: @".+\\(.*)\.(?:mp3|wav)$");
            if (match.Success)
            {
                string audiofileName = match.Groups[1].Value;
                audiofileName = paths[0].Replace('\\', '/');
                audioFilePath = audiofileName;
                fileName.text = "File: "+Path.GetFileNameWithoutExtension(audiofileName);
                buttonSplitTracks.SetEnabled(true);
            }
            else
            {
                ShowDialog("The File is not in the right format.");
            }
        }
        else
        {
            ShowDialog("No File Selected.");
        }

    }
    private void SplitTracks() 
    {
        if (audioFilePath != null)
        {
            ProcessSong();
        }
        else 
        {
            ShowDialog("No Song Selected.");
            return;
        }
    }
    private void UpdateSearchSongs() 
    {
        dropdownLibrary.choices.Clear();

        songDirectories.Clear();
        songsLibrary.Clear();
        int index = -1;
        string[] subdirectories = Directory.GetDirectories(libPath);
        string audioFileSong = Path.GetFileNameWithoutExtension(audioFilePath);
        foreach (string subdirectory in subdirectories)
        {
            string subDirName = Path.GetFileName(subdirectory);
            songsLibrary.Add(subDirName);
            songDirectories.Add(subdirectory);
            dropdownLibrary.choices.Add(subDirName);
            if (audioFileSong == subDirName) 
            {
                index = songsLibrary.IndexOf(subDirName);
            }
        }
        songSelected = songDirectories[index];
        string songName = songsLibrary[index];
        dropdownLibrary.value = songName;        
    }
    private async void ProcessSong() 
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>()
        {
            {"parameter1",audioFilePath},
            { "parameter2",libPath}
        };
        speedBar = 0.01f;
        BlockUI(true,"The song is separating into tracks...");
        await pythonServerAPI.CreateCommand("separate_tracks", parameters);
        BlockUI(false);
        UpdateSearchSongs();
    }
    private void SearchSongs() 
    {
        dropdownLibrary.choices.Clear();

        songDirectories.Clear();
        songsLibrary.Clear();

        string[] subdirectories = Directory.GetDirectories(libPath);
        foreach (string subdirectory in subdirectories) 
        {
            string subDirName = Path.GetFileName(subdirectory);
            songsLibrary.Add(subDirName);
            songDirectories.Add(subdirectory);
            dropdownLibrary.choices.Add(subDirName);
        }
        songSelected = songDirectories[0];
        string firstSong = songsLibrary[0];

        dropdownLibrary.value = firstSong;
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
            ShowDialog("No Song Selected.");
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
                    ShowDialog("Unexpected Error. Can't load Audiofile.");
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
