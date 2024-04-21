using System;
using System.Collections.Generic;
using UnityEngine;

namespace uk.novavoidhowl.dev.cvrfury.supporting_classes.runtime
{
  [Serializable]
  public class CVRFurySemVer
  {
    public int major;
    public int minor;
    public int patch;
    public string metadata;

    public override string ToString()
    {
      return $"{major}.{minor}.{patch}" + (string.IsNullOrEmpty(metadata) ? "" : $"+{metadata}");
    }
  }
}
