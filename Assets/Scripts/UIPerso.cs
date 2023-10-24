using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace UIPerso
{
    public class UIPerso : VisualElement
    {
          
        public GameObject Drum { get; set; }
        public GameObject DrumZurda { get; set; }
        public GameObject Kick { get; set; }
        public GameObject Snare { get; set; }

        public GameObject HiHat { get; set; }

        public GameObject Ride { get; set; }

        public GameObject Crash { get; set; }

        public GameObject Tom1 { get; set; }
       


        private bool isZurdo = true;

        private bool isBomboToggled = true;

        private string PlayerPrefsKeyBombo = "BomboState";
        private string PlayerPrefsKeyCaja = "CajaState";
        private string PlayerPrefsKeyHiHat = "HiHatState";
        private string PlayerPrefsKeyRide = "RideState";
        private string PlayerPrefsKeyTom = "TomState";
        private string PlayerPrefsKeyCrash = "CrashState";

        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<UIPerso> { }

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

        private string settingsTitle = "Customize";
        private Label settingsTitleSection = new Label();



        public UIPerso()
        {
            
            styleSheets.Add(Resources.Load<StyleSheet>(styleResource));
            AddToClassList(windowContainer);

            VisualElement window = new VisualElement();
            window.AddToClassList(windowClass);
            hierarchy.Add(window);
            

            float increaseHeight = 80; // Ajusta el valor según tus necesidades

            // Obtén la posición Y actual del elemento
            float currentY = window.resolvedStyle.top;

            // Aumenta la altura del elemento hacia arriba ajustando la posición Y
            window.style.height = window.resolvedStyle.height + increaseHeight;
            window.style.top = currentY - increaseHeight;

            window.MarkDirtyRepaint();

            window.style.width = 850;
            window.style.height = 600;

            VisualElement titleContainerText = new VisualElement();
            titleContainerText.AddToClassList(titleContainer);
            window.Add(titleContainerText);

            settingsTitleSection.text = settingsTitle;
            settingsTitleSection.AddToClassList(titleTextStyle);
            titleContainerText.Add(settingsTitleSection);

            // Crear el contenedor para el Toggle ("Check")
            ScrollView toggleScrollView = new ScrollView(ScrollViewMode.Vertical);
            toggleScrollView.AddToClassList("toggle-scrollview");
            

            VisualElement toggleContainer = new VisualElement();
            toggleScrollView.Add(toggleContainer); // Agrega el contenedor al ScrollView

            window.Add(toggleScrollView);

            Toggle checkToggle = new Toggle("Kick");
            checkToggle.style.marginLeft = 400;
            // Establece el estilo de la fuente y el tamaño de fuente
            checkToggle.style.fontSize = 25; // Elige el tamaño de fuente que desees
            checkToggle.style.color = new StyleColor(Color.white);

            checkToggle.value = PlayerPrefs.GetInt(PlayerPrefsKeyBombo) == 1;
            checkToggle.RegisterCallback<ChangeEvent<bool>>(evt => ToggleCallback(checkToggle, evt.newValue, Kick, PlayerPrefsKeyBombo));
            checkToggle.AddToClassList("centered-toggle");
            toggleContainer.Add(checkToggle);

            Toggle checkToggle2 = new Toggle("Snare");
            checkToggle2.style.marginLeft = 400;
            // Establece el estilo de la fuente y el tamaño de fuente
            checkToggle2.style.fontSize = 25; // Elige el tamaño de fuente que desees
            checkToggle2.style.color = new StyleColor(Color.white);
            checkToggle2.value = PlayerPrefs.GetInt(PlayerPrefsKeyCaja) == 1;
            checkToggle2.RegisterCallback<ChangeEvent<bool>>(evt => ToggleCallback(checkToggle2, evt.newValue, Snare, PlayerPrefsKeyCaja));
            toggleContainer.Add(checkToggle2);
            

        
            Toggle checkToggle3 = new Toggle("Hi-Hat");
            checkToggle3.style.marginLeft = 400;
            // Establece el estilo de la fuente y el tamaño de fuente
            checkToggle3.style.fontSize = 25; // Elige el tamaño de fuente que desees
            checkToggle3.style.color = new StyleColor(Color.white);
            checkToggle3.value = PlayerPrefs.GetInt(PlayerPrefsKeyHiHat) == 1;
            checkToggle3.RegisterCallback<ChangeEvent<bool>>(evt => ToggleCallback(checkToggle3, evt.newValue, HiHat, PlayerPrefsKeyHiHat));
            toggleContainer.Add(checkToggle3);

            Toggle checkToggle5 = new Toggle("Tom");
            checkToggle5.style.marginLeft = 400;
            // Establece el estilo de la fuente y el tamaño de fuente
            checkToggle5.style.fontSize = 25; // Elige el tamaño de fuente que desees
            checkToggle5.style.color = new StyleColor(Color.white);    
            checkToggle5.value = PlayerPrefs.GetInt(PlayerPrefsKeyTom) == 1;
            checkToggle5.RegisterCallback<ChangeEvent<bool>>(evt => ToggleCallback(checkToggle5, evt.newValue, Tom1, PlayerPrefsKeyTom));
            toggleContainer.Add(checkToggle5);


            Toggle checkToggle4 = new Toggle("Ride");
            checkToggle4.style.marginLeft = 400;
            // Establece el estilo de la fuente y el tamaño de fuente
            checkToggle4.style.fontSize = 25; // Elige el tamaño de fuente que desees
            checkToggle4.style.color = new StyleColor(Color.white);
            checkToggle4.value = PlayerPrefs.GetInt(PlayerPrefsKeyRide) == 1;
            checkToggle4.RegisterCallback<ChangeEvent<bool>>(evt => ToggleCallback(checkToggle4, evt.newValue, Ride, PlayerPrefsKeyRide));
            toggleContainer.Add(checkToggle4);



            Toggle checkToggle6 = new Toggle("Crash");
            checkToggle6.style.marginLeft = 400;
            // Establece el estilo de la fuente y el tamaño de fuente
            checkToggle6.style.fontSize = 25; // Elige el tamaño de fuente que desees
            checkToggle6.style.color = new StyleColor(Color.white);
            checkToggle6.value = PlayerPrefs.GetInt(PlayerPrefsKeyCrash) == 1;
            checkToggle6.RegisterCallback<ChangeEvent<bool>>(evt => ToggleCallback(checkToggle6, evt.newValue, Crash, PlayerPrefsKeyCrash));
            toggleContainer.Add(checkToggle6);
            


            // Crear el contenedor para el botón "Cambiar Orientacion"
            VisualElement buttonZurdoContainer = new VisualElement();
            buttonZurdoContainer.AddToClassList(buttonsContainer);
            window.Add(buttonZurdoContainer);

            Button buttonZurdo = new Button() { text = "Change Orientation" };
            buttonZurdo.AddToClassList(buttonStyleClass);
            buttonZurdoContainer.Add(buttonZurdo);
            buttonZurdo.clicked += () => ZurdoOption();

            // Crear el contenedor para el botón "Accept"
            VisualElement buttonAcceptContainer = new VisualElement();
            buttonAcceptContainer.AddToClassList(buttonsContainer);
            window.Add(buttonAcceptContainer);

            Button buttonAccept = new Button() { text = "Accept" };
            buttonAccept.AddToClassList(buttonStyleClass);
            buttonAcceptContainer.Add(buttonAccept);
            buttonAccept.clicked += () => AcceptOption();

            // Agregar la ventana principal a la UI
            Add(window);
        }


        public event Action Accept;
        private void AcceptOption()
        {
            Accept?.Invoke();
            Debug.Log("Funciona");
        }

        private void ZurdoOption()
        {
            isZurdo = !isZurdo; // Alternar el estado

            if (isZurdo)
            {
                Drum.SetActive(false);
                DrumZurda.SetActive(true);
            }
            else
            {
                Drum.SetActive(true);
                DrumZurda.SetActive(false);
            }

        }

        private void ToggleCallback(Toggle toggle, bool newValue, GameObject Parte, string playerPrefsKey)

        {
            string nombreParte = Parte.name;
            string nombreParteZurda = nombreParte;
            Transform drumZurda = DrumZurda.transform; // Asegúrate de que DrumZurda sea un GameObject accesible

            Transform parteZurda = drumZurda.Find(nombreParteZurda);

            Debug.Log(parteZurda);
            if (newValue)
            {
             
                Parte.SetActive(true);
                
                if (parteZurda != null)
                {
                    
                    parteZurda.gameObject.SetActive(true);
                }

            }
            else
            {
                

                Parte.SetActive(false);
                
                if (parteZurda != null)
                {
                    
                    parteZurda.gameObject.SetActive(false);
                }


            }
            PlayerPrefs.SetInt(playerPrefsKey, newValue ? 1 : 0);
            PlayerPrefs.Save();
        }

    }
}