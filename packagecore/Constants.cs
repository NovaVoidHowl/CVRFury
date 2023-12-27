using UnityEngine;

namespace uk.novavoidhowl.dev.cvrfury.packagecore
{
  public static class Constants
  {
    public const string VRCFURY_VERSION_COMPATIBILITY = "1.659.0";
    public const string PROGRAM_DISPLAY_NAME = "CVRFury";
    public const string SCRIPTING_DEFINE_SYMBOL = "NVH_CVRFURY_EXISTS";
    public const string PACKAGE_NAME = "uk.novavoidhowl.dev.cvrfury";
    public const string ASSETS_MANAGED_FOLDER = "Assets/NovaVoidHowl/CVRFury";

    public static readonly Color UI_UPDATE_OUT_OF_DATE_COLOR = new Color(1.0f, 0f, 0f); // Red
    public static readonly Color UI_UPDATE_OUT_OF_DATE_COLOR_TEXT = new Color(1.0f, 0.4f, 0.4f); // Red
    public static readonly Color UI_UPDATE_OK_COLOR = new Color(0f, 1.0f, 0f); // Green
    public static readonly Color UI_UPDATE_OK_COLOR_TEXT = new Color(0f, 1.0f, 0f); // Green
    public static readonly Color UI_UPDATE_NOT_INSTALLED_COLOR = new Color(1.0f, 0.92f, 0.016f); // Yellow
    public static readonly Color UI_UPDATE_NOT_INSTALLED_COLOR_TEXT = new Color(1.0f, 1.0f, 1.0f); // White

    public static readonly Color UI_UPDATE_DOWNGRADE_COLOR = new Color(0.0f, 0.0f, 1.0f); // Blue
    public static readonly Color UI_UPDATE_DOWNGRADE_COLOR_TEXT = new Color(0.4f, 0.4f, 1.0f); // Blue
  }
}
