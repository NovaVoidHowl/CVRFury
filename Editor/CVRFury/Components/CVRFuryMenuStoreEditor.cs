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
public class CVRFuryMenuStoreEditor : Editor
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
          // If the menuParameter is a toggleParameter, add height for its properties
          if (shortTypeName == "toggleParameter")
          {
            height += 4 * EditorGUIUtility.singleLineHeight;
            SerializedProperty targetsProperty = element.FindPropertyRelative("targets");
            if (targetsProperty != null)
            {
              height += targetsProperty.arraySize * EditorGUIUtility.singleLineHeight;
            }

            if (targetsProperty != null)
            {
              ReorderableList targetsList = new ReorderableList(
                serializedObject,
                targetsProperty,
                true, // draggable
                true, // displayHeader
                true, // displayAddButton
                true // displayRemoveButton
              );

              height += targetsList.GetHeight();
            }
          }

          // If the menuParameter is a testParameter, add height for its properties
          if (shortTypeName == "testParameter")
          {
            height += 2 * EditorGUIUtility.singleLineHeight;
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

          // If the menuParameter is a toggleParameter, draw fields for its properties
          if (shortTypeName == "toggleParameter")
          {
            SerializedProperty defaultStateProperty = element.FindPropertyRelative("defaultState");
            SerializedProperty useAnimationProperty = element.FindPropertyRelative("useAnimation");
            SerializedProperty generateTypeProperty = element.FindPropertyRelative("generateType");
            ReorderableList targetsList = null;
            SerializedProperty targetsProperty = element.FindPropertyRelative("targets");
            if (targetsProperty != null)
            {
              targetsList = new ReorderableList(
                serializedObject,
                targetsProperty,
                true, // draggable
                true, // displayHeader
                true, // displayAddButton
                true // displayRemoveButton
              );

              targetsList.drawHeaderCallback = (Rect rect) =>
              {
                EditorGUI.LabelField(rect, "Targets");
              };

              targetsList.elementHeightCallback = (index) =>
              {
                // Base height for the target and stateToSet properties
                float height = 2 * EditorGUIUtility.singleLineHeight;

                // Add some spacing
                height += EditorGUIUtility.standardVerticalSpacing;

                return height;
              };

              targetsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
              {
                SerializedProperty targetProperty = targetsList.serializedProperty.GetArrayElementAtIndex(index);
                SerializedProperty targetGameObjectProperty = targetProperty.FindPropertyRelative("target");
                SerializedProperty stateToSetProperty = targetProperty.FindPropertyRelative("stateToSet");

                if (targetGameObjectProperty != null && stateToSetProperty != null)
                {
                  EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    targetGameObjectProperty
                  );
                  EditorGUI.PropertyField(
                    new Rect(
                      rect.x,
                      rect.y + EditorGUIUtility.singleLineHeight,
                      rect.width,
                      EditorGUIUtility.singleLineHeight
                    ),
                    stateToSetProperty
                  );
                }
              };

              targetsList.DoList(
                new Rect(rect.x, rect.y + 5 * EditorGUIUtility.singleLineHeight, rect.width, targetsList.GetHeight())
              );
            }

            if (defaultStateProperty != null && useAnimationProperty != null && generateTypeProperty != null)
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
                useAnimationProperty
              );
              EditorGUI.PropertyField(
                new Rect(
                  rect.x,
                  rect.y + 4 * EditorGUIUtility.singleLineHeight,
                  rect.width,
                  EditorGUIUtility.singleLineHeight
                ),
                generateTypeProperty
              );
              if (targetsProperty != null)
              {
                targetsList.DoList(
                  new Rect(rect.x, rect.y + 5 * EditorGUIUtility.singleLineHeight, rect.width, targetsList.GetHeight())
                );
              }
            }
          }

          // If the menuParameter is a testParameter, draw fields for its properties
          if (shortTypeName == "testParameter")
          {
            SerializedProperty testStringProperty = element.FindPropertyRelative("testString");
            if (testStringProperty != null)
            {
              EditorGUI.PropertyField(
                new Rect(
                  rect.x,
                  rect.y + 2 * EditorGUIUtility.singleLineHeight,
                  rect.width,
                  EditorGUIUtility.singleLineHeight
                ),
                testStringProperty
              );
            }
          }
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
