using System.IO;
using UnityEditor;

// only need to change this line (and the asmdef) to bind to project specific constants
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;

namespace uk.novavoidhowl.dev.nvhpmm
{
  [InitializeOnLoad]
  public class AppInternalPackages
  {
    static AppInternalPackages()
    {
      EditorApplication.delayCall += refreshAppComponentsList;
    }

    public static void refreshAppComponentsList()
    {
      EditorApplication.delayCall -= refreshAppComponentsList;

      // empty the list
      SharedData.appComponentsList.Clear();
      // look at all all the files in the appComponents folder under Resources and add them to the list if they are .source files
      string[] files = Directory.GetFiles(
        "Packages/"
          + Constants.PACKAGE_NAME
          + "/Assets/Resources/"
          + Constants.PROGRAM_DISPLAY_NAME
          + "/nvhpmm/AppComponents/Editor/",
        "*.source"
      );

      foreach (string file in files)
      {
        // get the filename without the extension
        string fileName = Path.GetFileNameWithoutExtension(file);
        // add the filename to the list
        SharedData.appComponentsList.Add(fileName);
      }
    }
  }
}
