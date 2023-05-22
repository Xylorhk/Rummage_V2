using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemUIDescriptor : MonoBehaviour
{
    public UnityEngine.UI.Image backgroundImage;
    public UnityEngine.UI.Image currentImage;
    public Sprite backgroundSprite;
    public TMPro.TextMeshProUGUI currentTextMesh;
    public void ApplyDescriptors(Sprite newSprite, string newTitle)
    {
        currentTextMesh.text = newTitle;

        if (newSprite == null)
        {
            backgroundImage.enabled = false;
            currentImage.enabled = false;
        }
        else
        {
            backgroundImage.enabled = true;
            currentImage.enabled = true;
            backgroundImage.sprite = backgroundSprite;
            currentImage.sprite = newSprite;
        }
    }
}
