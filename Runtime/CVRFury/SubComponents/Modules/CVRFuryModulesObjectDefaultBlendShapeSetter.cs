using System;
using System.Collections.Generic;
using UnityEngine;
using uk.novavoidhowl.dev.cvrfury.supporting_classes.runtime;
using uk.novavoidhowl.dev.cvrfury.packagecore;
using System.Reflection;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  [Serializable]
  public class objectDefaultBlendShapeSetter : StateSetter
  {
    public objectDefaultBlendShapeSetter()
      : base("objectDefaultBlendShapeSetter") { }

    public string blendShape;
    public float blendShapeValue = 100;
    public Renderer renderer;
    public bool allRenderers = true;
  }
}
