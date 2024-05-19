using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace uk.novavoidhowl.dev.cvrfury.packagecore
{
  public static partial class Constants
  {
    public static readonly ReadOnlyCollection<string> ALWAYS_GLOBAL_PARAMETERS_FROM_VRCFT_V5 =
      new ReadOnlyCollection<string>(
        new List<string>
        {
          // V5 Parameters
          // Eye Gaze Parameters
          "v2/EyeLeftX",
          "v2/EyeLeftY",
          "v2/EyeRightX",
          "v2/EyeRightY",
          // Eye Expression Parameters
          "v2/EyeLidRight",
          "v2/EyeLidLeft",
          "v2/EyeLid",
          "v2/EyeSquintRight",
          "v2/EyeSquintLeft",
          "v2/EyeSquint",
          "v2/PupilDilation",
          "v2/PupilDiameterRight",
          "v2/PupilDiameterLeft",
          "v2/PupilDiameter",
          // Brow Parameters
          "v2/BrowPinchRight",
          "v2/BrowPinchLeft",
          "v2/BrowLowererRight",
          "v2/BrowLowererLeft",
          "v2/BrowInnerUpRight",
          "v2/BrowInnerUpLeft",
          "v2/BrowOuterUpRight",
          "v2/BrowOuterUpLeft",
          // Nose Parameters
          "v2/NoseSneerRight",
          "v2/NoseSneerLeft",
          "v2/NasalDilationRight",
          "v2/NasalDilationLeft",
          "v2/NasalConstrictRight",
          "v2/NasalConstrictLeft",
          // Cheek Parameters
          "v2/CheekSquintRight",
          "v2/CheekSquintLeft",
          "v2/CheekPuffSuckRight",
          "v2/CheekPuffSuckLeft",
          // Jaw Parameters
          "v2/JawOpen",
          "v2/MouthClosed",
          "v2/JawX",
          "v2/JawZ",
          "v2/JawClench",
          "v2/JawMandibleRaise",
          // Lip Parameters
          "v2/LipSuckUpperRight",
          "v2/LipSuckUpperLeft",
          "v2/LipSuckLowerRight",
          "v2/LipSuckLowerLeft",
          "v2/LipSuckCornerRight",
          "v2/LipSuckCornerLeft",
          "v2/LipFunnelUpperRight",
          "v2/LipFunnelUpperLeft",
          "v2/LipFunnelLowerRight",
          "v2/LipFunnelLowerLeft",
          "v2/LipPuckerUpperRight",
          "v2/LipPuckerUpperLeft",
          "v2/LipPuckerLowerRight",
          "v2/LipPuckerLowerLeft",
          // Mouth Parameters
          "v2/MouthUpperUpRight",
          "v2/MouthUpperUpLeft",
          "v2/MouthLowerDownRight",
          "v2/MouthLowerDownLeft",
          "v2/MouthUpperDeepenRight",
          "v2/MouthUpperDeepenLeft",
          "v2/MouthUpperX",
          "v2/MouthLowerX",
          "v2/MouthCornerPullRight",
          "v2/MouthCornerPullLeft",
          "v2/MouthCornerSlantRight",
          "v2/MouthCornerSlantLeft",
          "v2/MouthDimpleRight",
          "v2/MouthDimpleLeft",
          "v2/MouthFrownRight",
          "v2/MouthFrownLeft",
          "v2/MouthStretchRight",
          "v2/MouthStretchLeft",
          "v2/MouthRaiserUpper",
          "v2/MouthLowererLower",
          "v2/MouthPressRight",
          "v2/MouthPressLeft",
          "v2/MouthTightenerRight",
          "v2/MouthTightenerLeft",
          // Tongue Parameters
          "v2/TongueOut",
          "v2/TongueX",
          "v2/TongueY",
          "v2/TongueRoll",
          "v2/TongueArchY",
          "v2/TongueShape",
          "v2/TongueTwistRight",
          "v2/TongueTwistLeft",
          // Neck Parameters
          "v2/SoftPalateClose",
          "v2/ThroatSwallow",
          "v2/NeckFlexRight",
          "v2/NeckFlexLeft",
          // Simplified Eye Parameters
          "v2/EyeX",
          "v2/EyeY",
          // Simplified Brow Parameters
          "v2/BrowDownRight",
          "v2/BrowDownLeft",
          "v2/BrowOuterUp",
          "v2/BrowInnerUp",
          "v2/BrowUp",
          "v2/BrowExpressionRight",
          "v2/BrowExpressionLeft",
          "v2/BrowExpression",
          // Simplified Mouth Parameters
          "v2/MouthX",
          "v2/MouthUpperUp",
          "v2/MouthLowerDown",
          "v2/MouthOpen",
          "v2/MouthSmileRight",
          "v2/MouthSmileLeft",
          "v2/MouthSadRight",
          "v2/MouthSadLeft",
          "v2/SmileFrownRight",
          "v2/SmileFrownLeft",
          "v2/SmileFrown",
          "v2/SmileSadRight",
          "v2/SmileSadLeft",
          "v2/SmileSad",
          // Simplified Lip Parameters
          "v2/LipSuckUpper",
          "v2/LipSuckLower",
          "v2/LipSuck",
          "v2/LipFunnelUpper",
          "v2/LipFunnelLower",
          "v2/LipFunnel",
          "v2/LipPuckerUpper",
          "v2/LipPuckerLower",
          "v2/LipPucker",
          // Simplified Nose and Cheek Parameters
          "v2/NoseSneer",
          "v2/CheekSquint",
          "v2/CheekPuffSuck",
          // Tracking Active Parameters
          "EyeTrackingActive",
          "ExpressionTrackingActive",
          "LipTrackingActive"

        }
      );

    public static readonly ReadOnlyCollection<string> ALWAYS_GLOBAL_PARAMETERS_FROM_VRCFT_V4 =
      new ReadOnlyCollection<string>(
        new List<string>
        {
          // V4 Parameters
          // Eye Tracking Parameters
          "EyesX",
          "EyesY",
          "LeftEyeLid",
          "RightEyeLid",
          "CombinedEyeLid",
          "EyesWiden",
          "EyesDilation",
          "EyesPupilDiameter",
          "EyesSqueeze",
          "LeftEyeX",
          "LeftEyeY",
          "RightEyeX",
          "RightEyeY",
          "LeftEyeWiden",
          "RightEyeWiden",
          "LeftEyeSqueeze",
          "RightEyeSqueeze",
          "LeftEyeLidExpanded",
          "RightEyeLidExpanded",
          "CombinedEyeLidExpanded",
          "LeftEyeLidExpandedSqueeze",
          "RightEyeLidExpandedSqueeze",
          "CombinedEyeLidExpandedSqueeze",
          // Lip Tracking Parameters
          "JawRight",
          "JawLeft",
          "JawForward",
          "JawOpen",
          "MouthApeShape",
          "MouthUpperRight",
          "MouthUpperLeft",
          "MouthLowerRight",
          "MouthLowerLeft",
          "MouthUpperOverturn",
          "MouthLowerOverturn",
          "MouthPout",
          "MouthSmileRight",
          "MouthSmileLeft",
          "MouthSadRight",
          "MouthSadLeft",
          "CheekPuffRight",
          "CheekPuffLeft",
          "CheekSuck",
          "MouthUpperUpRight",
          "MouthUpperUpLeft",
          "MouthLowerDownRight",
          "MouthLowerDownLeft",
          "MouthUpperInside",
          "MouthLowerInside",
          "MouthLowerOverlay",
          "TongueLongStep1",
          "TongueLongStep2",
          "TongueDown",
          "TongueUp",
          "TongueLeft",
          "TongueRight",
          "TongueRoll",
          "TongueUpLeftMorph",
          "TongueUpRightMorph",
          "TongueDownLeftMorph",
          "TongueDownRightMorph",
          // Tracking Status Bools
          "EyeTrackingActive",
          "LipTrackingActive",
          // Combined Lip Parameters
          "JawX",
          "MouthUpper",
          "MouthLower",
          "MouthX",
          "MouthUpperInsideOverturn",
          "MouthLowerInsideOverturn",
          "SmileSadRight",
          "SmileSadLeft",
          "SmileSad",
          "TongueY",
          "TongueX",
          "TongueSteps",
          "PuffSuckRight",
          "PuffSuckLeft",
          "PuffSuck",
          "JawOpenApe",
          "JawOpenPuff",
          "JawOpenPuffRight",
          "JawOpenPuffLeft",
          "JawOpenSuck",
          "JawOpenForward",
          "JawOpenOverlay",
          "MouthUpperUpRightUpperInside",
          "MouthUpperUpLeftUpperInside",
          "MouthUpperUpRightApe",
          "MouthUpperUpRightPout",
          "MouthUpperUpRightOverlay",
          "MouthUpperUpRightSuck",
          "MouthUpperUpLeftUpperInside",
          "MouthUpperUpLeftPuffLeft",
          "MouthUpperUpLeftApe",
          "MouthUpperUpLeftPout",
          "MouthUpperUpLeftOverlay",
          "MouthUpperUpLeftSuck",
          "MouthUpperUpUpperInside",
          "MouthUpperUpInside",
          "MouthUpperUpPuff",
          "MouthUpperUpPuffLeft",
          "MouthUpperUpPuffRight",
          "MouthUpperUpApe",
          "MouthUpperUpPout",
          "MouthUpperUpOverlay",
          "MouthUpperUpSuck",
          "MouthLowerDownRightLowerInside",
          "MouthLowerDownRightPuffRight",
          "MouthLowerDownRightApe",
          "MouthLowerDownRightPout",
          "MouthLowerDownRightOverlay",
          "MouthLowerDownRightSuck",
          "MouthLowerDownLeftLowerInside",
          "MouthLowerDownLeftPuffLeft",
          "MouthLowerDownLeftApe",
          "MouthLowerDownLeftPout",
          "MouthLowerDownLeftOverlay",
          "MouthLowerDownLeftSuck",
          "MouthLowerDownLowerInside",
          "MouthLowerDownInside",
          "MouthLowerDownPuff",
          "MouthLowerDownPuffLeft",
          "MouthLowerDownPuffRight",
          "MouthLowerDownApe",
          "MouthLowerDownPout",
          "MouthLowerDownOverlay",
          "MouthLowerDownSuck",
          "SmileRightUpperOverturn",
          "SmileRightLowerOverturn",
          "SmileRightOverturn",
          "SmileRightApe",
          "SmileRightOverlay",
          "SmileRightPout",
          "SmileLeftUpperOverturn",
          "SmileLeftLowerOverturn",
          "SmileLeftOverturn",
          "SmileLeftApe",
          "SmileLeftOverlay",
          "SmileLeftPout",
          "SmileUpperOverturn",
          "SmileLowerOverturn",
          "SmileApe",
          "SmileOverlay",
          "SmilePout",
          "PuffRightUpperOverturn",
          "PuffRightLowerOverturn",
          "PuffRightOverturn",
          "PuffLeftUpperOverturn",
          "PuffLeftLowerOverturn",
          "PuffLeftOverturn",
          "PuffUpperOverturn",
          "PuffLowerOverturn",
          "PuffOverturn"
        }
      );

    // ref https://docs.vrcft.io/docs/tutorial-avatars/tutorial-avatars-extras/parameters

  }

}
