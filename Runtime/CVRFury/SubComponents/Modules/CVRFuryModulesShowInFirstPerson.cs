using System;
using System.Collections.Generic;
using UnityEngine;
using uk.novavoidhowl.dev.cvrfury.supporting_classes.runtime;
using uk.novavoidhowl.dev.cvrfury.packagecore;
using System.Reflection;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  [Serializable]
  public class showInFirstPerson : CVRFuryModule
  {
    public showInFirstPerson()
      : base("ShowInFirstPerson") { }

    public GameObject target;
    public bool show;

    // this is so that when imported from VRCFury it will trigger binding to the head
    // by default (normal add via CVRFury will not bind to the head)
    public bool bindToHead = false;
  }
}
