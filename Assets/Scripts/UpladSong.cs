using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using SFB;
using TMPro;
using System.Text.RegularExpressions;

public class UpladSong : MonoBehaviour
{
    public NewSongButtonController controller;
    public GameObject fileText;
    public ConvertToMidi convertToMidi;
    void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(ImportSong);
    }

    public void ImportSong() 
    {
        string [] paths = StandaloneFileBrowser.OpenFilePanel("Open File","","",false);
        if (paths.Length > 0) 
        {
            //Verificar el directorio
            Match match = Regex.Match(paths[0], pattern: @".+\\(.*)\.(?:mp3|wav)$");
            if (match.Success)
            {
                string fileName = match.Groups[1].Value;
                controller.SetAudioFilePath(paths[0].Replace('\\','/'));
                controller.ActivateButton(true);
                convertToMidi.SetDrumsPath(fileName);
                fileText.GetComponentInChildren<TextMeshProUGUI>().text = fileName;
            }
            else 
            {
                Debug.LogError("The File is not in the right format.");
                ResetButtons();

            }
        }
        else
        {
            Debug.LogError("No File selected.");
            ResetButtons();

        }
    }
    private void ResetButtons() 
    {
        controller.ActivateButton(false);
        convertToMidi.ActivateButton(false);
        convertToMidi.ResetSongDirectory();
        fileText.GetComponentInChildren<TextMeshProUGUI>().text = "No File Selected.";
    }
}
