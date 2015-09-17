using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;

public static class Utils
{
    /// <summary>
    /// Extension for getting a random item from a list
    /// </summary>
    /// <typeparam name="T">Type of list</typeparam>
    /// <param name="items">Target list</param>
    /// <returns></returns>
    public static T GetRandomItem<T>(this IEnumerable<T> items)
    {
        var random = new Random();
        return (T)(object)items.ToList<T>()[random.Next(0, items.Count())];
    }

    /// <summary>
    /// Extension to fade between colors
    /// </summary>
    /// <param name="source">The color to fade from</param>
    /// <param name="target">The color to fade to</param>
    /// <param name="percent">Percentage of fade</param>
    /// <returns></returns>
    public static Color Interpolate(this Color source, Color target, double percent)
    {
        var r = (byte)(source.R + (target.R - source.R) * percent);
        var g = (byte)(source.G + (target.G - source.G) * percent);
        var b = (byte)(source.B + (target.B - source.B) * percent);

        return Color.FromArgb(source.A, r, g, b);
    }

    /// <summary>
    /// Write a list of strings to a file at the specified path
    /// </summary>
    /// <param name="list">The list to write</param>
    /// <param name="filepath">The specified path</param>
    public static void WriteListToFile(IList<string> list, string filepath)
    {
        if (File.Exists(filepath)) File.Delete(filepath);
        using (StreamWriter stream = new StreamWriter(filepath))
        {
            foreach (string line in list)
            {
                stream.WriteLine(line);
            }
        }
    }

    /// <summary>
    /// Populates a list of strings from an embedded string resource
    /// </summary>
    /// <param name="resource">Resource Path (Properties.Resources.ProjectName...)</param>
    /// <returns></returns>
    public static IList<string> RetrieveInternalResource(string resource)
    {
        string[] text = resource.GetLines();
        return new List<string>(text);
    }

    /// <summary>
    /// Concatenate an array of strings with each member on a new line
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string[] GetLines(this string s)
    {
        return s.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
    }
}