using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Chocolate4.Dialogue.Runtime.Saving
{
    [System.Serializable]
    public class TreeSaveData
    {
        [JsonProperty]
        public int SelectedIndex { get; set; } = 0;
        [JsonProperty]
        public List<TreeItemModel> TreeItemData { get; set; }

        public TreeSaveData()
        {
            
        }

        public TreeSaveData(TreeSaveData treeData)
        {
            SelectedIndex = treeData.SelectedIndex;
            TreeItemData = treeData.TreeItemData.ToList();
        }
    }
}