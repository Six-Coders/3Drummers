using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitController : MonoBehaviour
{
    public Material material;
    private float currentAlpha = 0f;
    private Color color;
    public float SpeedChange = 4f;
    void Update()
    {
        material.SetColor("_color", color);
    }

    // Update is called once per frame
    void FixedUpdate()
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
    public void SetAlpha(float alpha) 
    {
        currentAlpha = alpha;
    }
    public void SetColor(Color newColor) 
    {
        color = newColor;
    }
}
