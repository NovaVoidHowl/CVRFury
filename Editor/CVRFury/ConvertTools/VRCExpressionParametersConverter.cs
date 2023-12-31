// editor only script to manage the dependencies
#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;

namespace uk.novavoidhowl.dev.cvrfury.converttools
{
  public class VRCExpressionParametersConverter : EditorWindow
  {
    // Declare textField as a member variable
    private TextField textField;

    // Declare barDelay as a member variable
    private int barDelay = 1000;

    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Conversion Tools/Convert VRCExpressionParameters")]
    public static void ShowWindow()
    {
      // Get existing open window or if none, make a new one:
      VRCExpressionParametersConverter window = (VRCExpressionParametersConverter)
        EditorWindow.GetWindow(typeof(VRCExpressionParametersConverter), true, "Convert VRCExpressionParameters");
      window.maxSize = new Vector2(800, 600);
      window.minSize = new Vector2(300, 200);
      window.Show();
    }

    public void OnEnable()
    {
      // Initialize textField if it's not already initialized
      if (textField == null)
      {
        textField = new TextField();
      }

      renderParamConverterGUI();
    }

    private void refreshParamConverterGUI()
    {
      // remove all children from the root
      rootVisualElement.Clear();
      // re-render the UI
      renderParamConverterGUI();
    }

    public void renderParamConverterGUI()
    {
      // load base UXML
      var baseTree = Resources.Load<VisualTreeAsset>(
        Constants.PROGRAM_DISPLAY_NAME + "/VRCConverters/UnityUXML/ParametersFileConverter"
      );

      // Check if the UXML file was loaded
      if (baseTree == null)
      {
        Debug.LogError(
          "Failed to load UXML file at 'UnityUXML/ParametersFileConverter'. Please ensure the file exists at the specified path."
        );
        // If the UXML file was not loaded add a new label to the root.
        rootVisualElement.Add(new Label("CRITICAL ERROR : UXML could not be loaded."));
        return;
      }

      // Load and apply the stylesheet
      var stylesheet = Resources.Load<StyleSheet>(
        Constants.PROGRAM_DISPLAY_NAME + "/VRCConverters/UnityStyleSheets/ParametersFileConverter"
      );

      // Check if the StyleSheet was loaded
      if (stylesheet == null)
      {
        Debug.LogError(
          "Failed to load StyleSheet at 'UnityStyleSheets/ParametersFileConverter'. Please ensure the file exists at the specified path."
        );
        // If the StyleSheet was not loaded add a new label to the root.
        rootVisualElement.Add(new Label("CRITICAL ERROR : StyleSheet could not be loaded."));
        return;
      }

      // Instantiate the UXML tree
      var ToolSetup = baseTree.Instantiate();

      // Create a temporary list to hold the children
      List<VisualElement> children = new List<VisualElement>(ToolSetup.Children());

      // Add the children of the instantiated UXML to the root
      foreach (var child in children)
      {
        rootVisualElement.Add(child);
      }

      // Apply the StyleSheet
      rootVisualElement.styleSheets.Add(stylesheet);

      // Get the parametersFileSelectorHolder element
      var parametersFileSelectorHolder = rootVisualElement.Q<VisualElement>("parametersFileSelectorHolder");

      // Get the parametersFileDropArea element
      var parametersFileDropArea = rootVisualElement.Q<VisualElement>("parametersFileDropArea");

      // Create a new TextField
      var newTextField = new TextField("Source Parameters File");

      // If textField is not null, set the value of newTextField to the value of textField
      if (textField != null)
      {
        newTextField.value = textField.value;
      }

      // Set textField to newTextField
      textField = newTextField;

      // Register a callback for the DragUpdated event
      parametersFileDropArea.RegisterCallback<DragUpdatedEvent>(evt =>
      {
        // Allow the drag and drop operation
        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
      });

      // Register a callback for the DragPerformEvent event
      parametersFileDropArea.RegisterCallback<DragPerformEvent>(evt =>
      {
        // Check if any files have been dragged and dropped
        if (DragAndDrop.paths.Length > 0)
        {
          // Get the file path of the dragged file
          var filePath = DragAndDrop.paths[0];

          // Set the text of the TextField to the file path
          textField.value = filePath;

          // Now you can use filePath in your code
          Debug.Log("File path: " + filePath);

          // refresh the UI
          refreshParamConverterGUI();
        }
        else
        {
          // No files have been dragged and dropped
          Debug.Log("No files dragged and dropped");
        }
      });

      // Make the TextField read-only
      textField.SetEnabled(false);
      // Add a class to the TextField
      textField.AddToClassList("parametersFilePath");

      // Add the textField to the parametersFileSelectorHolder
      parametersFileSelectorHolder.Add(textField);

      // bool  to show if the target file exists
      bool targetFileExists = false;

      // check if there is a file path in the text field
      if (textField.value != "")
      {
        // if there is a file path in the text field
        // create a button to clear the text field
        var clearButton = new Button(() =>
        {
          // clear the text field
          textField.value = "";

          // refresh the UI
          refreshParamConverterGUI();
        })
        {
          text = "Clear"
        };
        clearButton.AddToClassList("clearButton");
        parametersFileSelectorHolder.Add(clearButton);

        // get the converted file path for the related filename ending .CVRFury.asset
        var filePathWithoutExtension = Path.ChangeExtension(textField.value, null);
        var newFilePath = filePathWithoutExtension + ".CVRFury.asset";

        // check if the file already exists
        if (File.Exists(newFilePath))
        {
          // if the file already exists
          // create a button to open the file
          var openButton = new Button(() =>
          {
            // open the file
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(newFilePath, typeof(UnityEngine.Object)));

            // refresh the UI
            refreshParamConverterGUI();
          })
          {
            text = "Open Converted File"
          };
          openButton.AddToClassList("openButton");
          parametersFileSelectorHolder.Add(openButton);

          // set the progressLabel text to "Converted"
          var progressLabel = rootVisualElement.Q<Label>("progressLabel");
          progressLabel.text = "Converted";

          // set the progressBar value to 100
          var progressBar = rootVisualElement.Q<ProgressBar>("progressBar");
          progressBar.value = 100;

          // create a button to remove the converted file
          var removeButton = new Button(() =>
          {
            // remove the converted file
            AssetDatabase.DeleteAsset(newFilePath);

            // refresh the UI
            refreshParamConverterGUI();
          })
          {
            text = "Remove Converted File"
          };
          removeButton.AddToClassList("removeButton");
          parametersFileSelectorHolder.Add(removeButton);

          targetFileExists = true;
        }
      }

