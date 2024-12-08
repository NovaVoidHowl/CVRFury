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
  private ReorderableList relatedParametersStoresList;
  private List<Type> menuTypes;
  private List<int> conflictingStoresIndices = new List<int>();

  private void OnEnable()
  {
    SerializedProperty items = serializedObject.FindProperty("menuItems");
    SerializedProperty relatedParametersStores = serializedObject.FindProperty("relatedParametersStores");

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
    relatedParametersStoresList = new ReorderableList(serializedObject, relatedParametersStores, true, true, true, true)
    {
      drawHeaderCallback = (Rect rect) =>
      {
        EditorGUI.LabelField(rect, "Related Parameters Stores");
      },
      drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
      {
        var element = relatedParametersStoresList.serializedProperty.GetArrayElementAtIndex(index);
        EditorGUI.PropertyField(rect, element, GUIContent.none);
        if (conflictingStoresIndices.Contains(index))
        {
          GUIContent warningContent = new GUIContent(
            EditorGUIUtility.IconContent("console.warnicon").image,
            "Conflicting default state values detected"
          );
          GUI.Label(
            new Rect(rect.x + rect.width - 40, rect.y + 2, 20, EditorGUIUtility.singleLineHeight),
            warningContent
          );
        }
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

    // Check if the relatedParametersStores list is empty
    bool isRelatedParametersStoresEmpty = serializedObject.FindProperty("relatedParametersStores").arraySize == 0;

    // Check for conflicts in the relatedParametersStores
    bool hasConflicts = CheckForConflicts();

    // Disable the buttons if the relatedParametersStores list is empty or if there are conflicts
    EditorGUI.BeginDisabledGroup(isRelatedParametersStoresEmpty || hasConflicts);

    if (GUILayout.Button("Copy Default State(s) from Related Parameters Stores"))
    {
      PullDefaultValues();
    }

    EditorGUI.EndDisabledGroup();

    EditorGUILayout.Space();

    if (GUILayout.Button("Push Default State Values to Related Parameters Stores"))
    {
      PushDefaultValues();
    }

    EditorGUILayout.Space();
    EditorGUILayout.Space();

    relatedParametersStoresList.DoLayoutList();
    EditorGUILayout.Space();
    list.DoLayoutList();

    serializedObject.ApplyModifiedProperties();
  }

  private bool CheckForConflicts()
  {
    CVRFuryMenuStore store = (CVRFuryMenuStore)target;
    conflictingStoresIndices.Clear();
    Dictionary<string, float> defaultValues = new Dictionary<string, float>();

    for (int i = 0; i < store.relatedParametersStores.Count; i++)
    {
      var parameterStore = store.relatedParametersStores[i];
      foreach (var parameter in parameterStore.parameters)
      {
        if (defaultValues.ContainsKey(parameter.name))
        {
          if (defaultValues[parameter.name] != parameter.defaultValue)
          {
            conflictingStoresIndices.Add(i);
          }
        }
        else
        {
          defaultValues[parameter.name] = parameter.defaultValue;
        }
      }
    }

    return conflictingStoresIndices.Count > 0;
  }

  private void PullDefaultValues()
  {
    CVRFuryMenuStore store = (CVRFuryMenuStore)target;

    foreach (var parameterStore in store.relatedParametersStores)
    {
      foreach (var parameter in parameterStore.parameters)
      {
        var menuItem = store.menuItems.FirstOrDefault(item => item.MachineName == parameter.name);
        if (menuItem != null)
        {
          switch (parameter.valueType)
          {
            case CVRFuryParametersStore.ValueType.Float:
              if (menuItem is sliderParameter slider)
              {
                slider.defaultValue = parameter.defaultValue;
              }
              break;
            case CVRFuryParametersStore.ValueType.Int:
              if (menuItem is dropdownParameter dropdown)
              {
                dropdown.defaultIndex = parameter.defaultValue;
              }
              break;
            case CVRFuryParametersStore.ValueType.Bool:
              if (menuItem is toggleParameter toggle)
              {
                toggle.defaultState = parameter.defaultValue;
              }
              break;
          }
        }
      }
    }

    serializedObject.ApplyModifiedProperties();
  }

  private void PushDefaultValues()
  {
    CVRFuryMenuStore store = (CVRFuryMenuStore)target;

    foreach (var parameterStore in store.relatedParametersStores)
    {
      foreach (var parameter in parameterStore.parameters)
      {
        var menuItem = store.menuItems.FirstOrDefault(item => item.MachineName == parameter.name);
        if (menuItem != null)
        {
          switch (parameter.valueType)
          {
            case CVRFuryParametersStore.ValueType.Float:
              if (menuItem is sliderParameter slider)
              {
                parameter.defaultValue = slider.defaultValue;
              }
              break;
            case CVRFuryParametersStore.ValueType.Int:
              if (menuItem is dropdownParameter dropdown)
              {
                parameter.defaultValue = dropdown.defaultIndex;
              }
              break;
            case CVRFuryParametersStore.ValueType.Bool:
              if (menuItem is toggleParameter toggle)
              {
                parameter.defaultValue = toggle.defaultState;
              }
              break;
          }
        }
      }
    }

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

  private void legacyMachineNameFieldUpdate(SerializedProperty nameProperty, SerializedProperty machineNameProperty)
  {
    // if the value of machineNameProperty is empty, then set the value of machineNameProperty to the value of nameProperty
    if (machineNameProperty.stringValue == "")
    {
      machineNameProperty.stringValue = nameProperty.stringValue;
    }

    serializedObject.ApplyModifiedProperties();
  }
}
#endif
