using Chocolate4.Dialogue.Edit.Saving;
using Chocolate4.Dialogue.Edit.Tree.Utilities;
using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Edit.Utilities;
using Chocolate4.Dialogue.Edit.Search;
using Chocolate4.Dialogue.Edit.Graph.Utilities;
using Chocolate4.Dialogue.Edit.Graph.Utilities.DangerLogger;
using Chocolate4.Dialogue.Edit.Asset;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Chocolate4.Dialogue.Edit.Tree
{
    [Serializable]
    public class DialogueTreeView : IRebuildable, ISearchable
    {
        [SerializeField]
        private int cachedSelectedId;

        internal TreeView TreeView { get; private set; }
        internal DialogueDataOwner DataOwner { get; private set; }

        internal List<DialogueTreeItem> DialogueTreeItems => DataOwner.TreeData.TreeItemData
            .Select(model => model.RootItem)
            .ToList();

        private string[] ExistingNames => DialogueTreeItems
            .Select(item => item.DisplayName)
            .ToArray();

        private int SelectedIndex
        {
            get => DataOwner.TreeData.SelectedIndex;
            set => DataOwner.TreeData.SelectedIndex = value;
        }

        internal event Action<string> OnSituationSelected;
        internal event Action<DialogueTreeItem> OnTreeItemRemoved;
        internal event Action<DialogueTreeItem> OnTreeItemAdded;
        internal event Action<DialogueTreeItem> OnTreeItemRenamed;

        public void Rebuild()
        {
            CreateTreeView();
            RebuildTree(DataOwner.TreeData);
        }

        public void Search(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                SelectedIndex = TreeView.viewController.GetIndexForId(cachedSelectedId);
                RebuildTree(DataOwner.TreeData);
                return;
            }

            TreeUtilities.FilterTreeViewBy(value, TreeView, OnSelectionChanged);
        }

        internal void RemoveTreeItem(DialogueTreeItem item, int index)
        {
            if (TreeView.GetTreeCount() <= 1)
            {
                return;
            }

            TreeUtilities.ForceRefresh(TreeView, OnSelectionChanged);

            int id = TreeView.GetIdForIndex(index);
            if (!TreeView.TryRemoveItem(id))
            {
                return;
            }

            DataOwner.Owner.RegisterCompleteObjectUndo($"Removed tree item {item.DisplayName}");
            DataOwner.RemoveTreeItem(item);

            OnTreeItemRemoved?.Invoke(item);
        }

        internal void AddTreeItem(string defaultName, int index = -1, string idOverride = "")
        {
            int groupId = TreeView.GetIdForIndex(index);
            string name = ObjectNames.GetUniqueName(ExistingNames, defaultName);
            string parentId = TreeView.GetItemDataForId<DialogueTreeItem>(groupId).Id;

            DialogueTreeItem treeItem = new DialogueTreeItem(name);
            AddItemToGroup(treeItem, groupId);

            if (!idOverride.Equals(string.Empty))
            {
                treeItem.Id = idOverride;
            }

            DataOwner.Owner.RegisterCompleteObjectUndo($"Added tree item {name}");
            DataOwner.AddTreeItem(treeItem, TreeView.GetDepthOfItemById(groupId), parentId);

            OnTreeItemAdded?.Invoke(treeItem);
        }

        private void RebuildTree(TreeSaveData treeSaveData)
        {
            TreeItemModel[] rootModels = treeSaveData.TreeItemData
                .Where(itemSaveData => itemSaveData.Depth == 0)
                .ToArray();

            Dictionary<DialogueTreeItem, List<DialogueTreeItem>> parentToChildren = rootModels
                .ToDictionary(root => root.RootItem, value => new List<DialogueTreeItem>());

            int maxDepth = treeSaveData.TreeItemData.Max(model => model.Depth);
            for (int depth = 1; depth <= maxDepth; depth++)
            {
                var modelsAtDepth = treeSaveData.TreeItemData.Where(model => model.Depth == depth);
                foreach (var model in modelsAtDepth)
                {
                    var parentItem = 
                        treeSaveData.TreeItemData.Find(model => model.RootItem.Id.Equals(model.ParentId)).RootItem;

                    if (!parentToChildren.TryAdd(parentItem, new List<DialogueTreeItem>() { model.RootItem }))
                        parentToChildren[parentItem].Add(model.RootItem);
                }
            }

            var items = TreeUtilities.BuildTree(parentToChildren);
            TreeView.SetRootItems(items);

            TreeView.SetSelection(treeSaveData.SelectedIndex);
            TreeUtilities.ForceRefresh(TreeView, OnSelectionChanged);
        }

        private void AddItemToGroup(DialogueTreeItem treeItem, int groupID)
        {
            int itemId = GUID.Generate().GetHashCode();
            TreeView.AddItem(
                new TreeViewItemData<DialogueTreeItem>(itemId, treeItem), groupID
            );

            TreeUtilities.ForceRefresh(TreeView, OnSelectionChanged);
        }

        private void BindTreeViewItem(VisualElement element, int index)
        {
            DialogueTreeItem item = TreeView.GetItemDataForIndex<DialogueTreeItem>(index);

            Label renamableLabel = element.ElementAt(element.childCount - 1) as Label;
            renamableLabel.text = item.DisplayName;

            element.AddContextualMenu("Add Situation", _ => AddTreeItem(TreeViewConstants.DefaultSituationName, index));

            element.AddContextualMenu("Rename", _ => {

                VisualElementBuilder.Rename(renamableLabel, item.DisplayName, ExistingNames, finishedText => {
                    item.DisplayName = finishedText;
                    OnTreeItemRenamed?.Invoke(item);
                });
            });

            element.AddContextualMenu("Remove", _ => RemoveTreeItem(item, index));
        }

        private VisualElement MakeTreeViewItem()
        {
            VisualElement box = new VisualElement().WithHorizontalGrow();

            Image image = new Image() { image = TreeUtilities.GetSituationIcon() };
            image.WithMaxWidth(UIStyles.ListViewItemHeight);
            image.style.justifyContent = Justify.FlexStart;

            Label label = new Label();
            label.style.unityTextAlign = TextAnchor.MiddleLeft;

            box.Add(image);
            box.Add(label);
            return box;
        }

        private void CreateTreeView()
        {
            TreeView = new TreeView() 
            {
                reorderable = true,
                showAlternatingRowBackgrounds = AlternatingRowBackground.All
            };

            TreeView.viewDataKey = "dialogue-tree";
            TreeView.fixedItemHeight = UIStyles.ListViewItemHeight;
            TreeView.makeItem = MakeTreeViewItem;
            TreeView.bindItem = BindTreeViewItem;
            TreeView.itemIndexChanged += OnItemIndexChanged;
            TreeView.selectedIndicesChanged += OnSelectionChanged;
        }

        private void OnItemIndexChanged(int movedId, int parentId)
        {
            var movedItem = TreeView.GetItemDataForId<DialogueTreeItem>(movedId);
            if (movedItem == null)
            {
                Debug.LogError($"Could not tree item by moved id {movedId}");
                return;
            }

            var movedModel = DataOwner.TreeData.TreeItemData.Find(model => model.RootItem.Id.Equals(movedItem.Id));
            if (movedModel == null)
            {
                Debug.LogError($"Could not find model by moved id {movedItem.Id}");
                return;
            }

            var newParentItem = TreeView.GetItemDataForId<DialogueTreeItem>(parentId);
            if (newParentItem == null)
            {
                Debug.LogError($"Could not find parent of the moved element, parentId = {parentId}");
                return;
            }

            string modelsParentId = newParentItem.Id;
            movedModel.ParentId = modelsParentId;
        }

        private void OnSelectionChanged(IEnumerable<int> selectedIndices)
        {
            if (!selectedIndices.Any())
            {
                return;
            }

            int index = selectedIndices.First();
            if (SelectedIndex == index)
            {
                return;
            }

            if (DangerLogger.IsEditorInDanger())
            {
                TreeView.SetSelection(SelectedIndex);
                return;
            }

            SelectedIndex = index;

            DialogueTreeItem selectedSituation = 
                TreeView.GetItemDataForIndex<DialogueTreeItem>(SelectedIndex);

            if (selectedSituation == null)
            {
                return;
            }

            cachedSelectedId = TreeView.GetIdForIndex(SelectedIndex);
            OnSituationSelected?.Invoke(selectedSituation.Id);
        }
    }
}
