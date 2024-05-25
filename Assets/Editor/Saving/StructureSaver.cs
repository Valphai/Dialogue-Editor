using Chocolate4.Dialogue.Edit.Graph.Nodes;
using Chocolate4.Dialogue.Edit.Tree;
using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Runtime.Utilities;
using Chocolate4.Dialogue.Edit.Graph;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Chocolate4.Dialogue.Edit.Saving
{
    public class StructureSaver
    {
        public static SituationSaveData SaveSituation(
            string situationGuid, DialogueGraphView graphView
        )
        {
            List<NodeModel> nodeSaveDatas = new List<NodeModel>();
            List<GroupSaveData> groupSaveDatas = new List<GroupSaveData>();

            graphView.PerformOnAllGraphElementsOfType<BaseNode>(node => nodeSaveDatas.Add(node.Save()));
            graphView.PerformOnAllGraphElementsOfType<CustomGroup>(group => groupSaveDatas.Add(group.Save()));

            return new SituationSaveData(situationGuid, nodeSaveDatas, groupSaveDatas);
        }

        public static TreeSaveData SaveTree(TreeView treeView)
        {
            List<KeyValuePair<int, List<int>>> deepestChildIds = GetNodesByDepth(treeView);

            List<TreeItemSaveData> resultData = new List<TreeItemSaveData>();
            TreeSaveData result = new TreeSaveData() 
            { 
                treeItemData = resultData,
                selectedIndex = treeView.selectedIndex 
            };

            foreach (var deepestChildId in deepestChildIds)
            {
                int parentId = deepestChildId.Key;
                TreeItemSaveData childSaveData = SaveChild(treeView, parentId);

                resultData.Add(childSaveData);
            }

            return result;
        }

        private static List<KeyValuePair<int, List<int>>> GetNodesByDepth(TreeView treeView)
        {
            Dictionary<int, List<int>> parentToChildrenID = new Dictionary<int, List<int>>();

            int treeCount = treeView.GetTreeCount();

            for (int i = 0; i < treeCount; i++)
            {
                int nextId = treeView.GetIdForIndex(i);
                List<int> childIds = treeView.viewController.GetChildrenIds(nextId).ToList();

                parentToChildrenID.Add(nextId, childIds);
            }

            var deepestChildIds = parentToChildrenID.ToList();
            deepestChildIds.Sort((a, b) => treeView.GetDepthOfItemById(a.Key).CompareTo(treeView.GetDepthOfItemById(b.Key)));
            return deepestChildIds;
        }

        private static TreeItemSaveData SaveChild(TreeView treeView, int parentId)
        {
            DialogueTreeItem parent = treeView.GetItemDataForId<DialogueTreeItem>(parentId);

            List<string> childrenGuids = new List<string>();
            List<int> childrenIds = treeView.viewController.GetChildrenIds(parentId).ToList();
            if (!childrenIds.IsNullOrEmpty())
            {
                foreach (int childId in childrenIds)
                {
                    childrenGuids.Add(treeView.GetItemDataForId<DialogueTreeItem>(childId).id);
                }
            }

            TreeItemSaveData itemSaveData = new TreeItemSaveData(parent, childrenGuids, treeView.GetDepthOfItemById(parentId));
            return itemSaveData;
        }
    }
}
