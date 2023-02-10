using static System.IO.Path;
using Debug = UnityEngine.Debug;
using UnityEditor;

namespace gebirgsbaerbel.Utilities
{
    internal static class Folders
    {
        internal static void CreateDirectories(string root, params string[] dir)
        {
            foreach (var newDirectory in dir)
            {
                string path = Combine(root, newDirectory);
                if (AssetDatabase.IsValidFolder(path))
                {
                    Debug.Log("Subdirectory already exists: " + path);
                    continue;
                }

                var createdFolderGUID = AssetDatabase.CreateFolder(root, newDirectory);
                if (string.IsNullOrEmpty(createdFolderGUID))
                {
                    Debug.LogError("Error creating directory " + newDirectory);
                    continue;
                }
                Debug.Log("Subdirectory created: " + newDirectory + " in " + root);
            }
        }

    }
    
}

