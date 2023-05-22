using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericButton : MonoBehaviour
{
    [SerializeField] public string menuClick = string.Empty;
    public TMPro.TextMeshProUGUI buttonTextMesh;

    public Color normalColor = Color.white;
    public Color hoverColor = Color.white;

    public void PlayClickSFX()
    {
        if (menuClick != string.Empty)
        {
            AudioManager.Get().Play(menuClick);
        }
    }

    public void SwitchTextNormalColor()
    {
        buttonTextMesh.color = normalColor;
    }

    public void SwitchTextHoveredColor()
    {
        if (gameObject.GetComponent<UnityEngine.UI.Button>().interactable)
        {
            buttonTextMesh.color = hoverColor;
        }
        else
        {
            buttonTextMesh.color = normalColor;
        }
    }
}
