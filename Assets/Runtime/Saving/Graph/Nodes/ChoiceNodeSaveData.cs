using Chocolate4.Dialogue.Runtime.Nodes;
using System;
using System.Collections.Generic;

namespace Chocolate4.Dialogue.Runtime.Saving
{
    [Serializable]
    public class ChoiceNodeSaveData : NodeModel
    {
        public List<DialogueChoice> choices;
    }
}