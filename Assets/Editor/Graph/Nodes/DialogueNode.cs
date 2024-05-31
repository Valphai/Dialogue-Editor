using Chocolate4.Dialogue.Edit.Utilities;
using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Edit.Graph.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using Chocolate4.Dialogue.Edit.Entities.Utilities;

namespace Chocolate4.Dialogue.Edit.Graph.Nodes
{
    internal class DialogueNode : BaseNode, ITextHolder
    {
        private TextField textField;
        private DialogueEntity speaker;
        private Image entityPortrait;
        private Label entityLabel;

        public override string Name { get; set; } = "Dialogue Node";
        public string Text { get; set; }

        public override NodeModel Save()
        {
            NodeModel saveData = (NodeModel)base.Save();
            return new DialogueNodeSaveData() 
            { 
                text = Text, 
                speakerIdentifier = speaker == null ? string.Empty : speaker.Id, 
                nodeSaveData = saveData };
        }

        public override void Load(NodeModel saveData)
        {
            base.Load(saveData);
            DialogueNodeSaveData dialogueNodeSaveData = (DialogueNodeSaveData)saveData;

            DialogueEditorWindow.Window.DialogueAssetManager.EntitiesDatabase.TryGetEntity(dialogueNodeSaveData.speakerIdentifier, out speaker);
            SelectEntity(speaker);
            
            Text = dialogueNodeSaveData.text;
            textField.value = Text;
        }

        public override void PostInitialize()
        {
            base.PostInitialize();
            SelectEntity(speaker);
        }

        protected override void DrawTitle()
        {
            base.DrawTitle();

            List<VisualElement> titleChildren = titleContainer.Children().ToList();
            titleChildren.ForEach(child => titleContainer.Remove(child));
            (entityLabel, entityPortrait) = VisualElementBuilder.MakeHeaderWithEntity(titleContainer, titleChildren);

            IMGUISelectorMaker selectorMaker = new IMGUISelectorMaker();

            IMGUIContainer imguiContainer = selectorMaker.MakeIMGUISelector<DialogueEntity>((selectedEntity) => {
                SelectEntity(selectedEntity);
            });

            entityPortrait.Add(imguiContainer);
        }

        private void SelectEntity(DialogueEntity selectedEntity)
        {
            speaker = selectedEntity;
            entityPortrait.image = EntitiesUtilities.GetEntityImage(speaker);
            entityLabel.text = speaker == null ? string.Empty : speaker.entityName;
        }

        protected override void AddExtraContent(VisualElement contentContainer)
        {
            textField = contentContainer
                .WithMinHeight(UIStyles.MaxHeight)
                .WithMaxWidth(UIStyles.MaxWidth)
                .WithNodeTextField(Text, evt => Text = evt.newValue);
        }
    }
}
