using Chocolate4.Dialogue.Edit.Tree;
using Chocolate4.Dialogue.Edit.Utilities;
using Chocolate4.Dialogue.Runtime.Saving;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocolate4.Dialogue.Edit.Graph.Nodes
{
    public abstract class SituationTransferNode : BaseNode
    {
        private PopupField<DialogueTreeItem> popupField;

        public string NextSituationId { get; private set; } = string.Empty;

        public override NodeModel Save()
        {
            NodeModel saveData = (NodeModel)base.Save();
            return new SituationTransferNodeSaveData() { otherSituationId = NextSituationId, nodeSaveData = saveData };
        }

        public override void Load(NodeModel saveData)
        {
            base.Load(saveData);
            SituationTransferNodeSaveData situationSaveData = (SituationTransferNodeSaveData)saveData;
            NextSituationId = situationSaveData.otherSituationId;

            IEnumerable<DialogueTreeItem> situations = DialogueEditorWindow.Window.DialogueTreeView.dialogueTreeItems;
            
            DialogueTreeItem selectedSituation;
            try
            {
                selectedSituation = situations.First(treeItem => treeItem.id.Equals(NextSituationId));
            }
            catch (InvalidOperationException)
            {
                selectedSituation = situations.First();
                Debug.Log($"Removed situation with id: {NextSituationId} was navigated to by {this}. Changing the situation to: {selectedSituation.displayName}");
                NextSituationId = selectedSituation.id;
            }

            popupField.SetValueWithoutNotify(selectedSituation);
        }

        internal void UpdatePopup(DialogueTreeItem treeItem)
        {
            if (NextSituationId != treeItem.id)
            {
                return;
            }

            popupField.SetValueWithoutNotify(treeItem);
        }

        protected override void DrawTitle()
        {
            base.DrawTitle();
            titleContainer.WithTransferStyle();
        }

        protected override void AddExtraContent(VisualElement contentContainer)
        {
            DialogueTreeView dialogueTreeView = DialogueEditorWindow.Window.DialogueTreeView;
            CreatePopup(dialogueTreeView);

            contentContainer.Add(popupField);
        }

        private void CreatePopup(DialogueTreeView dialogueTreeView)
        {
            Func<DialogueTreeItem, string> formatListItemCallback = (selectedTreeItem) => selectedTreeItem.displayName;
            Func<DialogueTreeItem, string> formatSelectedValueCallback = selectedTreeItem => {
                
                NextSituationId = selectedTreeItem.id;
                SanitizeSelectedSituation();
                
                return selectedTreeItem.displayName;
            };

            popupField = new PopupField<DialogueTreeItem>(
                dialogueTreeView.dialogueTreeItems, 0, 
                formatSelectedValueCallback, formatListItemCallback
            );
        }

        protected virtual void SanitizeSelectedSituation()
        {
        }
    }
}
