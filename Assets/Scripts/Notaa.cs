using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoNota : MonoBehaviour
{
    public float velocidad = 5000;
    public Vector2 direccion = Vector2.left; // Mueve la nota hacia la izquierda
                                             // Puedes ajustar la direcci�n seg�n tus necesidades.

    void FixedUpdate()
    {
        // Mueve la nota en la direcci�n especificada.
        transform.Translate(direccion * velocidad * Time.deltaTime *250f);

        // Comprueba si la nota ha salido de la pantalla y la destruye.
        if (transform.position.x < -10f) // Ajusta el valor seg�n tus necesidades.
        {
            Destroy(gameObject);
        }
    }
}