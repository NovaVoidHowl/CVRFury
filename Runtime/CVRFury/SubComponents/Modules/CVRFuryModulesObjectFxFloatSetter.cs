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
  public class objectFxFloatSetter : StateSetter
  {
    public objectFxFloatSetter()
      : base("objectFxFloatSetter") { }

    public string fxFloatName;
    public float fxFloatValue;
  }
}
