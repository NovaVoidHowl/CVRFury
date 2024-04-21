using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace uk.novavoidhowl.dev.cvrfury.packagecore
{
  public static partial class Constants
  {
    public static readonly ReadOnlyCollection<string> ALWAYS_GLOBAL_PARAMETERS_FROM_VRC =
      new ReadOnlyCollection<string>(
        new List<string>
        {
          "IsLocal",
          "Viseme",
          "Voice",
          "GestureLeft",
          "GestureRight",
          "GestureLeftWeight",
          "GestureRightWeight",
          "AngularY",
          "VelocityX",
          "VelocityY",
          "VelocityZ",
          "VelocityMagnitude",
          "Upright",
          "Grounded",
          "Seated",
          "AFK",
          "Expression1",
          "Expression2",
          "Expression3",
          "Expression4",
          "Expression5",
          "Expression6",
          "Expression7",
          "Expression8",
          "Expression9",
          "Expression10",
          "Expression11",
          "Expression12",
          "Expression13",
          "Expression14",
          "Expression15",
          "Expression16",
          "TrackingType",
          "VRMode",
          "MuteSelf",
          "InStation",
          "Earmuffs",
          "IsOnFriendsList"
        }
      );

    // ref https://creators.vrchat.com/avatars/animator-parameters/


    // list of parameter stream mappings that are equivalent to the some defaults on VRC
    public static readonly Dictionary<string, VRCParameterTranslation> VRC_PARAMETER_STREAM_MAPPINGS = new Dictionary<
      string,
      VRCParameterTranslation
    >
    {
      {
        "IsLocal",
        new VRCParameterTranslation
        {
          CVR = "IsLocal",
          NeedsParameterStream = false,
          NeedsMod = true,
          ModURL = "",
          Supported = false
          // there is not a direct mapping for this one, but you can do something similar using
          // DeviceMode as a local parameter, that entails changing the logic of the animator
          // there may be a mod that directly maps this but not sure on that (needs to be checked)
        }
      },
      {
        "Viseme",
        new VRCParameterTranslation
        {
          CVR = "VisemeLevel",
          NeedsParameterStream = true,
          NeedsMod = false,
          ModURL = "",
          Supported = true
        }
      },
      {
        "Voice",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one
        }
      },
      {
        "GestureLeft",
        new VRCParameterTranslation
        {
          CVR = "TriggerLeftValue",
          NeedsParameterStream = true,
          NeedsMod = false,
          ModURL = "",
          Supported = true
        }
      },
      {
        "GestureRight",
        new VRCParameterTranslation
        {
          CVR = "TriggerRightValue",
          NeedsParameterStream = true,
          NeedsMod = false,
          ModURL = "",
          Supported = true
        }
      },
      {
        "AngularY",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one
        }
      },
      {
        "VelocityX",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one
        }
      },
      {
        "VelocityY",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one
        }
      },
      {
        "VelocityZ",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one
        }
      },
      {
        "VelocityMagnitude",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one
        }
      },
      {
        "Upright",
        new VRCParameterTranslation
        {
          CVR = "AvatarUpright",
          NeedsParameterStream = true,
          NeedsMod = false,
          ModURL = "",
          Supported = true
        }
      },
      {
        "Seated",
        new VRCParameterTranslation
        {
          CVR = "Sitting",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = true
        }
      },
      {
        "AFK",
        new VRCParameterTranslation
        {
          CVR = "AFK",
          NeedsParameterStream = false,
          NeedsMod = true,
          ModURL = "https://github.com/kafeijao/Kafe_CVR_Mods/blob/master/BetterAFK/README.md",
          Supported = true
        }
      },
      { 
        "Expression1",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one, think Emote is the closest but that is an int
          // that covers all the expressions
        }
      },
      { 
        "Expression2",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one, think Emote is the closest but that is an int
          // that covers all the expressions
        }
      },
      { 
        "Expression3",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one, think Emote is the closest but that is an int
          // that covers all the expressions
        }
      },
      { 
        "Expression4",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one, think Emote is the closest but that is an int
          // that covers all the expressions
        }
      },
      { 
        "Expression5",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one, think Emote is the closest but that is an int
          // that covers all the expressions
        }
      },
      { 
        "Expression6",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one, think Emote is the closest but that is an int
          // that covers all the expressions
        }
      },
      { 
        "Expression7",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one, think Emote is the closest but that is an int
          // that covers all the expressions
        }
      },
      { 
        "Expression8",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one, think Emote is the closest but that is an int
          // that covers all the expressions
        }
      },
      {
        "Expression9",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one, think Emote is the closest but that is an int
          // that covers all the expressions
        }
      },
      {
        "Expression10",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one, think Emote is the closest but that is an int
          // that covers all the expressions
        }
      },
      {
        "Expression11",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one, think Emote is the closest but that is an int
          // that covers all the expressions
        }
      },
      {
        "Expression12",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one, think Emote is the closest but that is an int
          // that covers all the expressions
        }
      },
      {
        "Expression13",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one, think Emote is the closest but that is an int
          // that covers all the expressions
        }
      },
      {
        "Expression14",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one, think Emote is the closest but that is an int
          // that covers all the expressions
        }
      },
      {
        "Expression15",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one, think Emote is the closest but that is an int
          // that covers all the expressions
        }
      },
      {
        "Expression16",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one, think Emote is the closest but that is an int
          // that covers all the expressions
        }
      },
      { 
        "TrackingType",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one, DeviceMode is the closest but not 1-to-1
        }
      },
      {
        "VRMode",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one
        }
      },
      {
        "MuteSelf",
        new VRCParameterTranslation
        {
          CVR = "LocalPlayerMuted",
          NeedsParameterStream = true,
          NeedsMod = false,
          ModURL = "",
          Supported = true
        }
      },
      {
        "InStation",
        new VRCParameterTranslation
        {
          CVR = "Sitting",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = true
        }
      },
      {
        "Earmuffs",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // no such thing as earmuffs parameter in CVR, so this is not supported
        }
      },
      {
        "IsOnFriendsList",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // no such thing as IsOnFriendsList parameter in CVR, so this is not supported
          // perhaps you can use a mod to get this information (needs to be checked)
        }
      }
    };
  }

  public class VRCParameterTranslation
  {
    public string CVR { get; set; }
    public bool NeedsParameterStream { get; set; }
    public bool NeedsMod { get; set; }
    public string ModURL { get; set; }
    public bool Supported { get; set; }
  }
}
