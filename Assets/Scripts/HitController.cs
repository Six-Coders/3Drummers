using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitController : MonoBehaviour
{
    public Material material;
    public AudioController audioController;
    private float currentAlpha = 0f;
    private Color color;
    private float SpeedChange = 2f;
    public ParticleSystem particleSystem;
    private bool firstHit = false;
    private float rateMultiplier = 100.0f;

    private void Start()
    {
        if (particleSystem != null) 
        {
            particleSystem.Stop();
        }
    }
    void Update()
    {
        material.SetColor("_color", color);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (audioController != null) 
        {
            if (audioController.AudioIsPlaying()) 
            {
                if (currentAlpha > 0)
                {
                    currentAlpha -= Time.deltaTime * SpeedChange;
                    if (!firstHit) 
                    {
                        if (particleSystem != null) 
                        {
                            particleSystem.Emit((int)(currentAlpha * rateMultiplier * 10));
                            particleSystem.Stop();
                        }
                    }
                    firstHit = true;
                }
                else
                {
                    currentAlpha = 0f;
                    firstHit = false;
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
