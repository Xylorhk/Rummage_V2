using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsUI : MonoBehaviour
{
    public List<GameObject> panelsToDisplay = new List<GameObject>();

    private int currentIndex = 0;

    public void ResetCurrentDisplay()
    {
        for (int i = 0; i < panelsToDisplay.Count; i++)
        {
            panelsToDisplay[i].SetActive(false);
        }

        currentIndex = 0;

        panelsToDisplay[currentIndex].SetActive(true);
    }

    public void NextCurrentDisplay()
    {
        panelsToDisplay[currentIndex].SetActive(false);

        currentIndex++;

        if (currentIndex == panelsToDisplay.Count)
        {
            currentIndex = 0;
        }

        panelsToDisplay[currentIndex].SetActive(true);
    }

    public void PreviousCurrentDisplay()
    {
        panelsToDisplay[currentIndex].SetActive(false);

        currentIndex--;

        if (currentIndex == -1)
        {
            currentIndex = panelsToDisplay.Count - 1;
        }

        panelsToDisplay[currentIndex].SetActive(true);
    }
}
