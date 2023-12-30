using System.IO;
using UnityEditor;

// only need to change this line (and the asmdef) to bind to project specific constants
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
