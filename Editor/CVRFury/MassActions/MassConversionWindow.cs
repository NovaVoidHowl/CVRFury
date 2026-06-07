#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.Constraint.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;
using uk.novavoidhowl.dev.cvrfury.compiled.vrccontacts;
using uk.novavoidhowl.dev.cvrfury.compiled.vrccolliders;
using uk.novavoidhowl.dev.cvrfury.compiled.vrcconstraints;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;

namespace uk.novavoidhowl.dev.cvrfury.massactions
{
  public class MassConversionWindow : EditorWindow
  {
    private readonly List<MassConversionQueueItem> queueItems = new List<MassConversionQueueItem>();

    private GameObject rootObject;
    private ObjectField rootObjectField;
    private Label summaryLabel;
    private ScrollView queueScrollView;
    private Toggle applyDynamicBoneToggle;
    private Toggle applyMagicaCloth1Toggle;
    private Toggle applyMagicaCloth2Toggle;

    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Mass Actions/Component Mass Converter")]
    public static void ShowWindow()
    {
      var window = OpenWindow();
      window.Show();
    }

    [MenuItem("GameObject/CVRFury/Component Mass Converter", false, 1)]
    private static void OpenFromContext()
    {
      if (Selection.activeGameObject == null)
        return;

      var window = OpenWindow();
      window.rootObject = Selection.activeGameObject;
      if (window.rootObjectField != null)
        window.rootObjectField.SetValueWithoutNotify(window.rootObject);
      window.Show();
      window.ScanHierarchy();
    }

    [MenuItem("GameObject/CVRFury/Component Mass Converter", true)]
    private static bool OpenFromContextValidation()
    {
      return Selection.activeGameObject != null;
    }

    private static MassConversionWindow OpenWindow()
    {
      var window = GetWindow<MassConversionWindow>();
      window.titleContent = new GUIContent(
        "Component Mass Converter",
        EditorGUIUtility.IconContent("d_PreMatCube").image
      );
      window.minSize = new Vector2(720, 460);
      return window;
    }

    private void CreateGUI()
    {
      BuildRootLayout();
      if (rootObject != null)
        ScanHierarchy();
      else
        RebuildQueueDisplay();
    }

    private void BuildRootLayout()
    {
      rootVisualElement.Clear();
      rootVisualElement.style.paddingLeft = 12;
      rootVisualElement.style.paddingRight = 12;
      rootVisualElement.style.paddingTop = 12;
      rootVisualElement.style.paddingBottom = 12;

      var header = new Label("Component Mass Converter");
      header.style.unityFontStyleAndWeight = FontStyle.Bold;
      header.style.fontSize = 16;
      header.style.marginBottom = 8;
      rootVisualElement.Add(header);

      rootObjectField = new ObjectField("Root Object")
      {
        objectType = typeof(GameObject),
        allowSceneObjects = true,
        value = rootObject
      };
      rootObjectField.RegisterValueChangedCallback(evt =>
      {
        rootObject = evt.newValue as GameObject;
        ScanHierarchy();
      });
      rootVisualElement.Add(rootObjectField);

      var toolbar = new VisualElement();
      toolbar.style.flexDirection = FlexDirection.Row;
      toolbar.style.marginTop = 4;
      toolbar.style.marginBottom = 8;
      rootVisualElement.Add(toolbar);

      toolbar.Add(MakeButton("Use Selection", UseCurrentSelection));
      toolbar.Add(MakeButton("Scan", ScanHierarchy));
      toolbar.Add(MakeButton("Select All", SelectAllItems));
      toolbar.Add(MakeButton("Select None", DeselectAllItems));
      toolbar.Add(MakeButton("Convert Selected", ConvertSelectedItems));
      // Kept ready for future queue filtering/search, where it can convert all rows still visible after filters.
      // toolbar.Add(MakeButton("Convert All Visible", ConvertAllVisibleItems));

      var apiStatus = new HelpBox(
        "Converter APIs loaded: Contacts "
          + VRCContactConversionActions.ApiVersion
          + ", Colliders "
          + VRCPhysBoneColliderConversionActions.ApiVersion
          + ", Constraints "
          + VRCConstraintConversionActions.ApiVersion
          + ".",
        HelpBoxMessageType.Info
      );
      rootVisualElement.Add(apiStatus);

      rootVisualElement.Add(BuildColliderBatchControls());

      summaryLabel = new Label();
      summaryLabel.style.marginTop = 8;
      summaryLabel.style.marginBottom = 6;
      summaryLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
      rootVisualElement.Add(summaryLabel);

      queueScrollView = new ScrollView();
      queueScrollView.style.flexGrow = 1;
      queueScrollView.style.borderTopWidth = 1;
      queueScrollView.style.borderBottomWidth = 1;
      queueScrollView.style.borderLeftWidth = 1;
      queueScrollView.style.borderRightWidth = 1;
      queueScrollView.style.borderTopColor = new Color(0.22f, 0.22f, 0.22f);
      queueScrollView.style.borderBottomColor = new Color(0.22f, 0.22f, 0.22f);
      queueScrollView.style.borderLeftColor = new Color(0.22f, 0.22f, 0.22f);
      queueScrollView.style.borderRightColor = new Color(0.22f, 0.22f, 0.22f);
      rootVisualElement.Add(queueScrollView);
    }

