using System;
using System.Collections.Generic;
using UnityEngine;
using uk.novavoidhowl.dev.cvrfury.supporting_classes.runtime;
using uk.novavoidhowl.dev.cvrfury.packagecore;
using System.Reflection;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  [Serializable]
  public class objectScaleSetter : StateSetter
  {
    public objectScaleSetter()
      : base("objectScaleSetter") { }

    public GameObject objectToSetScaleOn;
    public float scaleToBeSet;
  }
}
