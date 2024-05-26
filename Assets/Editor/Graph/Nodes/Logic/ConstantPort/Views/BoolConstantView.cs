using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocolate4.Dialogue.Edit.Graph.Nodes
{
    public class BoolConstantView : ConstantViewGeneric<bool>
    {
        [SerializeField]
        private Toggle toggleField;

        public override VisualElement CreateControl()
        {
            toggleField = new Toggle();
            toggleField.RegisterValueChangedCallback(OnChangeToggle);
            Add(toggleField);

            return this;
        }

        public override void UpdateControl(bool value)
        {
            toggleField.value = value;
        }

        void OnChangeToggle(ChangeEvent<bool> evt)
        {
            //m_Slot.owner.owner.owner.RegisterCompleteObjectUndo("Toggle Change");
            onValueChanged?.Invoke(evt.newValue);
            //m_Slot.owner.Dirty(ModificationScope.Node);
        }
    }
}