      // if there is no file path in the text field, disable the convert button
      if (textField.value == "")
      {
        // disable the convert button
        var convertButton = rootVisualElement.Q<Button>("convertButton");
        convertButton.SetEnabled(false);
      }
      else
      {
        // enable the convert button
        var convertButton = rootVisualElement.Q<Button>("convertButton");
        convertButton.SetEnabled(true);

        // register a callback for the convert button
        convertButton.RegisterCallback<MouseUpEvent>(evt =>
        {
          ConvertFile(rootVisualElement);
        });
      }

      // if the target file does exist, disable the convert button
      if (targetFileExists)
      {
        // disable the convert button
        var convertButton = rootVisualElement.Q<Button>("convertButton");
        convertButton.SetEnabled(false);
      }

      // temp section for build process
      // Create a button to refresh the ui
      var refreshButton = new Button(() =>
      {
        refreshParamConverterGUI();
      })
      {
        text = "Refresh UI"
      };
      refreshButton.AddToClassList("refreshButton");
      rootVisualElement.Add(refreshButton);
    }

    public async void ConvertFile(VisualElement rootVisualElement)
    {
      // get the file path from the text field
      var filePath = textField.value;

      // check if the file has the correct extension
      if (filePath.EndsWith(".asset"))
      {
        // if the file has the correct extension
        // load the file as plain text
        var parametersFile = File.ReadAllText(filePath);

        // check if the file is null
        if (string.IsNullOrEmpty(parametersFile))
        {
          // if the file is null
          // display an error message
          Debug.LogError("Failed to load file at path: " + filePath);
          return;
        }

        // check if the file has a line starting with 'm_Script:'
        if (parametersFile.ToString().Contains("m_Script:"))
        {
          // if the file has a line starting with 'm_Script:'
          // check if the m_Script line ends '{fileID: -1506855854, guid: 67cc4cb7839cd3741b63733d5adf0442, type: 3}'
          if (
            parametersFile.ToString().Contains("{fileID: -1506855854, guid: 67cc4cb7839cd3741b63733d5adf0442, type: 3}")
          )
          {
            // found the file we are looking for, is a stock VRCExpressionParameters file

            // get the controlsContainer element
            var controlsContainer = rootVisualElement.Q<VisualElement>("controlsContainer");

            // get the progressBar element from the controlsContainer
            var progressBar = controlsContainer.Q<ProgressBar>("progressBar");

            // get the progressLabel element from the controlsContainer
            var progressLabel = controlsContainer.Q<Label>("progressLabel");

            // set the value of the progressBar to 0
            progressBar.value = 0;
            // set the text of the progressLabel to "0%"
            progressLabel.text = "0%";
            // debug print the value of the progressBar
            Debug.Log("progressBar.value: " + progressBar.value);

            // Wait for barDelay
            await Task.Delay(barDelay);

            // get filepath without extension
            var filePathWithoutExtension = Path.ChangeExtension(filePath, null);
            var newFilePath = filePathWithoutExtension + ".CVRFury.asset";

            // set the text of the progressLabel to "10%"
            progressLabel.text = "10% -- initialising";
            // set the value of the progressBar to 10
            progressBar.value = 10;
            // debug print the value of the progressBar
            Debug.Log("progressBar.value: " + progressBar.value);

            // Wait for barDelay
            await Task.Delay(barDelay);

            // check if the file already exists
            if (File.Exists(newFilePath))
            {
              // if the file already exists
              // display an error popup to the user
              EditorUtility.DisplayDialog(
                "Error",
                "The file you have selected to convert already has a converted version. Please select a different file.",
                "OK"
              );

              // set the text of the progressLabel to "Conversion Error"
              progressLabel.text = "Conversion Error";
              // set the value of the progressBar to 0
              progressBar.value = 0;

              // Wait for barDelay
              await Task.Delay(barDelay);
            }
            else
            {
              // copy the file to the new location
              AssetDatabase.CopyAsset(filePath, newFilePath);

              // set the text of the progressLabel to "20%"
              progressLabel.text = "20% -- Duplicate File Created";
              // set the value of the progressBar to .2
              progressBar.value = 20;

              // Wait for barDelay
              await Task.Delay(barDelay);

              // in the new  file, replace  the line m_Script: {fileID: -1506855854, guid: 67cc4cb7839cd3741b63733d5adf0442, type: 3}
              // with m_Script: {fileID: 11500000, guid: 5b6d1c52f7faa5b4f8ab4fb331d31ffd, type: 3} , which is the CVRFury version of VRCExpressionParameters

              // read the new file as plain text
              var newFileString = File.ReadAllText(newFilePath);

              // set the text of the progressLabel to "20%"
              progressLabel.text = "30% -- Loading File";
              // set the value of the progressBar to .2
              progressBar.value = 30;

              // replace the line in the file
              newFileString = newFileString.Replace(
                "m_Script: {fileID: -1506855854, guid: 67cc4cb7839cd3741b63733d5adf0442, type: 3}",
                "m_Script: {fileID: 11500000, guid: 5b6d1c52f7faa5b4f8ab4fb331d31ffd, type: 3}"
              );

              // set the text of the progressLabel to "80% -- Rebinding Script"
              progressLabel.text = "80% -- Rebinding Script";
              // set the value of the progressBar to .8
              progressBar.value = 80;

              // Wait for barDelay
              await Task.Delay(barDelay);

              // write the new string to the file
              File.WriteAllText(newFilePath, newFileString);

              // set the text of the progressLabel to "90% -- Writing File"
              progressLabel.text = "90% -- Writing File";
              // set the value of the progressBar to .9
              progressBar.value = 90;

              // Wait for barDelay
              await Task.Delay(barDelay);

              // refresh the asset database
              AssetDatabase.Refresh();

              // set the text of the progressLabel to "100%"
              progressLabel.text = "100% -- Complete";
              // set the value of the progressBar to 1
              progressBar.value = 100;

              // Wait for barDelay
              await Task.Delay(barDelay);
              // popup to confirm conversion
              EditorUtility.DisplayDialog(
                "Conversion Complete",
                "The file has been converted. The new file can be found at: " + newFilePath,
                "OK"
              );
              // Wait for barDelay
              await Task.Delay(barDelay);
            }
          }
          else
          {
            // if the file does not end with '{fileID: -1506855854, guid: 67cc4cb7839cd3741b63733d5adf0442, type: 3}'
            // display an error popup to the user
            EditorUtility.DisplayDialog(
              "Error",
              "The file you have selected does not appear to be a VRCExpressionParameters file.",
              "OK"
            );
            // clear the text field
            textField.value = "";
          }
        }
        else
        {
          // display an error popup to the user
          EditorUtility.DisplayDialog(
            "Error",
            "The file you have selected does not appear to be a VRCExpressionParameters file.",
            "OK"
          );
          // clear the text field
          textField.value = "";
        }
      }
      else
      {
        // if the file does not have the correct extension

        // display an error popup to the user
        EditorUtility.DisplayDialog(
          "Error",
          "The file you have selected does not have the correct extension. Please select a file with the extension '.asset'.",
          "OK"
        );
        // clear the text field
        textField.value = "";
      }

      // refresh the UI
      refreshParamConverterGUI();
    }
  }
}
#endif
