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

namespace uk.novavoidhowl.dev.cvrfury.editor.components
{
  public partial class CVRFuryMenuStoreEditor : Editor
  {
    // all subMenuParameter related UI code is in this file

    private float CalculateSubMenuParameterBlockHeight()
    {
      float height = 0;

      // One row for the name field (drawn by main editor) + one row for Sub Menu Store object field
      height += 2.2f * EditorGUIUtility.singleLineHeight;

      return height;
    }

    private void DrawSubMenuParameterFields(string shortTypeName, SerializedProperty element, Rect rect)
    {
      if (shortTypeName == "subMenuParameter")
      {
        SerializedProperty subMenuStoreProperty = element.FindPropertyRelative("subMenuStore");

        if (subMenuStoreProperty != null)
        {
          EditorGUI.PropertyField(
            new Rect(
              rect.x,
              rect.y + 2.1f * EditorGUIUtility.singleLineHeight,
              rect.width,
              EditorGUIUtility.singleLineHeight
            ),
            subMenuStoreProperty,
            new GUIContent("Sub Menu Store")
          );
        }
      }
    }
  }
}
#endif
