using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Runtime.Utilities;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Chocolate4.Dialogue.Edit.Tree.Utilities
{
    public static class TreeUtilities
    {
        public static Texture2D GetSituationIcon()
        {
            return (Texture2D)EditorGUIUtility.Load(FilePathConstants.GetEditorVisualAssetPath(FilePathConstants.situationIconPath));
        }

        public static void ForceRefresh(TreeView treeView, Action<IEnumerable<int>> onSelectionChanged)
        {
            treeView.Rebuild();

            if (treeView.viewController == null || treeView.GetTreeCount() <= 0)
            {
                return;
            }

            // Force TreeView to call onSelectionChanged when it restores its own selection from view data.
            treeView.schedule.Execute(() => {
                onSelectionChanged(treeView.selectedIndices);
            });
        }

        public static List<TreeViewItemData<DialogueTreeItem>> BuildTree(Dictionary<DialogueTreeItem, List<DialogueTreeItem>> parentToChildren)
        {
            // Create a dictionary to store all nodes
            var nodeDict = new Dictionary<DialogueTreeItem, TreeViewItemData<DialogueTreeItem>>();

            // Create all nodes recursively
            foreach (var parent in parentToChildren.Keys)
            {
                CreateNode(parent, parentToChildren, nodeDict);
            }

            var allChildren = new HashSet<DialogueTreeItem>(parentToChildren.Values.SelectMany(v => v));
            var roots = parentToChildren.Keys
                .Where(element => !allChildren.Contains(element))
                .ToList();

            return roots
                .Select(root => nodeDict[root])
                .ToList();
        }

        private static TreeViewItemData<DialogueTreeItem> CreateNode(
            DialogueTreeItem parent, Dictionary<DialogueTreeItem, List<DialogueTreeItem>> parentToChildren,
            Dictionary<DialogueTreeItem, TreeViewItemData<DialogueTreeItem>> nodeDict
        )
        {
            if (nodeDict.ContainsKey(parent))
            {
                return nodeDict[parent];
            }

            var children = new List<TreeViewItemData<DialogueTreeItem>>();
            if (parentToChildren.ContainsKey(parent))
            {
                var parentChildren = parentToChildren[parent];
                foreach (var child in parentChildren)
                {
                    children.Add(CreateNode(child, parentToChildren, nodeDict));
                }
            }

            int id = GUID.Generate().GetHashCode();
            var node = new TreeViewItemData<DialogueTreeItem>(id, parent, children);
            nodeDict[parent] = node;
            return node;
        }

        //public static List<TreeViewItemData<DialogueTreeItem>> GetChildren(
        //    TreeSaveData treeSaveData, TreeItemModel treeItemSaveData, int nextId
        //)
        //{
        //    if (treeItemSaveData.childrenGuids.IsNullOrEmpty())
        //    {
        //        return null;
        //    }

        //    int count = treeItemSaveData.childrenGuids.Count;
        //    var children = new List<TreeViewItemData<DialogueTreeItem>>();
        //    int childStartingId = nextId + count;
        //    for (int i = 0; i < count; i++)
        //    {
        //        string childGuid = treeItemSaveData.childrenGuids[i];
        //        TreeItemModel childItemSaveData = treeSaveData.TreeItemData.Find(itemData => itemData.RootItem.Id == childGuid);
        //        DialogueTreeItem childItem = childItemSaveData.RootItem;

        //        children.Add(
        //            new TreeViewItemData<DialogueTreeItem>(
        //                nextId + i, 
        //                childItem, 
        //                GetChildren(treeSaveData, childItemSaveData, childStartingId)
        //            )
        //        );
        //    }

        //    return children;
        //}

        public static void FilterTreeViewBy(
            string filter, TreeView treeView, Action<IEnumerable<int>> onSelectionChanged
        )
        {
            List<DialogueTreeItem> displayedTreeItems = new List<DialogueTreeItem>();

            List<int> rootIds = treeView.GetRootIds().ToList();
            Dictionary<DialogueTreeItem, int> roots = 
                rootIds.ToDictionary(id => treeView.GetItemDataForId<DialogueTreeItem>(id));

            List<int> allIds = treeView.viewController.GetAllItemIds().ToList();
            Dictionary<DialogueTreeItem, int> allTreeItemIds = 
                allIds.ToDictionary(id => treeView.GetItemDataForId<DialogueTreeItem>(id));

            foreach (DialogueTreeItem root in roots.Keys)
            {
                var stack = new Stack<DialogueTreeItem>();
                stack.Push(root);
                while (stack.Count > 0)
                {
                    DialogueTreeItem current = stack.Pop();

                    if (current.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    {
                        displayedTreeItems.Add(current);
                    }

                    IEnumerable<int> childrenIds = 
                        treeView.GetChildrenIdsForIndex(treeView.viewController.GetIndexForId(allTreeItemIds[current]));

                    List<DialogueTreeItem> children = childrenIds
                        .Select(childId => treeView.GetItemDataForId<DialogueTreeItem>(childId))
                        .Where(child => child != current)
                        .ToList();

                    if (children.IsNullOrEmpty())
                    {
                        continue;
                    }

                    foreach (DialogueTreeItem child in children)
                    {
                        stack.Push(child);
                    }
                }
            }

            displayedTreeItems.Sort((a, b) => EditorUtility.NaturalCompare(a.DisplayName, b.DisplayName));

            treeView.SetRootItems(displayedTreeItems.Select(treeItem => new TreeViewItemData<DialogueTreeItem>(
                    allTreeItemIds[treeItem],
                    treeItem
                )).ToList()
            );

            treeView.ClearSelection();
            treeView.SetSelection(0);
            ForceRefresh(treeView, onSelectionChanged);
        }
    }
}
