using Chocolate4.Dialogue.Runtime.Utilities;
using Chocolate4.Dialogue.Edit.Graph.Utilities;
using System;
using System.Collections.Generic;

namespace Chocolate4.Dialogue.Runtime.Saving
{
    [Serializable]
    public class SituationSaveData : IHaveId
    {
        public string situationId;
        public List<NodeModel> nodeData;
        public List<TextNodeSaveData> textNodeData;
        public List<DialogueNodeSaveData> dialogueNodeData;
        public List<PropertyNodeSaveData> propertyNodeSaveData;
        public List<SituationTransferNodeSaveData> situationTransferNodeData;
        public List<ChoiceNodeSaveData> choiceNodeData;
        public List<GroupSaveData> groupData;

        public string Id => situationId;

        public SituationSaveData(
            string situationId, List<NodeModel> dataHolders, List<GroupSaveData> groupSaveData
        )
        {
            nodeData = new List<NodeModel>();
            textNodeData = new List<TextNodeSaveData>();
            dialogueNodeData = new List<DialogueNodeSaveData>();
            propertyNodeSaveData = new List<PropertyNodeSaveData>();
            situationTransferNodeData = new List<SituationTransferNodeSaveData>();
            choiceNodeData = new List<ChoiceNodeSaveData>();
            groupData = groupSaveData;

            this.situationId = situationId;

            if (dataHolders.IsNullOrEmpty())
            {
                return;
            }

            TypeExtensions.DistributeListElementsToFieldsOfImplementingTypes(dataHolders, this);
        }

        public bool TryMergeDataIntoHolder(out List<NodeModel> dataHolders)
        {
            dataHolders = 
                TypeExtensions.MergeFieldListsIntoOneImplementingType<NodeModel, SituationSaveData>(this);

            return !dataHolders.IsNullOrEmpty();
        }
    }
}
