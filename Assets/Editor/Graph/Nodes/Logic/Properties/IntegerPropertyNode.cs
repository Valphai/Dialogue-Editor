﻿using Chocolate4.Dialogue.Runtime.Utilities;
using UnityEditor.Experimental.GraphView;

namespace Chocolate4.Dialogue.Edit.Graph.Nodes
{
    public class IntegerPropertyNode : PropertyNode<int>
    {
        private ConstantViewGeneric<int> constantViewGeneric;

        public override string Name { get; set; } = "Integer";
        public override PropertyType PropertyType { get; protected set; } = PropertyType.Integer;

        protected override ConstantPortInput CreateConstantPortInput()
        {
            constantViewGeneric = new IntegerConstantView((value) => Value = value);
            return new ConstantPortInput(constantViewGeneric);
        }

        protected override void UpdateConstantViewGenericControl(int value)
        {
            constantViewGeneric.UpdateControl(value);
        }
    }
}