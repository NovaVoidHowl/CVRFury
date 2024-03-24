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
using ControlType = uk.novavoidhowl.dev.vrcstub.VRCExpressionsMenu.Control.ControlType;
using uk.novavoidhowl.dev.cvrfury.runtime;

namespace uk.novavoidhowl.dev.cvrfury.converttools
{
  public class VRCExpressionMenuConverter : EditorWindow
  {
    // Constants
    private static readonly List<string> VRCEXPRESSIONMENU_M_SCRIPT_IDS = new List<string>
    {
      "{fileID: -340790334, guid: 67cc4cb7839cd3741b63733d5adf0442, type: 3}"
    };
    private const string CVRFURY_M_SCRIPT_ID = "{fileID: 11500000, guid: d2b1b7e16fd63f64d8da1c4065469b76, type: 3}";

    // Declare textField as a member variable
    private TextField textField;

    // Declare barDelay as a member variable
    private int barDelay = 1000;

    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Conversion Tools/Convert VRCExpressionMenu")]
    public static void ShowWindow()
    {
      // Get existing open window or if none, make a new one:
      VRCExpressionMenuConverter window = (VRCExpressionMenuConverter)
        EditorWindow.GetWindow(typeof(VRCExpressionMenuConverter), true, "Convert VRCExpressionMenu");
      window.maxSize = new Vector2(800, 600);
      window.minSize = new Vector2(300, 300);
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
        Constants.PROGRAM_DISPLAY_NAME + "/VRCConverters/UnityUXML/MenuFileConverter"
      );

      // Check if the UXML file was loaded
      if (baseTree == null)
      {
        Debug.LogError(
          "Failed to load UXML file at 'UnityUXML/MenuFileConverter'. Please ensure the file exists at the specified path."
        );
        // If the UXML file was not loaded add a new label to the root.
        rootVisualElement.Add(new Label("CRITICAL ERROR : UXML could not be loaded."));
        return;
      }

      // Load and apply the stylesheet
      var stylesheet = Resources.Load<StyleSheet>(
        Constants.PROGRAM_DISPLAY_NAME + "/VRCConverters/UnityStyleSheets/MenuFileConverter"
      );