    private VisualElement BuildColliderBatchControls()
    {
      var container = new VisualElement();
      container.style.marginTop = 8;
      container.style.paddingTop = 6;
      container.style.paddingBottom = 6;
      container.style.paddingLeft = 8;
      container.style.paddingRight = 8;
      container.style.backgroundColor = new Color(0.17f, 0.17f, 0.17f);

      var title = new Label("Apply Collider Targets To Selected Collider Rows");
      title.style.unityFontStyleAndWeight = FontStyle.Bold;
      title.style.marginBottom = 4;
      container.Add(title);

      var row = new VisualElement();
      row.style.flexDirection = FlexDirection.Row;
      row.style.alignItems = Align.Center;
      container.Add(row);

      applyDynamicBoneToggle = new Toggle("Dynamic Bone") { value = true };
      applyMagicaCloth1Toggle = new Toggle("MagicaCloth 1") { value = true };
      applyMagicaCloth2Toggle = new Toggle("MagicaCloth 2") { value = true };

      row.Add(applyDynamicBoneToggle);
      row.Add(applyMagicaCloth1Toggle);
      row.Add(applyMagicaCloth2Toggle);
      row.Add(MakeButton("Apply", ApplyColliderTargetsToSelected));

      var cleanupRow = new VisualElement();
      cleanupRow.style.flexDirection = FlexDirection.Row;
      cleanupRow.style.alignItems = Align.Center;
      cleanupRow.style.marginTop = 6;
      container.Add(cleanupRow);

      cleanupRow.Add(MakeButton("Remove Converted VRC Colliders", RemoveConvertedColliderSources));

      return container;
    }

    private Button MakeButton(string text, Action action)
    {
      var button = new Button(action) { text = text };
      button.style.marginRight = 4;
      return button;
    }

    private void UseCurrentSelection()
    {
      if (Selection.activeGameObject == null)
        return;

      rootObject = Selection.activeGameObject;
      if (rootObjectField != null)
        rootObjectField.SetValueWithoutNotify(rootObject);
      ScanHierarchy();
    }

    private void ScanHierarchy()
    {
      queueItems.Clear();

      if (rootObject == null)
      {
        RebuildQueueDisplay();
        return;
      }

      var components = rootObject.GetComponentsInChildren<Component>(true);
      for (int i = 0; i < components.Length; i++)
      {
        var component = components[i];
        if (component == null)
          continue;

        AddComponentIfSupported(component);
      }

      RebuildQueueDisplay();
    }

