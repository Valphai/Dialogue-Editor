using Chocolate4.Dialogue.Edit.Graph.BlackBoard;
using Chocolate4.Dialogue.Edit.Utilities;
using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Runtime.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocolate4.Dialogue.Edit.Graph.Nodes
{
    public abstract class BlackboardPropertyNode : BaseNode, IPropertyNode
    {
        public string PropertyId { get; internal set; }
        public bool IsBoundToProperty => !string.IsNullOrEmpty(PropertyId);
        public abstract PropertyType PropertyType { get; protected set; }

        public override NodeModel Save()
        {
            NodeModel saveData = (NodeModel)base.Save();
            return new PropertyNodeSaveData()
            {
                nodeSaveData = saveData,
                propertyID = PropertyId,
                propertyType = PropertyType,
            };
        }

        public override void Load(NodeModel saveData)
        {
            base.Load(saveData);
            PropertyNodeSaveData propertySaveData = (PropertyNodeSaveData)saveData;
            PropertyId = propertySaveData.propertyID;
            PropertyType = propertySaveData.propertyType;

            UpdateLabel();
        }

        public override void Initialize(Vector3 startingPosition)
        {
            base.Initialize(startingPosition);

            VisualElement element = this.Q<VisualElement>("node-border");
            element?.WithOverflow();
        }

        public virtual void UnbindFromProperty()
        {
            Name = PropertyType.ToString();
            PropertyId = string.Empty;
            UpdateLabel();
        }

        public virtual void BindToProperty(IDialogueProperty property)
        {
            Name = property.DisplayName;
            PropertyId = property.Id;
            UpdateLabel();
        }

        protected override void DrawContent()
        {
        }

        protected Label UpdateLabel()
        {
            Label label = titleContainer.Q<Label>();
            label.text = Name;

            return label;
        }
    }
}