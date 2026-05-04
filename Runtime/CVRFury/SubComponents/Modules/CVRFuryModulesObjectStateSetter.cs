using System;
using System.Collections.Generic;
using UnityEngine;
using uk.novavoidhowl.dev.cvrfury.supporting_classes.runtime;
using uk.novavoidhowl.dev.cvrfury.packagecore;
using System.Reflection;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  [Serializable]
  public class objectStateSetter : StateSetter
  {
    public objectStateSetter()
      : base("objectStateSetter") { }

    public objectStatePair[] objectStatePairs;
  }
}
