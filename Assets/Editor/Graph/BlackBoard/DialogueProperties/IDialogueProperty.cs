using Chocolate4.Dialogue.Edit.Graph.Nodes;

namespace Chocolate4.Dialogue.Edit.Graph.BlackBoard
{
    public enum PropertyType
    {
        Bool,
        Integer
    }

    public interface IDialogueProperty
    {
        string DisplayName { get; set; }
        PropertyType PropertyType { get; }
        string Guid { get; }

        BaseNode ToConcreteNode();
    }
}