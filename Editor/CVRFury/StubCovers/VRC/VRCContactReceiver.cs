#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using uk.novavoidhowl.dev.vrcstub;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury
{
  [CustomEditor(typeof(VRCContactReceiver))]
  public class VRCContactReceiverEditor : VRCGeneralStubEditor
  {
    protected override string componentTypeSuffix => "Contact Receiver";
  }
}
#endif
