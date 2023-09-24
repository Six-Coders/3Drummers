using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

namespace UISettings 
{
    public class UISettings : VisualElement
    {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<UISettings> { }

        private const string styleResource = "SettingsStyleSheet";
        private const string windowContainer = "windowContainer";
        private const string windowClass = "window";
        private const string titleContainer = "title-Container";
        private const string titleTextStyle = "title-Text";
        private const string slidersContainer = "sliders-Container";
        private const string sliderClass = "slider-Volumen";
        private const string buttonsContainer = "buttons-Container";
        private const string normalTextClass = "normal-Text";
        private const string buttonStyleClass = "button";

        private string settingsTitle = "Settings";
        private Label settingsTitleSection = new Label();
        private Label generalVolumeLabel = new Label();
        private Label drumtrackVolumeLabel = new Label();
        private Label noDrumTrackVolumeLabel = new Label();

        private Slider generalVolume = new Slider();
        private Slider drumTrackVolume = new Slider();
        private Slider noDrumTrackVolume = new Slider();

        private string generalVolumeName = "General Volume";
        private string drumTrackVolumeName = "Drum Track Volume";
        private string noDrumTrackVolumeName = "Drumless Track Volume";
        private const float minVolume = 0.0000000000000000000000001f;
        public AudioMixer mixer = Resources.Load<AudioMixer>("Master");
        

        public UISettings() 
        {
            styleSheets.Add(Resources.Load<StyleSheet>(styleResource));
            AddToClassList(windowContainer);

            VisualElement window = new VisualElement();
            window.AddToClassList(windowClass);
            hierarchy.Add(window);

            VisualElement titleContainerText = new VisualElement();
            titleContainerText.AddToClassList(titleContainer);
            window.Add(titleContainerText);
            
            settingsTitleSection.text = settingsTitle;
            settingsTitleSection.AddToClassList(titleTextStyle);
            titleContainerText.Add(settingsTitleSection);

            VisualElement sliderSectionGeneralVolume = new VisualElement();
            sliderSectionGeneralVolume.AddToClassList(slidersContainer);
            window.Add(sliderSectionGeneralVolume);

            VisualElement sliderSectionDrumVolume = new VisualElement();
            sliderSectionDrumVolume.AddToClassList(slidersContainer);
            window.Add(sliderSectionDrumVolume);

            VisualElement sliderSectionNoDrumVolume = new VisualElement();
            sliderSectionNoDrumVolume.AddToClassList(slidersContainer);
            window.Add(sliderSectionNoDrumVolume);

            generalVolumeLabel.AddToClassList(normalTextClass);
            drumtrackVolumeLabel.AddToClassList(normalTextClass);
            noDrumTrackVolumeLabel.AddToClassList(normalTextClass);

            generalVolumeLabel.text = generalVolumeName;
            drumtrackVolumeLabel.text = drumTrackVolumeName;
            noDrumTrackVolumeLabel.text = noDrumTrackVolumeName;

            sliderSectionGeneralVolume.Add(generalVolumeLabel);
            sliderSectionDrumVolume.Add(drumtrackVolumeLabel);
            sliderSectionNoDrumVolume.Add(noDrumTrackVolumeLabel);

            
            generalVolume.AddToClassList(sliderClass);
            generalVolume.lowValue = minVolume;
            generalVolume.highValue = 1f;

            float generalVolumeOut;
            if (mixer.GetFloat("masterVolume", out generalVolumeOut)) 
            {
                float value = (float)Math.Pow(10, generalVolumeOut / 20f);
                generalVolume.value = value;
            }
            
            drumTrackVolume.AddToClassList(sliderClass);
            drumTrackVolume.lowValue = minVolume;
            drumTrackVolume.highValue = 1f;

            float drumTrackVolumeOut;
            if (mixer.GetFloat("drumTrackVolume", out drumTrackVolumeOut)) 
            {
                float value = (float)Math.Pow(10, drumTrackVolumeOut / 20f);
                drumTrackVolume.value = value;
            }
            
            noDrumTrackVolume.AddToClassList(sliderClass);
            noDrumTrackVolume.lowValue = minVolume;
            noDrumTrackVolume.highValue = 1f;

            float noDrumTrackVolumeOut;
            if (mixer.GetFloat("drumlessTrackVolume", out noDrumTrackVolumeOut)) 
            {
                float value = (float)Math.Pow(10, noDrumTrackVolumeOut / 20f);
                noDrumTrackVolume.value = value;
            }

            sliderSectionGeneralVolume.Add(generalVolume);
            sliderSectionDrumVolume.Add(drumTrackVolume);
            sliderSectionNoDrumVolume.Add(noDrumTrackVolume);

            VisualElement buttonContainer = new VisualElement();
            buttonContainer.AddToClassList(buttonsContainer);
            window.Add(buttonContainer);


            Button buttonAccept = new Button() { text = "Accept"};
            buttonAccept.AddToClassList(buttonStyleClass);
            buttonContainer.Add(buttonAccept);
            buttonAccept.clicked += () => AcceptOption();

            generalVolume.RegisterValueChangedCallback(v =>
            {
                float volume = (float)Math.Log10(v.newValue)*20;
                mixer.SetFloat("masterVolume", volume);
            });
            drumTrackVolume.RegisterValueChangedCallback(v =>
            {
                float volume = (float) Math.Log10(v.newValue)*20;
                mixer.SetFloat("drumTrackVolume", volume);
            });
            noDrumTrackVolume.RegisterValueChangedCallback(v =>
            {
                float volume = (float)Math.Log10(v.newValue)*20;
                mixer.SetFloat("drumlessTrackVolume", volume);
            });
            
        }
        public event Action Accept;

        private void AcceptOption() 
        {
            Accept?.Invoke();
        }
    }
}

