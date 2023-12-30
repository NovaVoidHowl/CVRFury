using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

// only need to change the following line, here
// in 'supportingClasses\AppInternalPackages.cs'
// and the asmdef, to bind to project specific constants

using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;

namespace uk.novavoidhowl.dev.nvhpmm
{
  public partial class ToolSetup : EditorWindow
  {
    private VisualElement RenderThirdPartyDependencies(string scriptingDefines)
    {
      var root = new VisualElement();

      var title = new Label("3rd Party Dependencies");
      // add the sectionTitle class to the title
      title.AddToClassList("sectionTitle");
      root.Add(title);

      // UIElements button to refresh the list of Third Party dependencies
      var refreshButton = new Button(() =>
      {
        ThirdPartyDependenciesPackages.refreshThirdPartyDependencies();
        refreshDepMgrUI();
      })
      {
        text = "Refresh Dependencies List"
      };

      // add the refreshButton class to the refreshButton
      refreshButton.AddToClassList("refreshButton");
      root.Add(refreshButton);

      int thirdPartyDependenciesCount = SharedData.ThirdPartyDependencies.Count;

      if (thirdPartyDependenciesCount == 0)
      {
        var helpBox = new Label("No 3rd party dependencies found.");
        // add the help box class to the help box
        helpBox.AddToClassList("helpBox");
        root.Add(helpBox);
      }
      else
      {
        var visualTree = Resources.Load<VisualTreeAsset>(
          Constants.PROGRAM_DISPLAY_NAME + "/nvhpmm/UnityUXML/ThirdPartyDependency"
        );
        foreach (var dependency in SharedData.ThirdPartyDependencies)
        {
          // bool for if there is a message to show
          bool showMessage = false;

          // bool for each level of message to show (info, warning, error)
          bool showInfo = false;
          bool showWarning = false;
          bool showError = false;
          string messageContent = "";

          // clone in the visual tree for the dependency
          var dependencyContainer = visualTree.CloneTree().Q<VisualElement>("dependencyContainer");

          // set the name of the dependency
          dependencyContainer.Q<Label>("dependencyName").text = dependency.Name;
          // set the description of the dependency
          dependencyContainer.Q<Label>("dependencyDescriptionTitle").text = "Description:";
          dependencyContainer.Q<Label>("dependencyDescription").text = dependency.Description;
          // set the type of the dependency
          dependencyContainer.Q<Label>("dependencyType").text = "Type: " + dependency.DependencyType + "\n";

          // get a reference to the 'detectionTypeBubble' UI element
          var detectionTypeBubble = dependencyContainer.Q("detectionTypeBubble");

          // get a reference to the 'detectionTypeBubbleLabel' UI element
          var detectionTypeBubbleLabel = detectionTypeBubble.Q<Label>("detectionTypeBubbleLabel");

          // string for prefix of the detection type
          string detectionTypePrefix = "Install Check Method \n";
          // logic to check if the dependency is installed
          bool installStatus = false;
          switch (dependency.InstallCheckMode)
          {
            case "Scripting Define Symbol":
              detectionTypeBubbleLabel.text = detectionTypePrefix + "Scripting Define Symbol";
              if (scriptingDefines.Contains(dependency.InstallCheckValue))
              {
                installStatus = true;
              }
              break;
            case "Package Manager":
              detectionTypeBubbleLabel.text = detectionTypePrefix + "Unity Package Manager";
              if (IsPackageInstalled(dependency.InstallCheckValue))
              {
                installStatus = true;
              }
              break;
            case "Package Manager Implicit":
              detectionTypeBubbleLabel.text = detectionTypePrefix + "Unity Package Manager";
              if (IsImplicitPackageInstalled(dependency.InstallCheckValue))
              {
                installStatus = true;
              }
              break;
            case "File Exists":
              detectionTypeBubbleLabel.text = detectionTypePrefix + "Check File";
              if (File.Exists(dependency.InstallCheckValue))
              {
                installStatus = true;
              }
              break;
            case "Folder Exists":
              detectionTypeBubbleLabel.text = detectionTypePrefix + "Check Folder";
              if (Directory.Exists(dependency.InstallCheckValue))
              {
                installStatus = true;
              }
              break;
            default:
              detectionTypeBubbleLabel.text = detectionTypePrefix + "Unknown";
              showError = true;
              messageContent = messageContent + "Unknown detection type: \"" + dependency.InstallCheckMode + "\"\n";

              break;
          }

          // Get a reference to the 'installStateSideBubble' UI element
          var installStateSideBubble = dependencyContainer.Q("installStateSideBubble");

          // get a reference to the 'installStateSideBubbleLabel' UI element
          var installStateSideBubbleLabel = dependencyContainer.Q<Label>("installStateSideBubbleLabel");

          // set the text of the 'installStateSideBubbleLabel' UI element
          installStateSideBubbleLabel.text = installStatus ? "Installed" : "Not Installed";

          // rotate the 'installStateSideBubbleLabel' UI element to the correct angle (90 degrees)
          installStateSideBubbleLabel.transform.rotation = Quaternion.Euler(0, 0, 90);

          // if the dependency is installed, set the class to installed
          if (installStatus)
          {
            installStateSideBubble.AddToClassList("installed");
            installStateSideBubble.AddToClassList("installedDependency");
            dependencyContainer.AddToClassList("installedDependency");
          }
          if (!installStatus)
          {
            installStateSideBubble.AddToClassList("notInstalled");
            if (dependency.DependencyType == "Optional")
            {
              installStateSideBubble.AddToClassList("optionalDependencyNotInstalled");
              dependencyContainer.AddToClassList("optionalDependencyNotInstalled");
            }
            else
            {
              messageContent = messageContent + "This dependency is mandatory and must be installed." + "\n";
              showError = true; // show the message box as a mandatory dependency is not installed
              installStateSideBubble.AddToClassList("mandatoryDependencyNotInstalled");
              dependencyContainer.AddToClassList("mandatoryDependencyNotInstalled");
            }
          }

          foreach (var button in dependency.Buttons)
          {
            var linkButton = new Button(() => Application.OpenURL(button.ButtonLink)) { text = button.ButtonText };
            // add the linkButton class to the button
            linkButton.AddToClassList("linkButton");
            dependencyContainer.Q<VisualElement>("ButtonsContainer").Add(linkButton);
          }

          // get a reference to the 'messageBox' UI element
          var messageBox = dependencyContainer.Q<VisualElement>("messageBox");

          // read the bool values for the message types from the dependency,
          // if any are true, set the bool for 'showMessage' to true
          if (showInfo || showWarning || showError)
          {
            showMessage = true;
          }

          // check if there is a message to be displayed
          if (showMessage)
          {
            // if there is a message, show the 'messageBox' UI element and set its text
            messageBox.style.display = DisplayStyle.Flex;
            // set the side bubble to the correct border type via class
            installStateSideBubble.AddToClassList("withMessage");
          }
          else
          {
            // if there is no message, hide the 'messageBox' UI element
            messageBox.style.display = DisplayStyle.None;
            // set the side bubble to the correct border type via class
            installStateSideBubble.AddToClassList("noMessage");
          }

          // get a reference to the 'messageBoxText' UI element
          var messageBoxText = messageBox.Q<Label>("message");
          // get a reference to the 'messageType' UI element
          var messageType = messageBox.Q<Label>("messageType");
          // get a reference to the 'icon' UI element
          var icon = messageBox.Q<VisualElement>("icon");

          if (showInfo)
          {
            // load the VectorImage from the Resources folder
            VectorImage infoIcon = Resources.Load<VectorImage>(
              Constants.PROGRAM_DISPLAY_NAME + "/nvhpmm/IconsAndImages/info"
            );

            // create a StyleBackground from the VectorImage
            StyleBackground infoBackground = new StyleBackground(infoIcon);

            // set the StyleBackground as the background image for the 'icon' UI element
            icon.style.backgroundImage = infoBackground;

            // set the text of 'messageType' to 'Info'
            messageType.text = "Info";

            // set the text of 'messageBoxText' to the message content
            messageBoxText.text = messageContent;
          }
          if (showWarning)
          {
            // load the VectorImage from the Resources folder
            VectorImage warningIcon = Resources.Load<VectorImage>(
              Constants.PROGRAM_DISPLAY_NAME + "/nvhpmm/IconsAndImages/warning"
            );

            // create a StyleBackground from the VectorImage
            StyleBackground warningBackground = new StyleBackground(warningIcon);

            // set the StyleBackground as the background image for the 'icon' UI element
            icon.style.backgroundImage = warningBackground;

            // set the text of 'messageType' to 'Warning'
            messageType.text = "Warning";

            // set the text of 'messageBoxText' to the message content
            messageBoxText.text = messageContent;
          }
          if (showError)
          {
            // load the VectorImage from the Resources folder
            VectorImage errorIcon = Resources.Load<VectorImage>(
              Constants.PROGRAM_DISPLAY_NAME + "/nvhpmm/IconsAndImages/error"
            );

            // create a StyleBackground from the VectorImage
            StyleBackground errorBackground = new StyleBackground(errorIcon);

            // set the StyleBackground as the background image for the 'icon' UI element
            icon.style.backgroundImage = errorBackground;

            // set the text of 'messageType' to 'Error'
            messageType.text = "Error";

            // set the text of 'messageBoxText' to the message content
            messageBoxText.text = messageContent;
          }

          root.Add(dependencyContainer);
        }
      }
      // send the root VisualElement back to the calling function
      return root;
    }
  }
}
