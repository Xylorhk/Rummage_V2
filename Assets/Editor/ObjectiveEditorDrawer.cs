using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[CustomPropertyDrawer(typeof(Objective))]
public class ObjectiveEditorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        Rect rectFoldout = new Rect(position.min.x, position.min.y, position.size.x, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(rectFoldout, property.isExpanded, label);
        int lines = 1;
        if (property.isExpanded)
        {
            Rect rectType = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(rectType, property.FindPropertyRelative("goalType"));
            Objective.GoalType currentType = (Objective.GoalType)property.FindPropertyRelative("goalType").enumValueIndex;

            Rect rectDescription = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(rectDescription, "Objective Description");

            GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textField);
            textAreaStyle.wordWrap = true;

            EditorGUI.BeginChangeCheck();
            Rect rectTextArea = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight * 3);
            string input = EditorGUI.TextArea(rectTextArea, property.FindPropertyRelative("objectiveDescription").stringValue, textAreaStyle);
            if (EditorGUI.EndChangeCheck())
            {
                property.FindPropertyRelative("objectiveDescription").stringValue = input;
            }

            lines += 2;

            Rect rectSceneName = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(rectSceneName, property.FindPropertyRelative("levelName"));

            switch (currentType)
            {
                case Objective.GoalType.Craft:
                    Rect rectCraftItemName = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(rectCraftItemName, property.FindPropertyRelative("itemName"));
                    break;
                case Objective.GoalType.Gather:
                    Rect rectGatherItemName = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(rectGatherItemName, property.FindPropertyRelative("itemName"));

                    Rect rectGatherItemType = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(rectGatherItemType, property.FindPropertyRelative("itemType"));

                    Rect rectNumbToCollect = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(rectNumbToCollect, property.FindPropertyRelative("numberToCollect"));

                    Rect rectItemsCollected = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(rectItemsCollected, property.FindPropertyRelative("collectedAmount"));
                    break;
                case Objective.GoalType.Location:
                    Rect rectRadiusToTarget = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(rectRadiusToTarget, property.FindPropertyRelative("activationRadius"));

                    Rect rectLocationWorldTarget = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(rectLocationWorldTarget, property.FindPropertyRelative("targetWorldPosition"));
                    break;
                case Objective.GoalType.Talk:
                    Rect rectNPCName = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(rectNPCName, property.FindPropertyRelative("npcName"));

                    Rect rectNPCText = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(rectNPCText, property.FindPropertyRelative("npcDialogue"));

                    Rect rectNPCSprite = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(rectNPCSprite, property.FindPropertyRelative("npcSprite"));
                    break;
                case Objective.GoalType.Activate:
                    Rect rectActivateItemName = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(rectActivateItemName, property.FindPropertyRelative("itemName"));
                    break;
                case Objective.GoalType.Restore:
                    Rect rectRestoreItemName = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(rectRestoreItemName, property.FindPropertyRelative("itemName"));
                    break;
                case Objective.GoalType.External:
                    Rect rectExternalName = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(rectExternalName, property.FindPropertyRelative("externalObjectiveName"));
                    break;
            }

            int numberOfStartEvents = (int)property.FindPropertyRelative("numbOfStartEvents").intValue;
            int numberOfCompleteEvents = (int)property.FindPropertyRelative("numbOfCompleteEvents").intValue;

            Rect rectStartEventSpace = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(rectStartEventSpace, " ");

            Rect rectNumbOfStartEvents = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(rectNumbOfStartEvents, property.FindPropertyRelative("numbOfStartEvents"));

            Rect rectStartEvent = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight * (numberOfStartEvents));
            EditorGUI.PropertyField(rectStartEvent, property.FindPropertyRelative("OnObjectiveStart"));

            if (numberOfStartEvents > 0)
            {
                lines += (4 + (int)(numberOfStartEvents - 1) * 3);
            }
            else
            {
                lines += 4;
            }
            

            Rect rectCompleteEventSpace = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight * 3);
            EditorGUI.LabelField(rectCompleteEventSpace, " ");

            Rect rectNumbOfCompleteEvents = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(rectNumbOfCompleteEvents, property.FindPropertyRelative("numbOfCompleteEvents"));

            Rect rectCompleteEvent = new Rect(position.min.x, position.min.y + lines++ * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight * (numberOfCompleteEvents));
            EditorGUI.PropertyField(rectCompleteEvent, property.FindPropertyRelative("OnObjectiveComplete"));
        }
        
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int totalLines = 2;

        if (property.isExpanded)
        {
            Objective.GoalType currentType = (Objective.GoalType)property.FindPropertyRelative("goalType").enumValueIndex;

            switch (currentType)
            {
                case Objective.GoalType.Craft:
                    totalLines += 11;
                    break;
                case Objective.GoalType.Gather:
                    totalLines += 14;
                    break;
                case Objective.GoalType.Location:
                    totalLines += 12;
                    break;
                case Objective.GoalType.Talk:
                    totalLines += 13;
                    break;
                case Objective.GoalType.Activate:
                    totalLines += 11;
                    break;
                case Objective.GoalType.Restore:
                    totalLines += 11;
                    break;
                case Objective.GoalType.External:
                    totalLines += 11;
                    break;
            }

            if (property.FindPropertyRelative("numbOfStartEvents").intValue > 0)
            {
                totalLines += (property.FindPropertyRelative("numbOfStartEvents").intValue - 1) * 3;
            }

            totalLines += 8;

            if (property.FindPropertyRelative("numbOfCompleteEvents").intValue > 0)
            {
                totalLines += (property.FindPropertyRelative("numbOfCompleteEvents").intValue - 1) * 3;
            }
        }

        return EditorGUIUtility.singleLineHeight * totalLines + EditorGUIUtility.standardVerticalSpacing * (totalLines - 1);
    }
}