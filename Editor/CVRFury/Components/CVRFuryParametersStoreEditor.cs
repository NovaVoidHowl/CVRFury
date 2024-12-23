#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Animations;
using uk.novavoidhowl.dev.cvrfury.runtime;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;

namespace uk.novavoidhowl.dev.cvrfury.editor.components
{
  [CustomEditor(typeof(CVRFuryParametersStore))]
  public class CVRFuryParametersStoreEditor : Editor
  {
    private VisualElement rootVisualElement;
    private SerializedProperty controllersProperty;
    private SerializedProperty parametersProperty;

    // Add new button fields
    private Button importParametersButton;
    private Button copyDefaultsButton;
    private Button pushDefaultsButton;

    private ListView parametersList;

    private List<int> conflictingControllerIndices = new List<int>();
    private Dictionary<string, AnimatorControllerParameterType> parameterTypes =
      new Dictionary<string, AnimatorControllerParameterType>();

    private bool isUpdating = false; // for recursion prevention
    private ListView controllersList;

    private bool needsControllerUpdate = true;

    private void OnEnable()
    {
      rootVisualElement = new VisualElement();
      controllersProperty = serializedObject.FindProperty("relatedAnimationControllers");
      parametersProperty = serializedObject.FindProperty("parameters");
      CreateGUI();
      rootVisualElement.Bind(serializedObject);
      needsControllerUpdate = true;
    }

    public override VisualElement CreateInspectorGUI()
    {
      return rootVisualElement;
    }

    private void CreateGUI()
    {
      // Load UXML and stylesheet
      var visualTree = Resources.Load<VisualTreeAsset>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryComponents/UnityUXML/CVRFuryParametersStore"
      );
      if (visualTree == null)
      {
        CoreLogError("Failed to load UXML");
        return;
      }

      var styleSheet = Resources.Load<StyleSheet>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryComponents/UnityStyleSheets/CVRFuryParametersStore"
      );
      if (styleSheet != null)
        rootVisualElement.styleSheets.Add(styleSheet);

      // Instantiate UXML
      visualTree.CloneTree(rootVisualElement);

      // Get references
      var controllers = serializedObject.FindProperty("relatedAnimationControllers");
      controllersList = rootVisualElement.Q<ListView>("controllers-list");
      var addButton = rootVisualElement.Q<Button>("add-controller-button");
      var removeButton = rootVisualElement.Q<Button>("remove-controller-button");
      var parametersListField = rootVisualElement.Q<PropertyField>("parameters-list");
      var controllersHeader = rootVisualElement.Q<Label>("stores-header"); // Changed from controllers-header to stores-header

      // Get button references
      importParametersButton = rootVisualElement.Q<Button>("import-parameters-button");
      copyDefaultsButton = rootVisualElement.Q<Button>("copy-defaults-button");
      pushDefaultsButton = rootVisualElement.Q<Button>("push-defaults-button");

      // Add button handlers
      importParametersButton.clicked += ImportParametersFromControllers;
      copyDefaultsButton.clicked += CopyDefaultsFromControllers;
      pushDefaultsButton.clicked += PushDefaultsToControllers;

      // Validate required elements
      if (controllersHeader == null)
      {
        CoreLogError("Failed to find controllers header");
        return;
      }

      if (controllersList == null)
      {
        CoreLogError("Failed to find controllers list");
        return;
      }

