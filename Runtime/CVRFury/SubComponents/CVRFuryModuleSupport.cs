using System;
using System.Collections.Generic;
using UnityEngine;
using uk.novavoidhowl.dev.vrcstub;
using uk.novavoidhowl.dev.cvrfury.supporting_classes.runtime;
using System.Reflection;

//// Supporting classes and structs for the CVRFuryModule

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  
  /// <summary>
  /// A struct to hold a pair of an object and a state to set it to.
  /// </summary>
  [Serializable]
  public struct objectStatePair
  {
    public enum objectState
    {
      disabled,
      enabled,
    }

    public objectState stateToSet;
    public GameObject objectToSetStateOn;
  }

  #region CCK stubs
  /// <summary>
  /// this class is a direct copy of the CVRParameterStreamEntry class from the CCK package
  /// it is used to store the data for the parameter stream link module
  /// this version based on 3.9.0 version of the CCK
  /// 
  /// notes:
  /// - UI limit needs to be put on accessing the 'TargetType' enum, at the moment the CCK only offers 'Animator'
  ///   so that's the only one we should expose
  /// - Type = input from the CVR systems (e.g. time, headset on head, etc)
  /// - ApplicationType = how to apply the value to the target
  /// - StaticValue = the value for use with the ApplicationTypes that reference a static value
  /// - Target = the object to apply the value to (will be blank for just the base avatar animator, but can be set to
  ///   a specific object for handling sub-animators or other objects)
  /// - ParameterName = the name of the parameter to apply the value to
  ///   
  /// </summary>
  [Serializable]
  public class CVRFuryParameterStreamEntry
  {
      public enum Type
      {
          TimeSeconds = 0,
          TimeSecondsUtc = 10,
          DeviceMode = 20,
          HeadsetOnHead = 30,
          ZoomFactor = 40,
          ZoomFactorCurve = 50,
          EyeMovementLeftX = 60,
          EyeMovementLeftY = 70,
          EyeMovementRightX = 80,
          EyeMovementRightY = 90,
          EyeBlinkingLeft = 100,
          EyeBlinkingRight = 110,
          VisemeLevel = 120,
          TimeSinceHeadsetRemoved = 130,
          TimeSinceLocalAvatarLoaded = 140,
          LocalWorldDownloadPercentage = 150,
          LocalFPS = 160,
          LocalPing = 170,
          LocalPlayerCount = 180,
          LocalTimeSinceFirstWorldJoin = 190,
          LocalTimeSinceWorldJoin = 200,
          LocalPlayerMuted = 210,
          LocalPlayerHudEnabled = 220,
          LocalPlayerNameplatesEnabled = 230,
          LocalPlayerHeight = 240,
          LocalPlayerLeftControllerType = 250,
          LocalPlayerRightControllerType = 251,
          LocalPlayerFullBodyEnabled = 260,
          TriggerLeftValue = 270,
          TriggerRightValue = 280,
          GripLeftValue = 290,
          GripRightValue = 300,
          GrippedObjectLeft = 310,
          GrippedObjectRight = 320,
          AvatarHeight = 400,
          AvatarUpright = 401,
          TransformGlobalPositionX = 500,
          TransformGlobalPositionY = 501,
          TransformGlobalPositionZ = 502,
          TransformGlobalRotationX = 510,
          TransformGlobalRotationY = 511,
          TransformGlobalRotationZ = 512,
          TransformLocalPositionX = 520,
          TransformLocalPositionY = 521,
          TransformLocalPositionZ = 522,
          TransformLocalRotationX = 530,
          TransformLocalRotationY = 531,
          TransformLocalRotationZ = 532,
          FluidVolumeSubmerged = 600,
          FluidVolumeDepth = 601,
          FluidVolumeTimeSinceEntered = 602,
          FluidVolumeTimeSinceExit = 603,
          InputCarSteering = 1000,
          InputCarAccelerate = 1001,
          InputCarBrake = 1002,
          InputCarHandbrake = 1003,
          InputCarBoost = 1004,
          InputMovementX = 1100,
          InputMovementY = 1101,
          InputLookX = 1110,
          InputLookY = 1111,
          InputJump = 1120,
                                      
          SeedOwner = 90000,
          SeedInstance = 90001,
      }

      public Type type = Type.TimeSeconds;

      public enum TargetType
      {
          Animator = 0,
          VariableBuffer = 1,
          AvatarAnimator = 2,
          CustomFloat = 3
      }

      public TargetType targetType = TargetType.Animator;

      public enum ApplicationType
      {
          Override = 0,
          AddToCurrent = 10,
          AddToStatic = 21,
          SubtractFromCurrent = 30,
          SubtractFromStatic = 41,
          SubtractWithCurrent = 50,
          SubtractWithStatic = 61,
          MultiplyWithCurrent = 70,
          MultiplyWithStatic = 81,
          CompareLessThen = 91,
          CompareLessThenEquals = 101,
          CompareEquals = 111,
          CompareMoreThenEquals = 121,
          CompareMoreThen = 131,
          Mod = 141,
          Pow = 151
      }

      public ApplicationType applicationType = ApplicationType.Override;

      public float staticValue;

      public GameObject target;

      public string parameterName;
  }
  #endregion

}
