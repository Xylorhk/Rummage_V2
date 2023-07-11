using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbingAnimHelper : MonoBehaviour
{
    [SerializeField] float xzOffset = 1.1f, yOffset = 1f;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorMove()
    {
        if (animator)
        {
            Vector3 offset = transform.parent.position;
            offset.x += animator.deltaPosition.x * xzOffset;
            offset.z += animator.deltaPosition.z * xzOffset;
            offset.y += animator.deltaPosition.y * yOffset;
            transform.parent.position = offset;
        }
    }
}