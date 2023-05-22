using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoTransform : MonoBehaviour
{
    public Color gizmoColor;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}
