using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;
using static System.IO.Path;
using static UnityEditor.AssetDatabase;
using UnityEditor;

namespace gebirgsbaerbel.Utilities
{
    internal static class Packages
    {
        static string GetGistURL(string gistID, string user)
            => $"https://gist.githubusercontent.com/{user}/{gistID}/raw/";

        internal static async Task ReplacePackagesFromGist(string id, string user)
        {
            var url = GetGistURL(id, user);
            var contents = await FileHelper.GetContents(url);
            ReplacePackageFile(contents);
        }

        internal static void InstallUnityPackage(string packageName)
        {
            UnityEditor.PackageManager.Client.Add($"com.unity.{packageName}");
        }

        internal static void ConfigureFolderAsCustomPackage(string path)
        {
            if (Directory.Exists(path))
            {
                Folders.CreateDirectories(path, "Editor", "Runtime", "Tests", "Documentation");
                string jsonTemplatePath = Combine(Application.dataPath, "Gebirgsbaerbel", "SetupUtilities", "Editor", "template-package.json");
                string jsonPath = Combine(path, "package.json");
                File.Copy(jsonTemplatePath, jsonPath, false);
            }
            else
            {
                Debug.LogWarning("You selected also a file and not only folders. Only the folders were made into UnityPackages.");
                Debug.LogWarning("You selected: " + path);
            }
            Refresh();
        }

        /// <summary>
        /// Replace the manifest.json file with new content.
        /// This file defines what packages are in the Unity Project.
        /// </summary>
        /// <param name="contents"></param>
        static void ReplacePackageFile(string contents)
        {
            var existing = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            File.WriteAllText(existing, contents);
            UnityEditor.PackageManager.Client.Resolve();
        }

    }
    
}