    private void AddComponentIfSupported(Component component)
    {
      if (component is VRCContactSender)
      {
        AddContactSender((VRCContactSender)component);
        return;
      }

      if (component is VRCContactReceiver)
      {
        AddContactReceiver((VRCContactReceiver)component);
        return;
      }

      if (component is VRCPhysBoneCollider)
      {
        AddPhysBoneCollider((VRCPhysBoneCollider)component);
        return;
      }

      var constraintAvailability = VRCConstraintConversionActions.GetAvailability(component);
      if (constraintAvailability.IsAvailable)
      {
        AddConstraint(component, constraintAvailability.Kind);
      }
    }

    private void AddContactSender(VRCContactSender component)
    {
      var availability = VRCContactConversionActions.GetSenderAvailability();
      var item = CreateQueueItem(component, MassConversionKind.ContactSender, "Contact Sender");
      ApplyAvailability(item, availability.IsAvailable, FirstContactMessageOrDefault(availability.Messages));
      queueItems.Add(item);
    }

    private void AddContactReceiver(VRCContactReceiver component)
    {
      var availability = VRCContactConversionActions.GetReceiverAvailability();
      var item = CreateQueueItem(component, MassConversionKind.ContactReceiver, "Contact Receiver");
      ApplyAvailability(item, availability.IsAvailable, FirstContactMessageOrDefault(availability.Messages));
      queueItems.Add(item);
    }

    private void AddPhysBoneCollider(VRCPhysBoneCollider component)
    {
      var availability = VRCPhysBoneColliderConversionActions.GetAvailability(component);
      var item = CreateQueueItem(component, MassConversionKind.PhysBoneCollider, "PhysBone Collider");
      item.ColliderTargets = GetDefaultColliderTargets(availability);
      item.CanConvertDynamicBone = availability.CanConvertDynamicBone;
      item.CanConvertMagicaCloth1 = availability.CanConvertMagicaCloth1;
      item.CanConvertMagicaCloth2 = availability.CanConvertMagicaCloth2;
      ApplyAvailability(item, item.ColliderTargets != PhysBoneColliderTarget.None, FirstColliderMessageOrDefault(availability.Messages));
      if (item.Status == MassConversionStatus.Ready)
        item.Message = "Ready: " + FormatColliderTargets(item.ColliderTargets);
      queueItems.Add(item);
    }

    private void AddConstraint(Component component, VRCConstraintKind kind)
    {
      var item = CreateQueueItem(component, MassConversionKind.VRCConstraint, "VRC " + kind + " Constraint");
      item.ConstraintKind = kind;
      item.Status = MassConversionStatus.Ready;
      item.Message = "Ready";
      queueItems.Add(item);
    }

    private MassConversionQueueItem CreateQueueItem(Component component, MassConversionKind kind, string displayName)
    {
      return new MassConversionQueueItem
      {
        SourceGameObject = component.gameObject,
        SourceComponent = component,
        Kind = kind,
        DisplayName = displayName,
        Selected = true,
        Status = MassConversionStatus.Pending,
        Message = "Pending",
        Path = BuildPath(component.gameObject)
      };
    }

    private void ApplyAvailability(MassConversionQueueItem item, bool isAvailable, string unavailableMessage)
    {
      item.Status = isAvailable ? MassConversionStatus.Ready : MassConversionStatus.Skipped;
      item.Selected = isAvailable;
      item.Message = isAvailable ? "Ready" : unavailableMessage;
    }

    private PhysBoneColliderTarget GetDefaultColliderTargets(VRCPhysBoneColliderConversionAvailability availability)
    {
      var targets = PhysBoneColliderTarget.None;
      if (availability.CanConvertDynamicBone)
        targets |= PhysBoneColliderTarget.DynamicBone;
      if (availability.CanConvertMagicaCloth1)
        targets |= PhysBoneColliderTarget.MagicaCloth1;
      if (availability.CanConvertMagicaCloth2)
        targets |= PhysBoneColliderTarget.MagicaCloth2;
      return targets;
    }

