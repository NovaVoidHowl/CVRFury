#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VF.Model;
using uk.novavoidhowl.dev.cvrfury.hierarchy;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;

namespace uk.novavoidhowl.dev.cvrfury.validators
{
  /// <summary>
  /// Utility class providing common validation methods for CVRFury components
  /// </summary>
  public static class ValidationUtils
  {
    /// <summary>
    /// Checks if a GameObject has any validation issues
    /// </summary>
    /// <param name="obj">GameObject to check</param>
    /// <returns>The highest severity issue type found</returns>
    public static IssueType GetGameObjectIssueLevel(GameObject obj)
    {
      if (obj == null)
        return IssueType.None;

      // Check for VRC stub components first (these are errors)
      if (HasVRCStubComponents(obj))
        return IssueType.Error;

      // Check for other warning-level issues
      if (HasCVRAvatarIssues(obj) || HasVRCFuryIssues(obj) || HasMissingComponents(obj))
        return IssueType.Warning;

      return IssueType.None;
    }

    /// <summary>
    /// Checks if a GameObject has VRC stub components that should be removed
    /// </summary>
    /// <param name="obj">GameObject to check</param>
    /// <returns>True if VRC stub components are found</returns>
    public static bool HasVRCStubComponents(GameObject obj)
    {
      if (obj == null)
        return false;

      var components = obj.GetComponents<Component>();
      foreach (var component in components)
      {
        if (component == null)
          continue;

        string typeName = component.GetType().FullName;
        if (Constants.VRCSTUB_COMPONENTS_TO_REMOVE.Contains(typeName))
          return true;
      }

      return false;
    }

    /// <summary>
    /// Checks if a GameObject has missing or null components
    /// </summary>
    /// <param name="obj">GameObject to check</param>
    /// <returns>True if missing components are found</returns>
    public static bool HasMissingComponents(GameObject obj)
    {
      if (obj == null)
        return false;

      var components = obj.GetComponents<Component>();
      foreach (var component in components)
      {
        if (component == null)
          return true;
      }

      return false;
    }

    /// <summary>
    /// Checks if a GameObject with CVRAvatar has configuration issues
    /// </summary>
    /// <param name="obj">GameObject to check</param>
    /// <returns>True if CVRAvatar issues are found</returns>
    public static bool HasCVRAvatarIssues(GameObject obj)
    {
      if (obj == null)
        return false;

      var cvrAvatarType = System.Type.GetType("ABI.CCK.Components.CVRAvatar, Assembly-CSharp");
      if (cvrAvatarType == null)
        return false;

      var cvrAvatar = obj.GetComponent(cvrAvatarType);
      if (cvrAvatar == null)
        return false;

      // Check body mesh
      var bodyMeshProperty = cvrAvatarType.GetField("bodyMesh");
      if (bodyMeshProperty != null)
      {
        var bodyMeshValue = bodyMeshProperty.GetValue(cvrAvatar);
        if (bodyMeshValue == null)
          return true;
      }

      // Check Animator component
      var animator = obj.GetComponent<Animator>();
      if (animator == null || animator.avatar == null)
        return true;

      return false;
    }

    /// <summary>
    /// Checks if VRCFury components on a GameObject have issues
    /// </summary>
    /// <param name="obj">GameObject to check</param>
    /// <returns>True if VRCFury issues are found</returns>
    public static bool HasVRCFuryIssues(GameObject obj)
    {
      if (obj == null)
        return false;

      var vrcFuryComponents = obj.GetComponents<VRCFury>();
      if (vrcFuryComponents == null || vrcFuryComponents.Length == 0)
        return false;

      foreach (var vrcFury in vrcFuryComponents)
      {
        if (vrcFury == null)
          continue;

        var serializedObject = new SerializedObject(vrcFury);
        var configProperty = serializedObject.FindProperty("config");
        if (configProperty == null)
          return true;

        // Check if the component has any features
        var featuresProperty = configProperty.FindPropertyRelative("features");
        if (featuresProperty != null && featuresProperty.arraySize == 0)
          return true;
      }

      return false;
    }

    /// <summary>
    /// Gets all VRC stub components on a GameObject
    /// </summary>
    /// <param name="obj">GameObject to check</param>
    /// <returns>List of VRC stub components</returns>
    public static List<Component> GetVRCStubComponents(GameObject obj)
    {
      var stubs = new List<Component>();
      if (obj == null)
        return stubs;

      var components = obj.GetComponents<Component>();
      foreach (var component in components)
      {
        if (component == null)
          continue;

        string typeName = component.GetType().FullName;
        if (Constants.VRCSTUB_COMPONENTS_TO_REMOVE.Contains(typeName))
          stubs.Add(component);
      }

      return stubs;
    }

    /// <summary>
    /// Gets all missing components on a GameObject
    /// </summary>
    /// <param name="obj">GameObject to check</param>
    /// <returns>Count of missing components</returns>
    public static int GetMissingComponentCount(GameObject obj)
    {
      if (obj == null)
        return 0;

      int count = 0;
      var components = obj.GetComponents<Component>();
      foreach (var component in components)
      {
        if (component == null)
          count++;
      }

      return count;
    }

    /// <summary>
    /// Recursively scans a GameObject and its children for issues
    /// </summary>
    /// <param name="root">Root GameObject to start scanning from</param>
    /// <param name="includeWarnings">Whether to include warning-level issues</param>
    /// <returns>Dictionary mapping GameObjects to their issue types</returns>
    public static Dictionary<GameObject, IssueType> ScanHierarchy(GameObject root, bool includeWarnings = true)
    {
      var results = new Dictionary<GameObject, IssueType>();
      if (root == null)
        return results;

      ScanHierarchyRecursive(root, results, includeWarnings);
      return results;
    }

    private static void ScanHierarchyRecursive(GameObject obj, Dictionary<GameObject, IssueType> results, bool includeWarnings)
    {
      if (obj == null)
        return;

      var issueType = GetGameObjectIssueLevel(obj);
      if (issueType != IssueType.None && (includeWarnings || issueType == IssueType.Error))
      {
        results[obj] = issueType;
      }

      // Recursively scan children
      for (int i = 0; i < obj.transform.childCount; i++)
      {
        var child = obj.transform.GetChild(i).gameObject;
        ScanHierarchyRecursive(child, results, includeWarnings);
      }
    }

    /// <summary>
    /// Gets a human-readable description of the primary issue with a GameObject
    /// </summary>
    /// <param name="obj">GameObject to describe</param>
    /// <returns>String description of the primary issue</returns>
    public static string GetPrimaryIssueDescription(GameObject obj)
    {
      if (obj == null)
        return "GameObject is null";

      // Check for VRC stub components first (highest priority)
      var stubs = GetVRCStubComponents(obj);
      if (stubs.Count > 0)
      {
        return $"Contains {stubs.Count} VRC stub component(s): {string.Join(", ", stubs.ConvertAll(c => c.GetType().Name))}";
      }

      // Check for missing components
      int missingCount = GetMissingComponentCount(obj);
      if (missingCount > 0)
      {
        return $"Has {missingCount} missing component reference(s)";
      }

      // Check for CVRAvatar issues
      if (HasCVRAvatarIssues(obj))
      {
        return "CVRAvatar component has configuration issues";
      }

      // Check for VRCFury issues
      if (HasVRCFuryIssues(obj))
      {
        return "VRCFury component has configuration issues";
      }

      return "No issues detected";
    }
  }
}

#endif