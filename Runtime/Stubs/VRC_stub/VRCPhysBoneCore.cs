// if for some reason the VRC SDK is in the project then disable this stub
#if !VRC_SDK_VRCSDK3

using System;
using System.Collections.Generic;
using UnityEngine;
using uk.novavoidhowl.dev.cvrfury.packagecore;


// core abstract class for VRCPhysBone related things
// this class set exists for data loss prevention

namespace uk.novavoidhowl.dev.vrcstub
{
  public abstract class VRCPhysBoneBase : MonoBehaviour
  {
    #region Enums
    public enum Version
    {
      [InspectorName("Version 1.0")]
      Version_1_0,
      [InspectorName("Version 1.1")]
      Version_1_1
    }

    public enum AdvancedBool
    {
      False,
      True,
      Other
    }

    [Serializable]
    public struct PermissionFilter
    {
      public bool allowSelf;

      public bool allowOthers;

      public PermissionFilter(bool value)
      {
        allowSelf = value;
        allowOthers = value;
      }

      public bool IsAllowed(AdvancedBool mainSetting, int idA, int idB)
      {
        switch (mainSetting)
        {
        default:
          return false;
        case AdvancedBool.True:
          return true;
        case AdvancedBool.Other:
          if (idA == idB)
          {
            return allowSelf;
          }
          return allowOthers;
        }
      }
    }

    public enum IntegrationType
    {
      Simplified,
      Advanced
    }

    public enum MultiChildType
    {
      Ignore,
      First,
      Average
    }

    public enum ImmobileType
    {
      [InspectorName("All Motion")]
      AllMotion,
      [InspectorName("World (Experimental)")]
      World
    }

    public enum LimitType
    {
      None,
      Angle,
      Hinge,
      Polar
    }

    #endregion

    #region  Structs

    public struct Bone
    {
      public Transform transform;

      public int parentIndex;

      public int childIndex;

      public int boneChainIndex;

      public int childCount;

      public Vector3 averageChildPos;

      public Vector3 restPosition;

      public Quaternion restRotation;

      public Vector3 restScale;

      public bool sphereCollision;

      public bool isEndBone => childCount == 0;
    }

    #endregion

    #region Properties

    public Version version;

    public static Version LatestVersion = Version.Version_1_1;

    [Tooltip("Determines how forces are applied.  Certain kinds of motion may require using a specific integration type.")]
    public IntegrationType integrationType;

    [Tooltip("The transform where this component begins.  If left blank, we assume we start at this game object.")]
    public Transform rootTransform;

    [Tooltip("List of ignored transforms that shouldn't be affected by this component.  Ignored transforms automatically include any of that transform's children.")]
    [NonReorderable]
    public List<Transform> ignoreTransforms = new List<Transform>();

    [Tooltip("Vector used to create additional bones at each endpoint of the chain. Only used if the value is non-zero.")]
    public Vector3 endpointPosition = Vector3.zero;

    [Tooltip("Determines how transforms with multiple children are handled. By default those transforms are ignored.")]
    public MultiChildType multiChildType;

    [Tooltip("Amount of force used to return bones to their rest position.")]
    [Range(0f, 1f)]
    public float pull = 0.2f;

    public AnimationCurve pullCurve;

    [Tooltip("Amount bones will wobble when trying to reach their rest position.")]
    [Range(0f, 1f)]
    public float spring = 0.2f;

    public AnimationCurve springCurve;

    [Tooltip("Amount bones will try and stay at their current orientation.")]
    [Range(0f, 1f)]
    public float stiffness = 0.2f;

    public AnimationCurve stiffnessCurve;

    [Tooltip("Amount of gravity applied to bones.  Positive value pulls bones down, negative pulls upwards.")]
    [Range(-1f, 1f)]
    public float gravity;

    public AnimationCurve gravityCurve;

    [Tooltip("Reduces gravity while bones are at their rest orientation.  Gravity will increase as bones rotate away from their rest orientation, reaching full gravity at 90 degress from rest.")]
    [Range(0f, 1f)]
    public float gravityFalloff;

    public AnimationCurve gravityFalloffCurve;

    [Tooltip("Determines how immobile is calculated.\n\nAll Motion - Reduces any motion as calculated from the root transform's parent.World - Reduces positional movement from locomotion, any movement due to animations or IK still affect bones normally.\n\n")]
    public ImmobileType immobileType;

    [Tooltip("Reduces the effect movement has on bones. The greater the value the less motion affects the chain as determined by the Immobile Type.")]
    [Range(0f, 1f)]
    public float immobile;

