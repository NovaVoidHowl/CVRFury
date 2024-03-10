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

public partial class CVRFuryMenuStoreEditor : Editor
{
  private ReorderableList reorderableList;

  // all DropdownParameter related UI code is in this file

  private float CalculateDropdownParameterBlockHeight()
  {
    float height = 0;

    height += 5 * EditorGUIUtility.singleLineHeight;

    return height;
  }

  private float CalculateDropdownParameterListHeight(SerializedProperty element)
  {
    SerializedProperty dropdownListProperty = element.FindPropertyRelative("dropdownList");
    if (dropdownListProperty != null)
    {
      return 2 * EditorGUIUtility.singleLineHeight + EditorGUIUtility.singleLineHeight * dropdownListProperty.arraySize;
    }
    return 0;
  }

  private void DrawDropdownParameterFields(string shortTypeName, SerializedProperty element, Rect rect)
  {
    if (shortTypeName == "dropdownParameter")
    {
      SerializedProperty defaultIndexProperty = element.FindPropertyRelative("defaultIndex");
      SerializedProperty generateTypeProperty = element.FindPropertyRelative("generateType");
      SerializedProperty dropdownListProperty = element.FindPropertyRelative("dropdownList");

      if (defaultIndexProperty != null && generateTypeProperty != null && dropdownListProperty != null)
      {
        // Get the dropdown list options
        string[] options = new string[dropdownListProperty.arraySize];
        for (int i = 0; i < dropdownListProperty.arraySize; i++)
        {
          options[i] = dropdownListProperty.GetArrayElementAtIndex(i).stringValue;
        }

        // Draw the defaultIndex as a dropdown list
        defaultIndexProperty.intValue = EditorGUI.Popup(
          new Rect(
            rect.x,
            rect.y + 2 * EditorGUIUtility.singleLineHeight,
            rect.width,
            EditorGUIUtility.singleLineHeight
          ),
          "Default Index",
          defaultIndexProperty.intValue,
          options
        );

        EditorGUI.PropertyField(
          new Rect(
            rect.x,
            rect.y + 3 * EditorGUIUtility.singleLineHeight,
            rect.width,
            EditorGUIUtility.singleLineHeight
          ),
          generateTypeProperty
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
          drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Dropdown List Names")
        };

        reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
          var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
          EditorGUI.PropertyField(
            new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
            element,
            GUIContent.none
          );
        };

        reorderableList.DoList(
          new Rect(
            rect.x,
            rect.y + 4 * EditorGUIUtility.singleLineHeight,
            rect.width,
            EditorGUIUtility.singleLineHeight * dropdownListProperty.arraySize
          )
        );
      }
    }
  }
}
