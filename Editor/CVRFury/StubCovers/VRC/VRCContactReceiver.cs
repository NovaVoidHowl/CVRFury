#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

// following is provided by the VRC Stub dlls included in the CVRFury project
using VRC.SDK3.Dynamics.Contact.Components;

namespace uk.novavoidhowl.dev.cvrfury
{
  [CustomEditor(typeof(VRCContactReceiver))]
  public class VRCContactReceiverEditor : VRCGeneralStubEditor
  {
    protected override string componentTypeSuffix => "Contact Receiver";
  }
}
#endif
