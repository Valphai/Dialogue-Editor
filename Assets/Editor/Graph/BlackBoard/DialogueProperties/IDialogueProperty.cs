using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Runtime.Utilities;
using Chocolate4.Dialogue.Edit.Graph.Utilities;

namespace Chocolate4.Dialogue.Edit.Graph.BlackBoard
{
    public interface IDialogueProperty : IHaveId
    {
        string DisplayName { get; set; }
        PropertyType PropertyType { get; }
    }
}