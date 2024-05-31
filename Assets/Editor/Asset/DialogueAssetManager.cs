using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Runtime.Utilities;
using Chocolate4.Dialogue.Runtime.Entities;
using Chocolate4.Dialogue.Runtime.Asset;
using Chocolate4.Dialogue.Edit.Tree;
using Chocolate4.Dialogue.Edit.Graph;
using Chocolate4.Dialogue.Edit.CodeGeneration;
using Chocolate4.Dialogue.Edit.Entities;
using Chocolate4.Dialogue.Edit.Utilities;
using Chocolate4.Dialogue.Edit.Entities.Utilities;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chocolate4.Dialogue.Edit.Asset
{
    [System.Serializable]
    internal class DialogueAssetManager
    {
        [SerializeField] 
        private int instanceId;

        [field:SerializeField]
        internal EntitiesHolder EntitiesDatabase { get; private set; }
        [field:SerializeField] 
        public DialogueAsset ImportedAsset { get; private set; }

        private DialogueDataOwner dataOwner;

        private string Path => AssetDatabase.GetAssetPath(instanceId);

        public DialogueAssetManager(
            DialogueAsset importedAsset, int instanceId, EntitiesHolder entitiesDatabase
        )
        {
            this.instanceId = instanceId;
            EntitiesDatabase = entitiesDatabase;
            ImportedAsset = importedAsset;

            EntitiesData entitiesData = new EntitiesData() {
                cachedEntities = (List<DialogueEntity>)entitiesDatabase.DialogueEntities 
            };

            Store(importedAsset.graphSaveData, importedAsset.TreeData, entitiesData);
        }

        internal void DialogueTreeView_OnTreeItemAdded(DialogueTreeItem treeItem)
        {
            if (dataOwner.TryFindSituation(treeItem.Id, out var _))
            {
                return;
            }

            dataOwner.Owner.RegisterCompleteObjectUndo($"Added tree item {treeItem.DisplayName}");
            dataOwner.AddTreeItem(treeItem);
            SituationSaveData item = new SituationSaveData(treeItem.Id, treeItem.DisplayName);
            dataOwner.SituationsData.Add(item);
        }
        
        internal void DialogueTreeView_OnTreeItemRenamed(DialogueTreeItem treeItem)
        {
            var renamedModel = dataOwner.SituationsEditorData
                .Select(situation => situation.SituationTreeItem)
                .FirstOrDefault(model => model.Id.Equals(treeItem.Id));
            if (renamedModel == null)
            {
                Debug.LogError($"Could not find model to rename by id {treeItem.Id}");
                return;
            }

            renamedModel.DisplayName = treeItem.DisplayName;
        }

        internal void Rebuild(DialogueTreeView treeView, DialogueGraphView graphView, DialogueEntitiesView entitiesView)
        {
            EntitiesDatabase.Reload();
            treeView.Rebuild();
            graphView.Rebuild();
            entitiesView.Rebuild();
        }

        internal void Store(GraphSaveData graphData, TreeSaveData treeData, EntitiesData entitiesData)
        {
            dataRebuilder.Store(graphData, treeData, entitiesData);
        }

        internal void Save(EntitiesData entitiesData)
        {
            Debug.Assert(ImportedAsset != null);

            Store(graphData, treeData, entitiesData);

            string oldFileName = ImportedAsset.FileName;

            SaveEntities(entitiesData);
            TrySaveAssetToFile();

            DialogueMasterCollectionGenerator.TryRegenerate(
                ImportedAsset.FileName, oldFileName, ImportedAsset.BlackboardData,
                ImportedAsset.TreeData.treeItemData
            );
        }

        private void SaveEntities(EntitiesData entitiesData)
        {
            List<DialogueEntity> existingEntities = 
                Runtime.EntitiesUtilities.GetAllEntities(EntitiesDatabase);

            string assetsRelativePath =
                FilePathConstants.GetEntitiesPathRelative(EntitiesDatabase.associatedAssetName);

            int[] existingIds = existingEntities.Select(entity => entity.GetInstanceID()).ToArray();

            List<int> cachedIds = new List<int>();
            foreach (DialogueEntity entity in entitiesData.cachedEntities)
            {
                int instanceId = entity.GetInstanceID();
                cachedIds.Add(instanceId);

                ScriptableObjectUtilities.CreateAssetAtPath(entity,
                    assetsRelativePath, EntitiesUtilities.GetEntityName(entity)
                );
            }

            for (int i = 0; i < existingIds.Length; i++)
            {
                int instanceId = existingIds[i];
                if (!cachedIds.Contains(instanceId))
                {
                    ScriptableObjectUtilities.RemoveAssetAtPath(instanceId);
                }
            }

            EntitiesDatabase.Reload();
            AssetDatabase.Refresh();
        }

        private void TrySaveAssetToFile()
        {
            ImportedAsset.FileName = System.IO.Path.GetFileNameWithoutExtension(Path);
            ImportedAsset.BlackboardData = dataOwner.BlackboardData;
            ImportedAsset.TreeData = dataOwner.TreeData;

            string assetJson = JsonConvert.SerializeObject(ImportedAsset);
            if (!FilePathConstants.FileIsDuplicate(Path, assetJson))
            {
                File.WriteAllText(Path, assetJson);
            }

            string situationsFolder = FilePathConstants.GetSituationsPathRelative(Path);
            if (!Directory.Exists(situationsFolder))
            {
                Directory.CreateDirectory(situationsFolder);
            }

            foreach (var situation in dataOwner.SituationsData)
            {
                string situationJson = JsonConvert.SerializeObject(situation);
                string situationPath = FilePathConstants.GetSeparatedPath(false, situationsFolder, situation.DisplayName + FilePathConstants.Json);

                if (FilePathConstants.FileIsDuplicate(situationPath, situationJson))
                    continue;

                File.WriteAllText(situationPath, assetJson);
            }


            AssetDatabase.ImportAsset(Path);
        }
    }
}