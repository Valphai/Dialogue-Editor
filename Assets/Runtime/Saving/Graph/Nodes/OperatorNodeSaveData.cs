﻿using Chocolate4.Dialogue.Edit.Graph.Nodes;
using System;
using System.Collections.Generic;

namespace Chocolate4.Dialogue.Runtime.Saving
{
    [Serializable]
    public class OperatorNodeSaveData : IDataHolder
    {
        public OperatorType operatorEnum;
        public List<string> constantViewValues;
        public NodeSaveData nodeSaveData;

        public NodeSaveData NodeData => nodeSaveData;
    }
}
