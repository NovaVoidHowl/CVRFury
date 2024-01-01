// if for some reason the VRC SDK is in the project then disable this stub
#if !VRC_SDK_VRCSDK3

using System;
using System.Collections.Generic;
using UnityEngine;

namespace uk.novavoidhowl.dev.vrcstub
{
  /// <summary>
  /// Stub for VRCExpressionsMenu
  /// This is a minimal implementation of the VRCExpressionsMenu class only concerned with the parameters
  ///
  /// named the same as the original class so that it can be used as a drop in replacement with VRCFury stubs
  /// in order to keep the code as similar as possible with the original VRCFury code base
  /// </summary>
  public class VRCExpressionsMenu : ScriptableObject
  {
    [Serializable]
    public class Control
    {
      public enum ControlType
      {
        Button = 101,
        Toggle = 102,
        SubMenu = 103,
        TwoAxisPuppet = 201,
        FourAxisPuppet = 202,
        RadialPuppet = 203
      }

      [Serializable]
      public class Parameter
      {
        public string name;

        private int _hash;

        public int hash
        {
          get
          {
            if (_hash == 0)
            {
              _hash = Animator.StringToHash(name);
            }
            return _hash;
          }
        }

        public static bool IsNull(Parameter parameter)
        {
          if (parameter != null)
          {
            return parameter.hash == 0;
          }
          return true;
        }
      }

      public enum Style
      {
        Style1,
        Style2,
        Style3,
        Style4
      }

      [Serializable]
      public struct Label
      {
        [Tooltip("(Optional) Label shown in the expression menu.")]
        public string name;

        [Tooltip("(Optional) Icon shown in the expression menu.")]
        public Texture2D icon;
      }

      [Tooltip("(Optional) Label shown in the expression menu.")]
      public string name;

      [Tooltip("(Optional) Icon shown in the expression menu.")]
      public Texture2D icon;

      [Tooltip("Type of control used.")]
      public ControlType type = ControlType.Button;

      [Tooltip("The specific parameter used by this control in the Animation Controller.")]
      public Parameter parameter;

      [Tooltip("The value the parameter is set to when this control is used.")]
      public float value = 1f;

      public Style style;

      public VRCExpressionsMenu subMenu;

      public Parameter[] subParameters;

      public Label[] labels;

      public Parameter GetSubParameter(int i)
      {
        if (subParameters != null && i >= 0 && i < subParameters.Length)
        {
          return subParameters[i];
        }
        return null;
      }

      public Label GetLabel(int i)
      {
        if (labels != null && i >= 0 && i < labels.Length)
        {
          return labels[i];
        }
        return default(Label);
      }
    }

    public const int MAX_CONTROLS = 8;

    public List<Control> controls = new List<Control>();
  }
}

#endif
