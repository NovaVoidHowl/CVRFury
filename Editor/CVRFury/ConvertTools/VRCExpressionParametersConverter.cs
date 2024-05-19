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
using uk.novavoidhowl.dev.vrcstub;
using uk.novavoidhowl.dev.cvrfury.runtime;

namespace uk.novavoidhowl.dev.cvrfury.converttools
{
  public class VRCExpressionParametersConverter : EditorWindow
  {
    // Constants
    private static readonly List<string> VRCEXPRESSIONPARAMETERS_M_SCRIPT_IDS = new List<string>
    {
      "{fileID: -1506855854, guid: 67cc4cb7839cd3741b63733d5adf0442, type: 3}"
    };
    private const string CVRFURY_M_SCRIPT_ID = "{fileID: 11500000, guid: 5b6d1c52f7faa5b4f8ab4fb331d31ffd, type: 3}";

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
        CoreLogError(
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
        CoreLogError(
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
          CoreLog("File path: " + filePath);

          // get the string from the file at filePath
          var parametersFileText = File.ReadAllText(filePath);

          // check if the file is null
          if (string.IsNullOrEmpty(parametersFileText))
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
          else
          {
            if (checkCVRFuryScriptID(parametersFileText))
            {
              // show popup to the user, to inform them that the file could not be loaded
              EditorUtility.DisplayDialog(
                "Error",
                "Chosen file appears to have already been converted to CVRFury format.\n\n"
                  + "Please select an un-converted VRCExpressionParameters file.",
                "OK"
              );
              // clear the text field
              textField.value = "";
            }
          }

          // refresh the UI
          refreshParamConverterGUI();
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
          CoreLogError("Failed to load file at path: " + filePath);
          return;
        }

        // check if the file has a line starting with 'm_Script:'
        if (parametersFile.ToString().Contains("m_Script:"))
        {
          // if the file has a line starting with 'm_Script:'
          // check if the m_Script line ends with the value in 'VRCEXPRESSIONPARAMETERS_M_SCRIPT_IDS'

          (bool IDmatch, string IDString) = checkScriptIDs(parametersFile, VRCEXPRESSIONPARAMETERS_M_SCRIPT_IDS);
          if (IDmatch)
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

            // Wait for barDelay
            await Task.Delay(barDelay);

            // get filepath without extension
            var filePathWithoutExtension = Path.ChangeExtension(filePath, null);
            var newFilePath = filePathWithoutExtension + ".Stage1.CVRFury.asset";
            var newFilePathFinal = filePathWithoutExtension + ".CVRFury.asset";

            // set the text of the progressLabel to "10%"
            progressLabel.text = "5% -- initialising";
            // set the value of the progressBar to 10
            progressBar.value = 5;

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
              progressLabel.text = "10% -- Duplicate File Created";
              // set the value of the progressBar to .2
              progressBar.value = 10;

              // Wait for barDelay
              await Task.Delay(barDelay);

              // in the new  file, replace  the line m_Script: $IDString
              // with m_Script: $CVRFURY_M_SCRIPT_ID , which is the CVRFury version of VRCExpressionParameters

              // read the new file as plain text
              var newFileString = File.ReadAllText(newFilePath);

              // set the text of the progressLabel to "20%"
              progressLabel.text = "15% -- Loading File";
              // set the value of the progressBar to .2
              progressBar.value = 15;

              // replace the line in the file
              newFileString = newFileString.Replace("m_Script: " + IDString, "m_Script: " + CVRFURY_M_SCRIPT_ID);

              // set the text of the progressLabel to "80% -- Rebinding Script"
              progressLabel.text = "40% -- Rebinding Script";
              // set the value of the progressBar to .8
              progressBar.value = 40;

              // Wait for barDelay
              await Task.Delay(barDelay);

              // write the new string to the file
              File.WriteAllText(newFilePath, newFileString);

              // set the text of the progressLabel to "90% -- Writing File"
              progressLabel.text = "45% -- Writing File";
              // set the value of the progressBar to .9
              progressBar.value = 45;

              // Wait for barDelay
              await Task.Delay(barDelay);

              // refresh the asset database
              AssetDatabase.Refresh();

              // set the text of the progressLabel to "50%"
              progressLabel.text = "50% -- Stage 1 Complete";
              // set the value of the progressBar to 1
              progressBar.value = 50;

              // Wait for barDelay
              await Task.Delay(barDelay);

              // set the text of the progressLabel
              progressLabel.text = "55% -- Loading Data";
              // set the value of the progressBar to 1
              progressBar.value = 55;

              // now its in a readable format, we can convert it to a CVRFury version

              // get data out of the file
              var parametersFileData = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(newFilePath);

              // set the text of the progressLabel
              progressLabel.text = "60% -- Data Loaded";
              // set the value of the progressBar to 1
              progressBar.value = 60;

              // Wait for barDelay
              await Task.Delay(barDelay);

              // for each parameter in the file, add the values into a new CVRFuryParametersStore file
              var newCVRFuryParametersStore = ScriptableObject.CreateInstance<CVRFuryParametersStore>();
              newCVRFuryParametersStore.parameters = new CVRFuryParametersStore.Parameter[
                parametersFileData.parameters.Length
              ];

              //divide 30 by the number of parameters (to get the value of each step)
              float stepValue = 30 / parametersFileData.parameters.Length;

              for (int i = 0; i < parametersFileData.parameters.Length; i++)
              {
                newCVRFuryParametersStore.parameters[i] = new CVRFuryParametersStore.Parameter
                {
                  name = parametersFileData.parameters[i].name,
                  // set the type of the parameter to match the type of the parameter in the VRCExpressionParameters file
                  valueType = parametersFileData.parameters[i].valueType switch
                  {
                    VRCExpressionParameters.ValueType.Bool => CVRFuryParametersStore.ValueType.Bool,
                    VRCExpressionParameters.ValueType.Float => CVRFuryParametersStore.ValueType.Float,
                    VRCExpressionParameters.ValueType.Int => CVRFuryParametersStore.ValueType.Int,
                    _ => CVRFuryParametersStore.ValueType.Float
                  },
                  defaultValue = parametersFileData.parameters[i].defaultValue
                };

                // set the text of the progressLabel
                progressLabel.text = (60 + (stepValue * i)) + "% -- Data Processing";
                // set the value of the progressBar to to 60 + (stepValue * i)
                progressBar.value = 60 + (stepValue * i);
              }

              // set the text of the progressLabel
              progressLabel.text = "90% -- Data processed";
              // set the value of the progressBar to 1
              progressBar.value = 90;

              // Wait for barDelay
              await Task.Delay(barDelay);

              // create a new file path for the CVRFuryParametersStore file
              var newCVRFuryParametersStoreFilePath = newFilePathFinal;

              // check if the file already exists
              if (File.Exists(newCVRFuryParametersStoreFilePath))
              {
                // Check if the menu option is set
                if (!EditorPrefs.GetBool("SuppressDeleteParameterIntermediate", false))
                {
                  // remove intermediate file
                  AssetDatabase.DeleteAsset(newFilePath);
                }

                // refresh the asset database
                AssetDatabase.Refresh();

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
                // create the new file
                AssetDatabase.CreateAsset(newCVRFuryParametersStore, newCVRFuryParametersStoreFilePath);

                // set the text of the progressLabel to "100%"
                progressLabel.text = "95% -- Cleaning Up";
                // set the value of the progressBar to 1
                progressBar.value = 95;

                // Wait for barDelay
                await Task.Delay(barDelay);

                // refresh the asset database
                AssetDatabase.Refresh();

                // Check if the menu option is set
                if (!EditorPrefs.GetBool("SuppressDeleteParameterIntermediate", false))
                {
                  // remove intermediate file
                  AssetDatabase.DeleteAsset(newFilePath);
                }

                // refresh the asset database
                AssetDatabase.Refresh();

                // set the text of the progressLabel to "Conversion Complete"
                progressLabel.text = "Conversion Complete";
                // set the value of the progressBar to 100
                progressBar.value = 100;

                // Wait for barDelay
                await Task.Delay(barDelay);

                // popup to confirm conversion
                EditorUtility.DisplayDialog(
                  "Conversion Complete",
                  "The output file can be found at:\n\n" + newFilePathFinal,
                  "OK"
                );

                // Wait for barDelay
                await Task.Delay(barDelay);
              }
            }
          }
          else
          {
            // if the file does not end with 'VRCEXPRESSIONPARAMETERS_M_SCRIPT_ID'
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

    // support functions -----------------------------------------------------------------------------------------------

    // function to check if string content is a VRCExpressionParameters file
    public (bool, string) checkScriptIDs(string parametersFileString, List<string> scriptIDs)
    {
      // output vars
      bool IDmatch = false;
      string IDString = "";

      // check provided string is not null or empty
      if (!string.IsNullOrEmpty(parametersFileString))
      {
        // not null or empty, so check if the string contains the line 'm_Script: '

        // check if the string contains the line 'm_Script: '
        if (parametersFileString.ToString().Contains("m_Script:"))
        {
          // if the string contains the line 'm_Script: '
          // check if any of the strings in scriptIDs are contained in the string
          foreach (string id in scriptIDs)
          {
            if (parametersFileString.ToString().Contains(id))
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

    // function to check if a file content is a the CVRFury version of VRCExpressionParameters
    public bool checkCVRFuryScriptID(string parametersFileString)
    {
      // output vars
      bool IDmatch = false;

      // check provided string is not null or empty
      if (!string.IsNullOrEmpty(parametersFileString))
      {
        // not null or empty, so check if the string contains the line 'm_Script: '

        // check if the string contains the line 'm_Script: '
        if (parametersFileString.ToString().Contains("m_Script:"))
        {
          // if the string contains the line 'm_Script: '
          // check if the string contains the line 'm_Script: ' + CVRFURY_M_SCRIPT_ID
          if (parametersFileString.ToString().Contains("m_Script: " + CVRFURY_M_SCRIPT_ID))
          {
            // got a match so set the output vars
            IDmatch = true;
          }
        }
      }

      // return the output vars
      return IDmatch;
    }
  }

  public class SuppressDeleteParameterIntermediateMenu
  {
    private const string MENU_PATH =
      "NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Debug/Suppress Delete Parameter Intermediate";
    private const string EDITOR_PREFS_KEY = "SuppressDeleteParameterIntermediate";

    [MenuItem(MENU_PATH)]
    private static void ToggleSuppressDeleteParameterIntermediateAsset()
    {
      // Toggle the value
      bool currentValue = EditorPrefs.GetBool(EDITOR_PREFS_KEY, false);
      EditorPrefs.SetBool(EDITOR_PREFS_KEY, !currentValue);
    }

    [MenuItem(MENU_PATH, true)]
    private static bool ToggleSuppressDeleteParameterIntermediateValidation()
    {
      // Toggle the checked state
      Menu.SetChecked(MENU_PATH, EditorPrefs.GetBool(EDITOR_PREFS_KEY, false));
      return true;
    }
  }
}
#endif
