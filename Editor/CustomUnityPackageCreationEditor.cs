using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Debug = UnityEngine.Debug;
using static UnityEditor.AssetDatabase;
using static System.IO.Path;
using System;
using Codice.CM.WorkspaceServer.Tree.GameUI.HeadTree;

namespace gebirgsbaerbel.Utilities
{
    public class CustomUnityPackageCreationEditor : EditorWindow
    {
        [SerializeField] 
        VisualTreeAsset uiDocument;
        
        TextField displayNameTextfield;
        TextField nameTextfield;
        TextField descriptionTextfield;
        TextField organizationTextfield;
        TextField countryTextfield;
        
        IntegerField majorVersionIntegerField;
        IntegerField minorVersionIntegerField;

        TextField authorNameTextfield;
        TextField emailTextfield;

        Toggle shouldAddRuntimeFolderToggle;
        Toggle shouldAddEditorFolderToggle;
        Toggle shouldAddTestsFolderToggle;
        Toggle shouldAddDocumentationFolderToggle;
        Toggle shouldAddSamplesFolderToggle;

        [MenuItem("Tools/Setup/Create Custom Unity Package", priority = 3)]
        public static void ShowWindow()
        {
            var window = GetWindow<CustomUnityPackageCreationEditor>("CustomUnityPackageCreationEditor");
            window.titleContent = new GUIContent("Custom Unity Package Creation");
            window.minSize = new Vector2(450, 430);
        }

        /// <summary>
        /// Connects all UI elements with the corresponding variables.
        /// </summary>
        private void CreateGUI()
        {
            var treeAsset = uiDocument.CloneTree();
            rootVisualElement.Add(treeAsset);

            rootVisualElement.Query<Button>("Create").First().clicked += CreateCustomPackage;

            displayNameTextfield = rootVisualElement.Query<TextField>("DisplayName");
            nameTextfield = rootVisualElement.Query<TextField>("Name");
            descriptionTextfield = rootVisualElement.Query<TextField>("Description");
            organizationTextfield = rootVisualElement.Query<TextField>("OrganizationName");
            countryTextfield = rootVisualElement.Query<TextField>("CountryCode");

            majorVersionIntegerField = rootVisualElement.Query<IntegerField>("Major");
            minorVersionIntegerField = rootVisualElement.Query<IntegerField>("Minor");

            authorNameTextfield = rootVisualElement.Query<TextField>("AuthorName");
            emailTextfield = rootVisualElement.Query<TextField>("Email");

            shouldAddRuntimeFolderToggle = rootVisualElement.Query<Toggle>("ShouldAddRuntimeFolder");
            shouldAddEditorFolderToggle = rootVisualElement.Query<Toggle>("ShouldAddEditorFolder");
            shouldAddTestsFolderToggle = rootVisualElement.Query<Toggle>("ShouldAddTestsFolder");
            shouldAddDocumentationFolderToggle = rootVisualElement.Query<Toggle>("ShouldAddDocumentationFolder");
            shouldAddSamplesFolderToggle = rootVisualElement.Query<Toggle>("ShouldAddSamplesFolder");

            // Automatically update the field content for name whenever the display name field is changed.
            // This way the display name can be changed to whatever the user wants and the name for the package
            // will match it automatically. This is done to conform to package naming conventions.
            displayNameTextfield.RegisterValueChangedCallback(e =>
            {
                var newValue = e.newValue;
                // Remove spaces in front and back, make lowercase and
                // replace space with hyphen to conform to package naming conventions.
                newValue = newValue.Trim().ToLower().Replace(" ", "-");
                nameTextfield.value = newValue;
            });
        }

