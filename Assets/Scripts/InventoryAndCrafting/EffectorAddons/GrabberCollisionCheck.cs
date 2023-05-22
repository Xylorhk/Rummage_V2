using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabberCollisionCheck : MonoBehaviour
{
    public GrabberEffector grabberEffector;

    private float colliderReduceSize = .2f;

    private Vector3 boxColliderSize;
    private float radius;
    private float height;

    private void Awake()
    {
        BoxCollider boxCheck = GetComponent<BoxCollider>();
        if (boxCheck != null)
        {
            boxColliderSize = boxCheck.size;
            boxCheck.size -= new Vector3(colliderReduceSize, colliderReduceSize, colliderReduceSize);
            return;
        }

        SphereCollider sphereCheck = GetComponent<SphereCollider>();
        if (sphereCheck != null)
        {
            radius = sphereCheck.radius;
            sphereCheck.radius -= colliderReduceSize;
            return;
        }

        CapsuleCollider capsuleCheck = GetComponent<CapsuleCollider>();
        if (capsuleCheck != null)
        {
            radius = capsuleCheck.radius;
            height = capsuleCheck.height;
            capsuleCheck.radius -= colliderReduceSize;
            capsuleCheck.height -= colliderReduceSize;
            return;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.isTrigger)
        {
            grabberEffector.DropCurrentObj();
        }
    }

    private void Update()
    {
        if (grabberEffector == null)
        {
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        BoxCollider boxCheck = GetComponent<BoxCollider>();
        if (boxCheck != null)
        {
            boxCheck.size += new Vector3(colliderReduceSize, colliderReduceSize, colliderReduceSize);
            return;
        }

        SphereCollider sphereCheck = GetComponent<SphereCollider>();
        if (sphereCheck != null)
        {
            sphereCheck.radius += colliderReduceSize;
            return;
        }

        CapsuleCollider capsuleCheck = GetComponent<CapsuleCollider>();
        if (capsuleCheck != null)
        {
            radius = capsuleCheck.radius;
            height = capsuleCheck.height;
            capsuleCheck.radius += colliderReduceSize;
            capsuleCheck.height += colliderReduceSize;
            return;
        }
    }
}
