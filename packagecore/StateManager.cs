using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace uk.novavoidhowl.dev.cvrfury.packagecore
{
  /// <summary>
  /// class to handle the state of the module installer play mode vs build mode
  /// </summary>
  public static class ModuleInstallerState
  {
    public static bool IsModuleInstallOnBuildCalled
    {
      get { return PlayerPrefs.GetInt("CVRFury-IsModuleInstallOnBuildCalled", 0) == 1; }
      set { PlayerPrefs.SetInt("CVRFury-IsModuleInstallOnBuildCalled", value ? 1 : 0); }
    }
  }
}
