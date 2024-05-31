using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Edit.Graph.Utilities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;
using Chocolate4.Dialogue.Edit.Saving;

namespace Chocolate4.Dialogue.Edit.Graph.Nodes
{
	public class CustomGroup : Group, IHaveId, ISaveable<GroupModel>
    {
        private GroupModel model;

        public string Id => model.Id;

        public CustomGroup(GroupModel groupModel) : base()
        {
            model = groupModel;
            id = Guid.NewGuid().ToString();
        }

        public override string ToString()
        {
            return $"{title} : {id}";
        }

        public void Rename(string newName) => model.DisplayName = newName;

        public GroupModel Save()
        {
            return new GroupModel()
            {
                Id = id,
                DisplayName = title,
                Position = this.GetPositionRaw(),
            };
        }

        public void Load(GroupModel saveData)
        {
            id = saveData.Id;
            title = saveData.DisplayName;
            SetPosition(new Rect(saveData.Position, Vector2.zero));
        }

        internal void AddToGroup(BaseNode node)
        {
            AddElement(node);
            node.GroupId = Id;
        }
    }
}