    private void RebuildQueueDisplay()
    {
      UpdateSummary();

      if (queueScrollView == null)
        return;

      queueScrollView.Clear();
      if (rootObject == null)
      {
        queueScrollView.Add(new HelpBox("Select a root object to scan.", HelpBoxMessageType.None));
        return;
      }

      if (queueItems.Count == 0)
      {
        queueScrollView.Add(new HelpBox("No supported conversion source components were found under the selected root.", HelpBoxMessageType.Info));
        return;
      }

      AddGroup(MassConversionKind.ContactSender, "Contact Senders");
      AddGroup(MassConversionKind.ContactReceiver, "Contact Receivers");
      AddGroup(MassConversionKind.PhysBoneCollider, "PhysBone Colliders");
      AddGroup(MassConversionKind.VRCConstraint, "VRC Constraints");
    }

    private void AddGroup(MassConversionKind kind, string title)
    {
      var groupItems = GetItems(kind);
      if (groupItems.Count == 0)
        return;

      var foldout = new Foldout
      {
        text = title + " (" + groupItems.Count + ")",
        value = true
      };
      foldout.style.marginBottom = 6;
      queueScrollView.Add(foldout);

      for (int i = 0; i < groupItems.Count; i++)
      {
        foldout.Add(BuildQueueRow(groupItems[i]));
      }
    }

    private List<MassConversionQueueItem> GetItems(MassConversionKind kind)
    {
      var items = new List<MassConversionQueueItem>();
      for (int i = 0; i < queueItems.Count; i++)
      {
        if (queueItems[i].Kind == kind)
          items.Add(queueItems[i]);
      }
      return items;
    }

    private VisualElement BuildQueueRow(MassConversionQueueItem item)
    {
      var row = new VisualElement();
      row.style.marginBottom = 4;
      row.style.paddingTop = 6;
      row.style.paddingBottom = 6;
      row.style.paddingLeft = 6;
      row.style.paddingRight = 6;
      row.style.backgroundColor = new Color(0.13f, 0.13f, 0.13f);

      var topLine = new VisualElement();
      topLine.style.flexDirection = FlexDirection.Row;
      topLine.style.alignItems = Align.Center;
      row.Add(topLine);

      var selectedToggle = new Toggle();
      selectedToggle.SetValueWithoutNotify(item.Selected);
      selectedToggle.SetEnabled(item.Status == MassConversionStatus.Ready);
      selectedToggle.RegisterValueChangedCallback(evt =>
      {
        item.Selected = evt.newValue;
        UpdateSummary();
      });
      topLine.Add(selectedToggle);

      var nameLabel = new Label(item.DisplayName);
      nameLabel.style.minWidth = 140;
      nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
      topLine.Add(nameLabel);

      var statusLabel = new Label(item.Status.ToString());
      statusLabel.style.minWidth = 90;
      statusLabel.style.color = GetStatusColor(item.Status);
      topLine.Add(statusLabel);

      var pathLabel = new Label(item.Path);
      pathLabel.style.flexGrow = 1;
      pathLabel.style.whiteSpace = WhiteSpace.Normal;
      pathLabel.RegisterCallback<ClickEvent>(evt =>
      {
        Selection.activeGameObject = item.SourceGameObject;
        EditorGUIUtility.PingObject(item.SourceGameObject);
      });
      topLine.Add(pathLabel);

      if (item.Kind == MassConversionKind.PhysBoneCollider)
        row.Add(BuildColliderTargetRow(item));

      if (!string.IsNullOrEmpty(item.Message))
      {
        var messageLabel = new Label(item.Message);
        messageLabel.style.marginLeft = 24;
        messageLabel.style.marginTop = 2;
        messageLabel.style.whiteSpace = WhiteSpace.Normal;
        messageLabel.style.color = item.Status == MassConversionStatus.Skipped ? new Color(1f, 0.75f, 0.35f) : Color.gray;
        row.Add(messageLabel);
      }

      return row;
    }

