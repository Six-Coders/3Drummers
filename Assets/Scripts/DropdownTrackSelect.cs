using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DropdownTrackSelect : MonoBehaviour
{
    public ConvertToMidi convertToMidi;
    public void Start()
    {
        TMPro.TMP_Dropdown dropDown = GetComponent<TMPro.TMP_Dropdown>();
        dropDown.interactable = false;
    }
    public void DropdownItemSelected(int index)
    {
        switch (index) 
        {
            case 0:
                convertToMidi.ActivateButton(false); break;
            case 1:
                Debug.Log("Escogi Drum Track");
                convertToMidi.ActivateButton(true); break;
            case 2: 
                convertToMidi.ActivateButton(false); break;
        }

    }
}