      // Setup ListView
      controllersList.selectionType = SelectionType.Single; // Enable selection
      controllersList.reorderMode = ListViewReorderMode.Animated; // Add reorder support
      controllersList.reorderable = true; // Enable reordering
      var selectedControllerIndex = -1; // Track selected index
      controllersList.fixedItemHeight = 26;
      controllersList.makeItem = () => new ObjectField();
      controllersList.bindItem = (element, index) =>
      {
        var field = element.Q<ObjectField>();
        if (field == null)
        {
          field = new ObjectField { objectType = typeof(RuntimeAnimatorController) };
          field.AddToClassList("parameter-field");
          field.AddToClassList("parameter-leftmost-field");
          field.style.flexGrow = 1;
          field.Q<VisualElement>("unity-object-field__selector").style.display = DisplayStyle.None;
          element.Add(field);
        }

        var controllers = serializedObject.FindProperty("relatedAnimationControllers");
        if (controllers != null && index < controllers.arraySize)
        {
          var controllerProperty = controllers.GetArrayElementAtIndex(index);
          field.value = controllerProperty.objectReferenceValue;

          field.RegisterValueChangedCallback(evt =>
          {
            if (!isUpdating)
            {
              controllerProperty.objectReferenceValue = evt.newValue;
              serializedObject.ApplyModifiedProperties();
              UpdateFieldVisuals(field, evt.newValue != null);
              needsControllerUpdate = true;
            }
          });

          UpdateFieldVisuals(field, controllerProperty.objectReferenceValue != null);

          // Clear existing warning container if it exists
          var existingWarning = field.Q<VisualElement>("warning-container");
          if (existingWarning != null)
          {
            existingWarning.RemoveFromHierarchy();
          }

          // Add warning icon if controller has conflicts
          if (controllerProperty.objectReferenceValue != null && conflictingControllerIndices.Contains(index))
          {
            var warningContainer = new VisualElement { name = "warning-container" };
            warningContainer.AddToClassList("warning-container");

            var warningIcon = new Image
            {
              image = EditorGUIUtility.IconContent("console.warnicon.sml").image,
              tooltip = "Parameter type conflicts detected"
            };
            warningIcon.AddToClassList("warning-icon");

            warningContainer.Add(warningIcon);
            field.Add(warningContainer);
            field.AddToClassList("warning-state");
          }
          else
          {
            field.RemoveFromClassList("warning-state");
          }
        }
      };

      // Setup drag and drop
      SetupDragAndDrop(
        controllersHeader,
        controllersProperty,
        () =>
        {
          needsControllerUpdate = true;
          OnControllersChanged();
        }
      );

      // Initial update
      needsControllerUpdate = true;

      // Add handler for reordering
      controllersList.itemIndexChanged += (first, second) =>
      {
        var controllers = serializedObject.FindProperty("relatedAnimationControllers");

        // Store the controller being moved
        var movedController = controllers.GetArrayElementAtIndex(first).objectReferenceValue;

        // If moving down
        if (first < second)
        {
          for (int i = first; i < second; i++)
          {
            var current = controllers.GetArrayElementAtIndex(i);
            var next = controllers.GetArrayElementAtIndex(i + 1);
            current.objectReferenceValue = next.objectReferenceValue;
          }
        }
        // If moving up
        else
        {
          for (int i = first; i > second; i--)
          {
            var current = controllers.GetArrayElementAtIndex(i);
            var prev = controllers.GetArrayElementAtIndex(i - 1);
            current.objectReferenceValue = prev.objectReferenceValue;
          }
        }

        // Place moved item in new position
        controllers.GetArrayElementAtIndex(second).objectReferenceValue = movedController;

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
      };

      // Add selection change handler
      controllersList.onSelectionChange += (items) =>
      {
        selectedControllerIndex = controllersList.selectedIndex;
      };

      void UpdateListSource() => controllersList.itemsSource = Enumerable.Range(0, controllers.arraySize).ToList();

      UpdateListSource();

      // Setup buttons
      addButton.clicked += () =>
      {
        controllers.arraySize++;
        serializedObject.ApplyModifiedProperties();
        UpdateListSource();
      };

      removeButton.clicked += () =>
      {
        if (
          controllers.arraySize > 0 && selectedControllerIndex >= 0 && selectedControllerIndex < controllers.arraySize
        )
        {
          // Shift elements up to remove selected item
          for (int i = selectedControllerIndex; i < controllers.arraySize - 1; i++)
          {
            var currentElement = controllers.GetArrayElementAtIndex(i);
            var nextElement = controllers.GetArrayElementAtIndex(i + 1);
            currentElement.objectReferenceValue = nextElement.objectReferenceValue;
          }

          controllers.arraySize--;
          selectedControllerIndex = -1; // Reset selection
          serializedObject.ApplyModifiedProperties();
          UpdateListSource();
          CheckForTypeConflicts();
          EditorUtility.SetDirty(target);
        }
      };

      // Setup parameter list binding
      if (parametersListField != null)
      {
        parametersListField.RegisterValueChangeCallback(evt =>
        {
          serializedObject.ApplyModifiedProperties();
        });
      }

      // Register serialization callback
      rootVisualElement.RegisterCallback<SerializedPropertyChangeEvent>(evt =>
      {
        if (evt.changedProperty.propertyPath is "parameters" or "relatedAnimationControllers")
          serializedObject.ApplyModifiedProperties();
      });

