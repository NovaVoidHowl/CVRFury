using System;
using System.Collections.Generic;
using UnityEngine;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  /// <summary>
  /// </summary>
  [CreateAssetMenu(fileName = "NewCVRFuryMenuStore", menuName = "CVRFury/Menu Store")]
  public class CVRFuryMenuStore : ScriptableObject
  {
    [SerializeReference]
    public List<menuParameter> menuItems = new List<menuParameter>();
  }

  [Serializable]
  public abstract class menuParameter
  {
    public bool viewerFoldoutState = false;
    public string name;

    [SerializeField]
    private string menuParameterType;

    public string MenuParameterType
    {
      get { return menuParameterType; }
    }

    protected menuParameter(string menuParameterType)
    {
      this.menuParameterType = menuParameterType;
    }
  }

  public class toggleParameter : menuParameter
  {
    public enum GenerateType
    {
      Float,
      Int,
      Bool
    }

    public List<targetGameObject> targets;

    public toggleParameter()
      : base("toggleParameter")
    {
      targets = new List<targetGameObject>();
    }

    public bool defaultState;
    public bool useAnimation;

    public GenerateType generateType = GenerateType.Float;
  }

  public class testParameter : menuParameter
  {
    public testParameter()
      : base("testParameter") { }

    public string testString;
  }

  [Serializable]
  public class targetGameObject
  {
    public GameObject target;
    public bool stateToSet;
  }

  //// More to be added, this is the list of menu items that will be available
  //   Toggle - done
  //   Dropdown
  //   MaterialColor
  //   Slider
  //   2DJoystick
  //   3DJoystick
  //   InputSingle
  //   InputVector2
  //   InputVector3
}
