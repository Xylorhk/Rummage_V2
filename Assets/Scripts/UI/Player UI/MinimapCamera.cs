using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    public float distanceFromPlayer = 20;
    public bool shouldRotateWithPlayer = false;

    Vector3 initialCameraOffset;
    Quaternion initialCameraRotation;
    Player playerRef;

    private void Start()
    {
        playerRef = Player.Instance;
        initialCameraRotation = transform.rotation;

        Vector3 newPos = playerRef.transform.position;
        newPos.y = playerRef.transform.position.y + distanceFromPlayer;

        initialCameraOffset = playerRef.transform.position - newPos;
    }

    private void Update()
    {
        transform.position = playerRef.transform.position - initialCameraOffset;

        if (shouldRotateWithPlayer)
        {
            transform.rotation = Quaternion.Euler(90f, playerRef.transform.eulerAngles.y, 0f);
        }
        else if (!shouldRotateWithPlayer && transform.rotation != initialCameraRotation)
        {
            transform.rotation = initialCameraRotation;
        }
    }
}
