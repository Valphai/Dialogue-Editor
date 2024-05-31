using Chocolate4.Dialogue.Edit.Utilities;
using Chocolate4.Dialogue.Runtime.Saving;

namespace Chocolate4.Dialogue.Edit.Graph.Nodes
{
    internal class StartNode : BaseNode
    {
        public StartNode(NodeModel model) : base(model)
        {
        }

        public override string Name { get; set; } = "Start Node";

        protected override void DrawTitle()
        {
            base.DrawTitle();
            titleContainer.WithTransferStyle();
        }

        protected override void DrawInputPort()
        {

        }
    }
}
