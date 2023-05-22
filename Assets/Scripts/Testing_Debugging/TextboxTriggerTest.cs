using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextboxTriggerTest : MonoBehaviour
{
    public TextAsset textFile;
    public Sprite talkerIcon;
    public bool shouldAutoAdvance = false;
    public float textScrollSpeed = 0.1f;

    private void OnTriggerEnter(Collider other)
    {
        Player playerCheck = null;
        playerCheck = other.gameObject.GetComponent<Player>();
        if (playerCheck != null)
        {
            Textbox.Instance.SetTypeSpeed(textScrollSpeed);
            Textbox.Instance.EnableTextbox(textFile, talkerIcon, shouldAutoAdvance);
        }
    }
}
