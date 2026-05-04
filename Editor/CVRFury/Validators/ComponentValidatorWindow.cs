#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using VF.Model;
using uk.novavoidhowl.dev.cvrfury.hierarchy;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.validators
{
  public class ComponentValidatorWindow : EditorWindow
  {
    private GameObject rootObject;
    private List<ComponentIssue> issues = new List<ComponentIssue>();
    private bool showWarnings = true;
    private bool showErrors = true;
    private bool autoRefresh = false;
    private double lastRefreshTime = 0;
    private const double REFRESH_INTERVAL = 1.0; // Refresh every second when auto-refresh is on
    private string searchFilter = "";
    private bool expandAll = false;
    private Dictionary<string, bool> groupExpandedStates = new Dictionary<string, bool>();

    // UI Elements
    private ObjectField rootObjectField;
    private Button useSelectedButton;
    private Toggle showErrorsToggle;
    private Toggle showWarningsToggle;
    private Toggle autoRefreshToggle;
    private Button refreshButton;
    private ToolbarSearchField searchField;
    private Button clearSearchButton;
    private Button expandAllButton;
    private ScrollView issuesScrollView;
    private VisualElement noIssuesMessage;
    private VisualElement issuesContent;
    private Label errorCountLabel;
    private Label warningCountLabel;
    private VisualElement errorStatus;
    private VisualElement warningStatus;
    private VisualElement successStatus;

    [Serializable]
    public class ComponentIssue
    {
      public GameObject gameObject;
      public Component component;
      public IssueType issueType;
      public string description;
      public string path;
      public string category;
    }

    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Validators/Component Validator")]
    public static void ShowWindow()
    {
      var window = GetWindow<ComponentValidatorWindow>();
      window.titleContent = new GUIContent(
        "Component Validator",
        EditorGUIUtility.IconContent("d_console.infoicon").image
      );
      window.minSize = new Vector2(600, 350);
      window.Show();
    }

    [MenuItem("GameObject/CVRFury/Validate Components", false, 0)]
    private static void ValidateFromContext()
    {
      if (Selection.activeGameObject != null)
      {
        var window = GetWindow<ComponentValidatorWindow>();
        window.titleContent = new GUIContent(
          "Component Validator",
          EditorGUIUtility.IconContent("d_console.infoicon").image
        );
        window.minSize = new Vector2(600, 350);
        window.rootObject = Selection.activeGameObject;
        window.Show();
        window.RefreshIssues();
      }
    }

    [MenuItem("GameObject/CVRFury/Validate Components", true)]
    private static bool ValidateFromContextValidation()
    {
      return Selection.activeGameObject != null;
    }

    private void CreateGUI()
    {
      // Load and apply the stylesheet
      CoreLogDebug(
        "Loading stylesheet for Component Validator Window\n"
          + "trying to load from: "
          + Constants.PROGRAM_DISPLAY_NAME
          + "/CVRFuryValidators/UnityStyleSheets/ComponentValidatorWindow"
      );

      var stylesheet = Resources.Load<StyleSheet>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryValidators/UnityStyleSheets/ComponentValidatorWindow"
      );

      if (stylesheet == null)
      {
        CoreLogError("Failed to load StyleSheet for Component Validator Window");
        var errorLabel = new Label("CRITICAL ERROR : StyleSheet could not be loaded.");
        rootVisualElement.Add(errorLabel);
        return;
      }

      rootVisualElement.styleSheets.Add(stylesheet);

      // Load UXML
      var visualTree = Resources.Load<VisualTreeAsset>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryValidators/UnityUXML/ComponentValidatorWindow"
      );

      if (visualTree == null)
      {
        CoreLogError("Failed to load UXML for Component Validator Window");
        var errorLabel = new Label("CRITICAL ERROR : UXML could not be loaded.");
        rootVisualElement.Add(errorLabel);
        return;
      }

      // Instantiate UXML
      visualTree.CloneTree(rootVisualElement);

      // Get UI element references
      SetupUIElements();

      // Setup event handlers
      SetupEventHandlers();

      // Initialize with current selection if available
      if (rootObject == null && Selection.activeGameObject != null)
      {
        rootObject = Selection.activeGameObject;
        if (rootObjectField != null)
          rootObjectField.value = rootObject;
      }

      // Initial refresh
      RefreshIssues();
    }

    private void SetupUIElements()
    {
      rootObjectField = rootVisualElement.Q<ObjectField>("RootObjectField");
      useSelectedButton = rootVisualElement.Q<Button>("UseSelectedButton");
      showErrorsToggle = rootVisualElement.Q<Toggle>("ShowErrorsToggle");
      showWarningsToggle = rootVisualElement.Q<Toggle>("ShowWarningsToggle");
      autoRefreshToggle = rootVisualElement.Q<Toggle>("AutoRefreshToggle");
      refreshButton = rootVisualElement.Q<Button>("RefreshButton");
      searchField = rootVisualElement.Q<ToolbarSearchField>("SearchField");
      clearSearchButton = rootVisualElement.Q<Button>("ClearSearchButton");
      expandAllButton = rootVisualElement.Q<Button>("ExpandAllButton");
      issuesScrollView = rootVisualElement.Q<ScrollView>("IssuesScrollView");
      noIssuesMessage = rootVisualElement.Q<VisualElement>("NoIssuesMessage");
      issuesContent = rootVisualElement.Q<VisualElement>("IssuesContent");
      errorCountLabel = rootVisualElement.Q<Label>("ErrorCount");
      warningCountLabel = rootVisualElement.Q<Label>("WarningCount");
      errorStatus = rootVisualElement.Q<VisualElement>("ErrorStatus");
      warningStatus = rootVisualElement.Q<VisualElement>("WarningStatus");
      successStatus = rootVisualElement.Q<VisualElement>("SuccessStatus");

      // Set initial values
      if (rootObjectField != null)
        rootObjectField.value = rootObject;
      if (showErrorsToggle != null)
        showErrorsToggle.value = showErrors;
      if (showWarningsToggle != null)
        showWarningsToggle.value = showWarnings;
      if (autoRefreshToggle != null)
        autoRefreshToggle.value = autoRefresh;
      if (searchField != null)
        searchField.value = searchFilter;
    }

    private void SetupEventHandlers()
    {
      if (rootObjectField != null)
      {
        rootObjectField.RegisterValueChangedCallback(evt =>
        {
          rootObject = evt.newValue as GameObject;
          RefreshIssues();
        });
      }

      if (useSelectedButton != null)
      {
        useSelectedButton.clicked += () =>
        {
          if (Selection.activeGameObject != null)
          {
            rootObject = Selection.activeGameObject;
            if (rootObjectField != null)
              rootObjectField.value = rootObject;
            RefreshIssues();
          }
        };
      }

      if (showErrorsToggle != null)
      {
        showErrorsToggle.RegisterValueChangedCallback(evt =>
        {
          showErrors = evt.newValue;
          UpdateIssuesDisplay();
        });
      }

      if (showWarningsToggle != null)
      {
        showWarningsToggle.RegisterValueChangedCallback(evt =>
        {
          showWarnings = evt.newValue;
          UpdateIssuesDisplay();
        });
      }

      if (autoRefreshToggle != null)
      {
        autoRefreshToggle.RegisterValueChangedCallback(evt =>
        {
          autoRefresh = evt.newValue;
        });
      }

      if (refreshButton != null)
      {
        refreshButton.clicked += RefreshIssues;
      }

      if (searchField != null)
      {
        searchField.RegisterValueChangedCallback(evt =>
        {
          searchFilter = evt.newValue;
          UpdateIssuesDisplay();
        });
      }

      if (clearSearchButton != null)
      {
        clearSearchButton.clicked += () =>
        {
          if (searchField != null)
            searchField.value = "";
          searchFilter = "";
          UpdateIssuesDisplay();
        };
      }

      if (expandAllButton != null)
      {
        expandAllButton.clicked += () =>
        {
          expandAll = !expandAll;
          expandAllButton.text = expandAll ? "Collapse All" : "Expand All";

          // Clear stored states so all groups use the expandAll setting
          groupExpandedStates.Clear();

          UpdateIssuesDisplay();
        };
      }
    }

    private void Update()
    {
      // Auto refresh
      if (autoRefresh && EditorApplication.timeSinceStartup - lastRefreshTime > REFRESH_INTERVAL)
      {
        RefreshIssues();
        lastRefreshTime = EditorApplication.timeSinceStartup;
      }
    }

    private void UpdateStatusIcons()
    {
      int errorCount = issues.Count(i => i.issueType == IssueType.Error);
      int warningCount = issues.Count(i => i.issueType == IssueType.Warning);

      // Update error status
      if (errorStatus != null)
      {
        if (errorCount > 0)
        {
          errorStatus.style.display = DisplayStyle.Flex;
          if (errorCountLabel != null)
            errorCountLabel.text = errorCount.ToString();
        }
        else
        {
          errorStatus.style.display = DisplayStyle.None;
        }
      }

      // Update warning status
      if (warningStatus != null)
      {
        if (warningCount > 0)
        {
          warningStatus.style.display = DisplayStyle.Flex;
          if (warningCountLabel != null)
            warningCountLabel.text = warningCount.ToString();
        }
        else
        {
          warningStatus.style.display = DisplayStyle.None;
        }
      }

      // Update success status
      if (successStatus != null)
      {
        if (errorCount == 0 && warningCount == 0 && rootObject != null)
        {
          successStatus.style.display = DisplayStyle.Flex;
        }
        else
        {
          successStatus.style.display = DisplayStyle.None;
        }
      }
    }

    private void UpdateIssuesDisplay()
    {
      if (issuesContent == null)
        return;

      // Store current expanded states before clearing
      StoreExpandedStates();

      var filteredIssues = issues
        .Where(
          i => (showErrors && i.issueType == IssueType.Error) || (showWarnings && i.issueType == IssueType.Warning)
        )
        .Where(
          i =>
            string.IsNullOrEmpty(searchFilter)
            || i.path.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0
            || i.description.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0
        )
        .ToList();

      // Clear existing content
      issuesContent.Clear();

      if (filteredIssues.Count == 0)
      {
        if (noIssuesMessage != null)
        {
          noIssuesMessage.style.display = DisplayStyle.Flex;
          var noIssuesLabel = noIssuesMessage.Q<Label>("NoIssuesLabel");
          if (noIssuesLabel != null)
          {
            if (rootObject == null)
            {
              noIssuesLabel.text = "Select a root object to validate its components.";
            }
            else
            {
              noIssuesLabel.text = "No issues found! All components are valid.";
            }
          }
        }
        return;
      }

      if (noIssuesMessage != null)
        noIssuesMessage.style.display = DisplayStyle.None;

      // Group issues by category and type
      var groupedIssues = filteredIssues
        .GroupBy(i => new { i.issueType, i.category })
        .OrderByDescending(g => g.Key.issueType) // Errors first
        .ThenBy(g => g.Key.category);

      foreach (var group in groupedIssues)
      {
        CreateIssueGroup(group.Key.issueType, group.Key.category, group.ToList());
      }
    }

    private void CreateIssueGroup(IssueType issueType, string category, List<ComponentIssue> groupIssues)
    {
      var issueGroup = new VisualElement();
      issueGroup.AddToClassList("issue-group");
      if (issueType == IssueType.Error)
      {
        issueGroup.AddToClassList("error-group");
      }
      else
      {
        issueGroup.AddToClassList("warning-group");
      }

      // Group header
      var header = new VisualElement();
      header.AddToClassList("issue-group-header");

      var icon = new VisualElement();
      icon.AddToClassList("issue-group-icon");
      if (issueType == IssueType.Error)
      {
        icon.AddToClassList("error-icon");
      }
      else
      {
        icon.AddToClassList("warning-icon");
      }

      var title = new Label($"{category} ({groupIssues.Count})");
      title.AddToClassList("issue-group-title");

      header.Add(icon);
      header.Add(title);

      // Content area
      var content = new VisualElement();
      content.AddToClassList("issue-group-content");

      // Toggle content visibility - use stored state or expandAll default
      bool isExpanded = GetStoredExpandedState(issueType, category);
      content.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;

      header.RegisterCallback<ClickEvent>(evt =>
      {
        isExpanded = !isExpanded;
        content.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;

        // Store the new expanded state
        string groupKey = GetGroupKey(issueType, category);
        groupExpandedStates[groupKey] = isExpanded;
      });

      // Add individual issues
      foreach (var issue in groupIssues)
      {
        var issueItem = CreateIssueItem(issue);
        content.Add(issueItem);
      }

      issueGroup.Add(header);
      issueGroup.Add(content);
      issuesContent.Add(issueGroup);
    }

    private VisualElement CreateIssueItem(ComponentIssue issue)
    {
      var issueItem = new VisualElement();
      issueItem.AddToClassList("issue-item");

      var pathLabel = new Label(issue.path);
      pathLabel.AddToClassList("issue-path");
      pathLabel.RegisterCallback<ClickEvent>(evt =>
      {
        // Select the game object
        Selection.activeGameObject = issue.gameObject;
        // Ping it in the hierarchy
        EditorGUIUtility.PingObject(issue.gameObject);
        // Focus the hierarchy window
        EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
      });

      var descriptionLabel = new Label(issue.description);
      descriptionLabel.AddToClassList("issue-description");

      issueItem.Add(pathLabel);
      issueItem.Add(descriptionLabel);

      return issueItem;
    }

    private void RefreshIssues()
    {
      issues.Clear();

      if (rootObject == null)
      {
        UpdateStatusIcons();
        UpdateIssuesDisplay();
        return;
      }

      // Scan the root object and all its children
      ScanGameObjectRecursive(rootObject, "");

      UpdateStatusIcons();
      UpdateIssuesDisplay();
    }

    private void ScanGameObjectRecursive(GameObject obj, string basePath)
    {
      if (obj == null)
        return;

      string currentPath = string.IsNullOrEmpty(basePath) ? obj.name : $"{basePath}/{obj.name}";

      // Check for issues on this object
      CheckGameObjectIssues(obj, currentPath);

      // Recursively check children
      for (int i = 0; i < obj.transform.childCount; i++)
      {
        var child = obj.transform.GetChild(i).gameObject;
        ScanGameObjectRecursive(child, currentPath);
      }
    }

    private void CheckGameObjectIssues(GameObject obj, string path)
    {
      // Check for VRC stub components (errors)
      var components = obj.GetComponents<Component>();
      foreach (var component in components)
      {
        if (component == null)
        {
          issues.Add(
            new ComponentIssue
            {
              gameObject = obj,
              component = null,
              issueType = IssueType.Warning,
              description = "Missing component (null reference)",
              path = path,
              category = "Missing Components"
            }
          );
          continue;
        }

        string typeName = component.GetType().FullName;
        if (Constants.VRCSTUB_COMPONENTS_TO_REMOVE.Contains(typeName))
        {
          issues.Add(
            new ComponentIssue
            {
              gameObject = obj,
              component = component,
              issueType = IssueType.Error,
              description = $"VRC Stub Component detected: {component.GetType().Name}",
              path = path,
              category = "VRC Stub Components"
            }
          );
        }
      }

      // Check for CVRAvatar specific issues
      CheckCVRAvatarIssues(obj, path);

      // Check VRCFury component issues
      CheckVRCFuryComponentIssues(obj, path);
    }

    private void CheckCVRAvatarIssues(GameObject obj, string path)
    {
      // Check if this object has a CVRAvatar component
      var cvrAvatarType = System.Type.GetType("ABI.CCK.Components.CVRAvatar, Assembly-CSharp");
      if (cvrAvatarType == null)
        return;

      var cvrAvatar = obj.GetComponent(cvrAvatarType);
      if (cvrAvatar == null)
        return;

      // Check body mesh
      var bodyMeshProperty = cvrAvatarType.GetField("bodyMesh");
      if (bodyMeshProperty != null)
      {
        var bodyMeshValue = bodyMeshProperty.GetValue(cvrAvatar);
        if (bodyMeshValue == null)
        {
          issues.Add(
            new ComponentIssue
            {
              gameObject = obj,
              component = cvrAvatar as Component,
              issueType = IssueType.Warning,
              description = "CVRAvatar body mesh is not set",
              path = path,
              category = "CVRAvatar Issues"
            }
          );
        }
      }

      // Check Animator component
      var animator = obj.GetComponent<Animator>();
      if (animator == null)
      {
        issues.Add(
          new ComponentIssue
          {
            gameObject = obj,
            component = null,
            issueType = IssueType.Warning,
            description = "CVRAvatar is missing Animator component",
            path = path,
            category = "CVRAvatar Issues"
          }
        );
      }
      else if (animator.avatar == null)
      {
        issues.Add(
          new ComponentIssue
          {
            gameObject = obj,
            component = animator,
            issueType = IssueType.Warning,
            description = "Animator avatar is not set",
            path = path,
            category = "CVRAvatar Issues"
          }
        );
      }
    }

    private void CheckVRCFuryComponentIssues(GameObject obj, string path)
    {
      var vrcFuryComponents = obj.GetComponents<VRCFury>();
      if (vrcFuryComponents == null || vrcFuryComponents.Length == 0)
        return;

      foreach (var vrcFury in vrcFuryComponents)
      {
        if (vrcFury == null)
          continue;

        // Use similar logic to HierarchyIcons for checking VRCFury component issues
        var serializedObject = new SerializedObject(vrcFury);
        var configProperty = serializedObject.FindProperty("config");
        if (configProperty == null)
        {
          issues.Add(
            new ComponentIssue
            {
              gameObject = obj,
              component = vrcFury,
              issueType = IssueType.Warning,
              description = "VRCFury component config is null",
              path = path,
              category = "VRCFury Issues"
            }
          );
          continue;
        }

        // Check if the component has any features
        var featuresProperty = configProperty.FindPropertyRelative("features");
        if (featuresProperty != null && featuresProperty.arraySize == 0)
        {
          issues.Add(
            new ComponentIssue
            {
              gameObject = obj,
              component = vrcFury,
              issueType = IssueType.Warning,
              description = "VRCFury component detected",
              path = path,
              category = "VRCFury Component"
            }
          );
        }

        // Additional VRCFury-specific validation could be added here
      }
    }

    private void StoreExpandedStates()
    {
      if (issuesContent == null)
        return;

      // Clear existing stored states
      groupExpandedStates.Clear();

      // Find all issue groups and store their expanded states
      var groups = issuesContent.Query<VisualElement>(className: "issue-group").ToList();
      foreach (var group in groups)
      {
        var header = group.Q<VisualElement>(className: "issue-group-header");
        var content = group.Q<VisualElement>(className: "issue-group-content");
        var title = header?.Q<Label>(className: "issue-group-title");

        if (title != null && content != null)
        {
          string groupKey = ExtractGroupKeyFromTitle(title.text);
          bool isExpanded = content.style.display == DisplayStyle.Flex;
          groupExpandedStates[groupKey] = isExpanded;
        }
      }
    }

    private string ExtractGroupKeyFromTitle(string titleText)
    {
      // Extract category name from "Category Name (count)" format
      int parenIndex = titleText.LastIndexOf(" (");
      if (parenIndex > 0)
      {
        return titleText.Substring(0, parenIndex);
      }
      return titleText;
    }

    private string GetGroupKey(IssueType issueType, string category)
    {
      return category; // We use just the category since that's what we extract from titles
    }

    private bool GetStoredExpandedState(IssueType issueType, string category)
    {
      string key = GetGroupKey(issueType, category);
      if (groupExpandedStates.ContainsKey(key))
      {
        return groupExpandedStates[key];
      }
      // Default to expanded state based on expandAll setting
      return expandAll;
    }
  }
}

#endif
