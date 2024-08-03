using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

//// Supporting classes and structs for the CVRFuryModule

namespace uk.novavoidhowl.dev.cvrfury.packagecore
{
  
  /// <summary>
  /// A struct to hold a pair of an object and a state to set it to.
  /// </summary>
  [Serializable]
  public struct objectStatePair
  {
    public enum objectState
    {
      disabled,
      enabled,
    }

    public objectState stateToSet;
    public GameObject objectToSetStateOn;
  }

  public class DropdownParameter
  {
    public string machineName;
    public List<DropdownParameterPair> pairs = new List<DropdownParameterPair>();
  }
  
  [System.Serializable]
  public class DropdownParameterPair{
    public string name;
    public float value = 0;
  }
}
