using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Emoter : MonoBehaviour
{
    private bool hasEmote = false;
    private Animator animator;

    public void InitEmoter(Animator characterAnim, bool characterHasEmote)
    {
        hasEmote = characterHasEmote;
        animator = characterAnim;
    }

    public void UpdateEmoter(bool characterHasEmote)
    {
        hasEmote = characterHasEmote;
    }

    public void PlayEmote(string emoteTrigger)
    {
        animator.SetTrigger(emoteTrigger);
    }
}
