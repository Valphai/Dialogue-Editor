using Chocolate4.Dialogue.Edit.Tree;
using Newtonsoft.Json;

namespace Chocolate4.Dialogue.Runtime.Saving
{
    [System.Serializable]
    public class TreeItemModel
    {
        [JsonProperty]
        public int Depth { get; }
        [JsonProperty]
        public string ParentId { get; set; }
        [JsonProperty]
        public DialogueTreeItem RootItem { get; }

        public TreeItemModel(DialogueTreeItem rootItem, int depth, string parentId)
        {
            RootItem = rootItem;
            Depth = depth;
            ParentId = parentId;
        }
    }
}