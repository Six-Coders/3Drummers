using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovimientoNota : MonoBehaviour
{
    public float velocidad = 5000;
    public Vector2 direccion = Vector2.left; // Mueve la nota hacia la izquierda
    public AudioSource audioPlayer;
    public Image image;
    private void OnEnable()
    {
        image = GetComponent<Image>();
        GameObject obj = GameObject.FindWithTag("AudioPlayer");
        audioPlayer = obj.GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        if ( audioPlayer != null)
        {
            if (audioPlayer.isPlaying)
            {
                transform.Translate(direccion * velocidad * Time.deltaTime * 250f);
                if (transform.position.x < 810 || transform.position.x > Screen.width) // Ajusta el valor según tus necesidades.
                {
                    image.enabled = false;
                }
                else 
                {
                    image.enabled = true;
                }
            }
        }
    }
}