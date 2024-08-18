//this whole file is editor only
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using uk.novavoidhowl.dev.cvrfury.runtime;
using uk.novavoidhowl.dev.cvrfury.packagecore;

namespace uk.novavoidhowl.dev.cvrfury.editor.components
{
  [CustomEditor(typeof(CVRFuryGismoAnchor))]
  public class CVRFuryGismoAnchorEditor : Editor
  {
    public override VisualElement CreateInspectorGUI()
    {
      // Create a new VisualElement to be the root of our inspector UI
      VisualElement root = new VisualElement();

      // set the name of the root element to allow styling
      root.name = "CVRFuryGismoAnchorEditor";

      // Load and apply the stylesheet
      var stylesheet = Resources.Load<StyleSheet>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryComponents/UnityStyleSheets/CVRFuryGismoAnchorInspector"
      );

      // Check if the StyleSheet was loaded
      if (stylesheet == null)
      {
        Debug.LogError(
          "Failed to load StyleSheet at '"
            + Constants.PROGRAM_DISPLAY_NAME
            + "/CVRFuryComponents/UnityStyleSheets/CVRFuryGismoAnchorInspector"
            + "'. Please ensure the file exists at the specified path."
        );
        // If the StyleSheet was not loaded add a new label to the root.
        root.Add(new Label("CRITICAL ERROR : StyleSheet could not be loaded."));
        return root;
      }
      // apply stylesheet
      root.styleSheets.Add(stylesheet);

      // Get the target component
      CVRFuryGismoAnchor anchor = (CVRFuryGismoAnchor)target;

      // Check if sourceDSUGameObject is null
      bool isEditable = anchor.sourceDSUGameObject == null;

      // if sourceDSUGameObject is not null, add a notification box to say that this component is controlled by a DSU
      if (!isEditable)
      {
        // add visualElement section to hold the notification
        VisualElement notificationSection = new VisualElement();

        Label notification = new Label("This component is controlled by a DSU\n editing is disabled");

        notificationSection.Add(notification);

        // Create a container for the buttons
        VisualElement buttonContainer = new VisualElement();
        buttonContainer.AddToClassList("buttonContainer");

        // add a button to select the DSU
        Button selectDSUButton = new Button(() =>
        {
          Selection.activeGameObject = anchor.sourceDSUGameObject;
        });
        selectDSUButton.text = "Select DSU";
        buttonContainer.Add(selectDSUButton);

        // add button to unlink from the DSU
        Button unlinkDSUButton = new Button(() =>
        {
          anchor.sourceDSUGameObject = null;
          isEditable = true;
          serializedObject.Update(); // Update the serialized properties
          serializedObject.ApplyModifiedProperties();
          // remove the notificationSection
          notificationSection.RemoveFromHierarchy();

          // Enable the properties fields
          EnablePropertiesFields(root, isEditable);
        });
        unlinkDSUButton.text = "Unlink from DSU";
        buttonContainer.Add(unlinkDSUButton);

        // add the button container to the notificationSection
        notificationSection.Add(buttonContainer);

        // add the dsuControlled class to the notificationSection
        notificationSection.AddToClassList("dsuControlled");

        root.Add(notificationSection);
      }

      // Add the properties section to the root element
      root.Add(RenderPropertiesSection(anchor, isEditable));

      // Enable the properties fields if the component is editable
      EnablePropertiesFields(root, isEditable);

      // Return the finished inspector UI
      return root;
    }

