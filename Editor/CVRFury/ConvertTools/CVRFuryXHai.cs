// Accreditation
// This code is based upon the prefabulous-for-conversions by Hai-Vr
// (https://github.com/hai-vr/prefabulous-for-conversions), which is licensed under the MIT License.

//MIT License
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// editor only script to manage the dependencies
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using Object = UnityEngine.Object;

using uk.novavoidhowl.dev.vrcstub;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;
using uk.novavoidhowl.dev.cvrfury.packagecore;

namespace uk.novavoidhowl.dev.cvrfury.converttools
{
  public static class CVRFuryXHai
  {
    public static void ConvertPrefabBackToUnityConstraints(string pathToPrefab)
    {
      // processing lock file
      string lockFilePath = pathToPrefab+"_ConvertPrefabBackToUnityConstraints.lock";
      File.Create(lockFilePath).Dispose();

      // Load the prefab
      var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(pathToPrefab);
      // Check the prefab is not null
      if (prefab == null)
      {
        CoreLogDebug($"Could not load prefab at path {pathToPrefab}");
        return;
      }
      // Create an instance of the prefab
      var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

      // Check the instance is not null
      if (instance == null)
      {
        CoreLogDebug($"Could not instantiate prefab at path {pathToPrefab}");
        return;
      }

      // Get all VRC Constraints in the instance (anything based on VRCConstraintBase class), including those on disabled GameObjects
      var foundConstraints = instance
        .GetComponentsInChildren<Component>(true)
        .Where(c => IsVrcConstraintOrConstraintStub(c.GetType()))
        .ToArray();

      CoreLogDebug($"Found {foundConstraints.Length} constraints in {pathToPrefab}");

      if (foundConstraints.Length == 0)
      {
        // Destroy the instantiated prefab instance
        Object.DestroyImmediate(instance);
        // remove the lock file
        File.Delete(lockFilePath);
        // No constraints found, nothing to do
        return;
      }

      foreach (var fromConstraint in foundConstraints)
      {
        var fromSerialized = new SerializedObject(fromConstraint);
        var fromTargetTransformNullable =
          fromSerialized.FindProperty("TargetTransform").objectReferenceValue as Transform;

        var whereToAddItTo =
          fromTargetTransformNullable != null ? fromTargetTransformNullable.gameObject : fromConstraint.gameObject;
        var toConstraint = whereToAddItTo.AddComponent(ToType(fromConstraint.GetType()));

        var toSerialized = new SerializedObject(toConstraint);

        if (false)
        {
          var toIterator = toSerialized.GetIterator();
          bool enterChildren = true;
          while (toIterator.Next(enterChildren))
          {
            enterChildren = true;
            var path = toIterator.propertyPath;
            CoreLogDebug("found:" + path);
          }
        }

        // Found in Aim Constraint first
        CopyVerbatim(fromSerialized, toSerialized, "m_Enabled");
        Copy(fromSerialized, toSerialized, "GlobalWeight", "m_Weight");

        Copy(fromSerialized, toSerialized, "RotationAtRest", "m_RotationAtRest");
        Copy(fromSerialized, toSerialized, "RotationOffset", "m_RotationOffset");
        Copy(fromSerialized, toSerialized, "AimAxis", "m_AimVector");
        Copy(fromSerialized, toSerialized, "UpAxis", "m_UpVector");
        Copy(fromSerialized, toSerialized, "WorldUpVector", "m_WorldUpVector");
        Copy(fromSerialized, toSerialized, "WorldUpTransform", "m_WorldUpObject");
        Copy(fromSerialized, toSerialized, "WorldUp", "m_UpType");
        Copy(fromSerialized, toSerialized, "AffectsRotationX", "m_AffectRotationX");
        Copy(fromSerialized, toSerialized, "AffectsRotationY", "m_AffectRotationY");
        Copy(fromSerialized, toSerialized, "AffectsRotationZ", "m_AffectRotationZ");
        Copy(fromSerialized, toSerialized, "IsActive", "m_IsContraintActive");
        //NOTE: unity 2021 uses m_IsContraintActive not m_IsActive
        Copy(fromSerialized, toSerialized, "Locked", "m_IsLocked");

        // Found in Look At Constraint first
        Copy(fromSerialized, toSerialized, "UseUpTransform", "m_UseUpObject");
        Copy(fromSerialized, toSerialized, "Roll", "m_Roll");

        // Found in Position Constraint first
        Copy(fromSerialized, toSerialized, "PositionAtRest", "m_TranslationAtRest");
        Copy(fromSerialized, toSerialized, "PositionOffset", "m_TranslationOffset");
        Copy(fromSerialized, toSerialized, "AffectsPositionX", "m_AffectTranslationX");
        Copy(fromSerialized, toSerialized, "AffectsPositionY", "m_AffectTranslationY");
        Copy(fromSerialized, toSerialized, "AffectsPositionZ", "m_AffectTranslationZ");

        // Found in Scale Constraint first
        Copy(fromSerialized, toSerialized, "ScaleAtRest", "m_ScaleAtRest");
        Copy(fromSerialized, toSerialized, "ScaleOffset", "m_ScaleOffset");
        Copy(fromSerialized, toSerialized, "AffectsScaleX", "m_AffectScalingX");
        Copy(fromSerialized, toSerialized, "AffectsScaleY", "m_AffectScalingY");
        Copy(fromSerialized, toSerialized, "AffectsScaleZ", "m_AffectScalingZ");

        // Found in all
        var fromSources = fromSerialized.FindProperty("Sources");
        var toSources = toSerialized.FindProperty("m_Sources");

        var deconstructedSources = DeconstructSources(fromSources);
        CoreLogDebug($"Deconstructed {deconstructedSources.Count} sources");

        toSources.arraySize = deconstructedSources.Count;
        for (var index = 0; index < deconstructedSources.Count; index++)
        {
          var deconstructed = deconstructedSources[index];

          var toElement = toSources.GetArrayElementAtIndex(index);
          toElement.FindPropertyRelative("sourceTransform").objectReferenceValue = deconstructed.transform;
          toElement.FindPropertyRelative("weight").floatValue = deconstructed.weight;
        }

        // Found only in Parent Constraint
        if (toConstraint is ParentConstraint)
        {
          var toTranslationOffsets = toSerialized.FindProperty("m_TranslationOffsets");
          var toRotationOffsets = toSerialized.FindProperty("m_RotationOffsets");
          toTranslationOffsets.arraySize = deconstructedSources.Count;
          toRotationOffsets.arraySize = deconstructedSources.Count;
          for (var index = 0; index < deconstructedSources.Count; index++)
          {
            var deconstructed = deconstructedSources[index];

            toTranslationOffsets.GetArrayElementAtIndex(index).vector3Value = deconstructed.parentPositionOffset;
            toRotationOffsets.GetArrayElementAtIndex(index).vector3Value = deconstructed.parentRotationOffset;
          }
        }

        toSerialized.ApplyModifiedPropertiesWithoutUndo();
      }

      foreach (var foundConstraint in foundConstraints)
      {
        Object.DestroyImmediate(foundConstraint);
      }

      // mark the prefab as dirty
      EditorUtility.SetDirty(instance);

      // Force Asset Database to save and refresh
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();


      // Apply changes to the prefab instance
      PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.UserAction);

