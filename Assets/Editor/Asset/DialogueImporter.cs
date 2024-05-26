using Chocolate4.Dialogue.Runtime.Entities;
using Chocolate4.Dialogue.Runtime.Asset;
using Chocolate4.Dialogue.Runtime.Utilities;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using Newtonsoft.Json;
using Chocolate4.Dialogue.Runtime.Saving;
using System.Collections.Generic;

namespace Chocolate4.Dialogue.Edit.Asset
{
    [ScriptedImporter(Version, FilePathConstants.Extension)]
    public class DialogueImporter : ScriptedImporter
    {
        private const int Version = 1;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx));

            string json;
            try
            {
                json = File.ReadAllText(ctx.assetPath);
            }
            catch (Exception exception)
            {
                ctx.LogImportError($"Could not read file '{ctx.assetPath}' ({exception})");
                return;
            }

            var asset = ScriptableObject.CreateInstance<DialogueAsset>();
            var entitiesDatabase = ScriptableObject.CreateInstance<EntitiesHolder>();

            try
            {
                asset.FromJson(json);
                asset.DialogueData.SituationData = ReadSituationJsons(ctx.assetPath);
            }
            catch (Exception exception)
            {
                ctx.LogImportError($"Could not parse input actions in JSON format from '{ctx.assetPath}' ({exception})");
                return;
            }

            entitiesDatabase.associatedAssetName = Path.GetFileNameWithoutExtension(assetPath);
            entitiesDatabase.name = EntitiesHolder.DataBase;

            entitiesDatabase.Reload();

            var assetIcon = (Texture2D)EditorGUIUtility.Load(FilePathConstants.GetEditorVisualAssetPath(FilePathConstants.assetIcon));
            
            ctx.AddObjectToAsset("<root>", asset, assetIcon);
            ctx.AddObjectToAsset("<entities>", entitiesDatabase);
        }

        private List<SituationSaveData> ReadSituationJsons(string assetPath)
        {
            string situationsFolder = FilePathConstants.GetSituationsPathRelative(assetPath);

            List<SituationSaveData> jsonObjects = new List<SituationSaveData>();
            foreach (string filePath in Directory.EnumerateFiles(situationsFolder))
            {
                if (!Path.GetExtension(filePath).Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string jsonText = File.ReadAllText(filePath);

                try
                {
                    var jsonObject = JsonConvert.DeserializeObject<SituationSaveData>(jsonText);

                    jsonObjects.Add(jsonObject);
                }
                catch (JsonReaderException)
                {
                    Console.WriteLine($"Invalid JSON format in {filePath}.");
                }
            }

            return jsonObjects;
        }
    }
}
