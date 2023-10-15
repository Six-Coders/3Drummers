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

    public GameObject kickNote;
    public GameObject snareNote;
    public GameObject hihatNote;
    public GameObject tomNote;
    public GameObject rideNote;
    public GameObject crashNote;



    // Llama a esta función para generar una nota.
    public void Generador(string instrumento,Color color)
    {
        // Crea una nueva instancia de la nota.
        GameObject nuevaNota = Instantiate(notaPrefab, contenedorDeNotas.transform);
        // Intenta obtener el componente RectTransform del objeto Image dentro de la nuevaNota.
        RectTransform imageRectTransform = nuevaNota.GetComponentInChildren<Image>().rectTransform;
        Vector3 notePosition = imageRectTransform.position;

        Image imageComponent = nuevaNota.GetComponentInChildren<Image>();

        // ------------------------------
        RectTransform kickRect = kickNote.GetComponentInChildren<Image>().rectTransform;
        Vector3 kickPosition = kickRect.position;

        RectTransform snareRect = snareNote.GetComponentInChildren<Image>().rectTransform;
        Vector3 snarePosition = snareRect.position;

        RectTransform hihatRect = hihatNote.GetComponentInChildren<Image>().rectTransform;
        Vector3 hihatPosition = hihatRect.position;

        RectTransform tomRect = tomNote.GetComponentInChildren<Image>().rectTransform;
        Vector3 tomPosition = tomRect.position;

        RectTransform crashRect = crashNote.GetComponentInChildren<Image>().rectTransform;
        Vector3 crashPosition = crashRect.position;

        RectTransform rideRect = rideNote.GetComponentInChildren<Image>().rectTransform;
        Vector3 ridePosition = rideRect.position;
        


        switch (instrumento)
        {
            case "kick":
                Vector3 newPositionKick = new Vector3(notePosition.x, kickPosition.y, 0);
                imageRectTransform.position = newPositionKick;
                break;
            
            case "snare":
                Vector3 newPositionSnare = new Vector3(notePosition.x, snarePosition.y, 0);
                imageRectTransform.position = newPositionSnare;
                break;

            case "hihat":
                Vector3 newPositionHihat = new Vector3(notePosition.x, hihatPosition.y, 0);
                imageRectTransform.position = newPositionHihat;
                break;

            case "tom1":
                Vector3 newPositionTom = new Vector3(notePosition.x, tomPosition.y, 0);
                imageRectTransform.position = newPositionTom;
                break;
            // Agrega más casos según sea necesario.
            
            case "crash":
                Vector3 newPositionCrash = new Vector3(notePosition.x, crashPosition.y, 0);
                imageRectTransform.position = newPositionCrash;
                break;
            case "ride":
                Vector3 newPositionRide = new Vector3(notePosition.x, ridePosition.y, 0);
                imageRectTransform.position = newPositionRide;
                break;
        }

        imageComponent.color = color;
        // Modifica la posición en el eje Y del RectTransform del objeto Image.
        

    }
}





