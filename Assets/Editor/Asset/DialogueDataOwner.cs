using Chocolate4.Dialogue.Runtime.Saving;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Chocolate4.Dialogue.Edit.Asset
{
    [System.Serializable]
    public class DialogueDataOwner
    {
        [JsonProperty]
        public List<SituationSaveData> SituationsData { get; set; }
        [JsonProperty]
        public TreeSaveData TreeData { get; set; }
        [JsonProperty]
        public BlackboardSaveData BlackboardData { get; set; }

        [JsonIgnore]
        public DialogueEditorAsset Owner { get; set; }

        public void ReplaceWith(DialogueDataOwner deserializedRefrence)
        {
            if (deserializedRefrence is not DialogueDataOwner dataOwner)
                throw new NotImplementedException($"{nameof(DialogueDataOwner)} could not handle {deserializedRefrence.GetType()}");

            SituationsData = dataOwner.SituationsData;
            TreeData = dataOwner.TreeData;
            BlackboardData = dataOwner.BlackboardData;
        }
    }
}