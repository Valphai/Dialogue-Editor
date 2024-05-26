using Chocolate4.Dialogue.Edit.Graph.Nodes;
using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Runtime.Utilities;
using System;

namespace Chocolate4.Dialogue.Edit.Graph.BlackBoard
{
    [Serializable]
    internal sealed class EventDialogueProperty : IDialogueProperty, IDraggableProperty
    {
        private DialoguePropertyModel model;

        public string Id => model.Id;
        public PropertyType PropertyType { get; } = PropertyType.Event;

        public string DisplayName
        {
            get => string.IsNullOrEmpty(model.DisplayName) ? Id.ToString() : model.DisplayName;
            set => model.DisplayName = value;
        }

        public EventDialogueProperty(DialoguePropertyModel model)
        {
            this.model = model;

            DisplayName = model.DisplayName ?? PropertyType.ToString();
            model.PropertyType = PropertyType;
        }

        public BaseNode ToConcreteNode() => new EventPropertyNode()
        {
            Name = DisplayName,
            PropertyId = Id,
        };
    }
}