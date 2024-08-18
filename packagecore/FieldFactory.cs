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
      container.name = "field-container__color__" + label.Replace(" ", "-");

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
      container.name = "field-container__enum__" + label.Replace(" ", "-");

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
      container.name = "field-container__float__" + label.Replace(" ", "-");

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

    public static VisualElement CreateIntField(SerializedProperty property, string label)
    {
      VisualElement container = new VisualElement();
      container.style.flexDirection = FlexDirection.Row;
      container.AddToClassList("field-container");
      container.AddToClassList("field-container__int");
      container.name = "field-container__int__" + label.Replace(" ", "-");

      Label fieldLabel = new Label(label);
      fieldLabel.AddToClassList("field-label");
      fieldLabel.AddToClassList("field-label__int");
      container.Add(fieldLabel);

      IntegerField intField = new IntegerField();
      intField.BindProperty(property);
      intField.AddToClassList("field-input");
      intField.AddToClassList("field-input__int");
      container.Add(intField);

      return container;
    }

    public static VisualElement CreateBoolField(SerializedProperty property, string label)
    {
      VisualElement container = new VisualElement();
      container.style.flexDirection = FlexDirection.Row;
      container.AddToClassList("field-container");
      container.AddToClassList("field-container__bool");
      container.name = "field-container__bool__" + label.Replace(" ", "-");

      Label fieldLabel = new Label(label);
      fieldLabel.AddToClassList("field-label");
      fieldLabel.AddToClassList("field-label__bool");
      container.Add(fieldLabel);

      Toggle boolField = new Toggle();
      boolField.BindProperty(property);
      boolField.AddToClassList("field-input");
      boolField.AddToClassList("field-input__bool");
      container.Add(boolField);

      return container;
    }

    public static VisualElement CreateStringField(SerializedProperty property, string label)
    {
      VisualElement container = new VisualElement();
      container.style.flexDirection = FlexDirection.Row;
      container.AddToClassList("field-container");
      container.AddToClassList("field-container__string");
      container.name = "field-container__string__" + label.Replace(" ", "-");

      Label fieldLabel = new Label(label);
      fieldLabel.AddToClassList("field-label");
      fieldLabel.AddToClassList("field-label__string");
      container.Add(fieldLabel);

      TextField textField = new TextField();
      textField.BindProperty(property);
      textField.AddToClassList("field-input");
      textField.AddToClassList("field-input__string");
      container.Add(textField);

      return container;
    }

    public static VisualElement CreateVector3Field(SerializedProperty property, string label)
    {
      VisualElement container = new VisualElement();
      container.style.flexDirection = FlexDirection.Row;
      container.AddToClassList("field-container");
      container.AddToClassList("field-container__vector3");
      container.name = "field-container__vector3__" + label.Replace(" ", "-");

      Label fieldLabel = new Label(label);
      fieldLabel.AddToClassList("field-label");
      fieldLabel.AddToClassList("field-label__vector3");
      container.Add(fieldLabel);

      Vector3Field vectorField = new Vector3Field();
      vectorField.BindProperty(property);
      vectorField.AddToClassList("field-input");
      vectorField.AddToClassList("field-input__vector3");
      container.Add(vectorField);

      return container;
    }
  }
}
#endif