        /// <summary>
        /// Take user input and automatically generate a custom package for Unity that can be imported into another project.
        /// Generates package.json and mandatory folders as well as any optional folders that were selected.
        /// Empty files are created for the README, LICENSE and CHANGELOG.
        /// </summary>
        private void CreateCustomPackage()
        {
            var packageInfo = GetPackageInfo();
            if (packageInfo == null)
            {
                return;
            }

            var shouldAddRuntimeFolder = shouldAddRuntimeFolderToggle.value;
            var shouldAddEditorFolder = shouldAddEditorFolderToggle.value;
            var shouldAddTestsFolder = shouldAddTestsFolderToggle.value;
            var shouldAddDocumentationFolder = shouldAddDocumentationFolderToggle.value;
            var shouldAddSamplesFolder = shouldAddSamplesFolderToggle.value;

            // Create Package folder
            var rootPath = Combine("Assets", packageInfo.organization);
            var packagePath = Combine(rootPath, packageInfo.displayName);

            Folders.CreateDirectories("Assets", packageInfo.organization);
            Folders.CreateDirectories(rootPath, packageInfo.displayName);

            // Create essential package files
            CreateFile(packagePath, "README.md");
            CreateFile(packagePath, "LICENSE.md");
            CopyChangelogTemplate(packagePath);

            var dataPath = Application.dataPath;
            CreatePackageJson(dataPath, packageInfo);
            CreateFolderWithASMDEFFile(packageInfo, "Runtime", shouldAddRuntimeFolder, packagePath, dataPath);
            CreateFolderWithASMDEFFile(packageInfo, "Editor", shouldAddEditorFolder, packagePath, dataPath);
            CreateTestsFolder(packageInfo, shouldAddTestsFolder, packagePath);
            if (shouldAddDocumentationFolder)
            {
                Folders.CreateDirectories(packagePath, "Documentation~");
            }
            if (shouldAddSamplesFolder)
            {
                Folders.CreateDirectories(packagePath, "Samples~");
            }
            Refresh();
            Close();
        }

        /// <summary>
        /// Copy the template file for the package
        /// </summary>
        /// <param name="packagePath">Path of the package that is currently created.</param>
        private void CopyChangelogTemplate(string packagePath)
        {
            var filePath = GetFullPath("Packages/com.gebirgsbaerbel.setup-utilities/Assets/Changelog-Template.md");
            var targetPath = Combine(packagePath, "CHANGELOG.md");
            Debug.Log(filePath + "     " + targetPath);
            if (File.Exists(filePath))
            {
                File.Copy(filePath, targetPath);
            }
        }

        /// <summary>
        /// Gets the package info from the UI elements.
        /// </summary>
        /// <returns></returns>
        private PackageInfo GetPackageInfo()
        {
            var name = nameTextfield.value;
            var displayName = displayNameTextfield.value;
            var organization = organizationTextfield.value;
            var description = descriptionTextfield.value;
            var country = countryTextfield.value;

            var major = majorVersionIntegerField.value;
            var minor = minorVersionIntegerField.value;

            var authorName = authorNameTextfield.value;
            var email = emailTextfield.value;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(displayName))
            {
                Debug.LogError("Some required fields are empty. Make sure name and displayName are set.");
                return null;
            }

            var author = new Author(authorName, email);
            var packageInfo = new PackageInfo(name, displayName, description, organization, country, major, minor, author);

            return packageInfo;
        }

        /// <summary>
        /// Creates the Tests folder and its required subfolders.
        /// </summary>
        /// <param name="packageInfo">Name of the folder that </param>
        /// <param name="shouldAddTestsFolder">If user selected toggle to create Tests folder</param>
        /// <param name="packagePath">File path where package is positioned</param>
        private static void CreateTestsFolder(PackageInfo packageInfo, bool shouldAddTestsFolder, string packagePath)
        {
            if (!shouldAddTestsFolder)
            {
                return;
            }
            var tests = "Tests";
            Folders.CreateDirectories(packagePath, tests);
            var testsPath = Combine(packagePath, tests);

            Folders.CreateDirectories(testsPath, "Runtime");
            string asmdefContentRuntime = packageInfo.organization.ToLower() + "." + packageInfo.name + ".Tests";
            CreateASMDEFFile(Combine(testsPath, "Runtime"), asmdefContentRuntime);

            Folders.CreateDirectories(testsPath, "Editor");
            string asmdefContentEditor = packageInfo.organization.ToLower() + "." + packageInfo.name + ".Editor." + tests;
            CreateASMDEFFile(Combine(testsPath, "Editor"), asmdefContentEditor);
        }

