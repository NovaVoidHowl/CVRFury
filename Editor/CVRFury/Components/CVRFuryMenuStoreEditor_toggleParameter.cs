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
  // all ToggleParameter related UI code is in this file

  private float CalculateToggleParameterBlockHeight()
  {
    float height = 0;

    height += 4.2f * EditorGUIUtility.singleLineHeight;

    return height;
  }

  private void DrawToggleParameterFields(string shortTypeName, SerializedProperty element, Rect rect)
  {
    if (shortTypeName == "toggleParameter")
    {
      SerializedProperty defaultStateProperty = element.FindPropertyRelative("defaultState");
      SerializedProperty generateTypeProperty = element.FindPropertyRelative("generateType");

      // Find the 'name' property
      SerializedProperty nameProperty = element.FindPropertyRelative("name");
      // find the 'MachineName' property
      SerializedProperty machineNameProperty = element.FindPropertyRelative("MachineName");

      // find the 'forceMachineName' property
      SerializedProperty forceMachineNameProperty = element.FindPropertyRelative("forceMachineName");

      // if the  machineNameProperty is empty, then use the TranslateMenuNameToParameterName function to generate a machine name
      if (string.IsNullOrEmpty(machineNameProperty.stringValue))
      {
        machineNameProperty.stringValue = TranslateMenuNameToParameterName(nameProperty.stringValue, forceMachineNameProperty.boolValue);
      }

      // Draw field for the MachineName
      if (machineNameProperty != null)
          {
            // Draw field for the menuParameter
            EditorGUI.PropertyField(
              new Rect(
                rect.x,
                rect.y + EditorGUIUtility.singleLineHeight * 2,
                rect.width,
                EditorGUIUtility.singleLineHeight
              ),
              machineNameProperty
            );
          }

      if (defaultStateProperty != null && generateTypeProperty != null)
      {
        EditorGUI.PropertyField(
          new Rect(
            rect.x,
            rect.y + 3.1f * EditorGUIUtility.singleLineHeight,
            rect.width,
            EditorGUIUtility.singleLineHeight
          ),
          defaultStateProperty
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
      }
    }
  }
}
#endif
