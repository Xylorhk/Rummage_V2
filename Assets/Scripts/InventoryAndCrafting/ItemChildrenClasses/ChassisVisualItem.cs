using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChassisVisualItem : VisualItem
{
    public List<Transform> componentTransforms = new List<Transform>();
    public Transform gripTransform;

    public void AddVisualTransforms(List<ChassisComponentTransform> chassisComponentTransforms, ChassisGripTransform chassisGripTransform)
    {
        for (int i = 0; i < chassisComponentTransforms.Count; i++)
        {
            GameObject chassisComponentGO = new GameObject($"ComponentTransform{i}");
            chassisComponentGO.transform.position = new Vector3(chassisComponentTransforms[i].componentTransform.position.x, chassisComponentTransforms[i].componentTransform.position.y, chassisComponentTransforms[i].componentTransform.position.z);
            chassisComponentGO.transform.rotation = new Quaternion(chassisComponentTransforms[i].componentTransform.rotation.x, chassisComponentTransforms[i].componentTransform.rotation.y, chassisComponentTransforms[i].componentTransform.rotation.z, chassisComponentTransforms[i].componentTransform.rotation.w);
            chassisComponentGO.transform.parent = this.gameObject.transform;
            componentTransforms.Add(chassisComponentGO.transform);
        }

        GameObject chassisGripGO = new GameObject($"GripTransform");
        chassisGripGO.transform.position = new Vector3(chassisGripTransform.gripTransform.position.x, chassisGripTransform.gripTransform.position.y, chassisGripTransform.gripTransform.position.z);
        chassisGripGO.transform.rotation = new Quaternion(chassisGripTransform.gripTransform.rotation.x, chassisGripTransform.gripTransform.rotation.y, chassisGripTransform.gripTransform.rotation.z, chassisGripTransform.gripTransform.rotation.w);
        chassisGripGO.transform.parent = this.gameObject.transform;
        gripTransform = chassisGripGO.transform;
    }

    public List<Transform> GetVisualComponentTransforms()
    {
        return componentTransforms;
    }

    public Transform GetVisualGripTransform()
    {
        return gripTransform;
    }
}
