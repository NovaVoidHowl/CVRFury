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

public partial class CVRFuryMenuStoreEditor : Editor
{
  // all 2DJoystickParameter related UI code is in this file

  private float CalculateTwoDJoystickParameterBlockHeight()
  {
    float height = 0;

    height += 8.3f * EditorGUIUtility.singleLineHeight;

    return height;
  }

  private void DrawTwoDJoystickParameterFields(string shortTypeName, SerializedProperty element, Rect rect)
  {
    if (shortTypeName == "twoDJoystickParameter")
    {
      SerializedProperty defaultXValueProperty = element.FindPropertyRelative("defaultXValue");
      SerializedProperty defaultYValueProperty = element.FindPropertyRelative("defaultYValue");

      SerializedProperty minXValueProperty = element.FindPropertyRelative("minXValue");
      SerializedProperty maxXValueProperty = element.FindPropertyRelative("maxXValue");
      SerializedProperty minYValueProperty = element.FindPropertyRelative("minYValue");
      SerializedProperty maxYValueProperty = element.FindPropertyRelative("maxYValue");

      // Find the 'xValuePostfix', 'yValuePostfix', and 'name' properties
      SerializedProperty xValuePostfixProperty = element.FindPropertyRelative("xValuePostfix");
      SerializedProperty yValuePostfixProperty = element.FindPropertyRelative("yValuePostfix");
      SerializedProperty nameProperty = element.FindPropertyRelative("name");

      // Create a string that contains the information you want to display
      string info =
        $" {nameProperty.stringValue}{xValuePostfixProperty.stringValue}, {nameProperty.stringValue}{yValuePostfixProperty.stringValue}";

      // Display the 'Parameters:' label using EditorGUI.PrefixLabel
      EditorGUI.PrefixLabel(
        new Rect(rect.x, rect.y + 2 * EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight),
        new GUIContent("Parameters:")
      );

      // Display the info string using EditorGUI.LabelField
      EditorGUI.LabelField(
        new Rect(
          rect.x + EditorGUIUtility.labelWidth,
          rect.y + 2 * EditorGUIUtility.singleLineHeight,
          rect.width - EditorGUIUtility.labelWidth,
          EditorGUIUtility.singleLineHeight
        ),
        info
      );

      EditorGUI.PropertyField(
        new Rect(
          rect.x,
          rect.y + 3.1f * EditorGUIUtility.singleLineHeight,
          rect.width,
          EditorGUIUtility.singleLineHeight
        ),
        minXValueProperty
      );
      EditorGUI.PropertyField(
        new Rect(
          rect.x,
          rect.y + 4.1f * EditorGUIUtility.singleLineHeight,
          rect.width,
          EditorGUIUtility.singleLineHeight
        ),
        maxXValueProperty
      );

      EditorGUI.PropertyField(
        new Rect(
          rect.x,
          rect.y + 5.1f * EditorGUIUtility.singleLineHeight,
          rect.width,
          EditorGUIUtility.singleLineHeight
        ),
        minYValueProperty
      );
      EditorGUI.PropertyField(
        new Rect(
          rect.x,
          rect.y + 6.1f * EditorGUIUtility.singleLineHeight,
          rect.width,
          EditorGUIUtility.singleLineHeight
        ),
        maxYValueProperty
      );

      if (defaultXValueProperty != null)
      {
        defaultXValueProperty.floatValue = EditorGUI.Slider(
          new Rect(
            rect.x,
            rect.y + 7.1f * EditorGUIUtility.singleLineHeight,
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
            rect.y + 8.1f * EditorGUIUtility.singleLineHeight,
            rect.width,
            EditorGUIUtility.singleLineHeight
          ),
          "Default Y Value",
          defaultYValueProperty.floatValue,
          minYValueProperty.floatValue, // min value
          maxYValueProperty.floatValue // max value
        );
      }
    }
  }
}
#endif