    public AnimationCurve immobileCurve;

    [Tooltip("Allows collision with colliders other than the ones specified on this component.  Currently the only other colliders are each player's hands as defined by their avatar.")]
    public AdvancedBool allowCollision = AdvancedBool.True;

    public PermissionFilter collisionFilter = new PermissionFilter(value: true);

    [Tooltip("Collision radius around each bone.  Used for both collision and grabbing.")]
    public float radius;

    public AnimationCurve radiusCurve;

    [Tooltip("List of colliders that specifically collide with these bones.")]
    [NonReorderable]
    public List<VRCPhysBoneColliderBase> colliders = new List<VRCPhysBoneColliderBase>();

    [Tooltip("Type of angular limit applied to each bone.")]
    public LimitType limitType;

    [Tooltip("Maximum angle each bone can rotate from its rest position.")]
    [Range(0f, 180f)]
    public float maxAngleX = 45f;

    public AnimationCurve maxAngleXCurve;

    [Tooltip("Maximum angle each bone can rotate from its rest position.")]
    [Range(0f, 90f)]
    public float maxAngleZ = 45f;

    public AnimationCurve maxAngleZCurve;

    [Tooltip("Rotates the angular limits on each axis.")]
    public Vector3 limitRotation;

    public AnimationCurve limitRotationXCurve;

    public AnimationCurve limitRotationYCurve;

    public AnimationCurve limitRotationZCurve;

    [Tooltip("Allows players to grab the bones.")]
    [WasSerializedAs("isGrabbable")]
    public AdvancedBool allowGrabbing = AdvancedBool.True;

    public PermissionFilter grabFilter = new PermissionFilter(value: true);

    [Tooltip("Allows players to pose the bones after grabbing.")]
    [WasSerializedAs("isPoseable")]
    public AdvancedBool allowPosing = AdvancedBool.True;

    public PermissionFilter poseFilter = new PermissionFilter(value: true);

    [Tooltip("When a bone is grabbed it will snap to the hand grabbing it.")]
    public bool snapToHand;

    [Tooltip("Controls how grabbed bones move.\nA value of zero results in bones using pull & spring to reach the grabbed position.\nA value of one results in bones immediately moving to the grabbed position.")]
    [Range(0f, 1f)]
    public float grabMovement = 0.5f;

    [Tooltip("Maximum amount the bones can stretch.  This value is a multiple of the original bone length.")]
    public float maxStretch;

    public AnimationCurve maxStretchCurve;

    [Tooltip("Maximum amount the bones can shrink.  This value is a multiple of the original bone length.")]
    [Range(0f, 1f)]
    public float maxSquish;

    public AnimationCurve maxSquishCurve;

    [Tooltip("The amount motion will affect the stretch/squish of the bones.  A value of zero means bones will only stretch/squish as a result of grabbing or collisions.")]
    [Range(0f, 1f)]
    public float stretchMotion;

    public AnimationCurve stretchMotionCurve;

    [Tooltip("Allows bone transforms to be animated.  Each frame bone rest position will be updated according to what was animated.")]
    public bool isAnimated;

    [Tooltip("When this component becomes disabled, the bones will automatically reset to their default rest position.")]
    public bool resetWhenDisabled;

    [Tooltip("Key-name used to provide multiple parameters to the avatar controller.")]
    public string parameter;

    public bool showGizmos = true;

    #endregion

  }


  public abstract class VRCPhysBoneColliderBase : MonoBehaviour
{
	public enum ShapeType
	{
		Sphere,
		Capsule,
		Plane
	}

	[Tooltip("Transform where this collider is placed.  If empty, we use this game object's transform.")]
	public Transform rootTransform;

	[Tooltip("Type of collision shape used by this collider.")]
	public ShapeType shapeType;

	[Tooltip("When enabled, this collider contains bones inside its bounds.")]
	public bool insideBounds;

	[Tooltip("Size of the collider extending from its origin.")]
	public float radius = 0.5f;

	[Tooltip("Height of the capsule along the Y axis.")]
	public float height = 2f;

	[Tooltip("Position offset from the root transform.")]
	public Vector3 position = Vector3.zero;

	[Tooltip("Rotation offset from the root transform.")]
	public Quaternion rotation = Quaternion.identity;

	[Tooltip("When enabled, this collider treats bones as spheres instead of capsules. This may be advantageous in situations where bones are constantly resting on colliders.  It will also be easier for colliders to pass through bones unintentionally.")]
	public bool bonesAsSpheres;

	public Vector3 axis => rotation * Vector3.up;

}

}

#endif
