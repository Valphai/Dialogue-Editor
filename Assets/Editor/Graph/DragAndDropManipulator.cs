using Chocolate4.Dialogue.Edit.Graph.BlackBoard;
using Chocolate4.Dialogue.Edit.Graph.Nodes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocolate4.Dialogue.Edit.Graph
{
    internal class DragAndDropManipulator : PointerManipulator
    {
        private DialogueGraphView dialogueGraphView;

        public DragAndDropManipulator(DialogueGraphView dialogueGraphView)
        {
            this.dialogueGraphView = dialogueGraphView;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<DragUpdatedEvent>(DragUpdatedHandler);
            target.RegisterCallback<DragExitedEvent>(DragExitedHandler);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<DragUpdatedEvent>(DragUpdatedHandler);
            target.UnregisterCallback<DragExitedEvent>(DragExitedHandler);
        }

        private void DragUpdatedHandler(DragUpdatedEvent evt)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
        }

        private void DragExitedHandler(DragExitedEvent evt)
        {
            if (evt.propagationPhase != PropagationPhase.AtTarget)
            {
                return;
            }

            Vector2 spawnPosition = dialogueGraphView.GetLocalMousePosition(evt.localMousePosition);
            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;

            IDraggableProperty selectedProperty = dialogueGraphView.DragSelectablesHandler.SelectedProperty;
            if (selectedProperty == null)
            {
                return;
            }

            BaseNode node = selectedProperty.ToConcreteNode();

            dialogueGraphView.AddNode(spawnPosition, node);
        }
    }
}
