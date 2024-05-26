using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocolate4.Dialogue.Edit.Graph.Nodes
{
    public abstract class ConstantViewGeneric<T> : VisualElement, IConstantViewControlCreator
    {
        [SerializeField]
        protected Action<T> onValueChanged;

        public void Initialize(Action<T> onValueChanged)
        {
            this.onValueChanged = onValueChanged;
        }

        public abstract VisualElement CreateControl();
        public abstract void UpdateControl(T value);
    }
}