      // After existing controller setup, add:
      parametersList = rootVisualElement.Q<ListView>("parameters-list");
      if (parametersList != null)
      {
        parametersList.viewDataKey = "CVRFuryParametersStore_ParametersList";
        parametersList.reorderMode = ListViewReorderMode.Animated;
        parametersList.reorderable = true;
        parametersList.showAddRemoveFooter = false;
        parametersList.showFoldoutHeader = false;
        parametersList.fixedItemHeight = 26;
        parametersList.selectionType = SelectionType.Single;

        var parameters = serializedObject?.FindProperty("parameters");
        if (parameters != null)
        {
          parametersList.itemsSource = Enumerable.Range(0, parameters.arraySize).ToList();
        }

        parametersList.makeItem = () =>
        {
          var itemContainer = new VisualElement();
          itemContainer.name = "parameter-item-" + System.Guid.NewGuid().ToString();
          itemContainer.AddToClassList("unity-list-view__reorderable-item");
          itemContainer.AddToClassList("unity-collection-view__item");
          itemContainer.AddToClassList("unity-list-view__item");
          itemContainer.style.flexDirection = FlexDirection.Row;

          var contentContainer = new VisualElement();
          contentContainer.name = "parameter-content";
          contentContainer.AddToClassList("parameter-content");
          contentContainer.style.flexGrow = 1;
          contentContainer.style.flexDirection = FlexDirection.Row;
          itemContainer.Add(contentContainer);

          return itemContainer;
        };

        parametersList.bindItem = (element, index) =>
        {
          try
          {
            if (element == null)
              return;

            // First try by name, then by class
            var contentContainer =
              element.Q<VisualElement>("parameter-content") ?? element.Q<VisualElement>(className: "parameter-content");

            if (contentContainer == null)
            {
              Debug.LogError($"Content container not found for index {index}");
              return;
            }

            contentContainer.Clear();

            var parameters = serializedObject?.FindProperty("parameters");
            if (parameters == null || index >= parameters.arraySize)
              return;

            var parameter = parameters.GetArrayElementAtIndex(index);
            if (parameter == null)
              return;

            // Name field
            var nameField = new TextField() { value = parameter.FindPropertyRelative("name")?.stringValue ?? "" };
            nameField.AddToClassList("parameter-name-field");
            nameField.RegisterValueChangedCallback(evt =>
            {
              if (parameter != null && serializedObject != null)
              {
                parameter.FindPropertyRelative("name").stringValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
              }
            });
            contentContainer.Add(nameField);

            // Type dropdown
            var typeField = new EnumField(CVRFuryParametersStore.ValueType.Float)
            {
              value = (CVRFuryParametersStore.ValueType)(
                parameter.FindPropertyRelative("valueType")?.enumValueIndex ?? 0
              )
            };
            typeField.AddToClassList("parameter-type-field");
            typeField.RegisterValueChangedCallback(evt =>
            {
              if (parameter != null && serializedObject != null)
              {
                parameter.FindPropertyRelative("valueType").enumValueIndex = evt.newValue.GetHashCode();
                serializedObject.ApplyModifiedProperties();
              }
            });
            contentContainer.Add(typeField);

            // Default value field
            var defaultValueField = new FloatField()
            {
              value = parameter.FindPropertyRelative("defaultValue")?.floatValue ?? 0f
            };
            defaultValueField.AddToClassList("parameter-value-field");
            defaultValueField.RegisterValueChangedCallback(evt =>
            {
              if (parameter != null && serializedObject != null)
              {
                parameter.FindPropertyRelative("defaultValue").floatValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
              }
            });
            contentContainer.Add(defaultValueField);
          }
          catch (Exception e)
          {
            Debug.LogError($"Error in bindItem for index {index}: {e}");
          }
        };

        parametersList.Rebuild();
      }
      var addParameterButton = rootVisualElement.Q<Button>("add-parameter-button");
      var removeParameterButton = rootVisualElement.Q<Button>("remove-parameter-button");

