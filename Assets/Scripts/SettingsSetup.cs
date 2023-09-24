using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

namespace UISettings 
{
    public class SettingsSetup : MonoBehaviour
    {
        public AudioMixer mixer;

        public void CreateSettingsWindow()
        {
            UIDocument document = GetComponent<UIDocument>();
            VisualElement root = document.rootVisualElement;
            UISettings uISettings = new UISettings();
            root.Add(uISettings);

            uISettings.Accept += () => root.Remove(uISettings);
        }
    }
}

