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
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;

[CustomEditor(typeof(CVRFuryMenuStore))]
public partial class CVRFuryMenuStoreEditor : Editor
{
  private ReorderableList list;
  private ReorderableList relatedParametersStoresList;
  private List<Type> menuTypes;
  private List<int> conflictingStoresIndices = new List<int>();

  // UIToolkit elements
  private VisualElement rootVisualElement;
  private Button pullDefaultsButton;
  private Button pushDefaultsButton;

  private bool isUpdating = false;

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

  public override VisualElement CreateInspectorGUI()
  {
    // Create root container
    rootVisualElement = new VisualElement();
    rootVisualElement.AddToClassList("cvr-fury-inspector");

    // Load stylesheet
    var stylesheet = Resources.Load<StyleSheet>(
      Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryComponents/UnityStyleSheets/CVRFuryMenuStore"
    );

    if (stylesheet == null)
    {
      CoreLogError("Failed to load StyleSheet");
      rootVisualElement.Add(new Label("CRITICAL ERROR : StyleSheet could not be loaded."));
      return rootVisualElement;
    }

    rootVisualElement.styleSheets.Add(stylesheet);

    // Create button container
    var buttonContainer = new VisualElement();
    buttonContainer.AddToClassList("cvr-fury-buttons-container");

    // Create buttons
    pullDefaultsButton = new Button(() => PullDefaultValues())
    {
      text = "Copy Default State(s) from Related Parameters Stores"
    };
    pullDefaultsButton.AddToClassList("cvr-fury-button");

    pushDefaultsButton = new Button(() => PushDefaultValues())
    {
      text = "Push Default State Values to Related Parameters Stores"
    };
    pushDefaultsButton.AddToClassList("cvr-fury-button");

    // Create Related Parameters Stores container
    var storesContainer = new VisualElement();
    storesContainer.AddToClassList("stores-container");

    var storesHeader = new Label("Related Parameters Stores");
    storesHeader.AddToClassList("stores-header");
    storesContainer.Add(storesHeader);

    // Create ListView for stores
    var storesList = new ListView();
    storesList.makeItem = () => new ObjectField();
    storesList.bindItem = (element, index) =>
    {
      var field = element as ObjectField;
      field.objectType = typeof(CVRFuryParametersStore);

      var relatedStores = serializedObject.FindProperty("relatedParametersStores");
      if (index < relatedStores.arraySize)
      {
        var storeProperty = relatedStores.GetArrayElementAtIndex(index);
        field.value = storeProperty.objectReferenceValue;

        // Find existing warning container and remove it
        var existingWarningContainer = field.Q<VisualElement>("warning-container");
        if (existingWarningContainer != null)
        {
          field.Remove(existingWarningContainer);
        }

        field.RegisterValueChangedCallback(evt =>
        {
          if (isUpdating)
            return;

          isUpdating = true;
          storeProperty.objectReferenceValue = evt.newValue;
          serializedObject.ApplyModifiedProperties();

          // Refresh conflict detection
          CheckForConflicts();

          // Force UI refresh
          EditorApplication.delayCall += () =>
          {
            storesList.Rebuild();
            isUpdating = false;
          };
        });

        // Add warning icon if conflicting
        if (conflictingStoresIndices.Contains(index))
        {
          var warningContainer = new VisualElement { name = "warning-container" };
          var warningIcon = new Image { image = EditorGUIUtility.IconContent("console.warnicon").image };
          warningIcon.tooltip = "Conflicting default state values detected";
          warningIcon.AddToClassList("warning-icon");
          warningContainer.Add(warningIcon);
          field.Add(warningContainer);
        }
      }
    };

    // Get array size and set itemsSource
    var relatedStores = serializedObject.FindProperty("relatedParametersStores");
    storesList.itemsSource = Enumerable.Range(0, relatedStores.arraySize).ToList();

    // Add list manipulation buttons
    var listControls = new VisualElement();
    listControls.style.flexDirection = FlexDirection.Row;

    var addButton = new Button(() =>
    {
      relatedStores.arraySize++;
      storesList.itemsSource = Enumerable.Range(0, relatedStores.arraySize).ToList();
      serializedObject.ApplyModifiedProperties();
      storesList.Rebuild(); // Force refresh
    })
    {
      text = "+"
    };

    var removeButton = new Button(() =>
    {
      if (relatedStores.arraySize > 0)
      {
        relatedStores.arraySize--;
        storesList.itemsSource = Enumerable.Range(0, relatedStores.arraySize).ToList();
        serializedObject.ApplyModifiedProperties();
        storesList.Rebuild(); // Force refresh
      }
    })
    {
      text = "-"
    };

    listControls.Add(addButton);
    listControls.Add(removeButton);

    storesContainer.Add(storesList);
    storesContainer.Add(listControls);

    // Create IMGUI container for remaining list
    var imguiContainer = new IMGUIContainer(() =>
    {
      serializedObject.Update();
      list.DoLayoutList();
      serializedObject.ApplyModifiedProperties();
    });

    // Add elements to root
    buttonContainer.Add(pullDefaultsButton);
    buttonContainer.Add(new VisualElement() { name = "spacer" });
    buttonContainer.Add(pushDefaultsButton);

    rootVisualElement.Add(buttonContainer);
    rootVisualElement.Add(new VisualElement() { name = "spacer" });
    rootVisualElement.Add(storesContainer);
    rootVisualElement.Add(new VisualElement() { name = "spacer" });
    rootVisualElement.Add(imguiContainer);

    // Add USS styles for new elements
    var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
      "Assets/Resources/CVRFury/CVRFuryComponents/UnityStyleSheets/CVRFuryMenuStore.uss"
    );
    if (styleSheet != null)
    {
      rootVisualElement.styleSheets.Add(styleSheet);
    }

    return rootVisualElement;
  }

  // Remove button state updates from OnInspectorGUI since they're now handled in IMGUIContainer
  public override void OnInspectorGUI()
  {
    serializedObject.Update();
    serializedObject.ApplyModifiedProperties();
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

  private bool CheckForConflicts()
  {
    var store = target as CVRFuryMenuStore;
    if (store == null || store.relatedParametersStores == null)
    {
      return false;
    }

    // Check for null entries in the list
    bool hasEmptySlots = store.relatedParametersStores.Any(x => x == null);
    if (hasEmptySlots)
    {
      return true; // Treat empty slots as a conflict to disable buttons
    }

    conflictingStoresIndices.Clear();
    Dictionary<string, float> defaultValues = new Dictionary<string, float>();

    for (int i = 0; i < store.relatedParametersStores.Count; i++)
    {
      var parameterStore = store.relatedParametersStores[i];
      if (parameterStore == null || parameterStore.parameters == null)
      {
        continue;
      }

      foreach (var parameter in parameterStore.parameters)
      {
        if (parameter == null || string.IsNullOrEmpty(parameter.name))
        {
          continue;
        }

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

    return conflictingStoresIndices.Count > 0 || hasEmptySlots;
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
