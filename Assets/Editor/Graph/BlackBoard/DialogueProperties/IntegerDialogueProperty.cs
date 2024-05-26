using Chocolate4.Dialogue.Edit.Asset;
using Chocolate4.Dialogue.Edit.Graph.Nodes;
using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Runtime.Utilities;
using System;

namespace Chocolate4.Dialogue.Edit.Graph.BlackBoard
{
    [Serializable]
    public class IntegerDialogueProperty : ValueDialogueProperty<int, IntegerConstantView>
    {
        public override PropertyType PropertyType { get; } = PropertyType.Integer;

        public IntegerDialogueProperty(DialoguePropertyModel model, DialogueDataOwner dataOwner) : base(model, dataOwner)
        {
        }
    }
}