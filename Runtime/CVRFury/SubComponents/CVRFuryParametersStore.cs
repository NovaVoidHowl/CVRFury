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

      public ValueType valueType = ValueType.Float;

      public float defaultValue = 0.0f;
    }

    public enum ValueType
    {
      Float,
      Int,
      Bool
    }

    public Parameter[] parameters;
  }
}
