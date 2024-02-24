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
      drawHeaderCallback = (Rect rect) =>
      {
        EditorGUI.LabelField(rect, "Menu Items");
      },
      drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
      {
        var element = items.GetArrayElementAtIndex(index);
        rect.y += 2;
        EditorGUI.PropertyField(
          new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
          element,
          GUIContent.none
        );

        var typeName = element.managedReferenceFullTypename;
        var type = Type.GetType(typeName);
        if (type != null && type.IsSubclassOf(typeof(menuParameter)))
        {
          var menuParameter = element.managedReferenceValue as menuParameter;
          var fields = menuParameter.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
          foreach (var field in fields)
          {
            if (field.IsPublic && field.GetCustomAttributes(typeof(SerializeField), false).Length > 0)
            {
              rect.y += EditorGUIUtility.singleLineHeight + 2;
              var value = field.GetValue(menuParameter);
              EditorGUI.LabelField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                field.Name + ": " + value
              );
            }
          }
        }
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
