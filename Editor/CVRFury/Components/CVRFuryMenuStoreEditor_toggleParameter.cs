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
  // all ToggleParameter related UI code is in this file

  private float CalculateToggleParameterBlockHeight()
  {
    float height = 0;

    height += 3.2f * EditorGUIUtility.singleLineHeight;

    return height;
  }

  private void DrawToggleParameterFields(string shortTypeName, SerializedProperty element, Rect rect)
  {
    if (shortTypeName == "toggleParameter")
    {
      SerializedProperty defaultStateProperty = element.FindPropertyRelative("defaultState");
      SerializedProperty generateTypeProperty = element.FindPropertyRelative("generateType");

      if (defaultStateProperty != null && generateTypeProperty != null)
      {
        EditorGUI.PropertyField(
          new Rect(
            rect.x,
            rect.y + 2 * EditorGUIUtility.singleLineHeight,
            rect.width,
            EditorGUIUtility.singleLineHeight
          ),
          defaultStateProperty
        );
        EditorGUI.PropertyField(
          new Rect(
            rect.x,
            rect.y + 3 * EditorGUIUtility.singleLineHeight,
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
