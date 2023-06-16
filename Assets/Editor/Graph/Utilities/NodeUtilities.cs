﻿using Chocolate4.Dialogue.Edit.Graph.Nodes;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;

namespace Chocolate4.Edit.Graph.Utilities
{
    public static class NodeUtilities
    {
        public enum PortType
        {
            Input,
            Output
        }

        public static List<BaseNode> GetConnections(Port port, PortType requestedPort)
        {
            IEnumerable<Edge> connections = port.connections;

            var connectionsMap = new List<BaseNode>();

            foreach (Edge connection in connections)
            {
                connectionsMap.Add(
                    (requestedPort == PortType.Input ? connection.input.node : connection.output.node) as BaseNode
                );
            }
            return connectionsMap;
        }

        public static bool IsConnectedTo(this BaseNode node, BaseNode another)
        {
            List<Port> inputPorts = node.inputContainer.Children()
                .OfType<Port>().ToList();

            List<Port> outputPorts = node.outputContainer.Children()
                .OfType<Port>().ToList();
            
            //IEnumerable<Port> anotherInputPorts = node.inputContainer.Children()
            //    .OfType<Port>();

            //IEnumerable<Port> anotherOutputPorts = node.outputContainer.Children()
            //    .OfType<Port>();

            IEnumerable<Port> allPorts = inputPorts.Concat(outputPorts);
            //IEnumerable<Port> anotherAllPorts = anotherInputPorts.Concat(anotherOutputPorts);

            return allPorts.Any(port => port.IsConnectedTo(another));
        }
    }
}