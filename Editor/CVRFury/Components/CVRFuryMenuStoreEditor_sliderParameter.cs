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
  // all SliderParameter related UI code is in this file

  private float CalculateSliderParameterBlockHeight()
  {
    float height = 0;

    height += 2.2f * EditorGUIUtility.singleLineHeight;

    return height;
  }

  private void DrawSliderParameterFields(string shortTypeName, SerializedProperty element, Rect rect)
  {
    if (shortTypeName == "sliderParameter")
    {
      SerializedProperty defaultValueProperty = element.FindPropertyRelative("defaultValue");

      if (defaultValueProperty != null)
      {
        defaultValueProperty.floatValue = EditorGUI.Slider(
          new Rect(
            rect.x,
            rect.y + 2 * EditorGUIUtility.singleLineHeight,
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
