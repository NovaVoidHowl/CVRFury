#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using System.Collections.Generic;

using uk.novavoidhowl.dev.cvrfury.packagecore;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;
using uk.novavoidhowl.dev.cvrfury.runtime;

namespace uk.novavoidhowl.dev.cvrfury.generator
{
  public class CreateLinkedStoresDialog : EditorWindow
  {
    private string input = "";
    private bool createController = true;
    private bool createMenuStore = true;
    private bool createParamStore = true;
    private bool controllerExists = false;
    private bool menuStoreExists = false;
    private bool paramStoreExists = false;
    private string folderPath;
    private bool wasConfirmed = false;
    private TaskCompletionSource<(
      string name,
      bool createController,
      bool createMenuStore,
      bool createParamStore
    )> completionSource;

    private TextField nameField;
    private Toggle createControllerToggle;
    private Toggle createMenuStoreToggle;
    private Toggle createParamStoreToggle;
    private Label warningLabel;
    private Button confirmButton;
    private Button cancelButton;

    public static async Task<(string name, bool createController, bool createMenuStore, bool createParamStore)> Show(
      string title,
      string description,
      string defaultText,
      string folder
    )
    {
      var window = CreateInstance<CreateLinkedStoresDialog>();
      window.titleContent = new GUIContent(title);
      window.input = defaultText;
      window.folderPath = folder;
      window.completionSource =
        new TaskCompletionSource<(string name, bool createController, bool createMenuStore, bool createParamStore)>();
      window.minSize = new Vector2(300, 200);
      window.maxSize = new Vector2(500, 250);
      window.ShowUtility();

      return await window.completionSource.Task;
    }

    private void CreateGUI()
    {
      rootVisualElement.Clear();

      // Load UXML
      var visualTree = Resources.Load<VisualTreeAsset>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryGenUI/UnityUXML/CVRFuryCreateLinkedStores"
      );
      if (visualTree == null)
      {
        CoreLogError("Failed to load UXML");
        return;
      }

      // Load stylesheet
      var styleSheet = Resources.Load<StyleSheet>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryGenUI/UnityStyleSheets/CVRFuryCreateLinkedStores"
      );
      if (styleSheet != null)
        rootVisualElement.styleSheets.Add(styleSheet);

      // Clone tree
      visualTree.CloneTree(rootVisualElement);

      // Get references
      nameField = rootVisualElement.Q<TextField>("nameField");
      createControllerToggle = rootVisualElement.Q<Toggle>("createControllerToggle");
      createMenuStoreToggle = rootVisualElement.Q<Toggle>("createMenuStoreToggle");
      createParamStoreToggle = rootVisualElement.Q<Toggle>("createParamStoreToggle");
      warningLabel = rootVisualElement.Q<Label>("warningLabel");
      confirmButton = rootVisualElement.Q<Button>("confirmButton");
      cancelButton = rootVisualElement.Q<Button>("cancelButton");

      // Setup initial values
      nameField.value = input;
      createControllerToggle.value = createController;
      createMenuStoreToggle.value = createMenuStore;
      createParamStoreToggle.value = createParamStore;
      CheckExistingAssets();

      // Register callbacks
      nameField.RegisterValueChangedCallback(evt =>
      {
        input = evt.newValue;
        CheckExistingAssets();
        UpdateButtonState();
      });

      createControllerToggle.RegisterValueChangedCallback(evt =>
      {
        createController = evt.newValue;
      });

      createMenuStoreToggle.RegisterValueChangedCallback(evt =>
      {
        createMenuStore = evt.newValue;
      });

      createParamStoreToggle.RegisterValueChangedCallback(evt =>
      {
        createParamStore = evt.newValue;
      });

      confirmButton.clicked += () =>
      {
        wasConfirmed = true;
        completionSource.TrySetResult((input, createController, createMenuStore, createParamStore));
        Close();
      };

      cancelButton.clicked += () =>
      {
        Close();
      };

