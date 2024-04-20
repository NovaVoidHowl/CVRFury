using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.editor.supporting_classes
{
  public static class General
  {
    public static string formatParameterNameForMachineName(string name)
    {
      // remove any spaces from the name
      return name.Replace(" ", "");
    }
  } 
}
