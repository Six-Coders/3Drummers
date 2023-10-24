using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIPerso
{
    public class PersoSetup : MonoBehaviour
    {
        public GameObject Drum;
        public GameObject DrumZurda;
        public GameObject Kick;
        public GameObject Caja;
        public GameObject Hihat;
        public GameObject Crash;
        public GameObject Ride;
        public GameObject Tom;

        public void CreatePersoWindow()
        {
            UIDocument document = GetComponent<UIDocument>();
            VisualElement root = document.rootVisualElement;
            UIPerso uIPerso = new UIPerso();
            root.Add(uIPerso);

            uIPerso.Drum = Drum;
            uIPerso.DrumZurda = DrumZurda;
            uIPerso.Kick = Kick;
            uIPerso.Snare = Caja;
            uIPerso.HiHat = Hihat;
            uIPerso.Crash = Crash;
            uIPerso.Ride = Ride;
            uIPerso.Tom1 = Tom;          

            uIPerso.Accept += () => root.Remove(uIPerso);
        }
    }
}
