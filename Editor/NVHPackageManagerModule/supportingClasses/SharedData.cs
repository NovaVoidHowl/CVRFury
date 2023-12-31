// editor only script to manage the dependencies
#if UNITY_EDITOR

using System.Collections.Generic;

namespace uk.novavoidhowl.dev.nvhpmm
{
  public static class SharedData
  {
    public const string NVHPMM_VERSION = "2.3.0";
    public static List<string> appComponentsList = new List<string>();
    public static List<PrimaryPackageDependency> PrimaryDependencies = new List<PrimaryPackageDependency>();
    public static List<ThirdPartyPackageDependency> ThirdPartyDependencies = new List<ThirdPartyPackageDependency>();
  }
}
#endif
