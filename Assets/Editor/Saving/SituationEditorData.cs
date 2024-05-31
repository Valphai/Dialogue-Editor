using Chocolate4.Dialogue.Edit.Graph.Utilities;
using Chocolate4.Dialogue.Edit.Tree;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Chocolate4.Dialogue.Edit.Saving
{
    [System.Serializable]
    public class SituationEditorData : IHaveId
    {
        [JsonProperty]
        public Vector2 GraphViewPosition { get; set; }
        [JsonProperty]
        public Vector2 GraphViewZoom { get; set; }
        [JsonProperty]
        public DialogueTreeItem SituationTreeItem { get; set; }
        [JsonProperty]
        public List<GroupModel> GroupData { get; set; } = new();

        [JsonIgnore]
        public string Id => SituationTreeItem.Id;

        public SituationEditorData(DialogueTreeItem treeItem)
        {
            SituationTreeItem = treeItem;
        }
    }
}