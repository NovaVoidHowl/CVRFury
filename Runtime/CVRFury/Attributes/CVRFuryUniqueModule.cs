using System;
using UnityEngine.Scripting;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
  public class CVRFuryUniqueModule : Attribute
  {
    public CVRFuryUniqueModule()
    {
      // Get the stack trace to find the class this attribute is applied to
      var stackTrace = new System.Diagnostics.StackTrace();
      var frame = stackTrace.GetFrame(1);
      var method = frame.GetMethod();
      var declaringType = method.DeclaringType;

      // Check if the declaring type is derived from CVRFuryModule
      if (declaringType == null || !IsDerivedFromCVRFuryModule(declaringType))
      {
        throw new InvalidOperationException(
          "CVRFuryUniqueModule attribute can only be used on classes derived from CVRFuryModule."
        );
      }
    }

    private bool IsDerivedFromCVRFuryModule(Type type)
    {
      return type != null && (type == typeof(CVRFuryModule) || type.IsSubclassOf(typeof(CVRFuryModule)));
    }
  }
}
