#if UNITY_EDITOR

using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.generator
{
  public class EnableDisableAnimCreator
  {
    public static bool CreateAnimations(
      GameObject targetGameObject,
      GameObject rootGameObject,
      string animationName,
      string animationPath,
      out string errorMessage
    )
    {
      errorMessage = string.Empty;

      // Validate inputs
      if (targetGameObject == null || rootGameObject == null)
      {
        errorMessage = "Both Target and Root GameObjects must be set";
        return false;
      }

      if (string.IsNullOrEmpty(animationPath))
      {
        errorMessage = "Animation path is required";
        return false;
      }

      // Create directory if needed
      if (!Directory.Exists(animationPath))
      {
        try
        {
          Directory.CreateDirectory(animationPath);
        }
        catch (System.Exception ex)
        {
          errorMessage = $"Failed to create directory: {ex.Message}";
          return false;
        }
      }

      try
      {
        // Create the animation clips
        AnimationClip enableClip = new AnimationClip();
        AnimationClip disableClip = new AnimationClip();

        enableClip.name = $"{animationName}_enable";
        disableClip.name = $"{animationName}_disable";

        // Get relative path between root and target
        string targetPath = GetGameObjectPath(targetGameObject);
        string rootPath = GetGameObjectPath(rootGameObject);
        string relativePath = GetRelativePath(targetPath, rootPath);

        // Create curve binding
        EditorCurveBinding curveBinding = new EditorCurveBinding
        {
          path = relativePath,
          propertyName = "m_IsActive",
          type = typeof(GameObject)
        };

        // Create curves
        AnimationUtility.SetEditorCurve(enableClip, curveBinding, AnimationCurve.Constant(0, 0, 1));
        AnimationUtility.SetEditorCurve(disableClip, curveBinding, AnimationCurve.Constant(0, 0, 0));

        // Save clips
        string enablePath = Path.Combine(animationPath, $"{enableClip.name}.anim").Replace("\\", "/");
        string disablePath = Path.Combine(animationPath, $"{disableClip.name}.anim").Replace("\\", "/");

        AssetDatabase.CreateAsset(enableClip, enablePath);
        AssetDatabase.CreateAsset(disableClip, disablePath);
        AssetDatabase.Refresh();

        return true;
      }
      catch (System.Exception ex)
      {
        errorMessage = $"Failed to create animations: {ex.Message}";
        return false;
      }
    }

    private static string GetRelativePath(string targetPath, string rootPath)
    {
      string[] targetSegments = targetPath.Split('/');
      string[] rootSegments = rootPath.Split('/');
      List<string> relativeSegments = new List<string>();
      bool foundRoot = false;

      foreach (string segment in targetSegments)
      {
        if (foundRoot)
        {
          relativeSegments.Add(segment);
        }
        else if (segment == rootSegments[rootSegments.Length - 1])
        {
          foundRoot = true;
        }
      }

      return string.Join("/", relativeSegments);
    }
  }
}
#endif
