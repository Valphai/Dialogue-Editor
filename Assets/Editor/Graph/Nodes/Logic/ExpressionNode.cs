using Chocolate4.Dialogue.Edit.Graph.Utilities.DangerLogger;
using Chocolate4.Dialogue.Edit.Utilities;
using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Edit.Graph.Utilities;
using UnityEngine.UIElements;

namespace Chocolate4.Dialogue.Edit.Graph.Nodes
{
    public class ExpressionNode : BaseNode, ITextHolder
    {
        private TextField textField;

        public override string Name { get; set; } = "Expression Node";
        public string Text { get; set; }

        public override NodeModel Save()
        {
            NodeModel saveData = (NodeModel)base.Save();
            return new TextNodeSaveData() { text = Text, nodeSaveData = saveData };
        }

        public override void Load(NodeModel saveData)
        {
            base.Load(saveData);
            TextNodeSaveData textSaveData = (TextNodeSaveData)saveData;
            Text = textSaveData.text;

            textField.value = Text;
        }

        protected override void DrawTitle()
        {
            Label Label = new Label() { text = Name };

            Label.WithFontSize(UIStyles.FontSize)
                .WithMarginTop(UIStyles.LogicMarginTop);

            titleContainer.Insert(0, Label);
            titleContainer.WithLogicStyle();
        }

        protected override void AddExtraContent(VisualElement contentContainer)
        {
            textField = contentContainer
                .WithMaxWidth(UIStyles.MaxWidth)
                .WithNodeTextField(Text);
            textField.WithMinHeight(UIStyles.SmallTextFieldHeight);

            textField.RegisterCallback<FocusOutEvent>(evt => DangerDetector.SanitizeExpression(this, textField.value));
        }
    }
}
