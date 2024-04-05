using System.IO;

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class PostProcessIOS : MonoBehaviour
{
    // Must be between 40 and 50 to ensure that it's not overriden by Podfile generation (40) and
    // that it's added before "pod install" (50).
    [PostProcessBuildAttribute(45)]
    private static void PostProcessBuild_iOS(BuildTarget target, string buildPath)
    {
        if (target == BuildTarget.iOS)
        {
            // pod file configuration
            string path = Path.Combine(buildPath, "Podfile");
            using StreamWriter sw = File.AppendText(path);
            if (!File.ReadAllText(path).Contains("Unity-iPhone"))
            {
                sw.WriteLine("platform :ios, '12.0'");
                sw.WriteLine("\n");
                sw.WriteLine("target 'UnityFramework' do");
                sw.WriteLine("  pod 'GPUUtilization', :configurations => ['Debug']");
                sw.WriteLine("end");
                sw.WriteLine("\n");
                sw.WriteLine("target 'Unity-iPhone' do");
                sw.WriteLine("end");
                sw.WriteLine("\n");
                sw.WriteLine("use_frameworks! :linkage => :static");
            }
        }
    }
}