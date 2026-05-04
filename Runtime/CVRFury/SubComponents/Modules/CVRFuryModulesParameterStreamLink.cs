using System;
using System.Collections.Generic;
using UnityEngine;
using uk.novavoidhowl.dev.cvrfury.supporting_classes.runtime;
using uk.novavoidhowl.dev.cvrfury.packagecore;
using System.Reflection;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  [Serializable]
  public class parameterStreamLink : CVRFuryModule
  {
    public parameterStreamLink()
      : base("parameterStreamLink") { }

    public List<CVRFuryParameterStreamEntry> entries = new();
  }
}
