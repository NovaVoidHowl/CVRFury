// editor only script to manage the dependencies
#if UNITY_EDITOR

using System.IO;
using UnityEditor;

// Only need to change the following line, in the following files:
//
// DepManager.cs
// render1stPartyDeps.cs
// render3rdPartyDeps.cs
// renderAppComponents.cs
// renderCoreError.cs
// Validation.cs
// AppInternalPackages.cs
// DepManagerConfig.cs
// PrimaryDependenciesPackages.cs
// ThirdPartyDependenciesPackages.cs
//
// and the asmdef, to bind to project specific constants

using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;

namespace uk.novavoidhowl.dev.nvhpmm
{
  public class DepManagerConfig
  {
    public bool ThirdPartyDepsEnabled { get; set; }
    public bool FirstPartyDepsEnabled { get; set; }
    public bool AppComponentsEnabled { get; set; }
  }
}
#endif
