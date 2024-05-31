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
using System.Runtime.Serialization;

namespace Chocolate4.Dialogue.Edit.Graph.Nodes
{
    internal abstract class BaseNode : Node, ISaveable<NodeModel>, IDangerCauser
    {
        private readonly NodeModel model;

        public abstract string Name { get; set; }
        public bool IsMarkedDangerous { get; set; }

        public string Id => model.NodeId;
        public string GroupId
        {
            get => model.GroupId;
            set => model.GroupId = value;
        }

        public override string ToString() => Name;

        protected BaseNode(NodeModel model)
        {
            this.model = model;
        }

        internal virtual void Initialize(Vector3 startingPosition)
        {
            Id = Guid.NewGuid().ToString();
            NodeType = GetType();

            SetPosition(new Rect(startingPosition, Vector2.zero));

            RegisterCallback<PointerMoveEvent>(OnPointerMove);
        }

        internal virtual void Draw()
        {
            DrawTitle();
            DrawPorts();
            DrawContent();

            RefreshExpandedState();
        }

        internal virtual void PostInitialize()
        {
        }

        public NodeModel Save()
        {
            throw new NotImplementedException();
        }

        public void Load(NodeModel saveData)
        {
            LoadPortTypes(model.InputPortModels, inputContainer);
            LoadPortTypes(model.OutputPortModels, outputContainer);
        }

        [OnSerializing]
        protected virtual void OnSerializingMethod(StreamingContext context)
        {
            model.InputPortModels = inputContainer.Query<DataPort>().ToList().Select(port => port.Save()).ToList();
            model.OutputPortModels = outputContainer.Query<DataPort>().ToList().Select(port => port.Save()).ToList();
            model.Position = this.GetPositionRaw();
        }

        [OnDeserialized]
        protected virtual void OnDeserializedMethod(StreamingContext context)
        {
            List<PortModel> inputPortData = model.InputPortModels.ToList();
            List<PortModel> outputPortData = model.OutputPortModels.ToList();

            LoadPortTypes(inputPortData, inputContainer);
            LoadPortTypes(outputPortData, outputContainer);
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
            return DataPort.Create<Edge>(name, Orientation.Horizontal, direction, capacity, type);
        }

        private void LoadPortTypes(List<PortModel> portDataCollection, VisualElement portContainer)
        {
            List<DataPort> ports = portContainer.Query<DataPort>().ToList();
            for (int i = 0; i < ports.Count; i++)
            {
                ports[i].Load(portDataCollection[i]);
            }
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            model.Position = this.GetPositionRaw();
        }
    }
}
