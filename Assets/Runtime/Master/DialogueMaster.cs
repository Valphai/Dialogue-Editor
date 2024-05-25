using Chocolate4.Dialogue.Runtime.Asset;
using Chocolate4.Dialogue.Runtime.Entities;
using Chocolate4.Dialogue.Runtime.Master.Collections;
using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Runtime.Utilities;
using Chocolate4.Dialogue.Runtime.Utilities.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Chocolate4.Dialogue.Runtime
{
    [DisallowMultipleComponent]
    public class DialogueMaster : MonoBehaviour
    {
        [SerializeField]
        private DialogueEditorAsset dialogueAsset;
        [SerializeField]
        private bool autoInitialize;

        private NodeModel currentNode;
        private SituationSaveData currentSituation;
        private string previousSituationId;
        private List<NodeModel> allNodes;
        private int selectedChoice;
        private ParseAdapter parseAdapter;
        private IDialogueMasterCollection collection;

        [field:SerializeField]
        public EntitiesHolder EntitiesDatabase { get; private set; }

        public bool HasInitialized { get; private set; }
        public string CurrentSituationName => FindSituationName(currentSituation);

        private void Awake()
        {
            if (autoInitialize)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Starts a given situation at StartNode. To proceed to next node you have to explicitly invoke <seealso cref="NextDialogueElement"/>.
        /// </summary>
        /// <param name="situationName">The name of the situation, it can be obtained from <seealso cref="collection"/></param>
        public void StartSituation(string situationName)
        {
            if (!HasInitialized)
            {
                Debug.LogError("Dialogue Master has not initialized yet!", this);
                return;
            }
            
            SituationSaveData newSituation = FindSituationByName(situationName);

            if (currentSituation != null)
            {
                previousSituationId = currentSituation.situationId;
                Debug.Log($"There was an active situation: {previousSituationId}.\nTrying to start new situation: {situationName}");
            }
            else
            {
                previousSituationId = newSituation.situationId;
            }

            currentSituation = newSituation;

            if (currentSituation == null)
            {
                Debug.LogError($"No situation was found with the name: {situationName}.");
                return;
            }

            currentNode = 
                currentSituation.nodeData.Find(node => node.nodeType.Contains(NodeConstants.StartNode));

            allNodes =
                TypeExtensions.MergeFieldListsIntoOneImplementingType<NodeModel, SituationSaveData>(currentSituation);
        }

        /// <summary>
        /// Gets next dialogue element as set in the editor graph.
        /// </summary>
        /// <returns>Information about the next dialogue element in the <seealso cref="DialogueNodeInfo"/> format.</returns>
        public DialogueNodeInfo NextDialogueElement()
        {
            if (!HasInitialized)
            {
                Debug.LogError("Tried to get next dialogue element but DialogueMaster was not initialized! This is not allowed.", this);
                return null;
            }

            if (currentSituation == null)
            {
                Debug.LogError("Tried to get next dialogue element but current situation was null! This is not allowed.");
                return null;
            }

            TryGetNextNode(ref currentNode);

            while (currentNode.IsNodeOfType(
                NodeConstants.ConditionNode, NodeConstants.ExpressionNode, 
                NodeConstants.FromSituationNode, NodeConstants.ToSituationNode,
                NodeConstants.EventPropertyNode, NodeConstants.StartNode)
            )
            {
                TryGetNextNode(ref currentNode);
            }

            if (currentNode.IsNodeOfType(NodeConstants.EndNode))
            {
                return DialogueNodeInfo.SituationEndedInfo();
            }
            else if (currentNode.IsNodeOfType(NodeConstants.DialogueNode))
            {
                DialogueNodeSaveData dialogueNodeSaveData = (currentNode as DialogueNodeSaveData);

                EntitiesDatabase.TryGetEntity(dialogueNodeSaveData.speakerIdentifier, out DialogueEntity speaker);
                return DialogueNodeInfo.DialogueInfo(dialogueNodeSaveData.text, speaker);
            }
            else if (currentNode.IsNodeOfType(NodeConstants.ChoiceNode))
            {
                List<Nodes.DialogueChoice> choices = (currentNode as ChoiceNodeSaveData).choices;
                return DialogueNodeInfo.ChoiceNodeInfo(choices.Select(choice => choice.text).ToList());
            }

            return DialogueNodeInfo.SituationEndedInfo();
        }

        /// <summary>
        /// When the player receives a choice node, this method needs to be used before they make the decision.
        /// </summary>
        /// <param name="index">Index of selected choice.</param>
        public void SetSelectedChoice(int index)
        {
            selectedChoice = index;
        }

        /// <summary>
        /// Allows you to get the collection of <seealso cref="dialogueAsset"/> and casts it to a given type.
        /// </summary>
        /// <returns>Collection of situations, variables and events defined in the asset.</returns>
        public T GetCollection<T>() where T : class, IDialogueMasterCollection
        {
            if (collection == null)
            {
                Debug.LogWarning($"Collection has not yet initialized. ");
                return null;
            }

            if (collection is not T)
            {
                Debug.LogWarning($"Tried to access collection {typeof(T)} but it was of type {collection.GetType()}");
                return null;
            }

            return (T)collection;
        }

        public void Initialize()
        {
            if (dialogueAsset == null)
            {
                Debug.LogError("dialogueAsset is missing in the DialogueMaster! Assign it in the inspector.", this);
                return;
            }

            if (EntitiesDatabase == null)
            {
                Debug.LogError("entitiesDatabase is missing in the DialogueMaster! Assign it in the inspector.", this);
                return;
            }

            EntitiesDatabase.Reload();
            collection = CreateCollection();
            parseAdapter = new ParseAdapter(collection);

            HasInitialized = true;
        }

        private IDialogueMasterCollection CreateCollection()
        {
            List<Type> collectionTypes = 
                TypeExtensions.GetTypes<IDialogueMasterCollection>(FilePathConstants.Chocolate4).ToList();

            string collectionName = FilePathConstants.GetCollectionName(dialogueAsset.fileName);
            Type assetMatchedCollectionType  = 
                collectionTypes.Find(type => type.Name.Equals(collectionName));

            return (IDialogueMasterCollection)Activator.CreateInstance(assetMatchedCollectionType);
        }

        private bool TryGetNextNode(ref NodeModel current)
        {
            NodeModel previousNode = current;
            current = NextNode();
            if (current == null)
            {
                current = GetStartOrFromNodeFrom(previousSituationId);
                Debug.Log($"{previousNode.GetNodeType()} could not find next node in {FindSituationName(currentSituation)} situation. Redirecting to {current.GetNodeType()}.");
                return false;
            }

            return true;
        }

        private NodeModel NextNode()
        {
            if (currentNode == null)
            {
                return null;
            }

            List<PortData> outputPortDataCollection = currentNode.outputPortDataCollection;

            string nodeType = currentNode.GetNodeType();

            if (NodeUtilities.IsNodeOfType(nodeType,
                NodeConstants.StartNode, 
                NodeConstants.DialogueNode, NodeConstants.FromSituationNode)
            )
            {
                return FindNode(outputPortDataCollection.First().otherNodeID);
            }
            else if (NodeUtilities.IsNodeOfType(nodeType, NodeConstants.ChoiceNode))
            {
                return HandleChoiceNode(outputPortDataCollection);
            }
            else if (NodeUtilities.IsNodeOfType(nodeType,  NodeConstants.ConditionNode))
            {
                return HandleConditionNode(outputPortDataCollection);
            }
            else if (NodeUtilities.IsNodeOfType(nodeType, NodeConstants.ExpressionNode))
            {
                return HandleExpressionNode(outputPortDataCollection);
            }
            else if (NodeUtilities.IsNodeOfType(nodeType, NodeConstants.ToSituationNode))
            {
                return HandleToSituationNode();
            }
            else if (NodeUtilities.IsNodeOfType(nodeType, NodeConstants.EventPropertyNode))
            {
                return HandleEventPropertyNode(outputPortDataCollection);
            }

            Debug.LogWarning($"There is nothing left after node {currentNode.GetNodeType()}!");
            return null;
        }

        private NodeModel HandleEventPropertyNode(List<PortData> outputPortDataCollection)
        {
            string eventName = FindPropertyNameById(((PropertyNodeSaveData)currentNode).propertyID);
            collection.CollectionType.GetMethod(
                $"Invoke{eventName}", BindingFlags.NonPublic | BindingFlags.Instance
            ).Invoke(collection, null);

            return FindNode(outputPortDataCollection.First().otherNodeID);
        }

        private NodeModel HandleToSituationNode()
        {
            string currentSituationId = currentSituation.Id;
            string nextSituation = FindSituationName(((SituationTransferNodeSaveData)currentNode).otherSituationId);
            StartSituation(nextSituation);
            return GetStartOrFromNodeFrom(currentSituationId);
        }

        private NodeModel GetStartOrFromNodeFrom(string situationId)
        {
            NodeModel fromNode = FindNode<SituationTransferNodeSaveData>(
                node => node.otherSituationId.Equals(situationId)
                && node.IsNodeOfType(NodeConstants.FromSituationNode)
            );

            return fromNode ?? FindNode<NodeModel>(node => node.IsNodeOfType(NodeConstants.StartNode));
        }

        private NodeModel HandleExpressionNode(List<PortData> outputPortDataCollection)
        {
            parseAdapter.EvaluateSetExpressions(((TextNodeSaveData)currentNode).text);

            return FindNode(outputPortDataCollection.First().otherNodeID);
        }

        private NodeModel HandleConditionNode(List<PortData> outputPortDataCollection)
        {
            if (parseAdapter.EvaluateConditions(((TextNodeSaveData)currentNode).text))
            {
                return FindNode(outputPortDataCollection.First().otherNodeID);
            }

            return FindNode(outputPortDataCollection.Last().otherNodeID);
        }

        private NodeModel HandleChoiceNode(List<PortData> outputPortDataCollection)
        {
            int count = outputPortDataCollection.Count;
            if (count == 1)
            {
                return FindNode(outputPortDataCollection.First().otherNodeID);
            }

            try
            {
                return FindNode(outputPortDataCollection[selectedChoice].otherNodeID);
            }
            catch (ArgumentOutOfRangeException)
            {
                Debug.LogError($"Selected choice index {selectedChoice} was greater than number of choices {count}! Aborting.");
                return null;
            }
        }

        private NodeModel FindNode<T>(Predicate<T> match) where T : NodeModel
        {
            if (currentSituation == null)
            {
                return null;
            }

            return TypeExtensions.GetListOfTypeFrom<T, SituationSaveData>(currentSituation).Find(match);
        }

        private NodeModel FindNode(string id)
        {
            if (currentSituation == null)
            {
                return null;
            }

            return allNodes.Find(node => node.nodeId == id);
        }

        private string FindPropertyNameById(string id)
        {
            return dialogueAsset.graphSaveData.blackboardSaveData.
                dialoguePropertiesSaveData.Find(property => property.id.Equals(id)).displayName;
        }

        private SituationSaveData FindSituationByName(string situationName)
        {
            TreeItemSaveData matchedData = 
                dialogueAsset.treeSaveData.treeItemData.Find(itemData => itemData.rootItem.displayName.Equals(situationName));
            try
            {
                return dialogueAsset.graphSaveData.situationSaveData.Find(data => data.Id == matchedData.rootItem.id);
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException($"Situation with the name {situationName} was not found.");
            }
        }

        private string FindSituationName(string id)
        {
            TreeItemSaveData matchedData =
                dialogueAsset.treeSaveData.treeItemData.Find(itemData => itemData.rootItem.id.Equals(id));

            if (matchedData == null)
            {
                Debug.LogWarning("Current situation is invalid.");
                return string.Empty;
            }

            return matchedData.rootItem.displayName;
        }

        private string FindSituationName(SituationSaveData saveData) => FindSituationName(saveData.Id);
    }
}