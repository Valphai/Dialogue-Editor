using Chocolate4.Dialogue.Edit.Asset;
using Chocolate4.Dialogue.Edit.Graph.Nodes;
using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Runtime.Utilities;
using System;

namespace Chocolate4.Dialogue.Edit.Graph.BlackBoard
{
    [Serializable]
    public abstract class ValueDialogueProperty<TValue, TConstantView> : IDialogueProperty, IExpandableDialogueProperty
        where TConstantView : ConstantViewGeneric<TValue>, new()
    {
        private TConstantView constantView;
        private DialogueDataOwner dataOwner;

        protected DialoguePropertyModel model;

        public string Id => model.Id;
        public abstract PropertyType PropertyType { get; }

        public string DisplayName
        {
            get => string.IsNullOrEmpty(model.DisplayName) ? Id.ToString() : model.DisplayName;
            set => model.DisplayName = value;
        }

        protected ValueDialogueProperty(DialoguePropertyModel model, DialogueDataOwner dataOwner)
        {
            this.model = model;
            this.dataOwner = dataOwner;

            DisplayName = model.DisplayName ?? PropertyType.ToString();
            model.PropertyType = PropertyType;
        }

        public IConstantViewControlCreator ToConstantView()
        {
            constantView = new TConstantView();
            constantView.Initialize((value) => {
                DataOwner.Owner.RegisterCompleteObjectUndo($"Changed value of {model.DisplayName} to {value}");
                model.Value = value.ToString();
            });
            return constantView;
        }

        public void UpdateConstantView() => 
            constantView.UpdateControl((TValue)Convert.ChangeType(model.Value, typeof(TValue)));
    }
}