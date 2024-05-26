using Chocolate4.Dialogue.Edit.Graph.Utilities;
using Newtonsoft.Json;
using System;

namespace Chocolate4.Dialogue.Edit.Tree
{
    [Serializable]
    public class DialogueTreeItem : IHaveId
    {
        [JsonProperty]
        public string Id { get; set; }
        [JsonIgnore]
        public string DisplayName { get; set; }

        public DialogueTreeItem(string displayName)
        {
            Id = Guid.NewGuid().ToString();
            DisplayName = displayName;
        }
    }
}
