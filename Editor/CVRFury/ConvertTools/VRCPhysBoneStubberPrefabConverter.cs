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
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.processtools
{
  public class VRCPhysBoneStubberPrefabConverter : EditorWindow
  {
    // Constants
    private static readonly List<string> VRCPHYSBONE_M_SCRIPT_IDS = new List<string>
    {
      "{fileID: 1661641543, guid: 2a2c05204084d904aa4945ccff20d8e5, type: 3}"
    };
    private const string CVRFURY_M_SCRIPT_ID = "{fileID: 11500000, guid: 475333861f76b9b43af5db2ad0c0c67d, type: 3}";

    // Declare textField as a member variable
    private TextField textField;

    // Declare barDelay as a member variable
    private int barDelay = 1000;

    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Conversion Tools/Prefab PhysBone Stubber")]
    public static void ShowWindow()
    {
      // Get existing open window or if none, make a new one:
      VRCPhysBoneStubberPrefabConverter window = (VRCPhysBoneStubberPrefabConverter)
        EditorWindow.GetWindow(typeof(VRCPhysBoneStubberPrefabConverter), true, "PhysBone Stubber");
      window.maxSize = new Vector2(800, 600);
      window.minSize = new Vector2(500, 300);
      window.Show();
    }

    public void OnEnable()
    {
      // Initialize textField if it's not already initialized
      if (textField == null)
      {
        textField = new TextField();
      }

      renderMenuConverterGUI();
    }

    private void refreshMenuConverterGUI()
    {
      // remove all children from the root
      rootVisualElement.Clear();
      // re-render the UI
      renderMenuConverterGUI();
    }

    public void renderMenuConverterGUI()
    {
      // load base UXML
      var baseTree = Resources.Load<VisualTreeAsset>(
        Constants.PROGRAM_DISPLAY_NAME + "/VRCConverters/UnityUXML/PhysBoneStubber"
      );

      // Check if the UXML file was loaded
      if (baseTree == null)
      {
        Debug.LogError(
          "Failed to load UXML file at 'UnityUXML/PhysBoneStubber'. Please ensure the file exists at the specified path."
        );
        // If the UXML file was not loaded add a new label to the root.
        rootVisualElement.Add(new Label("CRITICAL ERROR : UXML could not be loaded."));
        return;
      }

      // Load and apply the stylesheet
      var stylesheet = Resources.Load<StyleSheet>(
        Constants.PROGRAM_DISPLAY_NAME + "/VRCConverters/UnityStyleSheets/PhysBoneStubber"
      );

      // Check if the StyleSheet was loaded
      if (stylesheet == null)
      {
        Debug.LogError(
          "Failed to load StyleSheet at 'UnityStyleSheets/PhysBoneStubber'. Please ensure the file exists at the specified path."
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

      // Get the prefabFileSelectorHolder element
      var prefabFileSelectorHolder = rootVisualElement.Q<VisualElement>("prefabFileSelectorHolder");

      // Get the prefabFileDropArea element
      var prefabFileDropArea = rootVisualElement.Q<VisualElement>("prefabFileDropArea");

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
      prefabFileDropArea.RegisterCallback<DragUpdatedEvent>(evt =>
      {
        // Allow the drag and drop operation
        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
      });

      // Register a callback for the DragPerformEvent event
      prefabFileDropArea.RegisterCallback<DragPerformEvent>(evt =>
      {
        // Check if any files have been dragged and dropped
        if (DragAndDrop.paths.Length > 0)
        {
          // Get the file path of the dragged file
          var filePath = DragAndDrop.paths[0];

          // Set the text of the TextField to the file path
          textField.value = filePath;

          // Now you can use filePath in your code
          CoreLog("File path: " + filePath);

          // get the string from the file at filePath
          var prefabFileText = File.ReadAllText(filePath);

          // check if the file is null
          if (string.IsNullOrEmpty(prefabFileText))
          {
            // if the file is null
            // show popup to the user, to inform them that the file could not be loaded
            EditorUtility.DisplayDialog(
              "Error",
              "Chosen file appears to be null. Please select a different file.",
              "OK"
            );

            // clear the text field
            textField.value = "";
          }

          // get the file extension
          var fileExtension = Path.GetExtension(filePath);

          // check if the file has the correct extension, .prefab
          if (fileExtension != ".prefab")
          {
            // if the file does not have the correct extension
            // show popup to the user, to inform them that the file does not have the correct extension
            EditorUtility.DisplayDialog(
              "Error",
              "Chosen file does not have the correct extension. Please select a file with the extension '.prefab'.",
              "OK"
            );

            // clear the text field
            textField.value = "";
          }

          // refresh the UI
          refreshMenuConverterGUI();
        }
        else
        {
          // No files have been dragged and dropped
          CoreLog("No files dragged and dropped");
        }
      });

      // Make the TextField read-only
      textField.SetEnabled(false);
      // Add a class to the TextField
      textField.AddToClassList("prefabFilePath");

      // Add the textField to the prefabFileSelectorHolder
      prefabFileSelectorHolder.Add(textField);

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
          refreshMenuConverterGUI();
        })
        {
          text = "Clear"
        };
        clearButton.AddToClassList("clearButton");
        prefabFileSelectorHolder.Add(clearButton);

        // get the processed file path for the related filename ending -PhysBoneStubbed.prefab
        var filePathWithoutExtension = Path.ChangeExtension(textField.value, null);
        var newFilePath = filePathWithoutExtension + "-PhysBoneStubbed.prefab";

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
            refreshMenuConverterGUI();
          })
          {
            text = "Open Processed File"
          };
          openButton.AddToClassList("openButton");
          prefabFileSelectorHolder.Add(openButton);

          // set the progressLabel text to "Processed"
          var progressLabel = rootVisualElement.Q<Label>("progressLabel");
          progressLabel.text = "Processed";

          // set the progressBar value to 100
          var progressBar = rootVisualElement.Q<ProgressBar>("progressBar");
          progressBar.value = 100;

          // create a button to remove the processed file
          var removeButton = new Button(() =>
          {
            // remove the processed file
            AssetDatabase.DeleteAsset(newFilePath);

            // refresh the UI
            refreshMenuConverterGUI();
          })
          {
            text = "Remove Processed File"
          };
          removeButton.AddToClassList("removeButton");
          prefabFileSelectorHolder.Add(removeButton);

          targetFileExists = true;
        }
      }

      // if there is no file path in the text field, disable the process button
      if (textField.value == "")
      {
        // disable the process button
        var processButton = rootVisualElement.Q<Button>("processButton");
        processButton.SetEnabled(false);
      }
      else
      {
        // enable the process button
        var processButton = rootVisualElement.Q<Button>("processButton");
        processButton.SetEnabled(true);

        // register a callback for the process button
        processButton.RegisterCallback<MouseUpEvent>(evt =>
        {
          ConvertFile(rootVisualElement);
        });
      }

      // if the target file does exist, disable the process button
      if (targetFileExists)
      {
        // disable the process button
        var processButton = rootVisualElement.Q<Button>("processButton");
        processButton.SetEnabled(false);
      }

      // temp section for build process
      // Create a button to refresh the ui
      var refreshButton = new Button(() =>
      {
        refreshMenuConverterGUI();
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
      if (filePath.EndsWith(".prefab"))
      {
        // if the file has the correct extension
        // load the file as plain text
        var prefabFile = File.ReadAllText(filePath);

        // check if the file is null
        if (string.IsNullOrEmpty(prefabFile))
        {
          // if the file is null
          // display an error message
          Debug.LogError("Failed to load file at path: " + filePath);
          return;
        }

        // check if the file has a line starting with 'm_Script:'
        if (prefabFile.ToString().Contains("m_Script:"))
        {
          // if the file has a line starting with 'm_Script:'
          // check if the m_Script line ends with the value in 'VRCPHYSBONE_M_SCRIPT_IDS'

          (bool IDmatch, string IDString) = checkScriptIDs(prefabFile, VRCPHYSBONE_M_SCRIPT_IDS);
          if (IDmatch)
          {
            // found the file we are looking for file, (it has PhysBones in it)

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
            CoreLog("progressBar.value: " + progressBar.value);

            // Wait for barDelay
            await Task.Delay(barDelay);

            // get filepath without extension
            var filePathWithoutExtension = Path.ChangeExtension(filePath, null);
            var newFilePath = filePathWithoutExtension + "-PhysBoneStubbed.prefab";

            // set the text of the progressLabel to "10%"
            progressLabel.text = "10% -- initialising";
            // set the value of the progressBar to 10
            progressBar.value = 10;
            // debug print the value of the progressBar
            CoreLog("progressBar.value: " + progressBar.value);

            // Wait for barDelay
            await Task.Delay(barDelay);

            // check if the file already exists
            if (File.Exists(newFilePath))
            {
              // if the file already exists
              // display an error popup to the user
              EditorUtility.DisplayDialog(
                "Error",
                "The file you have selected to process already has a processed version. Please select a different file.",
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

              // in the new  file, replace  the line m_Script: $IDString
              // with m_Script: $CVRFURY_M_SCRIPT_ID , which is the stubbed version of the script

              // read the new file as plain text
              var newFileString = File.ReadAllText(newFilePath);

              // set the text of the progressLabel to "20%"
              progressLabel.text = "30% -- Loading File";
              // set the value of the progressBar to .2
              progressBar.value = 30;

              // replace the line in the file
              newFileString = newFileString.Replace("m_Script: " + IDString, "m_Script: " + CVRFURY_M_SCRIPT_ID);

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
                "The output file can be found at:\n\n" + newFilePath,
                "OK"
              );
              // Wait for barDelay
              await Task.Delay(barDelay);
            }
          }
          else
          {
            // if the file does not end with 'VRCExpressionMenu_M_SCRIPT_ID'
            // display an error popup to the user
            EditorUtility.DisplayDialog(
              "Error",
              "The file you have selected does not appear to have any VRCPhysBones in it.",
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
            "The file you have selected does not appear to be a VRCExpressionMenu file.",
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
          "The file you have selected does not have the correct extension. Please select a file with the extension '.prefab'.",
          "OK"
        );
        // clear the text field
        textField.value = "";
      }

      // refresh the UI
      refreshMenuConverterGUI();
    }

    // support functions -----------------------------------------------------------------------------------------------

    // function to check if string content is a VRCExpressionMenu file
    public (bool, string) checkScriptIDs(string prefabFileString, List<string> scriptIDs)
    {
      // output vars
      bool IDmatch = false;
      string IDString = "";

      // check provided string is not null or empty
      if (!string.IsNullOrEmpty(prefabFileString))
      {
        // not null or empty, so check if the string contains the line 'm_Script: '

        // check if the string contains the line 'm_Script: '
        if (prefabFileString.ToString().Contains("m_Script:"))
        {
          // if the string contains the line 'm_Script: '
          // check if any of the strings in scriptIDs are contained in the string
          foreach (string id in scriptIDs)
          {
            if (prefabFileString.ToString().Contains(id))
            {
              // got a match so set the output vars
              IDmatch = true;
              IDString = id;
            }
          }
        }
      }

      // return the output vars
      return (IDmatch, IDString);
    }
  }
}
#endif
