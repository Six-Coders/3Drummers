using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Generador3 : MonoBehaviour
{
    public GameObject notaPrefab;
    public Transform contenedorDeNotas;
    public float posicionInicialX = 0f;
    public float distanciaVerticalEntreNotas;


    // Llama a esta función para generar una nota.
    public void Generador(string instrumento,Color color)
    {
     
        // Crea una nueva instancia de la nota.
        GameObject nuevaNota = Instantiate(notaPrefab, contenedorDeNotas.transform);

        // Intenta obtener el componente RectTransform del objeto Image dentro de la nuevaNota.
        RectTransform imageRectTransform = nuevaNota.GetComponentInChildren<Image>().rectTransform;

        Image imageComponent = nuevaNota.GetComponentInChildren<Image>();




        float offsetY = 0;
        switch (instrumento)
        {
            case "snare":
               
                offsetY = -distanciaVerticalEntreNotas;
               
                break;
            case "tom1":
                // Maneja otros casos aquí si es necesario.
                offsetY = -distanciaVerticalEntreNotas*2;
                break;
            // Agrega más casos según sea necesario.
            case "hihat":
                offsetY = -distanciaVerticalEntreNotas * 3;
                // Maneja el caso predeterminado aquí si es necesario.
                break;
            case "crash":
                offsetY = -distanciaVerticalEntreNotas * 4;
                break;
            case "ride":
                offsetY = -distanciaVerticalEntreNotas * 5;
                break;
        }

        imageComponent.color = color;
        // Modifica la posición en el eje Y del RectTransform del objeto Image.
        Vector3 newPosition = imageRectTransform.position + new Vector3(0, offsetY, 0);
        imageRectTransform.position = newPosition;

    }
}





