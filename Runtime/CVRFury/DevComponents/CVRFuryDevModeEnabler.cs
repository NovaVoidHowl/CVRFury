using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  /// <summary>
  /// this component is used to
  /// enable the CVRFury Dev Mode in the editor
  /// just add one to the same GameObject that holds the one
  /// you want to put in dev mode
  /// </summary>
  [AddComponentMenu("CVRFury/Dev/Component Dev Mode Enabler")]
  public class CVRFuryDevModeEnabler : MonoBehaviour
  {
    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnDevModeChanged;

    [SerializeField]
    private bool devModeEnabled;
    public bool DevModeEnabled
    {
      get { return devModeEnabled; }
      set
      {
        if (devModeEnabled != value)
        {
          devModeEnabled = value;
          OnDevModeChanged?.Invoke(devModeEnabled);
        }
      }
    }
  }
}
