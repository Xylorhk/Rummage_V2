using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Uses code from the youtube channel "b3agz", https://youtu.be/MRAVwaGrmrk
/// </summary>

public class QuestMarker
{
    public Sprite sprite = null;
    public Vector2 position = Vector2.zero;
}

public class Compass : MonoBehaviour
{
    public RawImage compassImage;
    public QuestMarker questmarker = new QuestMarker();
    public GameObject compassGO;
    public GameObject questMarkerGO;

    public float questMarkerYPos = 0f;

    float compassUnit;

    private void Start()
    {
        compassUnit = compassImage.rectTransform.rect.width / 360f;
    }

    private void Update()
    {
        compassImage.uvRect = new Rect(Player.Instance.vThirdPersonCamera.transform.eulerAngles.y / 360f, 0f, 1f, 1f);

        if (compassGO.activeInHierarchy)
        {
            questMarkerGO.GetComponent<Image>().rectTransform.anchoredPosition = GetPosOnCompass();
        }
    }

    public void SetQuestMarker(Sprite markerImage, Vector3 posOfMarker)
    {
        compassGO.SetActive(true);
        questMarkerGO.SetActive(true);
        questmarker.sprite = markerImage;
        questmarker.position = new Vector2(posOfMarker.x, posOfMarker.z);

        questMarkerGO.GetComponent<Image>().sprite = questmarker.sprite;
    }

    public void ResetQuestMarker()
    {
        questMarkerGO.SetActive(false);

        if (questmarker.sprite != null)
        {
            questmarker.sprite = null;
        }
        questmarker.position = Vector2.zero;

        compassGO.SetActive(false);
    }

    Vector2 GetPosOnCompass()
    {
        Vector2 playerPos = new Vector2(Player.Instance.transform.position.x, Player.Instance.transform.position.z);
        Vector2 cameraForward = new Vector2(Player.Instance.vThirdPersonCamera.transform.forward.x, Player.Instance.vThirdPersonCamera.transform.forward.z);

        float angle = Vector2.SignedAngle(questmarker.position - playerPos, cameraForward);

        return new Vector2(compassUnit * angle, questMarkerYPos);
    }
}
