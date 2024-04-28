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
  // all SliderParameter related UI code is in this file

  private float CalculateSliderParameterBlockHeight()
  {
    float height = 0;

    height += 3.2f * EditorGUIUtility.singleLineHeight;

    return height;
  }

  private void DrawSliderParameterFields(string shortTypeName, SerializedProperty element, Rect rect)
  {
    if (shortTypeName == "sliderParameter")
    {
      // Find the 'name' property
      SerializedProperty nameProperty = element.FindPropertyRelative("name");

      // find the 'forceMachineName' property
      SerializedProperty forceMachineNameProperty = element.FindPropertyRelative("forceMachineName");

      // Create a string that contains the information to display
      string info = " " + TranslateMenuNameToParameterName(nameProperty.stringValue, forceMachineNameProperty.boolValue);

      // Display the 'Parameter:' label using EditorGUI.PrefixLabel
      EditorGUI.PrefixLabel(
        new Rect(rect.x, rect.y + 2 * EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight),
        new GUIContent("Parameter:")
      );

      // Display the info string using EditorGUI.LabelField
      EditorGUI.LabelField(
        new Rect(
          rect.x + EditorGUIUtility.labelWidth,
          rect.y + 2.1f * EditorGUIUtility.singleLineHeight,
          rect.width - EditorGUIUtility.labelWidth,
          EditorGUIUtility.singleLineHeight
        ),
        info
      );

      SerializedProperty defaultValueProperty = element.FindPropertyRelative("defaultValue");

      if (defaultValueProperty != null)
      {
        defaultValueProperty.floatValue = EditorGUI.Slider(
          new Rect(
            rect.x,
            rect.y + 3.1f * EditorGUIUtility.singleLineHeight,
            rect.width,
            EditorGUIUtility.singleLineHeight
          ),
          "Default Value",
          defaultValueProperty.floatValue,
          0, // min value
          1 // max value
        );
      }
    }
  }
}
#endif