      // Add handler for reordering
      parametersList.itemIndexChanged += (first, second) =>
      {
        var parameters = serializedObject.FindProperty("parameters");

        // Store the parameter being moved
        var firstParam = new CVRFuryParametersStore.Parameter
        {
          name = parameters.GetArrayElementAtIndex(first).FindPropertyRelative("name").stringValue,
          valueType = (CVRFuryParametersStore.ValueType)
            parameters.GetArrayElementAtIndex(first).FindPropertyRelative("valueType").enumValueIndex,
          defaultValue = parameters.GetArrayElementAtIndex(first).FindPropertyRelative("defaultValue").floatValue
        };

        // If moving down
        if (first < second)
        {
          for (int i = first; i < second; i++)
          {
            var current = parameters.GetArrayElementAtIndex(i);
            var next = parameters.GetArrayElementAtIndex(i + 1);
            current.FindPropertyRelative("name").stringValue = next.FindPropertyRelative("name").stringValue;
            current.FindPropertyRelative("valueType").enumValueIndex = next.FindPropertyRelative(
              "valueType"
            ).enumValueIndex;
            current.FindPropertyRelative("defaultValue").floatValue = next.FindPropertyRelative(
              "defaultValue"
            ).floatValue;
          }
        }
        // If moving up
        else
        {
          for (int i = first; i > second; i--)
          {
            var current = parameters.GetArrayElementAtIndex(i);
            var prev = parameters.GetArrayElementAtIndex(i - 1);
            current.FindPropertyRelative("name").stringValue = prev.FindPropertyRelative("name").stringValue;
            current.FindPropertyRelative("valueType").enumValueIndex = prev.FindPropertyRelative(
              "valueType"
            ).enumValueIndex;
            current.FindPropertyRelative("defaultValue").floatValue = prev.FindPropertyRelative(
              "defaultValue"
            ).floatValue;
          }
        }

        // Place moved item in new position
        var targetParam = parameters.GetArrayElementAtIndex(second);
        targetParam.FindPropertyRelative("name").stringValue = firstParam.name;
        targetParam.FindPropertyRelative("valueType").enumValueIndex = (int)firstParam.valueType;
        targetParam.FindPropertyRelative("defaultValue").floatValue = firstParam.defaultValue;

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
      };

      UpdateParametersSource();

      // Add selection support
      parametersList.selectionType = SelectionType.Single;
      var selectedIndex = -1;
      parametersList.onSelectionChange += (items) =>
      {
        selectedIndex = parametersList.selectedIndex;
      };

      // Setup parameter buttons
      addParameterButton.clicked += () =>
      {
        var parameters = serializedObject.FindProperty("parameters");
        parameters.arraySize++;
        var newParam = parameters.GetArrayElementAtIndex(parameters.arraySize - 1);
        newParam.FindPropertyRelative("name").stringValue = "New Parameter";
        newParam.FindPropertyRelative("valueType").enumValueIndex = 0;
        newParam.FindPropertyRelative("defaultValue").floatValue = 0f;
        serializedObject.ApplyModifiedProperties();
        UpdateParametersSource();
      };

      removeParameterButton.clicked += () =>
      {
        var parameters = serializedObject.FindProperty("parameters");
        if (parameters.arraySize > 0 && selectedIndex >= 0 && selectedIndex < parameters.arraySize)
        {
          // Shift elements up to remove selected item
          for (int i = selectedIndex; i < parameters.arraySize - 1; i++)
          {
            var currentElement = parameters.GetArrayElementAtIndex(i);
            var nextElement = parameters.GetArrayElementAtIndex(i + 1);
            currentElement.FindPropertyRelative("name").stringValue = nextElement
              .FindPropertyRelative("name")
              .stringValue;
            currentElement.FindPropertyRelative("valueType").enumValueIndex = nextElement
              .FindPropertyRelative("valueType")
              .enumValueIndex;
            currentElement.FindPropertyRelative("defaultValue").floatValue = nextElement
              .FindPropertyRelative("defaultValue")
              .floatValue;
          }

          parameters.arraySize--;
          selectedIndex = -1; // Reset selection
          serializedObject.ApplyModifiedProperties();
          UpdateParametersSource();
        }
      };

      var sortAlphaButton = rootVisualElement.Q<Button>("sort-alpha-button");
      var sortTypeButton = rootVisualElement.Q<Button>("sort-type-button");
      var sortZAButton = rootVisualElement.Q<Button>("sort-za-button");

      sortAlphaButton.clicked += SortParametersAlphabetically;
      sortTypeButton.clicked += SortParametersByType;
      sortZAButton.clicked += SortParametersReverseAlphabetically;

      CheckForTypeConflicts(); // Add this call at the end of CreateGUI() after setting up the controllersList

      controllersList.RegisterCallback<GeometryChangedEvent>(evt =>
      {
        OnControllersChanged();
      });