    private VisualElement BuildColliderTargetRow(MassConversionQueueItem item)
    {
      var targetRow = new VisualElement();
      targetRow.style.flexDirection = FlexDirection.Row;
      targetRow.style.marginLeft = 24;
      targetRow.style.marginTop = 4;

      targetRow.Add(MakeTargetToggle(item, "Dynamic Bone", PhysBoneColliderTarget.DynamicBone, item.CanConvertDynamicBone));
      targetRow.Add(MakeTargetToggle(item, "MagicaCloth 1", PhysBoneColliderTarget.MagicaCloth1, item.CanConvertMagicaCloth1));
      targetRow.Add(MakeTargetToggle(item, "MagicaCloth 2", PhysBoneColliderTarget.MagicaCloth2, item.CanConvertMagicaCloth2));

      return targetRow;
    }

    private Toggle MakeTargetToggle(
      MassConversionQueueItem item,
      string label,
      PhysBoneColliderTarget target,
      bool canUseTarget
    )
    {
      var toggle = new Toggle(label);
      toggle.SetValueWithoutNotify(HasTarget(item.ColliderTargets, target));
      toggle.SetEnabled(canUseTarget);
      toggle.RegisterValueChangedCallback(evt =>
      {
        SetColliderTarget(item, target, evt.newValue);
        RecalculateColliderItemState(item);
        RebuildQueueDisplay();
      });
      return toggle;
    }

    private void SelectAllItems()
    {
      for (int i = 0; i < queueItems.Count; i++)
      {
        if (queueItems[i].Status == MassConversionStatus.Ready)
          queueItems[i].Selected = true;
      }
      RebuildQueueDisplay();
    }

    private void DeselectAllItems()
    {
      for (int i = 0; i < queueItems.Count; i++)
      {
        queueItems[i].Selected = false;
      }
      RebuildQueueDisplay();
    }

    private void ApplyColliderTargetsToSelected()
    {
      var desiredTargets = PhysBoneColliderTarget.None;
      if (applyDynamicBoneToggle != null && applyDynamicBoneToggle.value)
        desiredTargets |= PhysBoneColliderTarget.DynamicBone;
      if (applyMagicaCloth1Toggle != null && applyMagicaCloth1Toggle.value)
        desiredTargets |= PhysBoneColliderTarget.MagicaCloth1;
      if (applyMagicaCloth2Toggle != null && applyMagicaCloth2Toggle.value)
        desiredTargets |= PhysBoneColliderTarget.MagicaCloth2;

      for (int i = 0; i < queueItems.Count; i++)
      {
        var item = queueItems[i];
        if (item.Kind != MassConversionKind.PhysBoneCollider || !item.Selected)
          continue;

        item.ColliderTargets = PhysBoneColliderTarget.None;
        if (item.CanConvertDynamicBone && HasTarget(desiredTargets, PhysBoneColliderTarget.DynamicBone))
          item.ColliderTargets |= PhysBoneColliderTarget.DynamicBone;
        if (item.CanConvertMagicaCloth1 && HasTarget(desiredTargets, PhysBoneColliderTarget.MagicaCloth1))
          item.ColliderTargets |= PhysBoneColliderTarget.MagicaCloth1;
        if (item.CanConvertMagicaCloth2 && HasTarget(desiredTargets, PhysBoneColliderTarget.MagicaCloth2))
          item.ColliderTargets |= PhysBoneColliderTarget.MagicaCloth2;

        RecalculateColliderItemState(item);
      }

      RebuildQueueDisplay();
    }

    private void RemoveConvertedColliderSources()
    {
      var itemsToRemove = new List<MassConversionQueueItem>();
      for (int i = 0; i < queueItems.Count; i++)
      {
        var item = queueItems[i];
        if (item.Kind == MassConversionKind.PhysBoneCollider && item.Status == MassConversionStatus.Converted)
          itemsToRemove.Add(item);
      }

      if (itemsToRemove.Count == 0)
      {
        EditorUtility.DisplayDialog(
          "No Converted VRC Colliders",
          "No converted PhysBone collider rows are available for source component removal.",
          "OK"
        );
        return;
      }

      int undoGroup = Undo.GetCurrentGroup();
      Undo.SetCurrentGroupName("CVRFury Remove Converted VRC Collider Sources");

      try
      {
        for (int i = 0; i < itemsToRemove.Count; i++)
        {
          RemoveConvertedColliderSource(itemsToRemove[i]);
        }
      }
      finally
      {
        Undo.CollapseUndoOperations(undoGroup);
      }

      RebuildQueueDisplay();
    }

