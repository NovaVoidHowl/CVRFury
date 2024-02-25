using System;
using System.Collections.Generic;
using UnityEngine;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  /// <summary>
  /// This class is used to store the menu items that will be used by CVRFury to inject additional CVR menu sections
  /// </summary>
  [CreateAssetMenu(fileName = "NewCVRFuryMenuStore", menuName = "CVRFury/Menu Store")]
  public class CVRFuryMenuStore : ScriptableObject
  {
    [SerializeReference]
    public List<menuParameter> menuItems = new List<menuParameter>();
  }

  // Base class for all menu items -------------------------------------------------------------------------------------

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

  // Menu items derivatives --------------------------------------------------------------------------------------------

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

  // Test class for development purposes (remove once one more derivative class added) ---------------------------------
  public class testParameter : menuParameter
  {
    public testParameter()
      : base("testParameter") { }

    public string testString;
  }

  //// Support classes -------------------------------------------------------------------------------------------------
  [Serializable]
  public class targetGameObject
  {
    public GameObject target;
    public bool stateToSet;
  }

  //// Notes for future development ------------------------------------------------------------------------------------

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