    private VisualElement RenderPropertiesSection(CVRFuryGismoAnchor anchor, bool isEditable)
    {
      // Create a new Foldout to hold the properties
      Foldout propertiesFoldout = new Foldout();
      propertiesFoldout.text = "Properties";
      propertiesFoldout.value = isEditable;
      propertiesFoldout.AddToClassList("propertiesFoldout");
      propertiesFoldout.name = "propertiesFoldout";

      //add visualElement section to hold the indicator properties
      VisualElement indicatorPropertiesSection = new VisualElement();
      indicatorPropertiesSection.AddToClassList("indicatorPropertiesSection");
      //add visualElement section to hold the pointer properties
      VisualElement pointerPropertiesSection = new VisualElement();
      pointerPropertiesSection.AddToClassList("pointerPropertiesSection");

      // Create and add custom fields for each property using FieldFactory
      propertiesFoldout.Add(
        FieldFactory.CreateStringField(serializedObject.FindProperty("moduleData.descriptionText"), "Description")
      );
      indicatorPropertiesSection.Add(
        FieldFactory.CreateColorField(serializedObject.FindProperty("moduleData.indicatorColor"), "Indicator Color")
      );
      indicatorPropertiesSection.Add(
        FieldFactory.CreateEnumField(serializedObject.FindProperty("moduleData.indicatorType"), "Indicator Type")
      );
      indicatorPropertiesSection.Add(
        FieldFactory.CreateFloatField(serializedObject.FindProperty("moduleData.indicatorScale"), "Indicator Scale")
      );

      pointerPropertiesSection.Add(
        FieldFactory.CreateColorField(serializedObject.FindProperty("moduleData.pointerColor"), "Pointer Color")
      );

      pointerPropertiesSection.Add(
        FieldFactory.CreateEnumField(serializedObject.FindProperty("moduleData.pointerType"), "Pointer Type")
      );

      pointerPropertiesSection.Add(
        FieldFactory.CreateFloatField(serializedObject.FindProperty("moduleData.pointerScale"), "Pointer Scale")
      );

      pointerPropertiesSection.Add(
        FieldFactory.CreateVector3Field(serializedObject.FindProperty("moduleData.rotation"), "Pointer Rotation")
      );

      // Add the sections to the foldout
      propertiesFoldout.Add(indicatorPropertiesSection);
      propertiesFoldout.Add(pointerPropertiesSection);

      // Return the properties foldout
      return propertiesFoldout;
    }

    private void EnablePropertiesFields(VisualElement root, bool isEditable)
    {
      // Find the foldout element
      Foldout propertiesFoldout = root.Q<Foldout>("propertiesFoldout");

      // // Enable or disable the foldout based on the isEditable flag
      // propertiesFoldout.SetEnabled(isEditable);

      // Find and enable/disable each field container within the foldout
      VisualElement descriptionFieldContainer = propertiesFoldout.Q<VisualElement>(
        "field-container__string__Description"
      );
      VisualElement colorFieldContainer = propertiesFoldout.Q<VisualElement>("field-container__color__Indicator-Color");
      VisualElement typeFieldContainer = propertiesFoldout.Q<VisualElement>("field-container__enum__Indicator-Type");
      VisualElement scaleFieldContainer = propertiesFoldout.Q<VisualElement>("field-container__float__Indicator-Scale");
      VisualElement pointerTypeFieldContainer = propertiesFoldout.Q<VisualElement>(
        "field-container__enum__Pointer-Type"
      );
      VisualElement pointerColorFieldContainer = propertiesFoldout.Q<VisualElement>(
        "field-container__color__Pointer-Color"
      );

      VisualElement pointerScaleFieldContainer = propertiesFoldout.Q<VisualElement>(
        "field-container__float__Pointer-Scale"
      );
      VisualElement pointerRotationFieldContainer = propertiesFoldout.Q<VisualElement>(
        "field-container__vector3__Pointer-Rotation"
      );

      // Enable or disable each field container based on the isEditable flag
      descriptionFieldContainer.SetEnabled(isEditable);
      colorFieldContainer.SetEnabled(isEditable);
      typeFieldContainer.SetEnabled(isEditable);
      scaleFieldContainer.SetEnabled(isEditable);
      pointerColorFieldContainer.SetEnabled(isEditable);
      pointerScaleFieldContainer.SetEnabled(isEditable);
      pointerTypeFieldContainer.SetEnabled(isEditable);
      pointerRotationFieldContainer.SetEnabled(isEditable);
    }

