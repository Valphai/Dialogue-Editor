using Chocolate4.Dialogue.Runtime.Saving;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chocolate4.Dialogue.Runtime.Utilities
{
    public static class NodeUtilities
    {
        public static string GetNodeType(this NodeModel nodeModel)
        {
            return nodeModel.nodeType.Split('.').Last();
        }
        
        public static bool IsNodeOfType(string nodeType, string type)
        {
            return nodeType.Equals(type);
        }

        public static bool IsNodeOfType(this NodeModel nodeModel, string type)
        {
            string nodeType = nodeModel.GetNodeType();
            return IsNodeOfType(nodeType, type);
        }
        
        public static bool IsNodeOfType(string nodeType, params string[] types)
        {
            return types.Any(type => IsNodeOfType(nodeType, type));
        }

        public static bool IsNodeOfType(this NodeModel nodeModel, params string[] types)
        {
            string nodeType = nodeModel.GetNodeType();
            return IsNodeOfType(nodeType, types);
        }

        public static List<NodeModel> GetParents(this NodeModel nodeModel, Func<string, NodeModel> findNode)
        {
            List<NodeModel> parents = new List<NodeModel>();

            foreach (PortData portData in nodeModel.inputPortDataCollection)
            {
                string otherNodeID = portData.otherNodeID;
                if (string.IsNullOrEmpty(otherNodeID))
                {
                    parents.Add(null);
                }

                parents.Add(findNode(otherNodeID));
            }

            return parents;
        }
    }
}