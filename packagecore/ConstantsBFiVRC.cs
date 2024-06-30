using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace uk.novavoidhowl.dev.cvrfury.packagecore
{
  public static partial class Constants
  {
    public static readonly ReadOnlyCollection<string> ALWAYS_GLOBAL_PARAMETERS_FROM_BFIVRC =
      new ReadOnlyCollection<string>(
        new List<string>
        {
          "BFI/Info/VersionMajor",
          "BFI/Info/VersionMinor",
          "BFI/Info/SecondsSinceLastUpdate",
          "BFI/Info/DeviceConnected",
          "BFI/Info/BatterySupported",
          "BFI/Info/BatteryLevel",
          "BFI/NeuroFB/FocusLeft",
          "BFI/NeuroFB/FocusLeftPos",
          "BFI/NeuroFB/FocusRight",
          "BFI/NeuroFB/FocusRightPos",
          "BFI/NeuroFB/FocusAvg",
          "BFI/NeuroFB/FocusAvgPos",
          "BFI/NeuroFB/RelaxLeft",
          "BFI/NeuroFB/RelaxLeftPos",
          "BFI/NeuroFB/RelaxRight",
          "BFI/NeuroFB/RelaxRightPos",
          "BFI/NeuroFB/RelaxAvg",
          "BFI/NeuroFB/RelaxAvgPos",
          "BFI/PwrBands/Left/Gamma",
          "BFI/PwrBands/Left/Beta",
          "BFI/PwrBands/Left/Alpha",
          "BFI/PwrBands/Left/Theta",
          "BFI/PwrBands/Left/Delta",
          "BFI/PwrBands/Right/Gamma",
          "BFI/PwrBands/Right/Beta",
          "BFI/PwrBands/Right/Alpha",
          "BFI/PwrBands/Right/Theta",
          "BFI/PwrBands/Right/Delta",
          "BFI/PwrBands/Avg/Gamma",
          "BFI/PwrBands/Avg/Beta",
          "BFI/PwrBands/Avg/Alpha",
          "BFI/PwrBands/Avg/Theta",
          "BFI/PwrBands/Avg/Delta",
          "BFI/Addons/Hueshift",
          "BFI/Biometrics/Supported",
          "BFI/Biometrics/HeartBeatsPerSecond",
          "BFI/Biometrics/HeartBeatsPerMinute",
          "BFI/Biometrics/OxygenPercent",
          "BFI/Biometrics/BreathsPerSecond",
          "BFI/Biometrics/BreathsPerMinute",
        }
      );

    // ref https://github.com/ChilloutCharles/BrainFlowsIntoVRChat

  }

}
