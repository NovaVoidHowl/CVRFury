#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using uk.novavoidhowl.dev.cvrfury.runtime;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.inspector
{
  public static partial class InspectorOverlay
  {
    private static readonly ComponentConfig[] ComponentConfigs = new[]
    {
      // CVR Fury components
      new ComponentConfig(
        "Component Dev Mode Enabler",
        "CVRFuryDevModeEnablerEditor",
        "Dev Mode Enabler",
        "CVR Fury",
        Constants.CVRFURY_HEADER_PREFIX_COLOUR,
        Constants.CVRFURY_HEADER_BACKGROUND_COLOUR,
        Constants.CVRFURY_HEADER_BACKGROUND_HOVER_COLOUR,
        DisplayMode.Both
      ),
      new ComponentConfig(
        "Data Storage Unit",
        "CVRFuryDataStorageUnitBase",
        "DSU",
        "CVR Fury",
        Constants.CVRFURY_HEADER_PREFIX_COLOUR,
        Constants.CVRFURY_HEADER_BACKGROUND_COLOUR,
        Constants.CVRFURY_HEADER_BACKGROUND_HOVER_COLOUR,
        DisplayMode.Both
      ),
      new ComponentConfig(
        "Avatar Collider Info Unit",
        "CVRFuryAvatarColliderInfoUnit",
        "Avatar Collider Info",
        "CVR Fury",
        Constants.CVRFURY_HEADER_PREFIX_COLOUR,
        Constants.CVRFURY_HEADER_BACKGROUND_COLOUR,
        Constants.CVRFURY_HEADER_BACKGROUND_HOVER_COLOUR,
        DisplayMode.Both
      ),
      new ComponentConfig(
        "Avatar Configuration",
        "CVRFuryAvatarConfiguration",
        "Avatar Configuration Options",
        "CVR Fury",
        Constants.CVRFURY_HEADER_PREFIX_COLOUR,
        Constants.CVRFURY_HEADER_BACKGROUND_COLOUR,
        Constants.CVRFURY_HEADER_BACKGROUND_HOVER_COLOUR,
        DisplayMode.Both
      ),
      // VRC Fury components
      new ComponentConfig(
        "VRC Fury (Script)",
        "VRCFuryStubBase",
        "Data Store - (expand for details)",
        "VRC Fury",
        Constants.VRCFURY_HEADER_PREFIX_COLOUR,
        Constants.VRCFURY_HEADER_BACKGROUND_COLOUR,
        Constants.VRCFURY_HEADER_BACKGROUND_HOVER_COLOUR,
        DisplayMode.Both
      ),
      // VRC components
      new ComponentConfig(
        "VRC Contact Receiver (Script)",
        "VRCContactReceiver",
        "Contact Receiver",
        "VRC",
        Constants.VRC_HEADER_PREFIX_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_HOVER_COLOUR,
        DisplayMode.Normal
      ),
      new ComponentConfig(
        "VRC Contact Sender (Script)",
        "VRCContactSender",
        "Contact Sender",
        "VRC",
        Constants.VRC_HEADER_PREFIX_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_HOVER_COLOUR,
        DisplayMode.Normal
      ),
      new ComponentConfig(
        "VRC Scale Constraint (Script)",
        "VRCScaleConstraint",
        "Scale Constraint",
        "VRC",
        Constants.VRC_HEADER_PREFIX_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_HOVER_COLOUR,
        DisplayMode.Normal
      ),
      new ComponentConfig(
        "VRC Rotation Constraint (Script)",
        "VRCRotationConstraint",
        "Rotation Constraint",
        "VRC",
        Constants.VRC_HEADER_PREFIX_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_HOVER_COLOUR,
        DisplayMode.Normal
      ),
      new ComponentConfig(
        "VRC Position Constraint (Script)",
        "VRCPositionConstraint",
        "Position Constraint",
        "VRC",
        Constants.VRC_HEADER_PREFIX_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_HOVER_COLOUR,
        DisplayMode.Normal
      ),
      new ComponentConfig(
        "VRC Parent Constraint (Script)",
        "VRCParentConstraint",
        "Parent Constraint",
        "VRC",
        Constants.VRC_HEADER_PREFIX_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_HOVER_COLOUR,
        DisplayMode.Normal
      ),
      new ComponentConfig(
        "VRC Look At Constraint (Script)",
        "VRCLookAtConstraint",
        "Look At Constraint",
        "VRC",
        Constants.VRC_HEADER_PREFIX_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_HOVER_COLOUR,
        DisplayMode.Normal
      ),
      new ComponentConfig(
        "VRC Aim Constraint (Script)",
        "VRCAimConstraint",
        "Aim Constraint",
        "VRC",
        Constants.VRC_HEADER_PREFIX_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_HOVER_COLOUR,
        DisplayMode.Normal
      ),
      new ComponentConfig(
        "VRC Phys Bone (Script)",
        "VRCPhysBone",
        "Phys Bone",
        "VRC",
        Constants.VRC_HEADER_PREFIX_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_HOVER_COLOUR,
        DisplayMode.Normal
      ),
      new ComponentConfig(
        "VRC Phys Bone Collider (Script)",
        "VRCPhysBoneCollider",
        "Phys Bone Collider",
        "VRC",
        Constants.VRC_HEADER_PREFIX_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_HOVER_COLOUR,
        DisplayMode.Normal
      ),
      // VRC Compiled Stubs
      new ComponentConfig(
        "VRC Avatar Descriptor (Script)",
        "VRCAvatarDescriptor",
        "Avatar Descriptor",
        "VRC",
        Constants.VRC_HEADER_PREFIX_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_HOVER_COLOUR,
        DisplayMode.Normal
      ),
      new ComponentConfig(
        "Pipeline Manager (Script)",
        "PipelineManager",
        "Pipeline Manager",
        "VRC",
        Constants.VRC_HEADER_PREFIX_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_COLOUR,
        Constants.VRC_HEADER_BACKGROUND_HOVER_COLOUR,
        DisplayMode.Normal
      ),
    };
  }
}
#endif
