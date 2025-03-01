using System;
using System.Collections.Generic;
using UnityEngine;
using uk.novavoidhowl.dev.vrcstub;
using uk.novavoidhowl.dev.cvrfury.supporting_classes.runtime;
using uk.novavoidhowl.dev.cvrfury.packagecore;
using System.Reflection;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  [DoNotShowOnList] // needs to be reviewed, not sure if this is needed any more
  [Serializable]
  public class parametersLink : CVRFuryModule
  {
    public parametersLink()
      : base("ParametersLink") { }

    public VRCExpressionParameters parameters;
  }
}
