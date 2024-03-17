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

[CustomEditor(typeof(CVRFuryMenuStore))]
public partial class CVRFuryMenuStoreEditor : Editor
{
  private ReorderableList list;
  private List<Type> menuTypes;

  private void OnEnable()
  {
    SerializedProperty items = serializedObject.FindProperty("menuItems");

    menuTypes = GetDerivedTypes<menuParameter>();

    list = new ReorderableList(serializedObject, items)
    {
      elementHeightCallback = (index) =>
      {
        // Get the element
        var element = list.serializedProperty.GetArrayElementAtIndex(index);
        string fullTypeName = element.managedReferenceFullTypename;
        string shortTypeName = fullTypeName.Split('.').Last();

        // Base height for the type name and the name property
        float height = EditorGUIUtility.singleLineHeight;

        // Check if the foldout is expanded
        SerializedProperty foldoutStateProperty = element.FindPropertyRelative("viewerFoldoutState");
        bool foldoutState = foldoutStateProperty.boolValue;
        if (foldoutState)
        {
          switch (shortTypeName)
          {
            case "toggleParameter":
              height += CalculateToggleParameterBlockHeight();
              break;
            case "dropdownParameter":
              height += CalculateDropdownParameterBlockHeight();
              height += CalculateDropdownParameterListHeight(element);
              break;
            case "materialColorParameter":
              // TODO: Add height for MaterialColorParameter - complexity due to gameObject references
              break;
            case "sliderParameter":
              height += CalculateSliderParameterBlockHeight();
              break;
            case "twoDJoystickParameter":
              height += CalculateTwoDJoystickParameterBlockHeight();
              break;
            case "threeDJoystickParameter":
              height += CalculateThreeDJoystickParameterBlockHeight();
              break;
            case "inputSingleParameter":
              // TODO: Add height for InputSingleParameter - no obvious use case, so not implemented
              break;
            case "inputVector2Parameter":
              // TODO: Add height for InputVector2Parameter - no obvious use case, so not implemented
              break;
            case "inputVector3Parameter":
              // TODO: Add height for InputVector3Parameter - no obvious use case, so not implemented
              break;
          }
        }

        // Add some spacing
        height += EditorGUIUtility.standardVerticalSpacing;

        return height;
      },
      drawHeaderCallback = (Rect rect) =>
      {
        EditorGUI.LabelField(rect, "Menu Items");
      },
      drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
      {
        // Get the element
        var element = list.serializedProperty.GetArrayElementAtIndex(index);
        // write the type name
        string fullTypeName = element.managedReferenceFullTypename;
        string shortTypeName = fullTypeName.Split('.').Last();

        // Get the name of the menuParameter
        SerializedProperty nameProperty = element.FindPropertyRelative("name");
        string name = nameProperty != null ? nameProperty.stringValue : "null";
        // if name is not set, put 'Unnamed' as the name
        if (name == "")
        {
          name = "Unnamed";
        }

        // Get the foldout state from the menuParameter
        SerializedProperty foldoutStateProperty = element.FindPropertyRelative("viewerFoldoutState");
        bool foldoutState = foldoutStateProperty.boolValue;

        foldoutState = EditorGUI.BeginFoldoutHeaderGroup(
          new Rect(rect.x + 10, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
          foldoutState,
          $"{name} - ({shortTypeName})"
        );

        // Store the foldout state back to the menuParameter
        foldoutStateProperty.boolValue = foldoutState;

        if (foldoutState)
        {
          // Get the serialized property of the menuParameter
          SerializedProperty menuParameterProperty = element.FindPropertyRelative("name");
          if (menuParameterProperty != null)
          {
            // Draw field for the menuParameter
            EditorGUI.PropertyField(
              new Rect(
                rect.x,
                rect.y + EditorGUIUtility.singleLineHeight,
                rect.width,
                EditorGUIUtility.singleLineHeight
              ),
              menuParameterProperty
            );
          }

          float totalHeightOffset = 0;
          // render all the sections for the menuParameters
          DrawToggleParameterFields(shortTypeName, element, rect);
          DrawDropdownParameterFields(shortTypeName, element, rect);
          DrawSliderParameterFields(shortTypeName, element, rect);
          DrawTwoDJoystickParameterFields(shortTypeName, element, rect);
          DrawThreeDJoystickParameterFields(shortTypeName, element, rect);

          // TODO:
          // DrawMaterialColorParameterFields(shortTypeName, element, rect); - complexity due to gameObject references
          // DrawInputSingleParameterFields(shortTypeName, element, rect); - no obvious use case, so not implemented
          // DrawInputVector2ParameterFields(shortTypeName, element, rect); - no obvious use case, so not implemented
          // DrawInputVector3ParameterFields(shortTypeName, element, rect); - no obvious use case, so not implemented
        }
        EditorGUI.EndFoldoutHeaderGroup();
      },
      onAddDropdownCallback = (Rect buttonRect, ReorderableList l) =>
      {
        var menu = new GenericMenu();
        foreach (var menuType in menuTypes)
        {
          menu.AddItem(new GUIContent(menuType.Name), false, clickHandler, menuType);
        }
        menu.ShowAsContext();
      },
      onRemoveCallback = (ReorderableList l) =>
      {
        ReorderableList.defaultBehaviours.DoRemoveButton(l);
      }
    };
  }

  void clickHandler(object target)
  {
    var menuType = (Type)target;
    var index = list.serializedProperty.arraySize;
    list.serializedProperty.arraySize++;
    list.index = index;
    var element = list.serializedProperty.GetArrayElementAtIndex(index);
    element.managedReferenceValue = Activator.CreateInstance(menuType);
    serializedObject.ApplyModifiedProperties();
  }

  public override void OnInspectorGUI()
  {
    serializedObject.Update();
    list.DoLayoutList();
    serializedObject.ApplyModifiedProperties();
  }

  private static List<Type> GetDerivedTypes<T>()
  {
    var derivedTypes = new List<Type>();
    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
    {
      foreach (var type in assembly.GetTypes())
      {
        if (type.IsSubclassOf(typeof(T)))
        {
          derivedTypes.Add(type);
        }
      }
    }
    return derivedTypes;
  }
}
#endif
