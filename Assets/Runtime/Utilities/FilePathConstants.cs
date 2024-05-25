using System.IO;
using UnityEngine;
using System.Linq;
using System;
using System.Text;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Chocolate4.Dialogue.Runtime.Utilities
{
    public static class FilePathConstants
    {
        public const string AssetName = "DialogueGraphBuilder";
        public const string Chocolate4 = "Chocolate4";
        public const string Assets = "Assets";
        public const string Resources = "Resources";
        public const string Collection = "Collection";
        public const string Extension = "chocolatedialogue";
        public const string Json = ".json";
        public const string EntitiesFolder = "Entities";
        public const string SituationsFolder = "Situations";

        public static string testCasesAsset = $"TestCasesDialogueEditor";
#if UNITY_EDITOR
        public static string placeholderEntityPath = $"Icons{DirSep}Placeholder Entity 256.png";
        public static string situationIconPath = $"Icons{DirSep}Situation Gem 128.png";
        public static string assetIcon = $"Icons{DirSep}Book Icon 64.png";

        public static string dialogueNodeStyle = $"DialogueSystem{DirSep}DialogueNodeStyle.uss";
        public static string dialogueGraphStyle = $"DialogueSystem{DirSep}DialogueGraphStyle.uss";

        private static string assetFolder = string.Empty;
        private static string AssetFolder
        {
            get
            {
                if (string.IsNullOrEmpty(assetFolder)
                    || !AssetDatabase.IsValidFolder(assetFolder)
                )
                {
                    assetFolder = FindPackageFolder();
                    return assetFolder;
                }

                return assetFolder;
            }
        }
#endif

        public static char DirSep => Path.DirectorySeparatorChar;

        public static string GetCollectionName(string fileName) => fileName + Collection;

#if UNITY_EDITOR
        public static string GetSeparatedPath(bool appendLastSep, params string[] folders)
        {
            string path = string.Join(DirSep, folders);
            if (appendLastSep)
            {
                path += DirSep;
            }
            return path;
        }

        public static string FindPackageFolder()
        {
            string directory;
            string assetsRelativePath;
            try
            {
                directory = 
                    Directory.GetDirectories(Application.dataPath, Chocolate4 + DirSep + AssetName + DirSep, SearchOption.AllDirectories).First();
                assetsRelativePath = GetPathRelativeTo(Assets, directory);
            }
            catch (System.Exception)
            {
                directory = Application.dataPath;
                assetsRelativePath = GetPathRelativeTo(Assets, directory);
                
                return assetsRelativePath;
            }

            string pathLastElement = assetsRelativePath.Split(DirSep).Last();

            int index = assetsRelativePath.IndexOf(pathLastElement);
            if (index >= 0)
                assetsRelativePath = assetsRelativePath.Substring(0, index);

            return assetsRelativePath;
        }

        public static string GetEditorVisualAssetPath(string asset)
        {
            return $"{AssetFolder}{DirSep}Editor{DirSep}VisualAssets{DirSep}" + asset;
        }

        public static string GetPathRelativeTo(string start, string path)
        {
            int lastIndex = path.LastIndexOf(start);
            return path.Substring(lastIndex);
        }

        public static string GetEntitiesPathRelative(string associatedAssetName)
        {
            string fullPath = GetEntitiesPath(associatedAssetName);
            string assetsRelativePath = GetPathRelativeTo(Assets, fullPath);
            return assetsRelativePath;
        }

        public static string GetEntitiesPath(string assetName)
        {
            return AssetFolder + DirSep + "Runtime" + DirSep + Resources + DirSep + Chocolate4 + DirSep + assetName + EntitiesFolder + DirSep;
        }

        public static string GetCollectionPath(string collectionName)
        {
            return AssetFolder + DirSep + "Runtime" + DirSep + "Master" + DirSep + "Collections" + DirSep + collectionName + ".cs";
        }

        public static bool FileIsDuplicate(string path, string generatedFile)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            string existingFile = File.ReadAllText(path);
            if (existingFile != generatedFile)
            {
                return false;
            }

            return true;
        }

        public static string GetSituationsPathRelative(string assetPath) => 
            Path.GetDirectoryName(assetPath) + DirSep + SituationsFolder + DirSep;
#endif
    }
}
