using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BasicTools.ButtonInspector;

public class PrimaryCraftingUIDescriptor : MonoBehaviour
{
    [Header("Main Mod Button Variables")]
    public TextMeshProUGUI titleTextMesh;
    public TextMeshProUGUI itemNameTextMesh;
    public Image buttonIconBackground;
    public Image buttonIcon;
    public Sprite iconBackground;

    [Header("Mod Item List Variables")]
    [Range(5, 10)]
    public int numbOfModItemsMax = 10;
    public GameObject externalSecondaryCraftingList;
    public GameObject secondaryCraftingList;
    public GameObject internalCraftingList;
    public GameObject upScrollButton;
    public GameObject downScrollButton;
    public VerticalLayoutGroup internalCraftingListVerticalLayoutGroup;

    private Vector2 externalCraftingListOriginalXY;
    private Vector2 internalCraftingListOriginalXY;
    [HideInInspector]
    public int numbOfItems = 0;
    [HideInInspector]
    public float heightOfModButton = 0;

    private void Awake()
    {
        externalCraftingListOriginalXY = externalSecondaryCraftingList.GetComponent<RectTransform>().anchoredPosition;
        internalCraftingListOriginalXY = internalCraftingList.GetComponent<RectTransform>().anchoredPosition;
    }

    public void SetUpList(float heightOfModItem)
    {
        heightOfModButton = heightOfModItem;
        float spacingVar = Mathf.Abs(internalCraftingListVerticalLayoutGroup.spacing) * numbOfModItemsMax;
        RectTransform externalRect = externalSecondaryCraftingList.GetComponent<RectTransform>();

        externalRect.sizeDelta = new Vector2(externalRect.sizeDelta.x, (heightOfModItem * numbOfModItemsMax) - spacingVar);

        RectTransform upScrollRect = upScrollButton.GetComponent<RectTransform>();
        RectTransform downScrollRect = downScrollButton.GetComponent<RectTransform>();

        upScrollRect.anchoredPosition = new Vector2(upScrollRect.anchoredPosition.x, externalRect.sizeDelta.y - 3);
        downScrollRect.anchoredPosition = new Vector2(downScrollRect.anchoredPosition.x, 3);

        upScrollButton.SetActive(false);
        downScrollButton.SetActive(false);
    }

    public void SetButtonInformation(string title, string itemTitle, Sprite itemSprite)
    {
        titleTextMesh.text = title;
        itemNameTextMesh.text = itemTitle;

        if (itemSprite == null)
        {
            buttonIconBackground.enabled = false;
            buttonIcon.enabled = false;
        }
        else
        {
            buttonIconBackground.enabled = true;
            buttonIcon.enabled = true;
            buttonIconBackground.sprite = iconBackground;
            buttonIcon.sprite = itemSprite;
        }       
    }

    public void UpdateScrollParameters(int currentNumbOfItems)
    {
        numbOfItems = currentNumbOfItems;
    }

    public void ScrollItemsUp()
    {
        RectTransform internalRect = internalCraftingList.GetComponent<RectTransform>();
        if (internalRect.anchoredPosition.y - heightOfModButton <= 0)
        {
            internalRect.anchoredPosition = new Vector2(internalRect.anchoredPosition.x, 0);
            return;
        }
        
        internalRect.anchoredPosition = new Vector2(internalRect.anchoredPosition.x, internalRect.anchoredPosition.y - heightOfModButton);
    }

    public void ScrollItemsDown()
    {
        RectTransform internalRect = internalCraftingList.GetComponent<RectTransform>();
        int itemsOverMax = numbOfModItemsMax - numbOfItems;
        if (itemsOverMax >= 0)
        {
            return;
        }
        else if (internalRect.anchoredPosition.y + heightOfModButton >= Mathf.Abs(itemsOverMax) * heightOfModButton)
        {
            internalRect.anchoredPosition = new Vector2(internalRect.anchoredPosition.x, Mathf.Abs(itemsOverMax) * heightOfModButton);
            return;
        }
        

        internalRect.anchoredPosition = new Vector2(internalRect.anchoredPosition.x, internalRect.anchoredPosition.y + heightOfModButton); 
    }

    public void ToggleSecondaryCrafting()
    {
        secondaryCraftingList.SetActive(!secondaryCraftingList.activeInHierarchy);
        if (numbOfItems > numbOfModItemsMax && !upScrollButton.activeInHierarchy && !downScrollButton.activeInHierarchy)
        {
            upScrollButton.SetActive(true);
            downScrollButton.SetActive(true);
        }
        else
        {
            upScrollButton.SetActive(false);
            downScrollButton.SetActive(false);
        }

        Transform[] otherTransforms = gameObject.transform.parent.GetComponentsInChildren<Transform>();
        for (int i = 0; i < otherTransforms.Length; i++)
        {
            PrimaryCraftingUIDescriptor currentPCUIDescriptor = otherTransforms[i].gameObject.GetComponent<PrimaryCraftingUIDescriptor>();
            if(currentPCUIDescriptor == null)
            {
                continue;
            }
            else if(otherTransforms[i] == gameObject.transform)
            {
                continue;
            }

            currentPCUIDescriptor.secondaryCraftingList.SetActive(false);
            currentPCUIDescriptor.upScrollButton.SetActive(false);
            currentPCUIDescriptor.downScrollButton.SetActive(false);
        }
    }

    public void DisableListItems()
    {
        secondaryCraftingList.SetActive(false);
        upScrollButton.SetActive(false);
        downScrollButton.SetActive(false);
    }

    public void MoveSecondaryCraftingRect(float newX, float newY)
    {
        RectTransform currentRect = externalSecondaryCraftingList.GetComponent<RectTransform>();
        currentRect.anchoredPosition = new Vector2(currentRect.anchoredPosition.x + newX, currentRect.anchoredPosition.y + newY);
    }

    public void ResetSecondaryCraftingRect()
    {
        externalSecondaryCraftingList.GetComponent<RectTransform>().anchoredPosition = externalCraftingListOriginalXY;
        secondaryCraftingList.SetActive(false);

        internalCraftingList.GetComponent<RectTransform>().anchoredPosition = internalCraftingListOriginalXY;
        upScrollButton.SetActive(false);
        downScrollButton.SetActive(false);
    }
}
