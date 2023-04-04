using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Reflection;
using static System.IO.Path;
using Debug = UnityEngine.Debug;

// Read all template files and copy them into the project, if they are not in the correct folder yet.
[InitializeOnLoad]
public class ScriptTemplateAdder
{

    /// <summary>
    /// Add a set of custom code templates.
    /// </summary>
    static ScriptTemplateAdder()
    {
        CopyTemplates();
    }

    static void CopyTemplates()
    {
        string targetFolder = Combine("Assets", "ScriptTemplates");
        if (!AssetDatabase.IsValidFolder(targetFolder))
        {
            AssetDatabase.CreateFolder("Assets", "ScriptTemplates");
        }
        
        var templateFolder = GetFullPath("Packages/com.gebirgsbaerbel.setup-utilities/Assets/ScriptTemplates");
        var templateFiles = Directory.EnumerateFiles(templateFolder, "*.txt");
        // Copy template files if they do not exist yet.
        foreach (var filePath in templateFiles)
        {
            var fileName = GetFileName(filePath);
            var targetPath = Combine(targetFolder, fileName);
            if (!File.Exists(targetPath))
            {
                File.Copy(filePath, targetPath);
            }
        }
    }
}