        /// <summary>
        /// Create a folder with a minimal ASMDEF file.
        /// </summary>
        /// <param name="packageInfo">Information about the package for which the ASMDEF file is created.</param>
        /// <param name="folderName">Name of the folder that should be created.</param>
        /// <param name="shouldAddFolder">If user selected toggle to create the specified folder</param>
        /// <param name="packagePath">File path where package is positioned. This path is in the format that can be used with AssetDatabase.</param>
        /// <param name="dataPath">Path to the same folder, but for usage with file system.</param>
        private static void CreateFolderWithASMDEFFile(PackageInfo packageInfo, string folderName, bool shouldAddFolder, string packagePath, string dataPath)
        {
            if (!shouldAddFolder)
            {
                return;
            }
            var path = Combine(dataPath, packageInfo.organization, packageInfo.displayName, folderName);
            Folders.CreateDirectories(packagePath, folderName);
            string asmdefContent = packageInfo.organization.ToLower() + "." + packageInfo.name + folderName;
            CreateASMDEFFile(path, asmdefContent);
        }

        /// <summary>
        /// Create Package.json file with the packageInfo.
        /// This info is used to define the package in the correct format for the Unity Package Manager.
        /// </summary>
        /// <param name="dataPath"></param>
        /// <param name="packageInfo"></param>
        private static void CreatePackageJson(string dataPath, PackageInfo packageInfo)
        {
            var jsonFilePath = Combine(dataPath, packageInfo.organization, packageInfo.displayName);
            var json = JsonUtility.ToJson(packageInfo, true);
            CreateFile(jsonFilePath, "package.json", json);
        }

        /// <summary>
        /// Create a minimal Assembly definitions file.
        /// Assembly files are required for Unity packages and allow for code to be organized with moludarity.
        /// Speeds up compile time as only recompilation is needed for files included in the package that was changed not the whole project.
        /// </summary>
        /// <param name="path">Folder where file will be created.</param>
        /// <param name="fileName">The name of the file without file ending. For a minimal asmdef file it is also the whole content of the file.</param>
        private static void CreateASMDEFFile(string path, string fileName)
        {
            var content = new ASMDEF(fileName);
            var json = JsonUtility.ToJson(content, true);
            CreateFile(path, fileName + ".asmdef", json);
        }

        /// <summary>
        /// Create a new file.
        /// </summary>
        /// <param name="folder">Folder where file will be created.</param>
        /// <param name="fileName">The name of the file with file ending.</param>
        /// <param name="content">The file content as plain text.</param>
        private static void CreateFile(string folder, string fileName, string content = "")
        {
            var filePath = Combine(folder, fileName);
            if (File.Exists(filePath))
            {
                Debug.LogWarning($"The file ${fileName} already exists");
            }
            File.WriteAllText(filePath, content);
        }

        [Serializable]
        private class Author
        {
            public string name;
            public string email;
            public string url;

            public Author(string name, string email, string url = "")
            {
                this.name = name;
                this.email = email;
                this.url = url;
            }
        }

        private class PackageInfo
        {
            public string name;
            public string displayName;
            public string description;
            public string organization;
            public string version = "0.0.1";
            // Suported Unity min version number
            public string unity;

            public Author author;

            internal PackageInfo(string name, string displayName, string description, string organization, string country, int major, int minor, Author author)
            {
                this.name = country + "." + organization.ToLower() + "." + name;
                this.displayName = displayName;
                this.description = description;
                this.organization = organization;
                unity = major + "." + minor;
                this.author = author;
            }
        }

        private class ASMDEF
        {
            public string name;

            public ASMDEF(string name)
            {
                this.name = name;
            }
        }
        
    }

}