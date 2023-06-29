﻿using Chocolate4.Dialogue.Runtime.Utilities;
using Chocolate4.Edit.Graph.Utilities;
using System;
using System.Collections.Generic;

namespace Chocolate4.Dialogue.Runtime.Saving
{
    [Serializable]
    public class SituationSaveData : IHaveId
    {
        public string situationId;
        public List<NodeSaveData> nodeData;
        public List<DialogueNodeSaveData> dialogueNodeData;
        public List<PropertyNodeSaveData> propertyNodeSaveData;
        public List<OperatorNodeSaveData> operatorNodeData;
        public List<EqualityNodeSaveData> equalityNodeData;
        public List<SituationTransferNodeSaveData> situationTransferNodeData;
        public List<SetNodeSaveData> setNodeData;
        //public List<GroupSaveData> groupData;

        public string Id => situationId;

        public SituationSaveData(string situationId, List<IDataHolder> dataHolders)
        {
            nodeData = new List<NodeSaveData>();
            dialogueNodeData = new List<DialogueNodeSaveData>();
            propertyNodeSaveData = new List<PropertyNodeSaveData>();
            operatorNodeData = new List<OperatorNodeSaveData>();
            equalityNodeData = new List<EqualityNodeSaveData>();
            situationTransferNodeData = new List<SituationTransferNodeSaveData>();
            setNodeData = new List<SetNodeSaveData>();

            this.situationId = situationId;
            //this.groupData = groupData;

            if (dataHolders.IsNullOrEmpty())
            {
                return;
            }

            TypeExtensions.DistributeListElementsToFieldsOfImplementingTypes(dataHolders, this);
        }

        public bool TryMergeDataIntoHolder(out List<IDataHolder> dataHolders)
        {
            dataHolders = 
                TypeExtensions.MergeFieldListsIntoOneImplementingType<IDataHolder, SituationSaveData>(this);

            return !dataHolders.IsNullOrEmpty();
        }
    }
}
