using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.packagecore
{
  public static partial class Constants
  {
    // colour data for VRC component headers
    public static readonly Color VRC_HEADER_PREFIX_COLOUR = new Color(
      hexadecimalToColour("#0060ff").r,
      hexadecimalToColour("#0060ff").g,
      hexadecimalToColour("#0060ff").b
    );
    public static readonly Color VRC_HEADER_BACKGROUND_COLOUR = new Color(
      hexadecimalToColour("#202020").r,
      hexadecimalToColour("#202020").g,
      hexadecimalToColour("#202020").b
    );
    public static readonly Color VRC_HEADER_BACKGROUND_HOVER_COLOUR = new Color(
      hexadecimalToColour("#262a2b").r,
      hexadecimalToColour("#262a2b").g,
      hexadecimalToColour("#262a2b").b
    );

    public static readonly ReadOnlyCollection<string> VRCSTUB_COMPONENTS_TO_REMOVE = new ReadOnlyCollection<string>(
      new List<string>
      {
        // from VRCAVstub
        "VRC.SDK3.Avatars.Components.VRCAvatarDescriptor",
        "VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver",
        "VRC.SDK3.Avatars.Components.VRCHeadChop",
        "VRC.SDK3.Avatars.Components.VRCSpatialAudioSource",
        // from VRCPBstub
        "VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone",
        "VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBoneCollider",
        // from VRCPConStub
        "VRC.SDK3.Dynamics.Constraint.Components.VRCAimConstraint",
        "VRC.SDK3.Dynamics.Constraint.Components.VRCLookAtConstraint",
        "VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint",
        "VRC.SDK3.Dynamics.Constraint.Components.VRCPositionConstraint",
        "VRC.SDK3.Dynamics.Constraint.Components.VRCRotationConstraint",
        "VRC.SDK3.Dynamics.Constraint.Components.VRCScaleConstraint",
        // from VRCPCstub
        "VRC.SDK3.Dynamics.Contact.Components.VRCContactReceiver",
        "VRC.SDK3.Dynamics.Contact.Components.VRCContactSender",
        // from VRCPMstub
        "VRC.Core.PipelineManager",
      }
    );

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
          CVR = "#IsLocal",
          NeedsParameterStream = true,
          ParameterStreamPairs = new List<CRVParameterStreamPair>
          {
            new CRVParameterStreamPair
            {
              TargetParmName = "#DeviceMode",
              ParameterStreamSource = CVRFuryParameterStreamEntry.Type.DeviceMode
            },
          },
          NeedsAnimator = true,
          NeedsMod = false,
          ModURL = "",
          Supported = true
          // there is not a direct mapping for this one, but you can do something similar using
          // DeviceMode as a local parameter, that entails changing the logic of the animator
        }
      },
      {
        "Viseme",
        new VRCParameterTranslation
        {
          CVR = "Viseme",
          NeedsParameterStream = true,
          ParameterStreamPairs = new List<CRVParameterStreamPair>
          {
            new CRVParameterStreamPair
            {
              TargetParmName = "Viseme",
              ParameterStreamSource = CVRFuryParameterStreamEntry.Type.VisemeLevel
            },
          },
          NeedsAnimator = false,
          NeedsMod = false,
          ModURL = "",
          Supported = true
        }
        // 1-to-1 mapping via parameter stream
      },
      {
        "Voice",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          ParameterStreamPairs = null,
          NeedsAnimator = false,
          NeedsMod = false,
          ModURL = "",
          Supported = false
          // there does not seem to be a direct mapping for this one
        }
      },
      // GestureLeft and GestureRight seem to be native now in CVR
      // may not match the type VRC expects though (they are float in CVR)
      // looks CVR has int version too, so may be able to use that if needed
      // (ref https://discord.com/channels/410126604237406209/1237590087781584958/1238864867574681751)
      //
      // {
      //   "GestureLeft",
      //   new VRCParameterTranslation
      //   {
      //     CVR = "GestureLeft",
      //     NeedsParameterStream = true,
      //     ParameterStreamPairs =
      //       new List<CRVParameterStreamPair> {
      //         new CRVParameterStreamPair {
      //           TargetParmName = "GestureLeft",
      //           ParameterStreamSource = CVRFuryParameterStreamEntry.Type.TriggerLeftValue
      //         },
      //       },
      //     NeedsAnimator = false,
      //     NeedsMod = false,
      //     ModURL = "",
      //     Supported = true
      //   }
      // },
      // {
      //   "GestureRight",
      //   new VRCParameterTranslation
      //   {
      //     CVR = "GestureRight",
      //     NeedsParameterStream = true,
      //     ParameterStreamPairs =
      //       new List<CRVParameterStreamPair> {
      //         new CRVParameterStreamPair {
      //           TargetParmName = "GestureRight",
      //           ParameterStreamSource = CVRFuryParameterStreamEntry.Type.TriggerRightValue
      //         },
      //       },
      //     NeedsAnimator = false,
      //     NeedsMod = false,
      //     ModURL = "",
      //     Supported = true
      //   }
      // },
      {
        "AngularY",
        new VRCParameterTranslation
        {
          CVR = "",
          NeedsParameterStream = false,
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          CVR = "Upright",
          NeedsParameterStream = true,
          ParameterStreamPairs = new List<CRVParameterStreamPair>
          {
            new CRVParameterStreamPair
            {
              TargetParmName = "Upright",
              ParameterStreamSource = CVRFuryParameterStreamEntry.Type.AvatarUpright
            },
          },
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          CVR = "MuteSelf",
          NeedsParameterStream = true,
          ParameterStreamPairs = new List<CRVParameterStreamPair>
          {
            new CRVParameterStreamPair
            {
              TargetParmName = "MuteSelf",
              ParameterStreamSource = CVRFuryParameterStreamEntry.Type.LocalPlayerMuted
            },
          },
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
          ParameterStreamPairs = null,
          NeedsAnimator = false,
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
    public List<CRVParameterStreamPair> ParameterStreamPairs { get; set; }
    public bool NeedsAnimator { get; set; }
    public bool NeedsMod { get; set; }
    public string ModURL { get; set; }
    public bool Supported { get; set; }
  }

  public class CRVParameterStreamPair
  {
    public string TargetParmName { get; set; }
    public CVRFuryParameterStreamEntry.Type ParameterStreamSource { get; set; }
  }
}
