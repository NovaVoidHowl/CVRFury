//this whole file is editor only
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace uk.novavoidhowl.dev.cvrfury.packagecore
{
  public static class FieldFactory
  {
    public static VisualElement CreateColorField(SerializedProperty property, string label)
    {
      VisualElement container = new VisualElement();
      container.style.flexDirection = FlexDirection.Row;
      container.AddToClassList("field-container");
      container.AddToClassList("field-container__color");
      container.name = "field-container__color";

      Label fieldLabel = new Label(label);
      fieldLabel.AddToClassList("field-label");
      fieldLabel.AddToClassList("field-label__color");
      container.Add(fieldLabel);

      ColorField colorField = new ColorField();
      colorField.BindProperty(property);
      colorField.AddToClassList("field-input");
      colorField.AddToClassList("field-input__color");
      container.Add(colorField);

      return container;
    }

    public static VisualElement CreateEnumField(SerializedProperty property, string label)
    {
      VisualElement container = new VisualElement();
      container.style.flexDirection = FlexDirection.Row;
      container.AddToClassList("field-container");
      container.AddToClassList("field-container__enum");
      container.name = "field-container__enum";

      Label fieldLabel = new Label(label);
      fieldLabel.AddToClassList("field-label");
      fieldLabel.AddToClassList("field-label__enum");
      container.Add(fieldLabel);

      EnumField enumField = new EnumField();
      enumField.BindProperty(property);
      enumField.AddToClassList("field-input");
      enumField.AddToClassList("field-input__enum");
      container.Add(enumField);

      return container;
    }

    public static VisualElement CreateFloatField(SerializedProperty property, string label)
    {
      VisualElement container = new VisualElement();
      container.style.flexDirection = FlexDirection.Row;
      container.AddToClassList("field-container");
      container.AddToClassList("field-container__float");
      container.name = "field-container__float";

      Label fieldLabel = new Label(label);
      fieldLabel.AddToClassList("field-label");
      fieldLabel.AddToClassList("field-label__float");
      container.Add(fieldLabel);

      FloatField floatField = new FloatField();
      floatField.BindProperty(property);
      floatField.AddToClassList("field-input");
      floatField.AddToClassList("field-input__float");
      container.Add(floatField);

      return container;
    }

    public static VisualElement CreateVector3Field(Vector3 vector, string label)
    {
      VisualElement container = new VisualElement();
      container.style.flexDirection = FlexDirection.Row;
      container.AddToClassList("field-container");
      container.AddToClassList("field-container__vector3");
      container.name = "field-container__vector3";

      Label fieldLabel = new Label(label);
      fieldLabel.AddToClassList("field-label");
      fieldLabel.AddToClassList("field-label__vector3");
      container.Add(fieldLabel);

      Vector3Field vectorField = new Vector3Field();
      vectorField.value = vector;
      vectorField.AddToClassList("field-input");
      vectorField.AddToClassList("field-input__vector3");
      container.Add(vectorField);

      return container;
    }
  }
}
#endif