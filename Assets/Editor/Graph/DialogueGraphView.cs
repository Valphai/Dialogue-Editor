using Chocolate4.Dialogue.Edit.Asset;
using Chocolate4.Dialogue.Edit.Graph.BlackBoard;
using Chocolate4.Dialogue.Edit.Graph.Nodes;
using Chocolate4.Dialogue.Edit.Graph.Utilities;
using Chocolate4.Dialogue.Edit.Graph.Utilities.DangerLogger;
using Chocolate4.Dialogue.Edit.Saving;
using Chocolate4.Dialogue.Edit.Tree;
using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Runtime.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using SearchWindow = Chocolate4.Dialogue.Edit.Search.SearchWindow;

namespace Chocolate4.Dialogue.Edit.Graph
{
    internal class DialogueGraphView : GraphView, IRebuildable
    {
        internal const string DefaultGroupName = "Dialogue Group";

        private BlackboardProvider blackboardProvider;
        private SearchWindow searchWindow;

        internal DialogueDataOwner DataOwner { get; private set; }
        internal DragSelectablesHandler DragSelectablesHandler { get; private set; }

        internal void Initialize()
        {
            HandleCallbacks();
            ResolveDependencies();

            AddManipulators();
            AddGridBackground();

            AddStyles();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            evt.menu.AppendAction(
                $"Add Group",
                actionEvent => CreateGroup(GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)), 
                AddGroupFlags
            );
            
            evt.menu.AppendAction(
                $"Convert To Property",
                actionEvent => ConvertToProperty(), 
                ConvertToPropertyStatus
            );
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            BaseNode startNode = (BaseNode)startPort.node;