      // nuke the prefab's .meta file
      File.Delete(pathToPrefab+".meta");

      // Force Asset Database to save and refresh
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();

      // Add a small delay to ensure the save operation is complete
      System.Threading.Thread.Sleep(100);


      // Destroy the instantiated prefab instance
      Object.DestroyImmediate(instance);

      // remove the lock file
      File.Delete(lockFilePath);
      // remove the meta file
      File.Delete(lockFilePath+".meta");

      CoreLogDebug($"Locked file removed: {lockFilePath}");
    }

    #region Supporting Structs
    private struct DeconstructedSource
    {
      public Transform transform;
      public float weight;
      public Vector3 parentPositionOffset;
      public Vector3 parentRotationOffset;
    }

    #endregion  // Supporting Structs

    #region Constants
    private static readonly HashSet<string> _candidates = new HashSet<string>
    {
      "uk.novavoidhowl.dev.vrcstub.VRCAimConstraint",
      "uk.novavoidhowl.dev.vrcstub.VRCLookAtConstraint",
      "uk.novavoidhowl.dev.vrcstub.VRCParentConstraint",
      "uk.novavoidhowl.dev.vrcstub.VRCPositionConstraint",
      "uk.novavoidhowl.dev.vrcstub.VRCRotationConstraint",
      "uk.novavoidhowl.dev.vrcstub.VRCScaleConstraint"
    };
    #endregion  // Constants

    #region  Supporting Functions
    private static Type ToType(Type getType)
    {
      switch (getType.FullName)
      {
        case "uk.novavoidhowl.dev.vrcstub.VRCAimConstraint":
          return typeof(AimConstraint);
        case "uk.novavoidhowl.dev.vrcstub.VRCLookAtConstraint":
          return typeof(LookAtConstraint);
        case "uk.novavoidhowl.dev.vrcstub.VRCParentConstraint":
          return typeof(ParentConstraint);
        case "uk.novavoidhowl.dev.vrcstub.VRCPositionConstraint":
          return typeof(PositionConstraint);
        case "uk.novavoidhowl.dev.vrcstub.VRCRotationConstraint":
          return typeof(RotationConstraint);
        case "uk.novavoidhowl.dev.vrcstub.VRCScaleConstraint":
          return typeof(ScaleConstraint);
        default:
          throw new ArgumentException($"Unknown type {getType.FullName}");
      }
    }

