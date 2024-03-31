using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace uk.novavoidhowl.dev.cvrfury.packagecore
{
  public static class Constants
  {
    public const string VRCFURY_VERSION_COMPATIBILITY = "1.659.0";
    public const string PROGRAM_DISPLAY_NAME = "CVRFury";
    public const string APP_COLOUR = "#0080FF";
    public const string APP_COLOUR_CRIT = "#FF0000";
    public const string SCRIPTING_DEFINE_SYMBOL = "NVH_CVRFURY_EXISTS";
    public const string PACKAGE_NAME = "uk.novavoidhowl.dev.cvrfury";
    public const string ASSETS_MANAGED_FOLDER = "Assets/_CVRFury";

    public static readonly Color UI_UPDATE_OUT_OF_DATE_COLOR = new Color(1.0f, 0f, 0f); // Red
    public static readonly Color UI_UPDATE_OUT_OF_DATE_COLOR_TEXT = new Color(1.0f, 0.4f, 0.4f); // Red
    public static readonly Color UI_UPDATE_OK_COLOR = new Color(0f, 1.0f, 0f); // Green
    public static readonly Color UI_UPDATE_OK_COLOR_TEXT = new Color(0f, 1.0f, 0f); // Green
    public static readonly Color UI_UPDATE_NOT_INSTALLED_COLOR = new Color(1.0f, 0.92f, 0.016f); // Yellow
    public static readonly Color UI_UPDATE_NOT_INSTALLED_COLOR_TEXT = new Color(1.0f, 1.0f, 1.0f); // White

    public static readonly Color UI_UPDATE_DOWNGRADE_COLOR = new Color(0.0f, 0.0f, 1.0f); // Blue
    public static readonly Color UI_UPDATE_DOWNGRADE_COLOR_TEXT = new Color(0.4f, 0.4f, 1.0f); // Blue

    public static readonly ReadOnlyCollection<string> COMPATIBLE_VRCFURY_FEATURES = new ReadOnlyCollection<string>(
      new List<string> { "ApplyDuringUpload", "FullController", "ArmatureLink" }
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