      controllersList.RegisterCallback<SerializedPropertyChangeEvent>(evt =>
      {
        if (evt.changedProperty.propertyPath.StartsWith("relatedAnimationControllers"))
        {
          OnControllersChanged();
        }
      });

      // Force initial update
      OnControllersChanged();
    }

    private void SetupDragAndDrop(VisualElement target, SerializedProperty controllers, Action updateSource)
    {
      target.RegisterCallback<DragEnterEvent>(evt =>
      {
        if (DragAndDrop.objectReferences.Any(obj => obj is RuntimeAnimatorController))
        {
          DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
          evt.StopPropagation();
        }
      });

      target.RegisterCallback<DragUpdatedEvent>(evt =>
      {
        if (DragAndDrop.objectReferences.Any(obj => obj is RuntimeAnimatorController))
        {
          DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
          evt.StopPropagation();
        }
      });

      target.RegisterCallback<DragPerformEvent>(evt =>
      {
        foreach (var obj in DragAndDrop.objectReferences)
        {
          if (obj is RuntimeAnimatorController controller)
          {
            controllers.arraySize++;
            controllers.GetArrayElementAtIndex(controllers.arraySize - 1).objectReferenceValue = controller;
            serializedObject.ApplyModifiedProperties();
          }
        }
        updateSource();
        CheckForTypeConflicts(); // Add this line
        evt.StopPropagation();
      });
    }

    private void UpdateFieldVisuals(ObjectField field, bool hasValue)
    {
      if (!hasValue)
      {
        field.AddToClassList("empty-slot");
      }
      else
      {
        field.RemoveFromClassList("empty-slot");
      }
    }

    public override void OnInspectorGUI()
    {
      serializedObject.Update();

      if (needsControllerUpdate)
      {
        CheckForTypeConflicts();
        needsControllerUpdate = false;
      }

      serializedObject.ApplyModifiedProperties();
    }

    private void ImportParametersFromControllers()
    {
      var store = target as CVRFuryParametersStore;
      var parameters = new List<CVRFuryParametersStore.Parameter>();
      var processedNames = new HashSet<string>();

      foreach (var controller in store.relatedAnimationControllers)
      {
        if (controller != null)
        {
          var animController = controller as AnimatorController;
          if (animController != null)
          {
            foreach (var parameter in animController.parameters)
            {
              if (!processedNames.Contains(parameter.name))
              {
                processedNames.Add(parameter.name);
                float defaultValue = parameter.type switch
                {
                  AnimatorControllerParameterType.Float => parameter.defaultFloat,
                  AnimatorControllerParameterType.Int => parameter.defaultInt,
                  AnimatorControllerParameterType.Bool => parameter.defaultBool ? 1f : 0f,
                  _ => 0f
                };

                var newParam = new CVRFuryParametersStore.Parameter
                {
                  name = parameter.name,
                  valueType = ConvertParameterType(parameter.type),
                  defaultValue = defaultValue
                };
                parameters.Add(newParam);
              }
            }
          }
        }
      }

      serializedObject.FindProperty("parameters").ClearArray();
      foreach (var param in parameters)
      {
        var index = serializedObject.FindProperty("parameters").arraySize++;
        var element = serializedObject.FindProperty("parameters").GetArrayElementAtIndex(index);
        element.FindPropertyRelative("name").stringValue = param.name;
        element.FindPropertyRelative("valueType").enumValueIndex = (int)param.valueType;
        element.FindPropertyRelative("defaultValue").floatValue = param.defaultValue;
      }

      serializedObject.ApplyModifiedProperties();
      EditorUtility.SetDirty(target);

      CheckForTypeConflicts(); // Add this line

      // update parameters list
      parametersList.itemsSource = Enumerable.Range(0, serializedObject.FindProperty("parameters").arraySize).ToList();
      parametersList.Rebuild();
    }

    private void CopyDefaultsFromControllers()
    {
      var store = target as CVRFuryParametersStore;
      var parameters = serializedObject.FindProperty("parameters");
      var parameterMap = new Dictionary<string, int>();

      // Build parameter map with indices
      for (int i = 0; i < parameters.arraySize; i++)
      {
        var param = parameters.GetArrayElementAtIndex(i);
        parameterMap[param.FindPropertyRelative("name").stringValue] = i;
      }

      // Update defaults from controllers
      foreach (var controller in store.relatedAnimationControllers)
      {
        if (controller != null)
        {
          var animController = controller as AnimatorController;
          if (animController != null)
          {
            foreach (var parameter in animController.parameters)
            {
              if (parameterMap.TryGetValue(parameter.name, out var index))
              {
                var param = parameters.GetArrayElementAtIndex(index);
                float newValue = parameter.type switch
                {
                  AnimatorControllerParameterType.Float => parameter.defaultFloat,
                  AnimatorControllerParameterType.Int => parameter.defaultInt,
                  AnimatorControllerParameterType.Bool => parameter.defaultBool ? 1f : 0f,
                  _ => 0f
                };
                param.FindPropertyRelative("defaultValue").floatValue = newValue;
              }
            }
          }
        }
      }

      serializedObject.ApplyModifiedProperties();
      EditorUtility.SetDirty(target);

      // Force UI updates
      UpdateParametersSource();
      parametersList.Rebuild();

      // Repaint the inspector
      Repaint();
    }

    private void PushDefaultsToControllers()
    {
      var store = target as CVRFuryParametersStore;
      var parameterMap = new Dictionary<string, CVRFuryParametersStore.Parameter>();

      // Build parameter map
      foreach (var param in store.parameters)
      {
        parameterMap[param.name] = param;
      }

      // Update controller defaults
      foreach (var controller in store.relatedAnimationControllers)
      {
        if (controller != null)
        {
          var animController = controller as AnimatorController;
          if (animController != null)
          {
            var serializedController = new SerializedObject(controller);
            var parameters = serializedController.FindProperty("m_AnimatorParameters");

            for (int i = 0; i < parameters.arraySize; i++)
            {
              var parameter = parameters.GetArrayElementAtIndex(i);
              var name = parameter.FindPropertyRelative("m_Name").stringValue;
              var typeProperty = parameter.FindPropertyRelative("m_Type");
              var paramType = (AnimatorControllerParameterType)typeProperty.intValue;

              if (parameterMap.TryGetValue(name, out var storeParam))
              {
                var defaultValue = storeParam.defaultValue;
                switch (paramType)
                {
                  case AnimatorControllerParameterType.Float:
                    parameter.FindPropertyRelative("m_DefaultFloat").floatValue = defaultValue;
                    break;
                  case AnimatorControllerParameterType.Int:
                    parameter.FindPropertyRelative("m_DefaultInt").intValue = (int)defaultValue;
                    break;
                  case AnimatorControllerParameterType.Bool:
                    parameter.FindPropertyRelative("m_DefaultBool").boolValue = defaultValue >= 0.5f;
                    break;
                }
              }
            }

            serializedController.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
          }
        }
      }
    }

    private CVRFuryParametersStore.ValueType ConvertParameterType(AnimatorControllerParameterType type)
    {
      return type switch
      {
        AnimatorControllerParameterType.Float => CVRFuryParametersStore.ValueType.Float,
        AnimatorControllerParameterType.Int => CVRFuryParametersStore.ValueType.Int,
        AnimatorControllerParameterType.Bool => CVRFuryParametersStore.ValueType.Bool,
        _ => CVRFuryParametersStore.ValueType.Float
      };
    }

    private CVRFuryParametersStore.ValueType ConvertParameterType(int typeIndex)
    {
      return (AnimatorControllerParameterType)typeIndex switch
      {
        AnimatorControllerParameterType.Float => CVRFuryParametersStore.ValueType.Float,
        AnimatorControllerParameterType.Int => CVRFuryParametersStore.ValueType.Int,
        AnimatorControllerParameterType.Bool => CVRFuryParametersStore.ValueType.Bool,
        _ => CVRFuryParametersStore.ValueType.Float
      };
    }

    private void SortParametersAlphabetically()
    {
      var parameters = serializedObject.FindProperty("parameters");
      var paramList = new List<(string name, int type, float value)>();

      // Gather parameters
      for (int i = 0; i < parameters.arraySize; i++)
      {
        var param = parameters.GetArrayElementAtIndex(i);
        paramList.Add(
          (
            param.FindPropertyRelative("name").stringValue,
            param.FindPropertyRelative("valueType").enumValueIndex,
            param.FindPropertyRelative("defaultValue").floatValue
          )
        );
      }

      // Sort alphabetically
      paramList.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));

      // Apply sorted order
      for (int i = 0; i < parameters.arraySize; i++)
      {
        var param = parameters.GetArrayElementAtIndex(i);
        param.FindPropertyRelative("name").stringValue = paramList[i].name;
        param.FindPropertyRelative("valueType").enumValueIndex = paramList[i].type;
        param.FindPropertyRelative("defaultValue").floatValue = paramList[i].value;
      }

      serializedObject.ApplyModifiedProperties();
      EditorUtility.SetDirty(target);
      parametersList.Rebuild();
    }

    private void SortParametersByType()
    {
      var parameters = serializedObject.FindProperty("parameters");
      var paramList = new List<(string name, int type, float value)>();

      // Gather parameters
      for (int i = 0; i < parameters.arraySize; i++)
      {
        var param = parameters.GetArrayElementAtIndex(i);
        paramList.Add(
          (
            param.FindPropertyRelative("name").stringValue,
            param.FindPropertyRelative("valueType").enumValueIndex,
            param.FindPropertyRelative("defaultValue").floatValue
          )
        );
      }

      // Sort by type, then alphabetically within each type
      paramList.Sort(
        (a, b) =>
        {
          var typeCompare = a.type.CompareTo(b.type);
          return typeCompare != 0 ? typeCompare : string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
        }
      );

      // Apply sorted order
      for (int i = 0; i < parameters.arraySize; i++)
      {
        var param = parameters.GetArrayElementAtIndex(i);
        param.FindPropertyRelative("name").stringValue = paramList[i].name;
        param.FindPropertyRelative("valueType").enumValueIndex = paramList[i].type;
        param.FindPropertyRelative("defaultValue").floatValue = paramList[i].value;
      }

      serializedObject.ApplyModifiedProperties();
      EditorUtility.SetDirty(target);
      parametersList.Rebuild();
    }

    private void SortParametersReverseAlphabetically()
    {
      var parameters = serializedObject.FindProperty("parameters");
      var paramList = new List<(string name, int type, float value)>();

      // Gather parameters
      for (int i = 0; i < parameters.arraySize; i++)
      {
        var param = parameters.GetArrayElementAtIndex(i);
        paramList.Add(
          (
            param.FindPropertyRelative("name").stringValue,
            param.FindPropertyRelative("valueType").enumValueIndex,
            param.FindPropertyRelative("defaultValue").floatValue
          )
        );
      }

      // Sort reverse alphabetically
      paramList.Sort((a, b) => string.Compare(b.name, a.name, StringComparison.OrdinalIgnoreCase));

      // Apply sorted order
      for (int i = 0; i < parameters.arraySize; i++)
      {
        var param = parameters.GetArrayElementAtIndex(i);
        param.FindPropertyRelative("name").stringValue = paramList[i].name;
        param.FindPropertyRelative("valueType").enumValueIndex = paramList[i].type;
        param.FindPropertyRelative("defaultValue").floatValue = paramList[i].value;
      }

      serializedObject.ApplyModifiedProperties();
      EditorUtility.SetDirty(target);
      parametersList.Rebuild();
    }

    private void UpdateParametersSource()
    {
      parametersList.itemsSource = Enumerable.Range(0, serializedObject.FindProperty("parameters").arraySize).ToList();
      parametersList.Rebuild();
    }

    private void UpdateButtonStates(bool hasConflicts)
    {
      if (importParametersButton != null)
        importParametersButton.SetEnabled(!hasConflicts);
      if (copyDefaultsButton != null)
        copyDefaultsButton.SetEnabled(!hasConflicts);
      if (pushDefaultsButton != null)
        pushDefaultsButton.SetEnabled(!hasConflicts);
    }

    private void ForceParameterFieldsRebuild()
    {
      if (parametersList == null || parametersList.itemsSource == null)
        return;

      for (int i = 0; i < parametersList.itemsSource.Count; i++)
      {
        var element = parametersList.GetRootElementForIndex(i);
        if (element != null)
        {
          var contentContainer = element.Q<VisualElement>("parameter-content");
          if (contentContainer == null)
          {
            contentContainer = new VisualElement();
            contentContainer.name = "parameter-content";
            contentContainer.AddToClassList("parameter-content");
            contentContainer.style.flexGrow = 1;
            contentContainer.style.flexDirection = FlexDirection.Row;
            element.Add(contentContainer);
          }
          contentContainer.Clear();
          parametersList.bindItem.Invoke(element, i);
        }
      }
    }

    private void OnControllersChanged()
    {
      if (isUpdating)
        return;

      try
      {
        isUpdating = true;

        serializedObject.Update();

        // Force complete rebuild of parameters
        if (parametersList != null)
        {
          var parameters = serializedObject?.FindProperty("parameters");
          if (parameters != null)
          {
            parametersList.itemsSource = Enumerable.Range(0, parameters.arraySize).ToList();
            parametersList.Rebuild();
            ForceParameterFieldsRebuild();
          }
        }

        // Force complete rebuild of controllers
        if (controllersList != null)
        {
          var controllers = serializedObject?.FindProperty("relatedAnimationControllers");
          if (controllers != null)
          {
            controllersList.itemsSource = Enumerable.Range(0, controllers.arraySize).ToList();
            controllersList.Rebuild();
          }
        }

        needsControllerUpdate = true;

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
      }
      finally
      {
        isUpdating = false;
      }
    }

    private void CheckForTypeConflicts()
    {
      if (isUpdating)
        return;

      try
      {
        isUpdating = true;
        conflictingControllerIndices.Clear();
        parameterTypes.Clear();

        var store = target as CVRFuryParametersStore;
        if (store == null)
          return;

        // Dictionary to store conflicts
        var conflicts = new Dictionary<string, List<(int, AnimatorControllerParameterType)>>();

        // First pass: collect all parameter types
        for (int i = 0; i < store.relatedAnimationControllers.Count; i++)
        {
          var controller = store.relatedAnimationControllers[i] as AnimatorController;
          if (controller == null)
            continue;

          foreach (var parameter in controller.parameters)
          {
            if (!parameterTypes.ContainsKey(parameter.name))
            {
              parameterTypes[parameter.name] = parameter.type;
            }
          }
        }

        // Second pass: check for conflicts
        for (int i = 0; i < store.relatedAnimationControllers.Count; i++)
        {
          var controller = store.relatedAnimationControllers[i] as AnimatorController;
          if (controller == null)
            continue;

          foreach (var parameter in controller.parameters)
          {
            if (parameterTypes.TryGetValue(parameter.name, out var expectedType))
            {
              if (expectedType != parameter.type)
              {
                // Add both the current controller and the first controller that defined this parameter
                if (!conflicts.ContainsKey(parameter.name))
                {
                  conflicts[parameter.name] = new List<(int, AnimatorControllerParameterType)>();
                  // Find all controllers that have this parameter
                  for (int j = 0; j < store.relatedAnimationControllers.Count; j++)
                  {
                    var otherController = store.relatedAnimationControllers[j] as AnimatorController;
                    if (otherController != null)
                    {
                      var param = otherController.parameters.FirstOrDefault(p => p.name == parameter.name);
                      if (param != null)
                      {
                        conflicts[parameter.name].Add((j, param.type));
                        // Add to conflicting indices if not already added
                        if (!conflictingControllerIndices.Contains(j))
                        {
                          conflictingControllerIndices.Add(j);
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }

        // Update the conflict banner using the helper method
        UpdateConflictBanner(store, conflicts);

        // Ensure parameters list is properly updated
        if (parametersList != null)
        {
          parametersList.Rebuild();
          ForceParameterFieldsRebuild();
        }

        if (controllersList != null)
        {
          controllersList.Rebuild();
        }

        // Update button states based on conflicts
        UpdateButtonStates(conflicts.Count > 0);

        // Force repaint
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
        Repaint();
      }
      finally
      {
        isUpdating = false;
      }
    }

    // Helper method to update the conflict banner
    private void UpdateConflictBanner(
      CVRFuryParametersStore store,
      Dictionary<string, List<(int, AnimatorControllerParameterType)>> conflicts
    )
    {
      var messageBanner = rootVisualElement.Q<VisualElement>("message-banner");
      if (conflicts.Count > 0)
      {
        messageBanner.Clear();
        messageBanner.style.display = DisplayStyle.Flex;
        messageBanner.AddToClassList("message-banner-error");

        var messageText = "Parameter type conflicts detected:\n";
        foreach (var conflict in conflicts)
        {
          messageText += $"\n• Parameter '{conflict.Key}' has conflicting types:";
          foreach (var (index, type) in conflict.Value)
          {
            var controllerName = (store.relatedAnimationControllers[index] as AnimatorController)?.name ?? "Unknown";
            messageText += $"\n  - {controllerName}: {type}";
          }
        }

        var label = new Label(messageText);
        label.AddToClassList("message-banner-text");
        messageBanner.Add(label);
      }
      else
      {
        messageBanner.style.display = DisplayStyle.None;
        messageBanner.RemoveFromClassList("message-banner-error");
        messageBanner.Clear();
      }
    }
  }
}
#endif // UNITY_EDITOR
