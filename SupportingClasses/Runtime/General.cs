using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.supporting_classes.runtime
{
  public static class General
  {
    public static string formatParameterNameForMachineName(string name)
    {
      // remove any spaces from the name
      return name.Replace(" ", "");
    }
  } 
}