      // Initial button state
      UpdateButtonState();
    }

    private void UpdateButtonState()
    {
      if (confirmButton != null)
      {
        bool hasValidName = !string.IsNullOrWhiteSpace(input);
        bool canCreateSomething =
          (createController && !controllerExists)
          || (createMenuStore && !menuStoreExists)
          || (createParamStore && !paramStoreExists);

        confirmButton.SetEnabled(hasValidName && canCreateSomething);
      }
    }

    private void OnDestroy()
    {
      if (!wasConfirmed)
      {
        completionSource.TrySetResult((null, false, false, false));
      }
    }

    private void CheckExistingAssets()
    {
      if (string.IsNullOrWhiteSpace(input))
        return;

      string potentialControllerPath = $"{folderPath}/{input}_Controller.controller";
      string potentialMenuStorePath = $"{folderPath}/{input}_MenuStore.asset";
      string potentialParamStorePath = $"{folderPath}/{input}_ParametersStore.asset";

      controllerExists = System.IO.File.Exists(potentialControllerPath);
      menuStoreExists = System.IO.File.Exists(potentialMenuStorePath);
      paramStoreExists = System.IO.File.Exists(potentialParamStorePath);

      // Update UI elements
      createControllerToggle.SetEnabled(!controllerExists);
      createMenuStoreToggle.SetEnabled(!menuStoreExists);
      createParamStoreToggle.SetEnabled(!paramStoreExists);

      if (controllerExists)
        createControllerToggle.value = false;
      if (menuStoreExists)
        createMenuStoreToggle.value = false;
      if (paramStoreExists)
        createParamStoreToggle.value = false;

      // Update warning message
      var warnings = new List<string>();
      if (controllerExists)
        warnings.Add("Controller");
      if (menuStoreExists)
        warnings.Add("Menu Store");
      if (paramStoreExists)
        warnings.Add("Parameter Store");

      if (warnings.Count > 0)
      {
        warningLabel.text = $"Following assets already exist: {string.Join(", ", warnings)}";
        warningLabel.style.display = DisplayStyle.Flex;
      }
      else
      {
        warningLabel.style.display = DisplayStyle.None;
      }

      UpdateButtonState();
    }
  }

  public class CreateLinkedStores
  {
    private static T FindExistingAsset<T>(string folderPath, string baseName, string suffix)
      where T : UnityEngine.Object
    {
      string assetPath = $"{folderPath}/{baseName}_{suffix}";
      return AssetDatabase.LoadAssetAtPath<T>(assetPath);
    }

    [MenuItem("Assets/Create/CVRFury/Linked Stores Set")]
    public static async void CreateLinkedStoresSet()
    {
      string folderPath = GetSelectedFolderPath();
      var (baseName, shouldCreateController, shouldCreateMenuStore, shouldCreateParamStore) =
        await CreateLinkedStoresDialog.Show(
          "Create Linked Stores",
          "Enter base name for the stores:",
          "NewCVRFurySet",
          folderPath
        );

      if (string.IsNullOrEmpty(baseName))
        return;

      // Find existing assets
      var existingMenuStore = FindExistingAsset<CVRFuryMenuStore>(folderPath, baseName, "MenuStore.asset");
      var existingParamStore = FindExistingAsset<CVRFuryParametersStore>(folderPath, baseName, "ParametersStore.asset");
      var existingController = FindExistingAsset<AnimatorController>(folderPath, baseName, "Controller.controller");

      // Create or use existing parameter store
      CVRFuryParametersStore paramStore = existingParamStore;
      if (shouldCreateParamStore && paramStore == null)
      {
        paramStore = ScriptableObject.CreateInstance<CVRFuryParametersStore>();
        string paramStorePath = $"{folderPath}/{baseName}_ParametersStore.asset";
        AssetDatabase.CreateAsset(paramStore, paramStorePath);
      }

      // Create or use existing menu store
      CVRFuryMenuStore menuStore = existingMenuStore;
      if (shouldCreateMenuStore && menuStore == null)
      {
        menuStore = ScriptableObject.CreateInstance<CVRFuryMenuStore>();
        string menuStorePath = $"{folderPath}/{baseName}_MenuStore.asset";
        AssetDatabase.CreateAsset(menuStore, menuStorePath);
      }

      // Create or use existing controller
      AnimatorController animator = existingController;
      if (shouldCreateController && animator == null && paramStore != null)
      {
        animator = new AnimatorController();
        string animatorPath = $"{folderPath}/{baseName}_Controller.controller";
        AssetDatabase.CreateAsset(animator, animatorPath);
      }

      // Clean and link assets
      if (menuStore != null && paramStore != null)
      {
        // Remove null entries
        menuStore.relatedParametersStores.RemoveAll(item => item == null);

        if (!menuStore.relatedParametersStores.Contains(paramStore))
        {
          menuStore.relatedParametersStores.Add(paramStore);
          EditorUtility.SetDirty(menuStore);
        }
      }

      if (paramStore != null && animator != null)
      {
        // Remove null entries
        paramStore.relatedAnimationControllers.RemoveAll(item => item == null);

        if (!paramStore.relatedAnimationControllers.Contains(animator))
        {
          paramStore.relatedAnimationControllers.Add(animator);
          EditorUtility.SetDirty(paramStore);
        }
      }

      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();

      // Select the most relevant asset
      if (menuStore != null)
        Selection.activeObject = menuStore;
      else if (paramStore != null)
        Selection.activeObject = paramStore;
      else if (animator != null)
        Selection.activeObject = animator;
    }

    private static string GetSelectedFolderPath()
    {
      string path = "Assets";
      Object[] selection = Selection.GetFiltered<Object>(SelectionMode.Assets);

      if (selection.Length > 0)
      {
        path = AssetDatabase.GetAssetPath(selection[0]);
        if (!string.IsNullOrEmpty(path) && !System.IO.Directory.Exists(path))
        {
          path = System.IO.Path.GetDirectoryName(path);
        }
      }

      return path;
    }
  }
}

#endif // UNITY_EDITOR
