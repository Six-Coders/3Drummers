using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovimientoNota : MonoBehaviour
{
    public float velocidad = 5000;
    public Vector2 direccion = Vector2.left; // Mueve la nota hacia la izquierda
    public AudioSource audioPlayer;                           

    private void OnEnable()
    {
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
                if (transform.position.x < 810) // Ajusta el valor seg�n tus necesidades.
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}