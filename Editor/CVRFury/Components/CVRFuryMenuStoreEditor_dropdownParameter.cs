#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using uk.novavoidhowl.dev.cvrfury.runtime;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

public partial class CVRFuryMenuStoreEditor : Editor
{
  private ReorderableList reorderableList;

  // all DropdownParameter related UI code is in this file

  private float CalculateDropdownParameterBlockHeight()
  {
    float height = 0;

    height += 8 * EditorGUIUtility.singleLineHeight;

    return height;
  }

  private float CalculateDropdownParameterListHeight(SerializedProperty element)
  {
    SerializedProperty dropdownListProperty = element.FindPropertyRelative("dropdownList");
    if (dropdownListProperty != null)
    {
      return ((EditorGUIUtility.singleLineHeight * 1.24f) * dropdownListProperty.arraySize);
    }
    return 0;
  }

  private void DrawDropdownParameterFields(string shortTypeName, SerializedProperty element, Rect rect)
  {
    if (shortTypeName == "dropdownParameter")
    {
      // Find the 'name' property
      SerializedProperty nameProperty = element.FindPropertyRelative("name");

      // find the 'forceMachineName' property
      SerializedProperty forceMachineNameProperty = element.FindPropertyRelative("forceMachineName");

      // find the 'MachineName' property
      SerializedProperty machineNameProperty = element.FindPropertyRelative("MachineName");

      // find the 'nameLinkedToMachineName' property
      SerializedProperty nameLinkedToMachineNameProperty = element.FindPropertyRelative("nameLinkedToMachineName");

      // Legacy content fixer: auto generate the machine name based on the name property
      legacyMachineNameFieldUpdate(nameProperty, machineNameProperty);

      // if the 'forceMachineName' property is true, then set the 'nameLinkedToMachineName' property to false
      if (forceMachineNameProperty.boolValue)
      {
        nameLinkedToMachineNameProperty.boolValue = false;
      }

      // if the nameLinkedToMachineNameProperty value is true, then use the TranslateMenuNameToParameterName function to generate a machine name
      if (nameLinkedToMachineNameProperty.boolValue)
      {
        machineNameProperty.stringValue = TranslateMenuNameToParameterName(
          nameProperty.stringValue,
          forceMachineNameProperty.boolValue
        );
      }

      renderMachineNameField(nameLinkedToMachineNameProperty, machineNameProperty, forceMachineNameProperty, rect);

      SerializedProperty defaultIndexProperty = element.FindPropertyRelative("defaultIndex");
      SerializedProperty generateTypeProperty = element.FindPropertyRelative("generateType");
      SerializedProperty dropdownListProperty = element.FindPropertyRelative("dropdownList");

      if (defaultIndexProperty != null && generateTypeProperty != null && dropdownListProperty != null)
      {
        // Get the dropdown list options
        string[] options = new string[dropdownListProperty.arraySize];
        for (int i = 0; i < dropdownListProperty.arraySize; i++)
        {
          options[i] = dropdownListProperty.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;
        }

        // Draw the defaultIndex as a dropdown list
        defaultIndexProperty.floatValue = EditorGUI.Popup(
          new Rect(
            rect.x,
            rect.y + 3.1f * EditorGUIUtility.singleLineHeight,
            rect.width,
            EditorGUIUtility.singleLineHeight
          ),
          "Default Option",
          (int)defaultIndexProperty.floatValue,
          options
        );

        EditorGUI.PropertyField(
          new Rect(
            rect.x,
            rect.y + 4.1f * EditorGUIUtility.singleLineHeight,
            rect.width,
            EditorGUIUtility.singleLineHeight
          ),
          generateTypeProperty,
          new GUIContent("Animator Parameter Type")
        );

        ReorderableList reorderableList = new ReorderableList(
          dropdownListProperty.serializedObject,
          dropdownListProperty,
          false, // draggable
          true, // displayHeader
          true, // displayAddButton
          true // displayRemoveButton
        )
        {
          drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Dropdown List Names & Values")
        };

        reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
          var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
          float valueWidth = rect.width / 8;
          float nameWidth = 7 * valueWidth;

          EditorGUI.PropertyField(
            new Rect(rect.x, rect.y, nameWidth - 4, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("name"),
            GUIContent.none
          );

          EditorGUI.PropertyField(
            new Rect(rect.x + nameWidth, rect.y, valueWidth, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("value"),
            GUIContent.none
          );

          // Check for duplicate values
          if (HasDuplicateValue(index))
          {
            // Create a GUIContent with an icon and a tooltip
            GUIContent warningContent = new GUIContent(
              EditorGUIUtility.IconContent("console.warnicon").image,
              "Duplicate value detected"
            );

            // Draw the warning icon with the tooltip
            GUI.Label(
              new Rect(rect.x + rect.width - 20, rect.y, 20, EditorGUIUtility.singleLineHeight),
              warningContent
            );
          }
        };

        bool HasDuplicateValue(int currentIndex)
        {
          var currentValueProperty = reorderableList.serializedProperty
            .GetArrayElementAtIndex(currentIndex)
            .FindPropertyRelative("value");

          for (int i = 0; i < reorderableList.serializedProperty.arraySize; i++)
          {
            if (i != currentIndex)
            {
              var valueProperty = reorderableList.serializedProperty
                .GetArrayElementAtIndex(i)
                .FindPropertyRelative("value");

              if (currentValueProperty.propertyType == valueProperty.propertyType)
              {
                switch (currentValueProperty.propertyType)
                {
                  case SerializedPropertyType.String:
                    if (currentValueProperty.stringValue == valueProperty.stringValue)
                    {
                      return true;
                    }
                    break;
                  case SerializedPropertyType.Integer:
                    if (currentValueProperty.intValue == valueProperty.intValue)
                    {
                      return true;
                    }
                    break;
                  case SerializedPropertyType.Float:
                    if (currentValueProperty.floatValue == valueProperty.floatValue)
                    {
                      return true;
                    }
                    break;
                  // Add more cases as needed
                }
              }
            }
          }
          return false;
        }

        reorderableList.DoList(
          new Rect(
            rect.x,
            rect.y + 5.1f * EditorGUIUtility.singleLineHeight,
            rect.width,
            EditorGUIUtility.singleLineHeight * dropdownListProperty.arraySize
          )
        );
      }
    }
  }
}
#endif
