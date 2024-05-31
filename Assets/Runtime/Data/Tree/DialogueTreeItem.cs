using Chocolate4.Dialogue.Edit.Graph.Utilities;
using Newtonsoft.Json;
using System;

namespace Chocolate4.Dialogue.Edit.Tree
{
    [Serializable]
    public class DialogueTreeItem : IHaveId
    {
        [JsonIgnore]
        public string DisplayName { get; set; }
        [JsonProperty]
        public string Id { get; set; }
        [JsonProperty]
        public int Depth { get; set; }
        [JsonProperty]
        public string ParentId { get; set; }

        public DialogueTreeItem(string displayName, int depth, string parentId)
        {
            Id = Guid.NewGuid().ToString();
            DisplayName = displayName;
            Depth = depth;
            ParentId = parentId;
        }
    }
}