      // Check if the StyleSheet was loaded
      if (stylesheet == null)
      {
        Debug.LogError(
          "Failed to load StyleSheet at 'UnityStyleSheets/MenuFileConverter'. Please ensure the file exists at the specified path."
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

      // Get the menuFileSelectorHolder element
      var menuFileSelectorHolder = rootVisualElement.Q<VisualElement>("menuFileSelectorHolder");

      // Get the menuFileDropArea element
      var menuFileDropArea = rootVisualElement.Q<VisualElement>("menuFileDropArea");

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
      menuFileDropArea.RegisterCallback<DragUpdatedEvent>(evt =>
      {
        // Allow the drag and drop operation
        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
      });

      // Register a callback for the DragPerformEvent event
      menuFileDropArea.RegisterCallback<DragPerformEvent>(evt =>
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
          var menuFileText = File.ReadAllText(filePath);

          // check if the file is null
          if (string.IsNullOrEmpty(menuFileText))
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
            if (checkCVRFuryScriptID(menuFileText))
            {
              // show popup to the user, to inform them that the file could not be loaded
              EditorUtility.DisplayDialog(
                "Error",
                "Chosen file appears to have already been converted to CVRFury format.\n\n"
                  + "Please select an un-converted VRCExpressionMenu file.",
                "OK"
              );
              // clear the text field
              textField.value = "";
            }
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
      textField.AddToClassList("menuFilePath");

      // Add the textField to the menuFileSelectorHolder
      menuFileSelectorHolder.Add(textField);

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
        menuFileSelectorHolder.Add(clearButton);

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
            refreshMenuConverterGUI();
          })
          {
            text = "Open Converted File"
          };
          openButton.AddToClassList("openButton");
          menuFileSelectorHolder.Add(openButton);

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
            refreshMenuConverterGUI();
          })
          {
            text = "Remove Converted File"
          };
          removeButton.AddToClassList("removeButton");
          menuFileSelectorHolder.Add(removeButton);

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
      if (filePath.EndsWith(".asset"))
      {
        // if the file has the correct extension
        // load the file as plain text
        var menuFile = File.ReadAllText(filePath);

        // check if the file is null
        if (string.IsNullOrEmpty(menuFile))
        {
          // if the file is null
          // display an error message
          Debug.LogError("Failed to load file at path: " + filePath);
          return;
        }

        // check if the file has a line starting with 'm_Script:'
        if (menuFile.ToString().Contains("m_Script:"))
        {
          // if the file has a line starting with 'm_Script:'
          // check if the m_Script line ends with the value in 'VRCEXPRESSIONMENU_M_SCRIPT_IDS'

          (bool IDmatch, string IDString) = checkScriptIDs(menuFile, VRCEXPRESSIONMENU_M_SCRIPT_IDS);
          if (IDmatch)
          {
            // found the file we are looking for, is a stock VRCExpressionMenu file

            // get the controlsContainer element
            var controlsContainer = rootVisualElement.Q<VisualElement>("controlsContainer");

            // get the progressBar element from the controlsContainer
            var progressBar = controlsContainer.Q<ProgressBar>("progressBar");

            // get the progressLabel element from the controlsContainer
            var progressLabel = controlsContainer.Q<Label>("progressLabel");

            // set the value of the progressBar
            progressBar.value = 0;
            // set the text of the progressLabel to "0%"
            progressLabel.text = "0%";
            // debug print the value of the progressBar
            CoreLog("progressBar.value: " + progressBar.value);

            // Wait for barDelay
            await Task.Delay(barDelay);

            // get filepath without extension
            var filePathWithoutExtension = Path.ChangeExtension(filePath, null);
            var newFilePath = filePathWithoutExtension + ".Stage1.CVRFury.asset";
            var newFilePathFinal = filePathWithoutExtension + ".CVRFury.asset";

            // set the text of the progressLabel to "10%"
            progressLabel.text = "10% -- initialising";
            // set the value of the progressBar
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
                "The file you have selected to convert already has a converted version. Please select a different file.",
                "OK"
              );

              // set the text of the progressLabel to "Conversion Error"
              progressLabel.text = "Conversion Error";
              // set the value of the progressBar
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
              // set the value of the progressBar
              progressBar.value = 10;

              // Wait for barDelay
              await Task.Delay(barDelay);

              // in the new  file, replace  the line m_Script: $IDString
              // with m_Script: $CVRFURY_M_SCRIPT_ID , which is the CVRFury version of VRCExpressionMenu

              // read the new file as plain text
              var newFileString = File.ReadAllText(newFilePath);

              // set the text of the progressLabel
              progressLabel.text = "15% -- Loading File";
              // set the value of the progressBar
              progressBar.value = 15;

              // replace the line in the file
              newFileString = newFileString.Replace("m_Script: " + IDString, "m_Script: " + CVRFURY_M_SCRIPT_ID);

              // set the text of the progressLabel
              progressLabel.text = "30% -- Rebinding Script";
              // set the value of the progressBar
              progressBar.value = 30;

              // Wait for barDelay
              await Task.Delay(barDelay);

              // write the new string to the file
              File.WriteAllText(newFilePath, newFileString);

              // set the text of the progressLabel to "90% -- Writing File"
              progressLabel.text = "40% -- Writing File";
              // set the value of the progressBar
              progressBar.value = 40;

              // Wait for barDelay
              await Task.Delay(barDelay);

              // refresh the asset database
              AssetDatabase.Refresh();

              // set the text of the progressLabel
              progressLabel.text = "50% -- Stage 1 Complete";
              // set the value of the progressBar
              progressBar.value = 50;

              // load the intermediate menu file based off the 'newFilePath' an the class 'VRCExpressionsMenu'(stub)
              VRCExpressionsMenu menuToImport = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(newFilePath);

              // set the text of the progressLabel
              progressLabel.text = "60% -- Loading Intermediate File";
              // set the value of the progressBar
              progressBar.value = 60;

              // Wait for barDelay
              await Task.Delay(barDelay);

              // set the text of the progressLabel
              progressLabel.text = "65% -- Processing Controls";
              // set the value of the progressBar
              progressBar.value = 65;

              // Wait for barDelay
              await Task.Delay(barDelay);

              // get the number of controls in the menuToImport
              var controlCount = menuToImport.controls.Count;

              // divide 30 by the number of controls
              var controlStep = 30 / controlCount;

              // create CVRFuryMenuStore to store the converted menu
              CVRFuryMenuStore convertedMenu = ScriptableObject.CreateInstance<CVRFuryMenuStore>();

              // for each control in the menuToImport
              foreach (var control in menuToImport.controls)
              {
                // set the text of the progressLabel
                progressLabel.text = "65% -- Processing Controls\n\n" + controlCount + " controls remaining";
                // set the value of the progressBar
                progressBar.value += controlStep;

                // get the type of the control
                ControlType controlType = control.type;

                // NOTES:
                // CVR has the menu item name and parameter linked together so for converted
                // stuff CVR name = VRC parameter
                // spaces in the name are ignored in the parameter
                // case sensitivity persist in the parameter so 'Test Var' in the name will be TestVar
                // in the parameter
                // '\' in the name will be ignored in the parameter
                // '/' in the name will be kept in the parameter

                // case statement to handle the different control types
                switch (controlType)
                {
                  case ControlType.Button:
                  // if the control is a button
                  // there is not really an analogue for this in CVR, best we can do for this is a toggle
                  //break; //this break is disabled to allow the toggle to be created
                  case ControlType.Toggle:
                    // create a new toggleParameter
                    toggleParameter newToggle = new toggleParameter();

                    // parse the Name Vs Parameter in the control
                    // compare the parameter to the name when the spaces are removed
                    // if they are the same then the name can be used as the new toggle name
                    // if they are different then the parameter is used as the new toggle name
                    if (control.name.Replace(" ", "") == control.parameter.name)
                    {
                      // set the name of the new toggle to the name of the control
                      newToggle.name = control.name;
                    }
                    else
                    {
                      // set the name of the new toggle to the parameter of the control,
                      newToggle.name = control.parameter.name;
                    }

                    // set the default state of the new toggle to the value of the control
                    newToggle.defaultState = control.value == 1f ? true : false;

                    // set the generateType of the new toggle to bool
                    newToggle.generateType = toggleParameter.GenerateType.Bool;

                    // add the new toggle to the convertedMenu
                    convertedMenu.menuItems.Add(newToggle);

                    break;

                  case ControlType.SubMenu:
                    // there is not really an analogue for this in CVR
                    // there is a mod that has a sub menu but it is not a standard CVR feature
                    // so for now not supported, maybe in the future
                    ////////////////
                    // TODO: add support for SubMenu
                    ////////////////
                    // send a warning to the console
                    CoreLog("SubMenu not supported, please add / up vote a feature request on the GitHub page");
                    break;

                  case ControlType.TwoAxisPuppet:
                    ////////////////
                    // TODO: add support for TwoAxisPuppet
                    ////////////////
                    break;

                  case ControlType.FourAxisPuppet:
                    ////////////////
                    // TODO: add support for FourAxisPuppet
                    ////////////////
                    break;

                  case ControlType.RadialPuppet:
                    ////////////////
                    // TODO: add support for RadialPuppet
                    ////////////////
                    break;

                  default:
                    // write warning to console about unhandled control type
                    CoreLog("Unhandled control type: " + controlType.ToString());
                    break;
                }
              }

              // set the text of the progressLabel
              progressLabel.text = "95% -- Writing Output File";
              // set the value of the progressBar
              progressBar.value = 95;

              // write the convertedMenu to a new file
              AssetDatabase.CreateAsset(convertedMenu, newFilePathFinal);

              // Wait for barDelay
              await Task.Delay(barDelay);

              // if EditorPrefs bool SuppressDeleteExpressionMenuIntermediate is false,
              // delete the intermediate file
              if (!EditorPrefs.GetBool("SuppressDeleteExpressionMenuIntermediate", false))
              {
                // set the text of the progressLabel
                progressLabel.text = "97% -- Cleaning Up";
                // set the value of the progressBar
                progressBar.value = 97;

                // delete the intermediate file
                AssetDatabase.DeleteAsset(newFilePath);
              }

              // set the text of the progressLabel
              progressLabel.text = "100% -- Conversion Complete";
              // set the value of the progressBar
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
              "The file you have selected does not appear to be a VRCExpressionMenu file.",
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
          "The file you have selected does not have the correct extension. Please select a file with the extension '.asset'.",
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
    public (bool, string) checkScriptIDs(string menuFileString, List<string> scriptIDs)
    {
      // output vars
      bool IDmatch = false;
      string IDString = "";

      // check provided string is not null or empty
      if (!string.IsNullOrEmpty(menuFileString))
      {
        // not null or empty, so check if the string contains the line 'm_Script: '

        // check if the string contains the line 'm_Script: '
        if (menuFileString.ToString().Contains("m_Script:"))
        {
          // if the string contains the line 'm_Script: '
          // check if any of the strings in scriptIDs are contained in the string
          foreach (string id in scriptIDs)
          {
            if (menuFileString.ToString().Contains(id))
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

    // function to check if a file content is a the CVRFury version of VRCExpressionMenu
    public bool checkCVRFuryScriptID(string menuFileString)
    {
      // output vars
      bool IDmatch = false;

      // check provided string is not null or empty
      if (!string.IsNullOrEmpty(menuFileString))
      {
        // not null or empty, so check if the string contains the line 'm_Script: '

        // check if the string contains the line 'm_Script: '
        if (menuFileString.ToString().Contains("m_Script:"))
        {
          // if the string contains the line 'm_Script: '
          // check if the string contains the line 'm_Script: ' + CVRFURY_M_SCRIPT_ID
          if (menuFileString.ToString().Contains("m_Script: " + CVRFURY_M_SCRIPT_ID))
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

  public class SuppressDeleteExpressionMenuIntermediateMenu
  {
    private const string MENU_PATH =
      "NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Debug/Suppress Delete Menu Intermediate";
    private const string EDITOR_PREFS_KEY = "SuppressDeleteExpressionMenuIntermediate";

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
