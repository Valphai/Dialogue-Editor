using Chocolate4.Dialogue.Edit.Graph.Utilities;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chocolate4.Dialogue.Runtime.Saving
{
    [Serializable]
    public class SituationSaveData : IHaveId
    {
        [JsonProperty]
        public string Id { get; set; }
        [JsonProperty]
        public string DisplayName { get; set; }
        [JsonProperty]
        public List<NodeModel> NodeData { get; private set; }

        public SituationSaveData(string situationId, string displayName) 
        {
            Id = situationId;
            DisplayName = displayName;
        }

        public SituationSaveData(string situationId, List<NodeModel> nodeModels)
        {
            Id = situationId;
            NodeData = nodeModels;
        }

        //public bool TryMergeDataIntoHolder(out List<NodeModel> dataHolders)
        //{
        //    dataHolders = 
        //        TypeExtensions.MergeFieldListsIntoOneImplementingType<NodeModel, SituationSaveData>(this);

        //    return !dataHolders.IsNullOrEmpty();
        //}
    }
}
