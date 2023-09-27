using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitController : MonoBehaviour
{
    public Material material;
    private float currentAlpha = 0f;
    private Color color;
    private float SpeedChange = 2f;
    private AudioSource audioPlayer;
    private void OnEnable()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("AudioPlayer");
        audioPlayer = obj.GetComponent<AudioSource>();
    }
    void Update()
    {
        material.SetColor("_color", color);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (audioPlayer != null) 
        {
            if (audioPlayer.isPlaying) 
            {
                if (currentAlpha > 0)
                {
                    currentAlpha -= Time.deltaTime * SpeedChange;
                }
                else
                {
                    currentAlpha = 0f;
                }
                material.SetFloat("_transparency", currentAlpha);
            }
        }
    }
    public void SetAlpha(float alpha) 
    {
        if (alpha < 0.5f) 
        {
            alpha = 0.5f;
        }
        currentAlpha = alpha;
    }
    public void SetColor(Color newColor) 
    {
        color = newColor;
    }
}
