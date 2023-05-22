using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Item), true)][CanEditMultipleObjects]
public class ItemEditor : Editor
{
    public SerializedProperty
        type_Prop,
        name_Prop,
        description_Prop,
        canPickUp_Prop,
        obtained_Prop,
        restored_Prop,
        scrapAmount_Prop,
        equip_Prop,
        localHandPos_Prop,
        localHandRot_Prop,
        image_Prop,
        componentTransforms_Prop,
        gripTransform_Prop,
        interactable_Prop;

    private void OnEnable()
    {
        type_Prop = serializedObject.FindProperty("itemType");
        name_Prop = serializedObject.FindProperty("itemName");
        description_Prop = serializedObject.FindProperty("description");
        canPickUp_Prop = serializedObject.FindProperty("canPickUp");
        obtained_Prop = serializedObject.FindProperty("isObtained");
        restored_Prop = serializedObject.FindProperty("isRestored");
        scrapAmount_Prop = serializedObject.FindProperty("restorationScrapAmount");
        equip_Prop = serializedObject.FindProperty("isEquipped");
        localHandPos_Prop = serializedObject.FindProperty("localHandPos");
        localHandRot_Prop = serializedObject.FindProperty("localHandRot");
        image_Prop = serializedObject.FindProperty("inventorySprite");
        componentTransforms_Prop = serializedObject.FindProperty("chassisComponentTransforms");
        gripTransform_Prop = serializedObject.FindProperty("chassisGripTransform");
        interactable_Prop = serializedObject.FindProperty("interactableOnStart");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        FieldInfo[] childFields = target.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        EditorGUILayout.PropertyField(type_Prop);
        Item.TypeTag currentType = (Item.TypeTag)type_Prop.enumValueIndex;

        switch (currentType)
        {
            case Item.TypeTag.chassis:
                EditorGUILayout.LabelField("");
                EditorGUILayout.LabelField("Item Visual Information", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(name_Prop, new GUIContent("Item Name"));
                EditorGUILayout.PropertyField(description_Prop, new GUIContent("Description"));
                EditorGUILayout.PropertyField(image_Prop, new GUIContent("Inventory Sprite"));

                EditorGUILayout.LabelField("");
                EditorGUILayout.LabelField("Item State Information", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(canPickUp_Prop, new GUIContent("Can Pick Up"));
                EditorGUILayout.PropertyField(obtained_Prop, new GUIContent("Is Obtained"));
                EditorGUILayout.PropertyField(restored_Prop, new GUIContent("Is Restored"));
                EditorGUILayout.PropertyField(scrapAmount_Prop, new GUIContent("Restoration Scrap Amount"));
                EditorGUILayout.PropertyField(equip_Prop, new GUIContent("Is Equipped"));

                EditorGUILayout.LabelField("");
                EditorGUILayout.LabelField("Chassis Specific Information", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(componentTransforms_Prop, new GUIContent("Chassis Component Transforms"));
                EditorGUILayout.PropertyField(gripTransform_Prop, new GUIContent("Chassis Grip Transform"));
                break;
            case Item.TypeTag.effector:
                EditorGUILayout.LabelField("");
                EditorGUILayout.LabelField("Item Visual Information", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(name_Prop, new GUIContent("Item Name"));
                EditorGUILayout.PropertyField(description_Prop, new GUIContent("Description"));
                EditorGUILayout.PropertyField(image_Prop, new GUIContent("Inventory Sprite"));

                EditorGUILayout.LabelField("");
                EditorGUILayout.LabelField("Item State Information", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(canPickUp_Prop, new GUIContent("Can Pick Up"));
                EditorGUILayout.PropertyField(obtained_Prop, new GUIContent("Is Obtained"));
                EditorGUILayout.PropertyField(restored_Prop, new GUIContent("Is Restored"));
                EditorGUILayout.PropertyField(scrapAmount_Prop, new GUIContent("Restoration Scrap Amount"));
                EditorGUILayout.PropertyField(equip_Prop, new GUIContent("Is Equipped"));
                break;
            case Item.TypeTag.grip:
                EditorGUILayout.LabelField("");
                EditorGUILayout.LabelField("Item Visual Information", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(name_Prop, new GUIContent("Item Name"));
                EditorGUILayout.PropertyField(description_Prop, new GUIContent("Description"));
                EditorGUILayout.PropertyField(image_Prop, new GUIContent("Inventory Sprite"));

                EditorGUILayout.LabelField("");
                EditorGUILayout.LabelField("Item State Information", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(canPickUp_Prop, new GUIContent("Can Pick Up"));
                EditorGUILayout.PropertyField(obtained_Prop, new GUIContent("Is Obtained"));
                EditorGUILayout.PropertyField(restored_Prop, new GUIContent("Is Restored"));
                EditorGUILayout.PropertyField(scrapAmount_Prop, new GUIContent("Restoration Scrap Amount"));
                EditorGUILayout.PropertyField(equip_Prop, new GUIContent("Is Equipped"));
                break;
            case Item.TypeTag.ammo:
                EditorGUILayout.LabelField("");
                EditorGUILayout.LabelField("Item Visual Information", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(name_Prop, new GUIContent("Item Name"));
                EditorGUILayout.PropertyField(description_Prop, new GUIContent("Description"));
                EditorGUILayout.PropertyField(image_Prop, new GUIContent("Inventory Sprite"));

                EditorGUILayout.LabelField("");
                EditorGUILayout.LabelField("Item State Information", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(canPickUp_Prop, new GUIContent("Can Pick Up"));
                EditorGUILayout.PropertyField(obtained_Prop, new GUIContent("Is Obtained"));
                EditorGUILayout.PropertyField(restored_Prop, new GUIContent("Is Restored"));
                EditorGUILayout.PropertyField(scrapAmount_Prop, new GUIContent("Restoration Scrap Amount"));
                EditorGUILayout.PropertyField(equip_Prop, new GUIContent("Is Equipped"));
                break;
            case Item.TypeTag.modifier:
                EditorGUILayout.LabelField("");
                EditorGUILayout.LabelField("Item Visual Information", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(name_Prop, new GUIContent("Item Name"));
                EditorGUILayout.PropertyField(description_Prop, new GUIContent("Description"));
                EditorGUILayout.PropertyField(image_Prop, new GUIContent("Inventory Sprite"));

                EditorGUILayout.LabelField("");
                EditorGUILayout.LabelField("Item State Information", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(canPickUp_Prop, new GUIContent("Can Pick Up"));
                EditorGUILayout.PropertyField(obtained_Prop, new GUIContent("Is Obtained"));
                EditorGUILayout.PropertyField(restored_Prop, new GUIContent("Is Restored"));
                EditorGUILayout.PropertyField(scrapAmount_Prop, new GUIContent("Restoration Scrap Amount"));
                EditorGUILayout.PropertyField(equip_Prop, new GUIContent("Is Equipped"));
                break;
            case Item.TypeTag.scrap:
                EditorGUILayout.PropertyField(name_Prop, new GUIContent("Item Name"));
                EditorGUILayout.PropertyField(canPickUp_Prop, new GUIContent("Can Pick Up"));
                break;
            case Item.TypeTag.external:
                EditorGUILayout.PropertyField(name_Prop, new GUIContent("Item Name"));
                EditorGUILayout.PropertyField(canPickUp_Prop, new GUIContent("Can Pick Up"));
                break;
        }

        EditorGUILayout.LabelField("");
        EditorGUILayout.PropertyField(interactable_Prop, new GUIContent("Interactable On Start"));

        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField("Player Local Information", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(localHandPos_Prop, new GUIContent("Local Hand Position"));
        EditorGUILayout.PropertyField(localHandRot_Prop, new GUIContent("Local Hand Rotation"));

        if (target.GetType().Name != "Item")
        {
            foreach (FieldInfo field in childFields)
            {

                if (field.IsPublic || field.GetCustomAttribute(typeof(SerializeField)) != null)
                {

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
