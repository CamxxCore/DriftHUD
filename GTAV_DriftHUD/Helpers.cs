using System.Collections.Generic;
using System.IO;

public static class Helpers
    {
    /// <summary>
    /// Creates a new configuration file. If one exists, it will be deleted
    /// </summary>
    public static void CreateConfig()
    {
        string path = "scripts\\GTAV_DriftHUD.ini";
        if (File.Exists(path)) File.Delete(path);
        Logger.Log("Creating configuration file...");
        IList<string> list = Utils.RetrieveInternalResource(GTAV_DriftHUD.Properties.Resources.GTAV_DriftHUD);
        Utils.WriteListToFile(list, path);
    }
}