    private static bool IsVrcConstraintOrConstraintStub(Type type)
    {
      return _candidates.Contains(type.FullName);
    }

    private static List<DeconstructedSource> DeconstructSources(SerializedProperty fromSources)
    {
      if (fromSources == null)
      {
        CoreLogDebug("fromSources is null");
        return new List<DeconstructedSource>();
      }

      var totalLengthProperty = fromSources.FindPropertyRelative("totalLength");
      if (totalLengthProperty == null)
      {
        CoreLogDebug("totalLength property is null or no sources are set");
        return new List<DeconstructedSource>();
      }
      var totalLength = totalLengthProperty.intValue;

      var overflow = fromSources.FindPropertyRelative("overflowList");
      if (overflow == null)
      {
        CoreLogDebug("overflow property is null");
        return new List<DeconstructedSource>();
      }
      var overflowLength = overflow.arraySize;

      const int maxKeyable = 16;
      var deconstructedSources = new List<DeconstructedSource>();
      for (var i = 0; i < totalLength; i++)
      {
        if (i < maxKeyable)
        {
          var sourceProperty = fromSources.FindPropertyRelative($"source{i}");
          if (sourceProperty == null)
          {
            CoreLogDebug($"source{i} property is null");
            continue;
          }
          deconstructedSources.Add(Deconstruct(sourceProperty));
        }
        else
        {
          var overflowIndex = i - maxKeyable;
          if (overflowIndex < overflowLength)
          {
            var overflowElement = overflow.GetArrayElementAtIndex(overflowIndex);
            if (overflowElement == null)
            {
              CoreLogDebug($"overflow element at index {overflowIndex} is null");
              continue;
            }
            deconstructedSources.Add(Deconstruct(overflowElement));
          }
        }
      }

      return deconstructedSources;
    }

    private static DeconstructedSource Deconstruct(SerializedProperty source)
    {
      return new DeconstructedSource
      {
        transform = source.FindPropertyRelative("SourceTransform").objectReferenceValue as Transform,
        weight = source.FindPropertyRelative("Weight").floatValue,
        parentPositionOffset = source.FindPropertyRelative("ParentPositionOffset").vector3Value,
        parentRotationOffset = source.FindPropertyRelative("ParentRotationOffset").vector3Value
      };
    }

    private static void CopyVerbatim(
      SerializedObject fromSerialized,
      SerializedObject toSerialized,
      string propertyPath
    )
    {
      Copy(fromSerialized, toSerialized, propertyPath, propertyPath);
    }

    private static void Copy(
      SerializedObject fromSerialized,
      SerializedObject toSerialized,
      string fromPropertyPath,
      string toPropertyPath
    )
    {
      var fromProperty = fromSerialized.FindProperty(fromPropertyPath);
      var toProperty = toSerialized.FindProperty(toPropertyPath);
      if (toProperty == null)
      {
        return;
      }
      CopyProperty(fromProperty, toProperty);
    }

    private static void CopyInsideElement(
      SerializedProperty fromElement,
      SerializedProperty toElement,
      string fromPropertyPath,
      string toPropertyPath
    )
    {
      var fromProperty = fromElement.FindPropertyRelative(fromPropertyPath);
      var toProperty = toElement.FindPropertyRelative(toPropertyPath);
      if (toProperty == null)
      {
        return;
      }
      CopyProperty(fromProperty, toProperty);
    }

    private static void CopyProperty(SerializedProperty fromProperty, SerializedProperty toProperty)
    {
      switch (toProperty.propertyType)
      {
        case SerializedPropertyType.Boolean:
          toProperty.boolValue = fromProperty.boolValue;
          break;
        case SerializedPropertyType.Float:
          toProperty.floatValue = fromProperty.floatValue;
          break;
        case SerializedPropertyType.Quaternion:
          toProperty.quaternionValue = fromProperty.quaternionValue;
          break;
        case SerializedPropertyType.Vector3:
          toProperty.vector3Value = fromProperty.vector3Value;
          break;
        case SerializedPropertyType.ObjectReference:
          toProperty.objectReferenceValue = fromProperty.objectReferenceValue;
          break;
        case SerializedPropertyType.Integer:
          toProperty.intValue = fromProperty.intValue;
          break;
        default:
          throw new ArgumentOutOfRangeException($"Unsupported: {toProperty.propertyType}");
      }
    }

    #endregion  // Supporting Functions
  }
}

#endif
