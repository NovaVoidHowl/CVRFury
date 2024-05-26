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

  private void renderMachineNameField(
    SerializedProperty nameLinkedToMachineNameProperty,
    SerializedProperty machineNameProperty,
    SerializedProperty forceMachineNameProperty,
    Rect rect
  )
  {
    // Icon for the link/unlink button
    Texture2D autoIcon =
      (
        nameLinkedToMachineNameProperty.boolValue
          ? EditorGUIUtility.Load("d_Linked")
          : EditorGUIUtility.Load("d_Unlinked")
      ) as Texture2D;

    // create the content for the button
    GUIContent autoButtonContent;

    // if forceMachineNameProperty set to true then say option disabled
    if (forceMachineNameProperty.boolValue)
    {
      autoButtonContent = new(autoIcon, "Name Link option disabled as this is an imported Menu entry");
    }
    else
    {
      autoButtonContent = new(autoIcon, "Unlink/Link Name to Machine");
    }
    
    

    // Create a GUIStyle to set the size of the image
    GUIStyle buttonStyle = new GUIStyle();
    buttonStyle.fixedWidth = 40; // Set the width of the image
    buttonStyle.fixedHeight = 40; // Set the height of the image

    // Draw a button to set the 'nameLinkedToMachineName' property to true/false
    if (nameLinkedToMachineNameProperty != null)
    {
      // check if the 'forceMachineName' property is true, and if it is, then disable this button
      EditorGUI.BeginDisabledGroup(forceMachineNameProperty.boolValue);

      // Draw the button
      if (
        GUI.Button(
          new Rect(150, rect.y + EditorGUIUtility.singleLineHeight * 2, 20, 20),
          autoButtonContent,
          buttonStyle // Use the GUIStyle here
        )
      )
      {
        // if this change would make the 'nameLinkedToMachineName' property true, then warn the user that the
        // custom name will be overwritten
        if (nameLinkedToMachineNameProperty.boolValue)
        {
          // currently true going to false, no data loss
          nameLinkedToMachineNameProperty.boolValue = false;
        }
        else
        {
          // currently false going to true, data loss possible
          if (
            EditorUtility.DisplayDialog(
              "Warning",
              "This will overwrite the custom Machine Name. Do you want to continue?",
              "Yes",
              "No"
            )
          )
          {
            nameLinkedToMachineNameProperty.boolValue = true;
          }
          else
          {
            nameLinkedToMachineNameProperty.boolValue = false;
          }
        }

        serializedObject.ApplyModifiedProperties(); // Apply the changes
      }

      // End the disabled group
      EditorGUI.EndDisabledGroup();
    }

    // Draw field for the MachineName
    if (machineNameProperty != null)
    {
      // check if the 'nameLinkedToMachineName' property is true, and if it is, then disable the 'MachineName' field
      EditorGUI.BeginDisabledGroup(nameLinkedToMachineNameProperty.boolValue);

      // Draw field for the menuParameter
      EditorGUI.PropertyField(
        new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 2, rect.width, EditorGUIUtility.singleLineHeight),
        machineNameProperty,
        new GUIContent("Parameter")
      );

      // End the disabled group
      EditorGUI.EndDisabledGroup();
    }
  }

    private void legacyMachineNameFieldUpdate(
    SerializedProperty nameProperty,
    SerializedProperty machineNameProperty
    )
    {
      // if the value of machineNameProperty is empty, then set the value of machineNameProperty to the value of nameProperty
      if (machineNameProperty.stringValue == "")
      {
        machineNameProperty.stringValue = nameProperty.stringValue;
      }

      // save the changes
      serializedObject.ApplyModifiedProperties();
    }
}
#endif
