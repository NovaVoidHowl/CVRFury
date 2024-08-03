// Version 0.0.1
// editor only script to manage what channel of the package is being used and apply updates to the package
#if UNITY_EDITOR

using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor;

using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.nvhpmm
{
  [ExecuteInEditMode]
  public partial class CoreUpdateManager : EditorWindow
  {
    private ChannelList channelList;
    private ReleaseContainer releaseContainer;
    private bool severComErrorCVRFuryAPI = false;
    private bool severComErrorGithubAPI = false;
    private ListRequest listRequest;
    private const string SelectedChannelKey = "SelectedChannel";

    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Update Manager", false, 10001)]
    public static void ShowWindow()
    {
      CoreUpdateManager window = (CoreUpdateManager)
        EditorWindow.GetWindow(
          typeof(CoreUpdateManager),
          true,
          "Core Update Manager - " + Constants.PROGRAM_DISPLAY_NAME
        );
      window.maxSize = new Vector2(600, 600);
      window.minSize = new Vector2(600, 300);
      window.Show();
    }

    private void OnEnable()
    {
      EditorCoroutineUtility.StartCoroutine(FetchChannelsDataFromCVRFuryAPI(), this);
      EditorCoroutineUtility.StartCoroutine(FetchReleaseDataFromGithubAPI(), this);

      EditorApplication.projectChanged += refreshDepMgrUI;
    }

    private void OnDisable()
    {
      EditorApplication.projectChanged -= refreshDepMgrUI;
    }

    private void refreshDepMgrUI()
    {
      // remove all children from the root
      rootVisualElement.Clear();
      // re-render the UI
      renderDepMgrUI();
    }

    private void renderDepMgrUI()
    {
      // set the root visual element to have padding
      rootVisualElement.style.paddingLeft = 10;
      rootVisualElement.style.paddingRight = 10;
      rootVisualElement.style.paddingTop = 10;
      rootVisualElement.style.paddingBottom = 10;

#if !NVH_CVRFURY_DEV_PACKAGE_OVERRIDE

      // Check if folder exists at Packages\uk.novavoidhowl.dev.cvrfury
      string packagePath = Path.Combine(Application.dataPath, "..", "Packages", "uk.novavoidhowl.dev.cvrfury");
      Debug.Log("Checking path: " + packagePath);

      DirectoryInfo dirInfo = new DirectoryInfo(packagePath);
      if (dirInfo.Exists && (dirInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
      {
        Debug.Log("Package found in Packages directory");

        // Add a label to say that the package is not installed
        Label ManualInstallLabel = new Label("Package Manually Installed");
        // Set text to 20px
        ManualInstallLabel.style.fontSize = 20;
        // Center the text
        ManualInstallLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        // Add the label to the UI
        rootVisualElement.Add(ManualInstallLabel);

        // Add a space
        rootVisualElement.Add(new Label(" "));

        // Add text area to tell the user that they are in manual mode
        TextElement ManualInstallText = new TextElement();
        ManualInstallText.text =
          "You are in manual mode, you will need to update the package manually"
          + "\nThis mode of operation is intended for development purposes only";
        // Set text to 14px
        ManualInstallText.style.fontSize = 16;
        // Center the text
        ManualInstallText.style.unityTextAlign = TextAnchor.MiddleCenter;
        // Add the text area to the UI
        rootVisualElement.Add(ManualInstallText);

        // Add a space
        rootVisualElement.Add(new Label(" "));

        // Add the current version of the package
        string packageVersionManual = getPackageVersion();
        // Add the version to the UI
        rootVisualElement.Add(new Label("Current Installed Version: " + packageVersionManual));
        // Center the text
        rootVisualElement.ElementAt(rootVisualElement.childCount - 1).style.unityTextAlign = TextAnchor.MiddleCenter;

        return;
      }
      else
      {
        Debug.Log("Package not found in Packages directory");
      }
#endif

      // add a space
      rootVisualElement.Add(new Label(" "));

      // get the version of the package
      string packageVersion = getPackageVersion();
      // add the version to the UI
      rootVisualElement.Add(new Label("Current Installed Version: " + packageVersion));

      // add a space
      rootVisualElement.Add(new Label(" "));

      if (severComErrorCVRFuryAPI)
      {
        Label errorLabel = new Label("Error fetching data from the update CVRFury API server");
        errorLabel.style.color = Color.red;
        // set background color to black
        errorLabel.style.backgroundColor = Color.black;
        // set padding to 10px
        errorLabel.style.paddingTop = 10;
        errorLabel.style.paddingBottom = 10;
        // center the text
        errorLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        // set font size to 20px
        errorLabel.style.fontSize = 20;
        rootVisualElement.Add(errorLabel);

        // add a space
        rootVisualElement.Add(new Label(" "));

        // add retry button
        Button retryButton = new Button(() =>
        {
          EditorCoroutineUtility.StartCoroutine(FetchChannelsDataFromCVRFuryAPI(), this);
        })
        {
          text = "Retry Connection",
          style = { width = 160, height = 30 }
        };
        rootVisualElement.Add(retryButton);
        // center the button
        retryButton.style.marginLeft = 220;
        //

        return;
      }

      if (severComErrorGithubAPI)
      {
        Label errorLabel = new Label("Error fetching data from the update Gitlab API server");
        errorLabel.style.color = Color.red;
        // set background color to black
        errorLabel.style.backgroundColor = Color.black;
        // set padding to 10px
        errorLabel.style.paddingTop = 10;
        errorLabel.style.paddingBottom = 10;
        // center the text
        errorLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        // set font size to 20px
        errorLabel.style.fontSize = 20;
        rootVisualElement.Add(errorLabel);

        // add a space
        rootVisualElement.Add(new Label(" "));

        // add retry button
        Button retryButton = new Button(() =>
        {
          EditorCoroutineUtility.StartCoroutine(FetchReleaseDataFromGithubAPI(), this);
        })
        {
          text = "Retry Connection",
          style = { width = 160, height = 30 }
        };
        rootVisualElement.Add(retryButton);
        // center the button
        retryButton.style.marginLeft = 220;
        //

        return;
      }

      // add a space
      rootVisualElement.Add(new Label(" "));

      // add a label for the channel selection
      rootVisualElement.Add(new Label("Update Options"));
      // make the label bold
      rootVisualElement.ElementAt(rootVisualElement.childCount - 1).style.unityFontStyleAndWeight = FontStyle.Bold;

      // add a space
      rootVisualElement.Add(new Label(" "));

      // Filter the channels to exclude those with the 'hide' property set
      var visibleChannels = channelList.channels.Where(c => !c.hide).ToList();

      // check if scripting define symbol 'NVH_CVRFURY_DEV_ENABLED' is set
#if NVH_CVRFURY_DEV_ENABLED
      // debug print that the dev mode is enabled
      Debug.Log("[CoreUpdateManager]Dev Mode is enabled");
      // show all channels
      visibleChannels = channelList.channels.ToList();
#endif

      // Convert the filtered list to a list of names
      var channelNames = visibleChannels.ConvertAll(c => c.name);

      // Load the saved selected channel
      string savedChannel = EditorPrefs.GetString(SelectedChannelKey, channelNames.FirstOrDefault());

      // Create a container for the dropdown and button
      VisualElement container = new VisualElement();
      container.style.flexDirection = FlexDirection.Row;

      // Add dropdown for channel selection
      DropdownField channelDropdown = new DropdownField("Channel", channelNames, channelNames.IndexOf(savedChannel));
      container.Add(channelDropdown);

      // Save the selected channel when changed
      channelDropdown.RegisterValueChangedCallback(evt =>
      {
        EditorPrefs.SetString(SelectedChannelKey, evt.newValue);
      });

      // Create the refresh button with an icon
      Button fetchDataButton = new Button(() =>
      {
        EditorCoroutineUtility.StartCoroutine(FetchChannelsDataFromCVRFuryAPI(), this);
      })
      {
        style =
        {
          width = 25,
          height = 25,
          paddingLeft = 4,
          paddingRight = 4,
          paddingTop = 4,
          paddingBottom = 4
        }
      };

      // Use a built-in refresh icon
      var refreshIcon = (Texture2D)EditorGUIUtility.IconContent("Refresh").image;
      VisualElement iconElement = new VisualElement();
      iconElement.style.backgroundImage = new StyleBackground(refreshIcon);
      iconElement.style.width = 16;
      iconElement.style.height = 16;
      iconElement.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;

      // Add the icon element to the button
      fetchDataButton.Add(iconElement);

      // Add the button to the container
      container.Add(fetchDataButton);

      // Add the container to the root visual element
      rootVisualElement.Add(container);

      // add a space
      rootVisualElement.Add(new Label(" "));

      // add the update button
      Button updateButton = new Button(() =>
      {
        updateCorePackage();
      })
      {
        text = "Update Core Package",
        style = { width = 200, height = 50 }
      };
      rootVisualElement.Add(updateButton);
    }

    private void updateCorePackage()
    {
      // update the package
      Debug.Log("Updating core package");
    }

    private IEnumerator FetchReleaseDataFromGithubAPI()
    {
      string channels_endpoint = Constants.GITHUB_API_BASE_URL + "/releases";

      using (UnityWebRequest webRequest = UnityWebRequest.Get(channels_endpoint))
      {
        yield return webRequest.SendWebRequest();

        if (
          webRequest.result == UnityWebRequest.Result.ConnectionError
          || webRequest.result == UnityWebRequest.Result.ProtocolError
        )
        {
          Debug.LogError(webRequest.error);
          severComErrorGithubAPI = true;
        }
        else
        {
          severComErrorGithubAPI = false;
          // Parse the JSON response
          string jsonResponse = webRequest.downloadHandler.text;

          // Wrap the JSON array in an object
          string wrappedJsonResponse = "{\"releases\":" + jsonResponse + "}";

          // Deserialize JSON
          releaseContainer = JsonUtility.FromJson<ReleaseContainer>(wrappedJsonResponse);

          // Use the deserialized data
          foreach (var release in releaseContainer.releases)
          {
            Debug.Log("Release: " + release.name);
          }
        }
      }

      // refresh the UI
      refreshDepMgrUI();
    }

    private IEnumerator FetchChannelsDataFromCVRFuryAPI()
    {
      string channels_endpoint = Constants.CVRFURY_API_BASE_URL + "/v1/channels";

      using (UnityWebRequest webRequest = UnityWebRequest.Get(channels_endpoint))
      {
        yield return webRequest.SendWebRequest();

        if (
          webRequest.result == UnityWebRequest.Result.ConnectionError
          || webRequest.result == UnityWebRequest.Result.ProtocolError
        )
        {
          Debug.LogError(webRequest.error);
          severComErrorCVRFuryAPI = true;
        }
        else
        {
          severComErrorCVRFuryAPI = false;
          // Parse the JSON response
          string jsonResponse = webRequest.downloadHandler.text;

          // Deserialize JSON to ChannelList
          channelList = JsonUtility.FromJson<ChannelList>(jsonResponse);

          // Process the JSON data as needed
          foreach (var channel in channelList.channels)
          {
            Debug.Log($"Channel Name: {channel.name}, State: {channel.state}, Hide: {channel.hide}");
          }
        }
      }

      // refresh the UI
      refreshDepMgrUI();
    }

    private string getPackageVersion()
    {
      string packageName = "uk.novavoidhowl.dev.cvrfury";
      listRequest = Client.List(true); // List all packages, including built-in ones

      while (!listRequest.IsCompleted)
      {
        // Wait for the request to complete
      }

      if (listRequest.Status == StatusCode.Success)
      {
        var package = listRequest.Result.FirstOrDefault(p => p.name == packageName);
        if (package != null)
        {
          return package.version;
        }
        else
        {
          return "Package not found";
        }
      }
      else
      {
        return "Error fetching package list";
      }
    }
  }

  #region support classes

  #region CVRFury API response classes
  [System.Serializable]
  public class Channel
  {
    public string state;
    public string name;
    public bool hide;
  }

  [System.Serializable]
  public class ChannelList
  {
    public List<Channel> channels;
  }
  #endregion // CVRFury API response classes

  #region github API response classes
  [System.Serializable]
  public class ReleaseContainer
  {
    public Release[] releases;
  }

  [System.Serializable]
  public class Release
  {
    public string tag_name;
    public string name;
    public string body;
    public bool prerelease;
    public string published_at;
  }
  #endregion // github API response classes
  #endregion // support classes
}
#endif
