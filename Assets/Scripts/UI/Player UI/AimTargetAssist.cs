using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimTargetAssist : MonoBehaviour
{
    private vThirdPersonCamera mainCamera;

    public GameObject aimTargetObj;
    [Range(4.5f, 20)]
    public float localDistFromCam = 6.5f;
    private float previousLocalDistFromCam = 0;

    private GameObject personalTargetObj;

    private void Start()
    {
        mainCamera = Player.Instance.vThirdPersonCamera;

        previousLocalDistFromCam = localDistFromCam;
        float posX = localDistFromCam / 6;
        float posY = localDistFromCam / 6;
        personalTargetObj = new GameObject();
        personalTargetObj.transform.parent = mainCamera.transform;
        personalTargetObj.transform.localPosition = new Vector3(posX, posY, localDistFromCam);
        personalTargetObj.transform.rotation = Quaternion.identity;
    }

    private void UpdateLocalDist()
    {
        previousLocalDistFromCam = localDistFromCam;
        float posX = localDistFromCam / 6;
        float posY = localDistFromCam / 6;
        personalTargetObj.transform.parent = mainCamera.transform;
        personalTargetObj.transform.localPosition = new Vector3(posX, posY, localDistFromCam);
        personalTargetObj.transform.rotation = Quaternion.identity;
    }

    private void Update()
    {
        if (previousLocalDistFromCam != localDistFromCam)
        {
            UpdateLocalDist();
        }

        aimTargetObj.transform.position = personalTargetObj.transform.position;
    }
}
