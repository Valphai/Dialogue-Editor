using Chocolate4.Dialogue.Edit.Saving;
using Chocolate4.Dialogue.Edit.Tree;
using Chocolate4.Dialogue.Runtime.Saving;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chocolate4.Dialogue.Edit.Asset
{
    [System.Serializable]
    public class DialogueDataOwner
    {
        [JsonProperty]
        public string ActiveSituationId { get; set; }
        [JsonProperty]
        public int SelectedTreeIndex { get; set; } = 0;
        [JsonProperty]
        public List<SituationSaveData> SituationsData { get; set; } = new();
        [JsonProperty]
        public List<SituationEditorData> SituationsEditorData { get; set; } = new();
        [JsonProperty]
        public BlackboardSaveData BlackboardData { get; set; }

        [JsonIgnore]
        public DialogueEditorAsset Owner { get; set; }

        public void ReplaceWith(DialogueDataOwner deserializedRefrence)
        {
            if (deserializedRefrence is not DialogueDataOwner dataOwner)
                throw new NotImplementedException($"{nameof(DialogueDataOwner)} could not handle {deserializedRefrence.GetType()}");

            SituationsData = dataOwner.SituationsData;
            //TreeData = dataOwner.TreeData;
            BlackboardData = dataOwner.BlackboardData;
        }

        internal bool TryFindActiveSituation(out SituationSaveData foundSituation) =>
            TryFindSituation(ActiveSituationId, out foundSituation);

        internal bool TryFindSituation(string id, out SituationSaveData foundSituation)
        {
            foundSituation = SituationsData.FirstOrDefault(situation => situation.Id.Equals(id));
            return foundSituation != null;
        }

        internal bool TryCacheActiveSituation() =>
            TryCacheSituation(ActiveSituationId);

        internal bool TryCacheSituation(string id)
        {
            if (!TryFindSituation(id, out SituationSaveData cachedSituationSaveData))
            {
                SituationsData.Add(data);
                return true;
            }

            int cachedIndex = SituationsData.IndexOf(cachedSituationSaveData);
            SituationsData[cachedIndex] = data;
            return false;
        }

        internal DialoguePropertyModel CreatePropertyModel()
        {
            var model = new DialoguePropertyModel();
            BlackboardData.dialoguePropertiesSaveData.Add(model);
            return model;
        }

        internal void RemoveTreeItem(DialogueTreeItem treeItem)
        {
            var treeModel = SituationsEditorData
                .Select(situation => situation.SituationTreeItem)
                .FirstOrDefault(model => model.Id.Equals(treeItem.Id));
            if (treeModel == null)
            {
                UnityEngine.Debug.LogError($"Could not remove item {treeItem.DisplayName}!");
                return;
            }

            var situation = SituationsEditorData.Find(situation => situation.SituationTreeItem == treeModel);
            situation.SituationTreeItem = null;
        }

        internal void AddTreeItem(DialogueTreeItem treeItem) => 
            SituationsEditorData.Add(new SituationEditorData(treeItem));
    }
}