using System.IO;
using System.Text.RegularExpressions;

namespace uk.novavoidhowl.dev.nvhpmm
{
  public sealed class PrimaryPackageDependency
  {
    public string Name { get; }
    public string Version { get; }
    public string InstalledVersion { get; set; }

    public PrimaryPackageDependency(string name, string version)
    {
      Name = name;
      Version = version;
      InstalledVersion = GetInstalledVersion(name);
    }

    private string GetInstalledVersion(string packageName)
    {
      string manifestPath = "Packages/manifest.json";
      string manifestContent = File.ReadAllText(manifestPath);
      string pattern = $"\"{packageName}\": \"([^\"]+)\"";

      Match match = Regex.Match(manifestContent, pattern);
      return match.Success ? match.Groups[1].Value : "Not installed";
    }
  }
}
