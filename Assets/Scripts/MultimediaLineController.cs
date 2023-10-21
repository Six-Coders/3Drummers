using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultimediaLineController : MonoBehaviour
{
    public AudioSource audioSource;
    public float maximum = 100;
    private float current = 0;

    public Image mask;
    void Update()
    {
        current = audioSource.time;
        GetCurrentFill();
    }

    void GetCurrentFill() 
    {
        float fillAmount = current / maximum;
        mask.fillAmount = fillAmount;
    }
}
