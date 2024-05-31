using Chocolate4.Dialogue.Edit.Utilities;
using Chocolate4.Dialogue.Runtime.Nodes;
using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Edit.Graph.Utilities;
using Chocolate4.Dialogue.Runtime.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;

namespace Chocolate4.Dialogue.Edit.Graph.Nodes
{
    internal class ChoiceNode : BaseNode
    {
        private List<DialogueChoice> choices = new List<DialogueChoice>();
        private VisualElement buttonedPortsContainer;
        private VisualElement extraContentContainer;

        public override string Name { get; set; } = "Choice Node";

        public override NodeModel Save()
        {
            NodeModel saveData = (NodeModel)base.Save();
            return new ChoiceNodeSaveData() { choices = choices.ToList(), nodeSaveData = saveData };
        }

        public override void Load(NodeModel saveData)
        {
            ChoiceNodeSaveData choicesSaveData = (ChoiceNodeSaveData)saveData;
            choices = choicesSaveData.choices.ToList();

            foreach (DialogueChoice choice in choices)
            {
                AddChoicePort(choice);
            }

            base.Load(saveData);
        }

        protected override void AddExtraContent(VisualElement contentContainer)
        {
            contentContainer
                .WithMaxWidth(UIStyles.MaxWidth)
                .WithBackgroundColor(UIStyles.DefaultDarkColor);

            extraContentContainer = contentContainer;
        }

        protected override void DrawOutputPort()
        {
            outputContainer.WithFlexGrow();
            VisualElement addButtonContainer = new VisualElement();
            addButtonContainer
                .WithButton("+")
                .WithOnClick(() => {
                    string[] existingNames = choices.Select(choice => choice.name).ToArray();
                    string uniqueName = ObjectNames.GetUniqueName(existingNames, NodeConstants.ChoiceOut);

                    DialogueChoice choice = new DialogueChoice() { name = uniqueName };
                    AddChoicePort(choice);
                    choices.Add(choice);
                }
            );

            buttonedPortsContainer = new VisualElement().WithFlexGrow();

            outputContainer.Add(addButtonContainer);
            outputContainer.Add(buttonedPortsContainer);
        }

        private void AddChoicePort(DialogueChoice choice)
        {
            int childCount = extraContentContainer.childCount;
            int choiceIndex = Math.Min(choice.ChoiceIndex, childCount);

            VisualElement container = new VisualElement()
                .WithHorizontalGrow();

            Port outputPort = DrawPort(choice.name, Direction.Output, Port.Capacity.Single, typeof(TransitionPortType));
            outputPort.WithFlexGrow();

            Foldout extraContentFoldout = new Foldout() { text = choice.name };
            TextField extraContentTextField = extraContentFoldout
                .WithNodeTextField(choice.text, evt => choice.text = evt.newValue);

            extraContentContainer.Insert(choiceIndex, extraContentFoldout);

            container
                .WithButton("-")
                .WithOnClick(() => {
                    choices.Remove(choice);
                    buttonedPortsContainer.Remove(container);
                    extraContentContainer.Remove(extraContentFoldout);
                    RemoveChoicePort(outputPort);
                });

            container.Add(outputPort);
            buttonedPortsContainer.Insert(choiceIndex, container);
        }

        internal void RemoveChoicePort(Port outputPort)
        {
            List<Edge> connectedEdges = outputPort.connections.ToList();
            foreach (var edge in connectedEdges)
            {
                outputPort.Disconnect(edge);
                edge.RemoveFromHierarchy();
            }

            RefreshPorts();
        }
    }
}
