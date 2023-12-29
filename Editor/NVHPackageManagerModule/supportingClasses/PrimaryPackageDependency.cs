using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace uk.novavoidhowl.dev.nvhpmm
{
  public sealed class PrimaryPackageDependency
  {
    public string Name { get; }
    public string Version { get; }
    public string Description { get; }
    public string InstalledVersion { get; set; }

    public List<DependencyLinkButton> Buttons { get; set; }

    public PrimaryPackageDependency(string name, string version, string description)
    {
      Name = name;
      Version = version;
      Description = description;
      InstalledVersion = GetInstalledVersion(name);
      Buttons = new List<DependencyLinkButton>();
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
