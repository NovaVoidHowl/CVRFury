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

      // Create a string that contains the information to display
      string info = " " + TranslateMenuNameToParameterName(nameProperty.stringValue);

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
