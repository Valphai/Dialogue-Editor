using Chocolate4.Dialogue.Edit.Graph.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Chocolate4.Dialogue.Runtime.Saving
{
    [Serializable]
    public class SituationSaveData : IHaveId
    {
        [JsonProperty]
        public Vector2 GraphViewPosition { get; private set; }
        [JsonProperty]
        public Vector2 GraphViewZoom { get; private set; }
        [JsonProperty]
        public List<NodeModel> NodeData { get; private set; }
        [JsonProperty]
        public List<GroupSaveData> GroupData { get; private set; }
        [JsonProperty]
        public string DisplayName { get; private set; } // TODO: set this
        [JsonProperty]
        public string Id { get; set; }

        public SituationSaveData(
            string situationId, List<NodeModel> nodeModels, List<GroupSaveData> groupSaveData
        )
        {
            NodeData = nodeModels;
            GroupData = groupSaveData;

            Id = situationId;
        }

        //public bool TryMergeDataIntoHolder(out List<NodeModel> dataHolders)
        //{
        //    dataHolders = 
        //        TypeExtensions.MergeFieldListsIntoOneImplementingType<NodeModel, SituationSaveData>(this);

        //    return !dataHolders.IsNullOrEmpty();
        //}
    }
}