    private void RemoveConvertedColliderSource(MassConversionQueueItem item)
    {
      if (item.SourceComponent == null)
      {
        item.Selected = false;
        item.Status = MassConversionStatus.SourceRemoved;
        item.Message = "Source VRC PhysBone Collider component is already gone.";
        return;
      }

      try
      {
        var sourceCollider = item.SourceComponent as VRCPhysBoneCollider;
        if (sourceCollider == null)
        {
          item.Selected = false;
          item.Status = MassConversionStatus.Failed;
          item.Message = "Source component is no longer a VRC PhysBone Collider.";
          return;
        }

        var sourceGameObject = item.SourceGameObject;
        Undo.DestroyObjectImmediate(sourceCollider);
        if (sourceGameObject != null)
          EditorUtility.SetDirty(sourceGameObject);

        item.SourceComponent = null;
        item.Selected = false;
        item.Status = MassConversionStatus.SourceRemoved;
        item.Message = "Removed source VRC PhysBone Collider component.";
      }
      catch (Exception ex)
      {
        Debug.LogException(ex);
        item.Selected = false;
        item.Status = MassConversionStatus.Failed;
        item.Message = ex.Message;
      }
    }

    private void ConvertSelectedItems()
    {
      ExecuteQueueItems(false);
    }

    private void ConvertAllVisibleItems()
    {
      ExecuteQueueItems(true);
    }

    private void ExecuteQueueItems(bool includeAllReadyRows)
    {
      var itemsToConvert = new List<MassConversionQueueItem>();
      for (int i = 0; i < queueItems.Count; i++)
      {
        var item = queueItems[i];
        if (item.Status != MassConversionStatus.Ready)
          continue;
        if (!includeAllReadyRows && !item.Selected)
          continue;

        itemsToConvert.Add(item);
      }

      if (itemsToConvert.Count == 0)
      {
        EditorUtility.DisplayDialog("No Rows To Convert", "No ready conversion rows were selected.", "OK");
        return;
      }

      int undoGroup = Undo.GetCurrentGroup();
      Undo.SetCurrentGroupName(includeAllReadyRows ? "CVRFury Convert All Visible Rows" : "CVRFury Convert Selected Rows");

      try
      {
        for (int i = 0; i < itemsToConvert.Count; i++)
        {
          ExecuteQueueItem(itemsToConvert[i]);
        }
      }
      finally
      {
        Undo.CollapseUndoOperations(undoGroup);
      }

      RebuildQueueDisplay();
    }

    private void ExecuteQueueItem(MassConversionQueueItem item)
    {
      if (item.SourceComponent == null)
      {
        item.Selected = false;
        item.Status = MassConversionStatus.SourceRemoved;
        item.Message = "Source component is no longer present.";
        return;
      }

      try
      {
        switch (item.Kind)
        {
          case MassConversionKind.ContactSender:
            ApplyContactResult(
              item,
              VRCContactConversionActions.ConvertSender(
                (VRCContactSender)item.SourceComponent,
                VRCContactConversionOptions.ForBatch()
              )
            );
            break;

          case MassConversionKind.ContactReceiver:
            ApplyContactResult(
              item,
              VRCContactConversionActions.ConvertReceiver(
                (VRCContactReceiver)item.SourceComponent,
                VRCContactConversionOptions.ForBatch()
              )
            );
            break;

          case MassConversionKind.PhysBoneCollider:
            ApplyColliderResult(
              item,
              VRCPhysBoneColliderConversionActions.Convert(
                (VRCPhysBoneCollider)item.SourceComponent,
                item.ColliderTargets,
                VRCPhysBoneColliderConversionOptions.ForBatch()
              )
            );
            break;

          case MassConversionKind.VRCConstraint:
            ApplyConstraintResult(
              item,
              VRCConstraintConversionActions.Convert(
                item.SourceComponent,
                VRCConstraintConversionOptions.ForBatch()
              )
            );
            break;
        }
      }
      catch (Exception ex)
      {
        Debug.LogException(ex);
        item.Selected = false;
        item.Status = MassConversionStatus.Failed;
        item.Message = ex.Message;
      }
    }

