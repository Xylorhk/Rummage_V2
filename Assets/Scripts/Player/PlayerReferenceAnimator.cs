using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReferenceAnimator : SingletonMonoBehaviour<PlayerReferenceAnimator>
{
    public Animator animator;
    public Dictionary<string, KeyValuePair<Vector3, Quaternion>> idleTransforms = new Dictionary<string, KeyValuePair<Vector3, Quaternion>>();

    new void Awake()
    {
        base.Awake();

        animator.speed = 0;
        idleTransforms.Clear();
    }

    public void SwitchPlayerAnimLayer(int index)
    {
        animator.SetFloat("Blend", index);
    }

    public void UpdateIdleTransforms()
    {
        animator.speed = 1;
        animator.speed = 0;
        idleTransforms.Clear();

        Transform[] childTransforms = gameObject.GetComponentsInChildren<Transform>();

        for (int i = 1; i < childTransforms.Length; i++)
        {
            idleTransforms.Add(childTransforms[i].gameObject.name, new KeyValuePair<Vector3, Quaternion>(childTransforms[i].localPosition, childTransforms[i].rotation));
        }
    }
}
