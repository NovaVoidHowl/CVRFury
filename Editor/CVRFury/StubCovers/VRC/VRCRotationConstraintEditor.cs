#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

// The following are provided as part of the VRCStubs included in this package
using VRC.SDK3.Dynamics.Constraint.Components;

namespace uk.novavoidhowl.dev.cvrfury
{
  [CustomEditor(typeof(VRCRotationConstraint))]
  public class VRCRotationConstraintEditor : VRCGeneralStubEditor
  {
    protected override string componentTypeSuffix => "Rotation Constraint";
  }
}
#endif