    private void ApplyContactResult(MassConversionQueueItem item, VRCContactConversionResult result)
    {
      item.Selected = false;
      item.Status = result.Success
        ? (result.SourceRemoved ? MassConversionStatus.SourceRemoved : MassConversionStatus.Converted)
        : MassConversionStatus.Failed;
      item.Message = BuildContactResultMessage(result);
    }

    private void ApplyColliderResult(MassConversionQueueItem item, VRCPhysBoneColliderConversionResult result)
    {
      item.Selected = false;
      item.Status = result.Success ? MassConversionStatus.Converted : MassConversionStatus.Failed;
      item.Message = BuildColliderResultMessage(result);
    }

    private void ApplyConstraintResult(MassConversionQueueItem item, VRCConstraintConversionResult result)
    {
      item.Selected = false;
      item.Status = result.Success
        ? (result.SourceRemoved ? MassConversionStatus.SourceRemoved : MassConversionStatus.Converted)
        : MassConversionStatus.Failed;
      item.Message = BuildConstraintResultMessage(result);
    }

    private string BuildContactResultMessage(VRCContactConversionResult result)
    {
      var message = string.IsNullOrEmpty(result.SummaryMessage) ? result.SummaryTitle : result.SummaryMessage;
      for (int i = 0; i < result.Messages.Count; i++)
      {
        if (result.Messages[i].Severity == uk.novavoidhowl.dev.cvrfury.compiled.vrccontacts.ConversionMessageSeverity.Info)
          continue;

        message += "\n" + result.Messages[i].Severity + ": " + result.Messages[i].Text;
      }
      return message;
    }

    private string BuildColliderResultMessage(VRCPhysBoneColliderConversionResult result)
    {
      var message = string.IsNullOrEmpty(result.SummaryMessage) ? result.SummaryTitle : result.SummaryMessage;
      for (int i = 0; i < result.Messages.Count; i++)
      {
        if (result.Messages[i].Severity == uk.novavoidhowl.dev.cvrfury.compiled.vrccolliders.ConversionMessageSeverity.Info)
          continue;

        message += "\n" + result.Messages[i].Severity + ": " + result.Messages[i].Text;
      }
      return message;
    }

    private string BuildConstraintResultMessage(VRCConstraintConversionResult result)
    {
      var message = string.IsNullOrEmpty(result.SummaryMessage) ? result.SummaryTitle : result.SummaryMessage;
      for (int i = 0; i < result.Messages.Count; i++)
      {
        if (result.Messages[i].Severity == uk.novavoidhowl.dev.cvrfury.compiled.vrcconstraints.ConversionMessageSeverity.Info)
          continue;

        message += "\n" + result.Messages[i].Severity + ": " + result.Messages[i].Text;
      }
      return message;
    }

    private void RecalculateColliderItemState(MassConversionQueueItem item)
    {
      if (item.ColliderTargets == PhysBoneColliderTarget.None)
      {
        item.Status = MassConversionStatus.Skipped;
        item.Selected = false;
        item.Message = "No available collider conversion targets selected.";
        return;
      }

      item.Status = MassConversionStatus.Ready;
      item.Message = "Ready: " + FormatColliderTargets(item.ColliderTargets);
    }

    private void SetColliderTarget(MassConversionQueueItem item, PhysBoneColliderTarget target, bool enabled)
    {
      if (enabled)
        item.ColliderTargets |= target;
      else
        item.ColliderTargets &= ~target;
    }

    private bool HasTarget(PhysBoneColliderTarget targets, PhysBoneColliderTarget target)
    {
      return (targets & target) != 0;
    }

