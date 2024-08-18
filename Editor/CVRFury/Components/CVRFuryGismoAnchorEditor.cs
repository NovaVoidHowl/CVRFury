//this whole file is editor only
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using uk.novavoidhowl.dev.cvrfury.runtime;
using uk.novavoidhowl.dev.cvrfury.packagecore;

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

    // Create and add custom fields for each property using FieldFactory
    propertiesFoldout.Add(
      FieldFactory.CreateColorField(serializedObject.FindProperty("moduleData.indicatorColor"), "Color")
    );
    propertiesFoldout.Add(
      FieldFactory.CreateEnumField(serializedObject.FindProperty("moduleData.indicatorType"), "Type")
    );
    propertiesFoldout.Add(
      FieldFactory.CreateFloatField(serializedObject.FindProperty("moduleData.indicatorScale"), "Scale")
    );
    propertiesFoldout.Add(FieldFactory.CreateVector3Field(anchor.moduleData.rotation, "Rotation"));

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
    VisualElement colorFieldContainer = propertiesFoldout.Q<VisualElement>("field-container__color");
    VisualElement typeFieldContainer = propertiesFoldout.Q<VisualElement>("field-container__enum");
    VisualElement scaleFieldContainer = propertiesFoldout.Q<VisualElement>("field-container__float");
    VisualElement rotationFieldContainer = propertiesFoldout.Q<VisualElement>("field-container__vector3");

    // Enable or disable each field container based on the isEditable flag
    colorFieldContainer.SetEnabled(isEditable);
    typeFieldContainer.SetEnabled(isEditable);
    scaleFieldContainer.SetEnabled(isEditable);
    rotationFieldContainer.SetEnabled(isEditable);
    
    
  }


  void OnSceneGUI()
  {
    CVRFuryGismoAnchor anchor = (CVRFuryGismoAnchor)target;
    if (anchor.moduleData != null)
    {
      // Display the description text
      Handles.Label(anchor.transform.position, anchor.moduleData.descriptionText);

      // Draw the appropriate wireframe shape
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
    }
  }
}

#endif
