using System;
using System.Collections.Generic;
using UnityEngine;
using static uk.novavoidhowl.dev.cvrfury.supporting_classes.runtime.General;

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
    public bool forceMachineName = false;

    // as of CCK3.10 the machine name is exposed in the inspector, this is to allow the user to set it
    // so no more need to force set it
    public string MachineName;

    // echo of what you get in the CCK menu items, if true then name and machine name are kept in sync
    // intrinsically set to false if forceMachineName is true
    public bool nameLinkedToMachineName = true;


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

    public float defaultState;

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

    public float defaultIndex;
    public GenerateType generateType = GenerateType.Int;

    [SerializeField]
    public List<DropdownParameterPair> dropdownList = new List<DropdownParameterPair>();
  }

  public class sliderParameter : menuParameter
  {
    public sliderParameter()
      : base("sliderParameter") { }

    [Range(0.0f, 1.0f)]
    public float defaultValue = 0.0f;
  }

  public class twoDJoystickParameter : menuParameter
  {
    public twoDJoystickParameter()
      : base("twoDJoystickParameter") { }

    public float defaultXValue = 0.0f;
    public float defaultYValue = 0.0f;
    public float minXValue = 0.0f;
    public float maxXValue = 1.0f;
    public float minYValue = 0.0f;
    public float maxYValue = 1.0f;

    public string xValuePostfix = "-x";
    public string yValuePostfix = "-y";
  }

  public class threeDJoystickParameter : menuParameter
  {
    public threeDJoystickParameter()
      : base("threeDJoystickParameter") { }

    public float defaultXValue = 0.0f;
    public float defaultYValue = 0.0f;
    public float defaultZValue = 0.0f;
    public float minXValue = 0.0f;
    public float maxXValue = 1.0f;
    public float minYValue = 0.0f;
    public float maxYValue = 1.0f;
    public float minZValue = 0.0f;
    public float maxZValue = 1.0f;

    public string xValuePostfix = "-x";
    public string yValuePostfix = "-y";
    public string zValuePostfix = "-z";
  }

  //// Notes for future development ------------------------------------------------------------------------------------

  //// More to be added, this is the list of menu items that will be available
  //   Toggle - done
  //   Dropdown - done
  //   MaterialColor - will be complex due to gameObject references - on hold
  //   Slider - done
  //   2DJoystick - done
  //   3DJoystick - done
  //   InputSingle -- not added as its not clear what this would be used for
  //   InputVector2 -- not added as its not clear what this would be used for
  //   InputVector3 -- not added as its not clear what this would be used for

  // Supporting classes -------------------------------------------------------------------------------------------------

}
