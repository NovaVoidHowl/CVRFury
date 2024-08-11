using System;
using System.Collections.Generic;
using UnityEngine;
using uk.novavoidhowl.dev.vrcstub;
using uk.novavoidhowl.dev.cvrfury.supporting_classes.runtime;
using uk.novavoidhowl.dev.cvrfury.packagecore;
using System.Reflection;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  [Serializable]
  [CVRFuryUniqueModule]
  public class removeLegacyCVRToggles : CVRFuryModule
  {
    // note this is a unique module and only one instance of this module can be applied to an avatar
    public removeLegacyCVRToggles()
      : base("RemoveLegacyCVRToggles") { }

    public bool removeEnabled = true;
  }
}
