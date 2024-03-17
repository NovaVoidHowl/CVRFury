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

    public toggleParameter()
      : base("toggleParameter") { }

    public bool defaultState;

    public GenerateType generateType = GenerateType.Bool; // default to bool as it is the most efficient for a toggle

    // no point in UseAnimation or target game objects as this is just to make a menu item
    // it is expected that the user will make their own animations and set them up appropriately
  }

  public class dropdownParameter : menuParameter
  {
    public enum GenerateType
    {
      Float,
      Int
    }

    public dropdownParameter()
      : base("dropdownParameter") { }

    public int defaultIndex;
    public GenerateType generateType = GenerateType.Int;

    // list of strings for the dropdown list
    public List<string> dropdownList = new List<string>();
  }

  public class sliderParameter : menuParameter
  {
    public sliderParameter()
      : base("sliderParameter") { }

    [Range(0.0f, 1.0f)]
    public float defaultValue = 0.0f;
  }

  //// Notes for future development ------------------------------------------------------------------------------------

  //// More to be added, this is the list of menu items that will be available
  //   Toggle - done
  //   Dropdown - done
  //   MaterialColor - will be complex due to gameobject references - on hold
  //   Slider - done
  //   2DJoystick
  //   3DJoystick
  //   InputSingle
  //   InputVector2
  //   InputVector3
}
