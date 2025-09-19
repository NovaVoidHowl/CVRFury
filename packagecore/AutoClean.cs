// This file is here to execute auto cleaning tasks on projects, such as removing legacy/unused files.
// It executes on startup
// User is notified if any files were removed, but not asked for confirmation.
// This is to support breaking changes, to the structure of the package, that would otherwise cause confusion/issues.

//this whole file is editor only
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using uk.novavoidhowl.dev.cvrfury.packagecore;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.packagecore
{
  [InitializeOnLoad]
  public class AutoClean
  {
    //list of files to clean up, theses are all relative to the Assets folder
    private static readonly string[] LIST_OF_FILES_TO_CLEAN_UP =
    {
      "_CVRFury/Editor/CVRFury_VRCComponentHandler.cs",
      "_CVRFury/Editor/CVRFury_VRCContactConverter.cs"
    };

    //static constructor, runs on load
    static AutoClean()
    {
      //run the cleanup
      RunAutoClean();
    }

    private static void RunAutoClean()
    {
      List<string> filesRemoved = new List<string>();

      foreach (string relativeFilePath in LIST_OF_FILES_TO_CLEAN_UP)
      {
        string fullPath = Path.Combine(Application.dataPath, relativeFilePath);
        if (File.Exists(fullPath))
        {
          try
          {
            File.Delete(fullPath);
            filesRemoved.Add(relativeFilePath);
          }
          catch (Exception e)
          {
            CoreLogCritical($"[CVRFury] Failed to delete legacy file: {relativeFilePath}. Exception: {e.Message}");
          }
        }
      }

      if (filesRemoved.Count > 0)
      {
        //refresh the asset database to reflect the changes
        AssetDatabase.Refresh();

        //notify the user
        string message = "[CVRFury] The following legacy files have been removed:\n" + string.Join("\n", filesRemoved);
        EditorUtility.DisplayDialog("CVRFury - Cleanup Complete", message, "OK");
      }
    }
  }
}
#endif //UNITY_EDITOR
