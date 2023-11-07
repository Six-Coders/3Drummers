using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteContainerController : MonoBehaviour
{
    public AudioController audioController;
    public float noteSpeed = 2f;
    // Update is called once per frame
    void Update()
    {
        if (audioController.AudioIsPlaying()) 
        {
            transform.localPosition -= Vector3.right * (noteSpeed * Time.deltaTime * 250f);
        }
    }
}
