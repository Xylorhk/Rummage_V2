using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempWinCondition : MonoBehaviour
{
    public GameObject winScreenCanvas;
    public Animator anim;

    private void Start()
    {
        winScreenCanvas.SetActive(false);
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Player"))
        {
            Cursor.lockState = CursorLockMode.None;
            winScreenCanvas.SetActive(true);
            anim.SetTrigger("HasMcGuffin");
        }
    }
}
