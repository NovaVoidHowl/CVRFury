using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.packagecore
{
  public static partial class Constants
  {
    // colour data for VRCFury component headers
    public static readonly Color VRCFURY_HEADER_PREFIX_COLOUR = new Color(
      hexadecimalToColour("#ffb125").r,
      hexadecimalToColour("#ffb125").g,
      hexadecimalToColour("#ffb125").b
    );
    public static readonly Color VRCFURY_HEADER_BACKGROUND_COLOUR = new Color(
      hexadecimalToColour("#111111").r,
      hexadecimalToColour("#111111").g,
      hexadecimalToColour("#111111").b
    );
    public static readonly Color VRCFURY_HEADER_BACKGROUND_HOVER_COLOUR = new Color(
      hexadecimalToColour("#202030").r,
      hexadecimalToColour("#202030").g,
      hexadecimalToColour("#202030").b
    );

    // note this is not the main version number but rather a breaking change counter,
    // up to version 1.744 it was 2, and then after that it was 3
    public static readonly int MAX_VRCFURY_VERSION_DATA = 3;
    public static readonly int MAX_VRCFURY_VERSION_IMPORT = 2;

    // version numbers for the user to reference
    public static readonly string MAX_VRCFURY_DATA_USER_VERSION = "1.909.0";
    public static readonly string MAX_VRCFURY_IMPORT_USER_VERSION = "1.744";

    // note this is just and enable list, the feature still needs to allow the version to be imported
    // (see COMPATIBLE_VRCFURY_FEATURES)
    public static readonly ReadOnlyCollection<int> VRCFURY_IMPORT_SUPPORTED_VERSIONS = new ReadOnlyCollection<int>(
      new List<int> { 2, 3 }
    );

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

    // the int is the highest version of VRCFury datastore that is supported for data import for that feature
    public static readonly ReadOnlyCollection<KeyValuePair<string, int>> COMPATIBLE_VRCFURY_FEATURES =
      new ReadOnlyCollection<KeyValuePair<string, int>>(
        new List<KeyValuePair<string, int>>
        {
          new KeyValuePair<string, int>("ApplyDuringUpload", 3),
          new KeyValuePair<string, int>("FullController", 3),
          new KeyValuePair<string, int>("ArmatureLink", 3),
          new KeyValuePair<string, int>("ShowInFirstPerson", 3),
          new KeyValuePair<string, int>("DeleteDuringUpload", 3),
          new KeyValuePair<string, int>("Gizmo", 3)
        }
      );

    // this set of features are not compatible with CVR Fury and should be removed by the user
    public static readonly ReadOnlyCollection<string> CVR_INCOMPATIBLE_VRCFURY_FEATURES =
      new ReadOnlyCollection<string>(new List<string> { "SetIcon", "SecurityLock" });

    // this set of features are not needed for import in to CVR Fury as they there functionality is natively handled
    // by CVR Fury
    public static readonly ReadOnlyCollection<string> CVR_UN_NEEDED_VRCFURY_FEATURES = new ReadOnlyCollection<string>(
      new List<string> { "AnchorOverrideFix", "AnchorOverrideFix2" }
    );

    // this set of features are blocked from being imported in to CVR Fury as they are not supported
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