    private void UpdateSummary()
    {
      if (summaryLabel == null)
        return;

      int selected = 0;
      int ready = 0;
      int skipped = 0;
      int converted = 0;
      int failed = 0;
      int sourceRemoved = 0;

      for (int i = 0; i < queueItems.Count; i++)
      {
        var item = queueItems[i];
        if (item.Selected)
          selected++;
        if (item.Status == MassConversionStatus.Ready)
          ready++;
        if (item.Status == MassConversionStatus.Skipped)
          skipped++;
        if (item.Status == MassConversionStatus.Converted)
          converted++;
        if (item.Status == MassConversionStatus.Failed)
          failed++;
        if (item.Status == MassConversionStatus.SourceRemoved)
          sourceRemoved++;
      }

      summaryLabel.text =
        "Found "
        + queueItems.Count
        + " | Selected "
        + selected
        + " | Ready "
        + ready
        + " | Skipped "
        + skipped
        + " | Converted "
        + converted
        + " | Failed "
        + failed
        + " | Source Removed "
        + sourceRemoved;
    }

    private string BuildPath(GameObject gameObject)
    {
      if (gameObject == rootObject)
        return gameObject.name;

      var parts = new List<string>();
      var current = gameObject.transform;
      while (current != null)
      {
        parts.Add(current.name);
        if (current.gameObject == rootObject)
          break;
        current = current.parent;
      }

      parts.Reverse();
      return string.Join("/", parts.ToArray());
    }

    private string FirstContactMessageOrDefault(
      List<uk.novavoidhowl.dev.cvrfury.compiled.vrccontacts.ConversionMessage> messages
    )
    {
      if (messages == null || messages.Count == 0)
        return "Unavailable";

      return messages[0].Text;
    }

    private string FirstColliderMessageOrDefault(
      List<uk.novavoidhowl.dev.cvrfury.compiled.vrccolliders.ConversionMessage> messages
    )
    {
      if (messages == null || messages.Count == 0)
        return "Unavailable";

      return messages[0].Text;
    }

    private string FormatColliderTargets(PhysBoneColliderTarget targets)
    {
      var names = new List<string>();
      if (HasTarget(targets, PhysBoneColliderTarget.DynamicBone))
        names.Add("Dynamic Bone");
      if (HasTarget(targets, PhysBoneColliderTarget.MagicaCloth1))
        names.Add("MagicaCloth 1");
      if (HasTarget(targets, PhysBoneColliderTarget.MagicaCloth2))
        names.Add("MagicaCloth 2");

      return names.Count == 0 ? "None" : string.Join(", ", names.ToArray());
    }

    private Color GetStatusColor(MassConversionStatus status)
    {
      switch (status)
      {
        case MassConversionStatus.Ready:
          return new Color(0.45f, 0.9f, 0.55f);
        case MassConversionStatus.Skipped:
          return new Color(1f, 0.75f, 0.35f);
        case MassConversionStatus.Failed:
          return new Color(1f, 0.45f, 0.45f);
        case MassConversionStatus.Converted:
        case MassConversionStatus.SourceRemoved:
          return new Color(0.45f, 0.75f, 1f);
        default:
          return Color.gray;
      }
    }

    private enum MassConversionKind
    {
      ContactSender,
      ContactReceiver,
      PhysBoneCollider,
      VRCConstraint
    }

    private enum MassConversionStatus
    {
      Pending,
      Ready,
      Skipped,
      Converted,
      Failed,
      SourceRemoved
    }

    private sealed class MassConversionQueueItem
    {
      public GameObject SourceGameObject;
      public Component SourceComponent;
      public MassConversionKind Kind;
      public PhysBoneColliderTarget ColliderTargets;
      public VRCConstraintKind ConstraintKind;
      public bool Selected;
      public MassConversionStatus Status;
      public string Message;
      public string Path;
      public string DisplayName;
      public bool CanConvertDynamicBone;
      public bool CanConvertMagicaCloth1;
      public bool CanConvertMagicaCloth2;
    }
  }
}

#endif
