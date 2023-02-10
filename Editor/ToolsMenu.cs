using UnityEngine;
using UnityEditor;
using static UnityEditor.AssetDatabase;

namespace gebirgsbaerbel.Utilities
{

    public static class ToolsMenu
    {
        [MenuItem("Tools/Setup/Create Default Folders", priority = 0)]
        static void CreateDefaultFolders()
        {
            Folders.CreateDirectories("Assets", "Scripts", "Animations", "Prefabs", "Scenes");
            Refresh();
        }

        [MenuItem("Tools/Load Minimal URP Manifest")]
        static async void LoadNewManifest()
        {
            await Packages.ReplacePackagesFromGist("b84c041d1f138e7a394a3375d55940ef", "gebirgsbaerbel");
        }

        [MenuItem("Tools/Packages/Terrain + TerrainPhysics")]
        static void AddTerrain() {
            Packages.InstallUnityPackage("terrain");
            Packages.InstallUnityPackage("terrainphysics");
        }


        [MenuItem("Tools/Packages/Tilemap")]
        static void AddTilemap() => Packages.InstallUnityPackage("tilemap");

        [MenuItem("Tools/Packages/ProBuilder")]
        static void AddProBuilder() => Packages.InstallUnityPackage("probuilder");

    }
    
}

