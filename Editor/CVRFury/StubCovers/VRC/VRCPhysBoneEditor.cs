#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

// VRC PhysBone components provided by compiled stub DLLs
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace uk.novavoidhowl.dev.cvrfury
{
  [CustomEditor(typeof(VRCPhysBone))]
  public class VRCPhysBoneEditor : VRCGeneralStubEditor
  {
    protected override string componentTypeSuffix => "PhysBone";
  }
}
#endif