            ports.ForEach(port => {

                if (startPort.node == port.node)
                {
                    return;
                }

                if (startPort.direction == port.direction)
                {
                    return;
                }

                if (startPort.portType != port.portType)
                {
                    return;
                }

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        public void Rebuild()
        {
            if (!DataOwner.TryFindActiveSituation(out SituationSaveData situationData))
            {
                return;
            }

            contentViewContainer.transform.position = situationData.GraphViewPosition;
            contentViewContainer.transform.scale = situationData.GraphViewZoom;

            DeleteElements(graphElements);
            RebuildGraph(situationData);

            BlackboardSaveData blackboardSaveData = DataOwner.BlackboardData;
            if (blackboardSaveData != null)
            {
                blackboardProvider.Rebuild(blackboardSaveData);
            }
        }

        internal Vector2 GetLocalMousePosition(Vector2 mousePosition)
        {
            Vector2 mousePositionWorld = mousePosition;
            return contentViewContainer.WorldToLocal(mousePositionWorld);
        }

        internal void AddNode(Vector2 startingPosition, BaseNode node)
        {
            node.Initialize(startingPosition);
            node.Draw();
            node.PostInitialize();

            AddElement(node);
        }

        internal BaseNode CreateNode(Vector2 startingPosition, Type nodeType)
        {
            BaseNode node = (BaseNode)Activator.CreateInstance(nodeType);
            AddNode(startingPosition, node);

            return node;
        }

        internal CustomGroup CreateGroup(Vector2 startingPosition)
        {
            CustomGroup group = new CustomGroup()
            {
                title = DefaultGroupName
            };

            group.SetPosition(new Rect(startingPosition, Vector2.zero));

            AddElement(group);

            foreach (GraphElement element in selection)
            {
                if (element is not BaseNode)
                {
                    continue;
                }

                if (element is StartNode)
                {
                    continue;
                }

                BaseNode node = (BaseNode)element;
                group.AddToGroup(node);
            }

            return group;
        }

        internal void DialogueTreeView_OnSituationSelected(string newSituationId)
        {
            bool foundNewSituation = DataOwner.TryFindSituation(newSituationId, out SituationSaveData situationData);

            DataOwner.Owner.RegisterCompleteObjectUndo($"Selected situation {situationData.DisplayName}");
            DataOwner.ActiveSituationId = newSituationId;

            DeleteElements(graphElements);
            if (foundNewSituation)
            {
                RebuildGraph(situationData);
            }

            blackboardProvider.UpdatePropertyBinds();
        }

        internal void DialogueTreeView_OnTreeItemRemoved(DialogueTreeItem treeItem)
        {
            if (!DataOwner.TryFindSituation(treeItem.Id, out SituationSaveData cachedSituationData))
            {
                return;
            }

            DataOwner.Owner.RegisterCompleteObjectUndo($"Removed situation {cachedSituationData.DisplayName}");
            if (!DataOwner.SituationsData.Remove(cachedSituationData))
            {
                Debug.LogError($"Situation {cachedSituationData.DisplayName} could not be removed.");
                return;
            }
        }

        internal void DialogueTreeView_OnTreeItemRenamed(DialogueTreeItem treeItem)
        {
            PerformOnAllGraphElementsOfType<SituationTransferNode>(node => node.UpdatePopup(treeItem));
        }

        internal void ValidateForSave()
        {
            List<BaseNode> starterNodes = graphElements
                .Where(graphElement => graphElement is StartNode || graphElement is FromSituationNode)
                .Select(node => (BaseNode)node)
                .ToList();

            Func<BaseNode, bool> onEveryNextNode = portOwner => {

                if (portOwner.IsMarkedDangerous)
                {
                    return false;
                }

                List<Port> outputPorts = portOwner.outputContainer.Query<Port>().ToList();
                if (outputPorts.All(port => port.IsConnectedToAny()))
                {
                    return false;
                }


                DangerLogger.WarnDanger($"{portOwner} disrupts the flow of the dialogue. Disconnect it from From Situation Nodes/Start Node or ensure all its output ports are connected to another node.", portOwner);

                DangerLogger.MarkNodeDangerous(portOwner, () => {

                    // need to refresh outputPorts every time because of dynamic choice node.
                    outputPorts = portOwner.outputContainer.Query<Port>().ToList();

                    if (outputPorts.All(port => port.IsConnectedToAny())
                        || starterNodes.All(starterNode => !starterNode.IsConnectedAtAnyPointTo(portOwner))
                    )
                    {
                        DangerLogger.UnmarkNodeDangerous(portOwner);
                        return true;
                    }

                    return false;
                });

                return true;
            };

            foreach (BaseNode starterNode in starterNodes)
            {
                if (Utilities.NodeUtilities.FindMatchingNode(starterNode, Direction.Output, onEveryNextNode))
                {
                    return;
                }
            }
        }

        internal void PerformOnAllGraphElementsOfType<T>(Action<T> onElementFound) where T : GraphElement
        {
            GraphUtilities.PerformOnGraphElementsOfType<T>(graphElements, graphElement => {
                onElementFound?.Invoke(graphElement);
            });
        }

        private void OnElementsRemovedFromGroup(Group group, IEnumerable<GraphElement> removedElements)
        {
            GraphUtilities.PerformOnGraphElementsOfType<BaseNode>(removedElements, element => {
                element.GroupId = string.Empty;
            });
        }

        private void OnElementsAddedToGroup(Group group, IEnumerable<GraphElement> newElements)
        {
            GraphUtilities.PerformOnGraphElementsOfType<BaseNode>(newElements, element => {

                if (element is StartNode or EndNode)
                {
                    group.RemoveElement(element);
                    return;
                }

                element.GroupId = ((CustomGroup)group).Id;
            });
        }

        private void ResolveDependencies()
        {
            DragSelectablesHandler = new DragSelectablesHandler();

            searchWindow = ScriptableObject.CreateInstance<SearchWindow>();
            searchWindow.Initialize(this);

            blackboardProvider = new BlackboardProvider(this);
            Add(blackboardProvider.Blackboard);
        }

        private void HandleCallbacks()
        {
            deleteSelection = OnDeleteSelection;
            graphViewChanged = OnGraphViewChange;
            nodeCreationRequest = OnNodeCreationRequest;

            elementsAddedToGroup = OnElementsAddedToGroup;
            elementsRemovedFromGroup = OnElementsRemovedFromGroup;
            groupTitleChanged += OnGroupTitleChanged;

            serializeGraphElements += CutCopyOperation;
            unserializeAndPaste += PasteOperation;

            RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!DataOwner.TryFindActiveSituation(out var foundSituation))
            {
                return;
            }

            foundSituation.GraphViewPosition = contentViewContainer.transform.position;
            foundSituation.GraphViewZoom = contentViewContainer.transform.scale;
        }

        private void OnGroupTitleChanged(Group group, string newTitle)
        {
            if (group is not CustomGroup customGroup)
            {
                Debug.LogError($"Could not rename group {group.title}!");
                return;
            }

            DataOwner.Owner.RegisterCompleteObjectUndo($"Renamed group to {newTitle}");
            customGroup.Rename(newTitle);
        }

        private void RebuildGraph(SituationSaveData situationData)
        {
            List<NodeModel> nodeModels = situationData.NodeData;
            if (!nodeModels.IsNullOrEmpty())
            {
                AddStartingNodes();
                return;
            }

            RebuildNodesAndGroups(nodeModels, situationData.GroupData, out List<BaseNode> _, out List<CustomGroup> _);
        }

        private List<BaseNode> CreateNodes(List<NodeModel> nodeModels)
        {
            List<BaseNode> nodes = new List<BaseNode>();
            List<Type> nodeTypes = TypeExtensions.GetTypes<BaseNode>(FilePathConstants.Chocolate4).ToList();
            foreach (NodeModel nodeModel in nodeModels)
            {
                Type matchedType = nodeTypes.First(type => type.ToString().Contains(nodeModel.NodeType));
                BaseNode node = CreateNode(nodeModel.Position, matchedType);
                node.Load(nodeModel);
                nodes.Add(node);
            }

            return nodes;
        }

        private List<CustomGroup> CreateGroups(List<GroupModel> groupData)
        {
            List<CustomGroup> groups = new List<CustomGroup>();
            foreach (GroupModel data in groupData)
            {
                CustomGroup group = CreateGroup(data.Position);
                group.Load(data);
                groups.Add(group);
            }

            return groups;
        }

        private void RebuildGroupMembers(List<BaseNode> nodes, List<CustomGroup> groups)
        {
            foreach (CustomGroup group in groups)
            {
                List<BaseNode> nodesInGroup = nodes.Where(node => node.GroupId.Equals(group.Id)).ToList();

                foreach (BaseNode node in nodesInGroup)
                {
                    group.AddToGroup(node);
                }
            }
        }

        private void RebuildConnections(List<BaseNode> nodes)
        {
            foreach (BaseNode node in nodes)
            {
                List<PortModel> allOutputPortData = node.GetAllPortData(Direction.Output);
                foreach (PortModel portData in allOutputPortData)
                {
                    if (string.IsNullOrEmpty(portData.thisPortName))
                    {
                        continue;
                    }

                    string childID = portData.otherNodeID;
                    List<BaseNode> connections =
                        nodes.Where(childNode => childNode.Id == childID).ToList();

                    List<Port> ports = node.Query<Port>().ToList();
                    Port outputPort = node.outputContainer.Q<Port>(portData.thisPortName);

                    ConnectPorts(outputPort, portData, connections);
                }
            }
        }

        private void ConnectPorts(Port port, PortModel portData, List<BaseNode> connections)
        {
            foreach (BaseNode otherNode in connections)
            {
                Port otherPort = otherNode.inputContainer.Q<Port>(portData.otherPortName);
                if (otherPort == null)
                {
                    continue;
                }

                Edge edge = port.ConnectTo(otherPort);

                AddElement(edge);
                otherNode.RefreshPorts();
            }
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new DragAndDropManipulator(this));
        }

