using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Chocolate4.Dialogue.Runtime.Utilities;

namespace Chocolate4.Dialogue.Edit.Utilities
{
    public static class ScriptableObjectUtilities
    {
        private const string Asset = ".asset";

        public static T CreateAssetAtPath<T>(string path, string fileName) where T : ScriptableObject
        {
            ScriptableObject so = ScriptableObject.CreateInstance<T>();
            CreateAssetAtPath(so, path, fileName);

            return (T)so;
        }

        public static void CreateAssetAtPath(ScriptableObject so, string path, string fileName)
        {
            string assetEndPath;
            string uniqueName = GetUniqueNameFromPath(path, fileName);

            string instancePath = AssetDatabase.GetAssetPath(so.GetInstanceID());
            if (File.Exists(instancePath))
            {
                string oldName = Path.GetFileName(instancePath);

                if (oldName.Equals(fileName + Asset))
                {
                    return;
                }

                AssetDatabase.RenameAsset(instancePath, uniqueName + Asset);
                AssetDatabase.Refresh();
                return;
            }

            assetEndPath = path + uniqueName + Asset;

            AssetDatabase.CreateAsset(so, assetEndPath);
            AssetDatabase.Refresh();
        }

        public static string GetUniqueNameFromPath(string path, string fileName, string[] existingNames = null)
        {
            string[] fileNames = Directory.GetFiles(path);
            if (!existingNames.IsNullOrEmpty())
            {
                fileNames = fileNames.Concat(existingNames).ToArray();
            }

            for (int i = 0; i < fileNames.Length; i++)
            {
                fileNames[i] = Path.GetFileName(fileNames[i]).Replace(Asset, string.Empty);
            }

            string unqueName = ObjectNames.GetUniqueName(fileNames, fileName);
            return unqueName;
        }
    }
}