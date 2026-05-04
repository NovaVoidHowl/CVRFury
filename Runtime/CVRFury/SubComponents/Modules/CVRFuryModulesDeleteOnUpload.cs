using System;
using System.Collections.Generic;
using UnityEngine;
using uk.novavoidhowl.dev.cvrfury.supporting_classes.runtime;
using uk.novavoidhowl.dev.cvrfury.packagecore;
using System.Reflection;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  [Serializable]
  public class deleteOnUpload : CVRFuryModule
  {
    public deleteOnUpload()
      : base("DeleteOnUpload") { }

    public GameObject target;
  }
}
