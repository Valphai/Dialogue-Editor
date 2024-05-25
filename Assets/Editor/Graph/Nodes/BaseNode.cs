using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Edit.Graph.Utilities;
using Chocolate4.Dialogue.Edit.Utilities;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Chocolate4.Dialogue.Runtime.Utilities;
using Chocolate4.Dialogue.Edit.Graph.Utilities.DangerLogger;
using System.Linq;
using Chocolate4.Dialogue.Graph.Edit;

namespace Chocolate4.Dialogue.Edit.Graph.Nodes
{
    public abstract class BaseNode : Node, ISaveable<NodeModel>, IHaveId, IDangerCauser
    {
        public string GroupId { get; set; }
        public string Id { get; set; }
        public Type NodeType { get; set; }
        public abstract string Name { get; set; }
        public bool IsMarkedDangerous { get; set; }

        public override string ToString() => Name;

        public virtual NodeModel Save()
        {
            List<PortData> inputPortData = inputContainer.Query<DataPort>().ToList().Select(port => port.Save()).ToList();
            List<PortData> outputPortData = outputContainer.Query<DataPort>().ToList().Select(port => port.Save()).ToList();

            return new NodeModel()
            {
                InputPortDataCollection = inputPortData,
                OutputPortDataCollection = outputPortData,
                NodeId = Id,
                NodeType = NodeType.ToString(),
                Position = this.GetPositionRaw(),
                GroupId = GroupId,
            };
        }

        public virtual void Load(NodeModel saveData)
        {
            Id = saveData.NodeId;
            GroupId = saveData.GroupId;

            List<PortData> inputPortData = saveData.InputPortDataCollection.ToList();
            List<PortData> outputPortData = saveData.OutputPortDataCollection.ToList();
            LoadPortTypes(inputPortData, inputContainer);
            LoadPortTypes(outputPortData, outputContainer);
        }

        public virtual void Initialize(Vector3 startingPosition)
        {
            Id = Guid.NewGuid().ToString();
            NodeType = GetType();

            SetPosition(new Rect(startingPosition, Vector2.zero));
        }

        public virtual void PostInitialize()
        {
        }

        public virtual void Draw()
        {
            DrawTitle();
            DrawPorts();
            DrawContent();

            RefreshExpandedState();
        }

        protected virtual void DrawTitle()
        {
            Label titleLabel = titleContainer.Q<Label>();
            titleLabel.text = Name;

            titleLabel
                .WithFontSize(UIStyles.FontSize)
                .WithMaxWidth(UIStyles.MaxWidth)
                .WithHorizontalGrow()
                .WithExpandableHeight();
            titleContainer.WithStoryStyle();
        }

        protected virtual void DrawPorts()
        {
            DrawInputPort();
            DrawOutputPort();
        }

        protected virtual void DrawContent()
        {
            VisualElement contentContainer = new VisualElement();

            AddExtraContent(contentContainer);

            extensionContainer.Add(contentContainer);
        }

        protected virtual void AddExtraContent(VisualElement contentContainer)
        {
            
        }

        protected virtual void DrawOutputPort()
        {
            Port outputPort = DrawPort(NodeConstants.TransferOut, Direction.Output, Port.Capacity.Single, typeof(TransitionPortType));
            outputContainer.Add(outputPort);
        }

        protected virtual void DrawInputPort()
        {
            Port inputPort = DrawPort(NodeConstants.TransferIn, Direction.Input, Port.Capacity.Multi, typeof(TransitionPortType));
            inputContainer.Add(inputPort);
        }

        protected virtual DataPort DrawPort(string name, Direction direction, Port.Capacity capacity, Type type)
        {
            DataPort port = DataPort.Create<Edge>(name, Orientation.Horizontal, direction, capacity, type);

            return port;
        }

        private void LoadPortTypes(List<PortData> portDataCollection, VisualElement portContainer)
        {
            List<DataPort> ports = portContainer.Query<DataPort>().ToList();
            for (int i = 0; i < ports.Count; i++)
            {
                ports[i].Load(portDataCollection[i]);
            }
        }
    }
}
