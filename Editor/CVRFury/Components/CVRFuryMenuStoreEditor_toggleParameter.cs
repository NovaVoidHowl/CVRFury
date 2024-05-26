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
