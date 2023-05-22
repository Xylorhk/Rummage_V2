using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

//Maybe do this to prevent Garbage???
public static class SerializationSurrogates
{
    //public static GameObject EMPTY_GAMEOBJECT;
}

public class TransformSurrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        var transform = (Transform)obj;

        float[] position = { transform.position.x, transform.position.y, transform.position.z };
        float[] eulerAngles = { transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z };
        float[] scale = { transform.localScale.x, transform.localScale.y, transform.localScale.z };

        info.AddValue("_position", position);
        info.AddValue("_eulerAngles", eulerAngles);
        info.AddValue("_scale", scale);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        var transform = (Transform)obj;

        GameObject tempObj = new GameObject();
        transform = tempObj.transform;
        MonoBehaviour.Destroy(tempObj);

        float[] tempPos = (float[])(info.GetValue("_position", typeof(float[])));
        float[] tempEul = (float[])(info.GetValue("_eulerAngles", typeof(float[])));
        float[] tempScl = (float[])(info.GetValue("_scale", typeof(float[])));

        transform.position = new Vector3(tempPos[0], tempPos[1], tempPos[2]);
        transform.rotation = Quaternion.Euler(new Vector3(tempEul[0], tempEul[1], tempEul[2]));
        transform.localScale = new Vector3(tempScl[0], tempScl[1], tempScl[2]);

        return transform;
    }
}

public class Vector3Surrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        var vector3 = (Vector3)obj;

        float[] tempVect = { vector3.x, vector3.y, vector3.z };
        info.AddValue("_vectorValue", tempVect);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        var vector3 = (Vector3)obj;

        float[] tempVect = (float[])(info.GetValue("_vectorValue", typeof(float[])));
        vector3 = new Vector3(tempVect[0], tempVect[1], tempVect[2]);

        return vector3;
    }
}

public class ItemSurrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        var item = (Item)obj;

        info.AddValue("_isObtained", item.isObtained);
        info.AddValue("_isRestored", item.isRestored);
        info.AddValue("_isEquipped", item.isEquipped);

        if (item.itemType == Item.TypeTag.chassis)
        {
            info.AddValue("_isChassis", true);
            info.AddValue("_componentTransforms", item.chassisComponentTransforms);
            info.AddValue("_gripTransform", item.chassisGripTransform);
        }
        else
        {
            info.AddValue("_isChassis", false);
        }
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        var item = (Item)obj;

        item.isObtained = (bool)info.GetValue("_isObtained", typeof(bool));
        item.isRestored = (bool)info.GetValue("_isRestored", typeof(bool));
        item.isEquipped = (bool)info.GetValue("_isEquipped", typeof(bool));

        bool isChassis = (bool)info.GetValue("_isChassis", typeof(bool));
        if (isChassis)
        {
            item.chassisComponentTransforms = (List<ChassisComponentTransform>)info.GetValue("_componentTransforms", typeof(List<ChassisComponentTransform>));
            item.chassisGripTransform = (ChassisGripTransform)info.GetValue("_gripTransform", typeof(ChassisGripTransform));
        }

        return item;
    }
}

public class ChassisComponentTransformSurrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        var chassisComponent = (ChassisComponentTransform)obj;

        info.AddValue("_isOccupied", chassisComponent.IsComponentTransformOccupied());
        info.AddValue("_componentItem", chassisComponent.GetComponentTransformItem());
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        var chassisComponent = (ChassisComponentTransform)obj;

        bool isOccupied = (bool)info.GetValue("_isOccupied", typeof(bool));
        if (isOccupied)
        {
            chassisComponent.AddNewComponentTransform((Item)info.GetValue("_componentItem", typeof(Item)));
        }

        return chassisComponent;
    }
}

public class ChassisGripTransformSurrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        var chassisGrip = (ChassisGripTransform)obj;

        info.AddValue("_isOccupied", chassisGrip.IsGripTransformOccupied());
        info.AddValue("_gripItem", chassisGrip.GetGripTransformItem(), typeof(Item));
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        var chassisGrip = (ChassisGripTransform)obj;

        bool isOccupied = (bool)info.GetValue("_isOccupied", typeof(bool));
        if (isOccupied)
        {
            Item gripItem = (Item)info.GetValue("_gripItem", typeof(Item));
            chassisGrip.AddNewGripTransform(gripItem);
        }

        return chassisGrip;
    }
}