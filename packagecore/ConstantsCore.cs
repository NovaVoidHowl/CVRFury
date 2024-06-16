using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace uk.novavoidhowl.dev.cvrfury.packagecore
{
  public static partial class Constants
  {
    public const string PROGRAM_DISPLAY_NAME = "CVRFury";
    public const string APP_COLOUR = "#0080FF";
    public const string APP_COLOUR_ERROR = "#FF8000";
    public const string APP_COLOUR_CRIT = "#FF0000";
    public const string APP_COLOUR_DBG = "#B7FF00";
    public const string DEBUG_PRINT_EDITOR_PREF = "CVRFURY_DEBUG";
    public const string CLEANUP_DISABLE_PREF = "CVRFURY_CLEANUP_DISABLE";

    public const string SCRIPTING_DEFINE_SYMBOL = "NVH_CVRFURY_EXISTS";
    public const string PACKAGE_NAME = "uk.novavoidhowl.dev.cvrfury";

    public const string COMPONENTS_SOURCE_FOLDER = "/nvhpmm/AppComponents/";
    public const string ASSETS_MANAGED_FOLDER = "Assets/_CVRFury";
    public const string ASSETS_TEMP_FOLDER = "Assets/_CVRFury/TempFiles";
    public const string ASSETS_MANAGED_FOLDER_GENERATED = ASSETS_MANAGED_FOLDER + "/Generated/";

    public static readonly Color UI_UPDATE_OUT_OF_DATE_COLOR = new Color(1.0f, 0f, 0f); // Red
    public static readonly Color UI_UPDATE_OUT_OF_DATE_COLOR_TEXT = new Color(1.0f, 0.4f, 0.4f); // Red
    public static readonly Color UI_UPDATE_OK_COLOR = new Color(0f, 1.0f, 0f); // Green
    public static readonly Color UI_UPDATE_OK_COLOR_TEXT = new Color(0f, 1.0f, 0f); // Green
    public static readonly Color UI_UPDATE_NOT_INSTALLED_COLOR = new Color(1.0f, 0.92f, 0.016f); // Yellow
    public static readonly Color UI_UPDATE_NOT_INSTALLED_COLOR_TEXT = new Color(1.0f, 1.0f, 1.0f); // White

    public static readonly Color UI_UPDATE_DOWNGRADE_COLOR = new Color(0.0f, 0.0f, 1.0f); // Blue
    public static readonly Color UI_UPDATE_DOWNGRADE_COLOR_TEXT = new Color(0.4f, 0.4f, 1.0f); // Blue

    public static readonly int CLIP_VIEW_DELAY = 4000;

    public static readonly string DOCS_URL = "https://repo.cvrfury.uk";

    // note this is not the main version number but rather a breaking change counter,
    // up to version 1.744 it was 2, and then after that it was 3
    public static readonly int MAX_VRCFURY_VERSION = 2;
    public static readonly string MAX_VRCFURY_VERSION_USER_VERSION = "1.744";

    public static readonly ReadOnlyCollection<string> CVRFURY_COMPONENTS_TO_REMOVE = new ReadOnlyCollection<string>(
      new List<string>
      {
        "uk.novavoidhowl.dev.cvrfury.runtime.CVRFuryDevModeEnabler",
        "uk.novavoidhowl.dev.cvrfury.runtime.CVRFuryDevSkinnedMeshRendererExtendedDataViewer",
        "uk.novavoidhowl.dev.cvrfury.runtime.CVRFuryDataStorageUnit",
        "uk.novavoidhowl.dev.cvrfury.runtime.CVRFuryGameObjectInfoTag",
        "uk.novavoidhowl.dev.cvrfury.runtime.CVRFuryNukeGameObject",
        "uk.novavoidhowl.dev.cvrfury.runtime.CVRFuryAvatarInfoUnit",
        "uk.novavoidhowl.dev.cvrfury.runtime.CVRFuryDynamicBoneConfig"
      }
    );
    public static readonly ReadOnlyCollection<string> CVRFURY_DEP_COMPONENTS_TO_REMOVE = new ReadOnlyCollection<string>(
      new List<string>
      {
        "uk.novavoidhowl.dev.cvrfury.deployable.CVRFuryMagicaCloth2Config"
      }
    );
    // list of components that are not to be considered as a something to be removed from the list of components
    // preventing the adding of the nuke component
    public static readonly ReadOnlyCollection<string> CVRFURY_NUKE_FILTER = new ReadOnlyCollection<string>(
      new List<string>
      {
        "uk.novavoidhowl.dev.cvrfury.deployable.CVRFuryMagicaCloth2Config"
      }
    );
  }
}