        private void AddGridBackground()
        {
            GridBackground background = new GridBackground();

            Insert(0, background);
        }

        private void AddStyles()
        {
            StyleSheet graphStyleSheet = (StyleSheet)EditorGUIUtility.Load(FilePathConstants.GetEditorVisualAssetPath(FilePathConstants.dialogueGraphStyle));
            StyleSheet nodeStyleSheet = (StyleSheet)EditorGUIUtility.Load(FilePathConstants.GetEditorVisualAssetPath(FilePathConstants.dialogueNodeStyle));

            styleSheets.Add(graphStyleSheet);
            styleSheets.Add(nodeStyleSheet);
        }

        private void ConvertToProperty()
        {
            IEnumerable<IPropertyNode> selectedPropertyNodes = selection.OfType<IPropertyNode>();
            IEnumerable<Type> propertyTypes = TypeExtensions.GetTypes<IDialogueProperty>(FilePathConstants.Chocolate4);

            foreach (IPropertyNode propertyNode in selectedPropertyNodes)
            {
                Type typeToMake =
                    propertyTypes.First(type => type.ToString().Contains(propertyNode.PropertyType.ToString()));

                IDialogueProperty property = (IDialogueProperty)Activator.CreateInstance(typeToMake);

                blackboardProvider.AddProperty(property, true);
                propertyNode.BindToProperty(property);
            }

            DangerLogger.TryFixErrorsAutomatically();
        }

        private DropdownMenuAction.Status ConvertToPropertyStatus(DropdownMenuAction _)
        {
            IEnumerable<IPropertyNode> selectedProperties = selection.OfType<IPropertyNode>();

            if (selectedProperties.IsNullOrEmpty())
            {
                return DropdownMenuAction.Status.Hidden;
            }

            if (selectedProperties.Any(node => node.IsBoundToProperty))
            {
                return DropdownMenuAction.Status.Hidden;
            }

            return DropdownMenuAction.Status.Normal;
        }

        private GraphViewChange OnGraphViewChange(GraphViewChange change)
        {
            if (change.elementsToRemove.IsNullOrEmpty())
            {
                return change;
            }

            foreach (GraphElement elementToRemove in change.elementsToRemove)
            {
                if (elementToRemove.userData is IDialogueProperty)
                {
                    blackboardProvider.HandlePropertyRemove(elementToRemove.userData as IDialogueProperty);
                    return change;
                }
            }

            return change;
        }

