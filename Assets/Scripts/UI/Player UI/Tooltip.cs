using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BasicTools.ButtonInspector;

public class Tooltip : MonoBehaviour
{
    public Vector2 mouseSpacing;
    public Vector2 spacing;

    public TextMeshProUGUI itemTitleTextMesh;
    public TextMeshProUGUI itemDescriptionTextMesh;

    public RectTransform rectTransform;

    private void Awake()
    {
        itemTitleTextMesh.text = "";
        itemDescriptionTextMesh.text = "";
        gameObject.SetActive(false);
    }

    public void EnableTooltip(string itemTitle, string itemDescription)
    {
        UpdatePosition();
        itemTitleTextMesh.text = itemTitle;
        itemDescriptionTextMesh.text = itemDescription;
        this.gameObject.SetActive(true);
    }

    public void DisableTooltip()
    {
        itemTitleTextMesh.text = "";
        itemDescriptionTextMesh.text = "";

        this.gameObject.SetActive(false);
    }

    void UpdatePosition()
    {
        Vector2 finalTooltipPos = Vector2.zero;
        Vector2 mousePos = Input.mousePosition;

        finalTooltipPos = new Vector2(mousePos.x + mouseSpacing.x, mousePos.y + mouseSpacing.y);


        if (finalTooltipPos.x + (rectTransform.sizeDelta.x * 2) > (Screen.width - spacing.x))
        {
            finalTooltipPos.x = (Screen.width - spacing.x) - (rectTransform.sizeDelta.x * 2);
        }
        else if (finalTooltipPos.x < spacing.x)
        {
            finalTooltipPos.x = spacing.x;
        }

        if (finalTooltipPos.y + (rectTransform.sizeDelta.y * 2) > Screen.height - spacing.y)
        {
            finalTooltipPos.y = (Screen.height - spacing.y) - (rectTransform.sizeDelta.y * 2);
        }
        else if (finalTooltipPos.y < spacing.y)
        {
            finalTooltipPos.y = spacing.y;
        }

        rectTransform.position = finalTooltipPos;
    }

    private void Update()
    {
        UpdatePosition();
    }
}
