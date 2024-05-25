using System;

namespace Chocolate4.Dialogue.Runtime.Saving
{
    [Serializable]
    public class DialogueNodeSaveData : NodeModel
    {
        public string text;
        public string speakerIdentifier;
    }
}