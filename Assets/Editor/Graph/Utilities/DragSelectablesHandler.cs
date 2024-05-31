using Chocolate4.Dialogue.Edit.Graph.BlackBoard;
using UnityEngine;

namespace Chocolate4.Dialogue.Edit.Graph.Utilities
{
    internal class DragSelectablesHandler
    {
        [field: SerializeField]
        public IDraggableProperty SelectedProperty { get; private set; }

        public void RegisterSelectedProperty(IDraggableProperty selectedProperty)
        {
            SelectedProperty = selectedProperty;
        }

        public void UnregisterSelectedProperty()
        {
            SelectedProperty = null;
        }
    }
}