        private void OnDeleteSelection(string operationName, AskUser askUser)
        {
            HashSet<GraphElement> cannotRemove = new HashSet<GraphElement>();
            foreach (ISelectable selectable in selection)
            {
                if (selectable is not GraphElement)
                {
                    continue;
                }

                if (selectable is StartNode)
                {
                    cannotRemove.Add(selectable as GraphElement);
                }

                if (selectable is ChoiceNode choiceNode)
                {
                    List<Port> outputPorts = choiceNode.outputContainer.Query<Port>().ToList();
                    foreach (Port port in outputPorts)
                    {
                        choiceNode.RemoveChoicePort(port);
                    }
                }
            }

            foreach (GraphElement element in cannotRemove)
            {
                selection.Remove(element);
            }

            foreach (GraphElement element in selection)
            {
                if (element is IDangerCauser dangerCauser)
                {
                    DangerLogger.UnmarkNodeDangerous((BaseNode)dangerCauser);
                }
            }
            
            //graph.owner.RegisterCompleteObjectUndo(operationName);
            DeleteSelection();
            ClearSelection();

            DangerLogger.TryFixErrorsAutomatically();
        }

        private void PasteOperation(string operationName, string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return;
            }

            ClearSelection();

            SituationSaveData saveData = JsonUtility.FromJson<SituationSaveData>(data);
            List<NodeModel> cache =
                TypeExtensions.MergeFieldListsIntoOneImplementingType<NodeModel, SituationSaveData>(saveData);

            Vector2 center = GetLocalMousePosition(DialogueEditorWindow.Window.rootVisualElement.contentRect.center);

            GraphUtilities.GeneratePastedIds(saveData.GroupData, cache);
            RebuildNodesAndGroups(cache, saveData.GroupData, out List<BaseNode> nodes, out List<CustomGroup> _);

            nodes.ForEach(node => {
                Vector2 position = node.GetPositionRaw();
                node.SetPosition(new Rect(position + center, Vector2.zero));
            });

            selection.AddRange(nodes);
        }

        private void RebuildNodesAndGroups(
            List<NodeModel> nodeModels, List<GroupModel> groupModels,
            out List<BaseNode> nodes, out List<CustomGroup> groups
        )
        {
            nodes = CreateNodes(nodeModels);
            RebuildConnections(nodes);

            groups = CreateGroups(groupModels);
            RebuildGroupMembers(nodes, groups);
        }

        private string CutCopyOperation(IEnumerable<GraphElement> elements)
        {
            elements = elements.ToList();
            List<NodeModel> nodeCopyCache = new List<NodeModel>();
            List<GroupModel> groupCopyCache = new List<GroupModel>();

            Dictionary<string, List<NodeModel>> oldGroupIds = new Dictionary<string, List<NodeModel>>();

            foreach (GraphElement element in elements)
            {
                if (element is FromSituationNode)
                {
                    continue;
                }

                if (element is BaseNode baseNode)
                {
                    NodeModel dataHolder = baseNode.Save();
                    nodeCopyCache.Add(dataHolder);
                    continue;
                }

                if (element is CustomGroup customGroup)
                {
                    GroupModel groupData = customGroup.Save();
                    groupCopyCache.Add(groupData);
                    continue;
                }
            }

            Vector2 center = GetLocalMousePosition(DialogueEditorWindow.Window.rootVisualElement.contentRect.center);
            nodeCopyCache.ForEach(data => data.Position -= center);

            SituationSaveData situationCache = new SituationSaveData("cache", nodeCopyCache, groupCopyCache);
            return JsonUtility.ToJson(situationCache);
        }

        private void OnNodeCreationRequest(NodeCreationContext ctx)
        {
            UnityEditor.Experimental.GraphView.SearchWindow.Open(
                new SearchWindowContext(ctx.screenMousePosition), searchWindow
            );
        }

        private void AddStartingNodes()
        {
            Vector2 offset = Vector2.right * GraphConstants.StartingNodesOffset;
            Vector2 middlePoint = new Vector2(
                contentContainer.contentRect.width * .5f, contentContainer.contentRect.height * .5f
            );

            CreateNode(middlePoint - offset, typeof(StartNode));
            CreateNode(middlePoint + offset, typeof(EndNode));
        }

        private DropdownMenuAction.Status AddGroupFlags(DropdownMenuAction _)
        {
            BaseNode[] baseNodes = selection.OfType<BaseNode>().ToArray();
            if (
                baseNodes.Any(node => node is StartNode or EndNode)
                || baseNodes.Any(node => !string.IsNullOrEmpty(node.GroupId))
            )
            {
                return DropdownMenuAction.Status.Hidden;
            }

            return DropdownMenuAction.Status.Normal;
        }
    }
}
