using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingIndicator : MonoBehaviour
{
    public GameObject loadingIndicatorPrefab;
    private GameObject currentIndicatorInstance = null;
    public GameObject UIblocker;

    private void Start()
    {
        UIblocker.SetActive(true);
    }

    // Start is called before the first frame update
    public void ShowLoadingIndicator(string message)
    {
        UIblocker.SetActive(true);
        if (currentIndicatorInstance == null)
        {
            currentIndicatorInstance = Instantiate(loadingIndicatorPrefab, transform);
            GetComponentInChildren<TextMeshProUGUI>().text = message;
        }
    }

    public void HideLoadingIndicator()
    {

        if (currentIndicatorInstance != null)
        {
            Destroy(currentIndicatorInstance);
            currentIndicatorInstance = null;
            GetComponentInChildren<TextMeshProUGUI>().text = null;
            UIblocker.SetActive(false);
        }
    }
}