    void OnSceneGUI()
    {
      CVRFuryGismoAnchor anchor = (CVRFuryGismoAnchor)target;
      if (anchor.moduleData != null)
      {
        // Display the description text
        Vector3 labelPosition = anchor.transform.position + new Vector3(0, -0.05f, 0); // Adjust the offset as needed
        Handles.Label(labelPosition, anchor.moduleData.descriptionText);

        // Draw the appropriate wire-frame shape
        float scale = anchor.moduleData.indicatorScale / 10;
        Handles.color = anchor.moduleData.indicatorColor;

        switch (anchor.moduleData.indicatorType)
        {
          case CVRFuryGismo.typeOfIndicator.sphere:
            Handles.DrawWireDisc(anchor.transform.position, Vector3.up, scale);
            Handles.DrawWireDisc(anchor.transform.position, Vector3.right, scale);
            Handles.DrawWireDisc(anchor.transform.position, Vector3.forward, scale);
            break;

          case CVRFuryGismo.typeOfIndicator.cube:
            Vector3 size = Vector3.one * scale * 2;
            Handles.DrawWireCube(anchor.transform.position, size);
            break;

          case CVRFuryGismo.typeOfIndicator.cylinder:
            Handles.DrawWireDisc(anchor.transform.position, Vector3.up, scale);
            Handles.DrawWireDisc(anchor.transform.position + Vector3.up * scale, Vector3.up, scale);
            Handles.DrawLine(
              anchor.transform.position + Vector3.forward * scale,
              anchor.transform.position + Vector3.forward * scale + Vector3.up * scale
            );
            Handles.DrawLine(
              anchor.transform.position - Vector3.forward * scale,
              anchor.transform.position - Vector3.forward * scale + Vector3.up * scale
            );
            Handles.DrawLine(
              anchor.transform.position + Vector3.right * scale,
              anchor.transform.position + Vector3.right * scale + Vector3.up * scale
            );
            Handles.DrawLine(
              anchor.transform.position - Vector3.right * scale,
              anchor.transform.position - Vector3.right * scale + Vector3.up * scale
            );
            break;

          case CVRFuryGismo.typeOfIndicator.cone:
            Handles.DrawWireDisc(anchor.transform.position, Vector3.up, scale);
            Handles.DrawLine(
              anchor.transform.position + Vector3.forward * scale,
              anchor.transform.position + Vector3.up * scale * 2
            );
            Handles.DrawLine(
              anchor.transform.position - Vector3.forward * scale,
              anchor.transform.position + Vector3.up * scale * 2
            );
            Handles.DrawLine(
              anchor.transform.position + Vector3.right * scale,
              anchor.transform.position + Vector3.up * scale * 2
            );
            Handles.DrawLine(
              anchor.transform.position - Vector3.right * scale,
              anchor.transform.position + Vector3.up * scale * 2
            );
            break;

          case CVRFuryGismo.typeOfIndicator.pyramid:
            Vector3 baseCenter = anchor.transform.position;
            Vector3 top = baseCenter + Vector3.up * scale * 2;
            Vector3[] baseVertices = new Vector3[]
            {
              baseCenter + Vector3.forward * scale + Vector3.right * scale,
              baseCenter + Vector3.forward * scale - Vector3.right * scale,
              baseCenter - Vector3.forward * scale + Vector3.right * scale,
              baseCenter - Vector3.forward * scale - Vector3.right * scale
            };

            for (int i = 0; i < baseVertices.Length; i++)
            {
              Handles.DrawLine(baseVertices[i], top);
              Handles.DrawLine(baseVertices[i], baseVertices[(i + 1) % baseVertices.Length]);
            }
            break;
        }
        // Draw the pointer
        float pointerScale = anchor.moduleData.pointerScale / 20;
        Quaternion rotation = Quaternion.Euler(anchor.moduleData.rotation);
        Handles.color = anchor.moduleData.pointerColor;

        // Calculate the end position of the line
        Vector3 direction = rotation * Vector3.forward;
        Vector3 endPosition = anchor.transform.position + direction * pointerScale;

        // Draw the line
        Handles.DrawLine(anchor.transform.position, endPosition);

        // Draw the cap
        switch (anchor.moduleData.pointerType)
        {
          case CVRFuryGismo.typeOfPointer.cone:
            Handles.ConeHandleCap(0, endPosition, rotation, 0.01f, EventType.Repaint);
            break;
          case CVRFuryGismo.typeOfPointer.cube:
            Handles.CubeHandleCap(0, endPosition, rotation, 0.01f, EventType.Repaint);
            break;
          case CVRFuryGismo.typeOfPointer.sphere:
            Handles.SphereHandleCap(0, endPosition, rotation, 0.01f, EventType.Repaint);
            break;
          case CVRFuryGismo.typeOfPointer.cylinder:
            Handles.CylinderHandleCap(0, endPosition, rotation, 0.01f, EventType.Repaint);
            break;
          case CVRFuryGismo.typeOfPointer.circle:
            Handles.CircleHandleCap(0, endPosition, rotation, 0.01f, EventType.Repaint);
            break;
          case CVRFuryGismo.typeOfPointer.rectangle:
            Handles.RectangleHandleCap(0, endPosition, rotation, 0.01f, EventType.Repaint);
            break;
          default:
            Handles.ArrowHandleCap(0, endPosition, rotation, 0.01f, EventType.Repaint);
            break;
        }
      }
    }
  }
}
#endif
