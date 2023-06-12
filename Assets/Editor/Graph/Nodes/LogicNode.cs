﻿using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Chocolate4.Dialogue.Edit.Graph.Nodes
{
    public class LogicNode : BaseNode
    {
        public override void Initialize(Vector3 startingPosition)
        {
            base.Initialize(startingPosition);

            Choices.Add("True");
            Choices.Add("False");
        }

        protected override void DrawPorts()
        {
            DrawInputPort();

            foreach (string choice in Choices)
            {
                Port choicePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                choicePort.portName = choice;
                outputContainer.Add(choicePort);
            }
        }
    }
}
