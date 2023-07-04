﻿using Chocolate4.Dialogue.Edit.Utilities;
using Chocolate4.Edit.Graph.Utilities;
using Chocolate4.Runtime.Utilities;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Chocolate4.Dialogue.Edit.Graph.Nodes
{
    public class ConditionNode : BaseNode
    {
        public override string Name { get; set; } = "Condition Node";
        public string Text { get; set; }

        protected override void DrawTitle()
        {
            Label Label = new Label() { text = Name };

            Label.WithFontSize(UIStyles.FontSize)
                .WithMarginTop(UIStyles.LogicMarginTop);

            titleContainer.Insert(0, Label);
            titleContainer.WithLogicStyle();
        }

        protected override void AddExtraContent(VisualElement contentContainer)
        {
            contentContainer
                .WithMaxWidth(UIStyles.MaxWidth)
                .WithTextField(Text, evt => Text = evt.newValue)
                .WithMinHeight(UIStyles.SmallTextFieldHeight);
        }

        protected override void DrawOutputPort()
        {
            Port truePort = DrawPort(NodeConstants.ConditionOutputTrue, Direction.Output, Port.Capacity.Single, typeof(TransitionPortType));
            Port falsePort = DrawPort(NodeConstants.ConditionOutputFalse, Direction.Output, Port.Capacity.Single, typeof(TransitionPortType));

            outputContainer.Add(truePort);
            outputContainer.Add(falsePort);
        }
    }
}
