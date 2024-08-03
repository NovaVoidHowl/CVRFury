using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace uk.novavoidhowl.dev.cvrfury.packagecore
{
  public static partial class Constants
  {
    public static readonly ReadOnlyCollection<string> ALWAYS_GLOBAL_PARAMETERS_FROM_OVR_TOOLKIT =
      new ReadOnlyCollection<string>(
        new List<string>
        {
          "ToggleEditMode", // bool
          "ToggleWindows",  // bool
          "ToggleKeyboard", // bool
          "MediaPlaying",   // bool
          "LowestBattery",  // float
          "CurrentProfile", // int
          "hmdBattery",      // float
          "leftControllerBattery", // float
          "rightControllerBattery", // float
          "averageControllerBattery", // float
          "averageTrackerBattery", // float
          "isOverlayOpen", // bool
          "isKeyboardOpen", // bool
          "isWristVisible", // bool
          "openOverlayCount" // int
        }
      );

    // https://wiki.ovrtoolkit.co.uk/#/OSC?id=xsoverlay-osc-parameters
    
    public static readonly ReadOnlyCollection<string> ALWAYS_GLOBAL_PARAMETERS_FROM_WLXOVERLAY_S =
      new ReadOnlyCollection<string>(
        new List<string>
        {
          "isOverlayOpen",    // bool
          "isKeyboardOpen",   // bool
          "isWristVisible",   // bool
          "openOverlayCount"  // int
        }
      );
    // https://github.com/galister/wlx-overlay-s/blob/main/src/backend/osc.rs

    public static readonly ReadOnlyCollection<string> ALWAYS_GLOBAL_PARAMETERS_FROM_XSOVERLAY =
      new ReadOnlyCollection<string>(
        new List<string>
        {
          "isOverlayOpen",    // bool
          "isKeyboardOpen",   // bool
          "isWristVisible"   // bool
        }
      );

    // https://xsoverlay.vercel.app/

  }
}
