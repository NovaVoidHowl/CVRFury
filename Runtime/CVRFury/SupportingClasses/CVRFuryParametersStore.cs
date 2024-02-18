using System;
using UnityEngine;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  /// <summary>
  /// Storage class for CVRFury parameters
  /// </summary>
  [CreateAssetMenu(fileName = "NewCVRFuryParametersStore", menuName = "CVRFury/Parameters Store")]
  public class CVRFuryParametersStore : ScriptableObject
  {
    [Serializable]
    public class Parameter
    {
      public string name;

      public ValueType valueType;

      public float defaultValue;
    }

    public enum ValueType
    {
      Int,
      Float,
      Bool
    }

    public Parameter[] parameters;
  }
}
