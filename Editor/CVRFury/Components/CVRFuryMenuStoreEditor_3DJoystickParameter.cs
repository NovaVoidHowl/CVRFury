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
  // all 3DJoystickParameter related UI code is in this file

  private float CalculateThreeDJoystickParameterBlockHeight()
  {
    float height = 0;

    height += 12.3f * EditorGUIUtility.singleLineHeight;

    return height;
  }

  private void DrawThreeDJoystickParameterFields(string shortTypeName, SerializedProperty element, Rect rect)
  {
    if (shortTypeName == "threeDJoystickParameter")
    {
      SerializedProperty defaultXValueProperty = element.FindPropertyRelative("defaultXValue");
      SerializedProperty defaultYValueProperty = element.FindPropertyRelative("defaultYValue");
      SerializedProperty defaultZValueProperty = element.FindPropertyRelative("defaultZValue");

      SerializedProperty minXValueProperty = element.FindPropertyRelative("minXValue");
      SerializedProperty maxXValueProperty = element.FindPropertyRelative("maxXValue");
      SerializedProperty minYValueProperty = element.FindPropertyRelative("minYValue");
      SerializedProperty maxYValueProperty = element.FindPropertyRelative("maxYValue");
      SerializedProperty minZValueProperty = element.FindPropertyRelative("minZValue");
      SerializedProperty maxZValueProperty = element.FindPropertyRelative("maxZValue");

      // Find the 'xValuePostfix', 'yValuePostfix', and 'name' properties
      SerializedProperty xValuePostfixProperty = element.FindPropertyRelative("xValuePostfix");
      SerializedProperty yValuePostfixProperty = element.FindPropertyRelative("yValuePostfix");
      SerializedProperty zValuePostfixProperty = element.FindPropertyRelative("zValuePostfix");
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

      // Create a string that contains the information you want to display
      string info =
        $" {nameProperty.stringValue}{xValuePostfixProperty.stringValue},"
        + $" {nameProperty.stringValue}{yValuePostfixProperty.stringValue},"
        + $" {nameProperty.stringValue}{zValuePostfixProperty.stringValue}";

      // Display the 'Parameters:' label using EditorGUI.PrefixLabel
      EditorGUI.PrefixLabel(
        new Rect(rect.x, rect.y + 3 * EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight),
        new GUIContent("Axis Parameters:")
      );

      // Display the info string using EditorGUI.LabelField
      EditorGUI.LabelField(
        new Rect(
          rect.x + EditorGUIUtility.labelWidth,
          rect.y + 3 * EditorGUIUtility.singleLineHeight,
          rect.width - EditorGUIUtility.labelWidth,
          EditorGUIUtility.singleLineHeight
        ),
        info
      );

      EditorGUI.PropertyField(
        new Rect(
          rect.x,
          rect.y + 4.1f * EditorGUIUtility.singleLineHeight,
          rect.width,
          EditorGUIUtility.singleLineHeight
        ),
        minXValueProperty
      );
      EditorGUI.PropertyField(
        new Rect(
          rect.x,
          rect.y + 5.1f * EditorGUIUtility.singleLineHeight,
          rect.width,
          EditorGUIUtility.singleLineHeight
        ),
        maxXValueProperty
      );

      EditorGUI.PropertyField(
        new Rect(
          rect.x,
          rect.y + 6.1f * EditorGUIUtility.singleLineHeight,
          rect.width,
          EditorGUIUtility.singleLineHeight
        ),
        minYValueProperty
      );
      EditorGUI.PropertyField(
        new Rect(
          rect.x,
          rect.y + 7.1f * EditorGUIUtility.singleLineHeight,
          rect.width,
          EditorGUIUtility.singleLineHeight
        ),
        maxYValueProperty
      );
      EditorGUI.PropertyField(
        new Rect(
          rect.x,
          rect.y + 8.1f * EditorGUIUtility.singleLineHeight,
          rect.width,
          EditorGUIUtility.singleLineHeight
        ),
        minZValueProperty
      );
      EditorGUI.PropertyField(
        new Rect(
          rect.x,
          rect.y + 9.1f * EditorGUIUtility.singleLineHeight,
          rect.width,
          EditorGUIUtility.singleLineHeight
        ),
        maxZValueProperty
      );

      if (defaultXValueProperty != null)
      {
        defaultXValueProperty.floatValue = EditorGUI.Slider(
          new Rect(
            rect.x,
            rect.y + 10.1f * EditorGUIUtility.singleLineHeight,
            rect.width,
            EditorGUIUtility.singleLineHeight
          ),
          "Default X Value",
          defaultXValueProperty.floatValue,
          minXValueProperty.floatValue, // min value
          maxXValueProperty.floatValue // max value
        );
      }
      if (defaultYValueProperty != null)
      {
        defaultYValueProperty.floatValue = EditorGUI.Slider(
          new Rect(
            rect.x,
            rect.y + 11.1f * EditorGUIUtility.singleLineHeight,
            rect.width,
            EditorGUIUtility.singleLineHeight
          ),
          "Default Y Value",
          defaultYValueProperty.floatValue,
          minYValueProperty.floatValue, // min value
          maxYValueProperty.floatValue // max value
        );
      }
      if (defaultZValueProperty != null)
      {
        defaultZValueProperty.floatValue = EditorGUI.Slider(
          new Rect(
            rect.x,
            rect.y + 12.1f * EditorGUIUtility.singleLineHeight,
            rect.width,
            EditorGUIUtility.singleLineHeight
          ),
          "Default Z Value",
          defaultZValueProperty.floatValue,
          minZValueProperty.floatValue, // min value
          maxZValueProperty.floatValue // max value
        );
      }
    }
  }
}
#endif
