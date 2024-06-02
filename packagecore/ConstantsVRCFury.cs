using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace uk.novavoidhowl.dev.cvrfury.packagecore
{
  public static partial class Constants
  {
    public const string VRCFURY_VERSION_COMPATIBILITY = "1.659.0";

    public static readonly ReadOnlyCollection<string> VRCFURY_COMPONENTS_TO_REMOVE = new ReadOnlyCollection<string>(
      new List<string>
      {
        "VF.Component.VRCFuryGlobalCollider",
        "VF.Component.VRCFuryHapticPlug",
        "VF.Component.VRCFuryHapticSocket",
        "VF.Component.VRCFuryHapticTouchReceiver",
        "VF.Component.VRCFuryHapticTouchSender",
        "VF.Component.VRCFuryPlayComponent",
        "VF.Component.VRCFurySocketGizmo",
        "VF.Component.VRCFuryNoUpdateWhenOffscreen",
        "VF.Model.VRCFury"
      }
    );

    public static readonly ReadOnlyCollection<string> COMPATIBLE_VRCFURY_FEATURES = new ReadOnlyCollection<string>(
      new List<string>
      {
        "ApplyDuringUpload",
        "FullController",
        "ArmatureLink",
        "ShowInFirstPerson",
        "DeleteDuringUpload"
      }
    );
    public static readonly ReadOnlyCollection<string> CVR_INCOMPATIBLE_VRCFURY_FEATURES =
      new ReadOnlyCollection<string>(new List<string> { "SetIcon", "SecurityLock" });
    public static readonly ReadOnlyCollection<string> BLOCK_LISTED_VRCFURY_FEATURES = new ReadOnlyCollection<string>(
      new List<string>
      {
        "SpsOptions",
        "ZawooIntegration",
        "SenkyGestureDriver",
        "DirectTreeOptimizer",
        "AvatarScale",
        "AvatarScale2",
        "TpsScaleFix",
        "Gizmo",
        "CrossEyeFix",
        "CrossEyeFix2",
        "OGBIntegration",
        "OGBIntegration2",
        "MakeWriteDefaultsOff",
        "MakeWriteDefaultsOff2",
        "TPSIntegration",
        "TPSIntegration2",
        "LegacyPrefabSupport",
        "LegacyPrefabSupport2",
      }
    );
  }
}
