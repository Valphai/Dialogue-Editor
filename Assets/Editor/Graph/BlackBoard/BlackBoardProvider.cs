using Chocolate4.Dialogue.Edit.Asset;
using Chocolate4.Dialogue.Edit.Graph.Nodes;
using Chocolate4.Dialogue.Edit.Graph.Utilities.DangerLogger;
using Chocolate4.Dialogue.Edit.Utilities;
using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Runtime.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocolate4.Dialogue.Edit.Graph.BlackBoard
{
    internal class BlackboardProvider
    {
        private Dictionary<string, BlackboardRow> propertyRows;
        private BlackboardSection section;
        private List<IDialogueProperty> properties;

        public Blackboard Blackboard { get; private set; }
        internal DialogueDataOwner DataOwner { get; private set; }

        public BlackboardProvider(GraphView graphView)
        {
            Blackboard = new Blackboard(graphView)
            {
                scrollable = true,
                title = "Globals",
                editTextRequested = EditTextRequested,
                addItemRequested = AddItemRequested,
            };
        }

        public void Rebuild(BlackboardSaveData blackboardData)
        {
            Build();

            if (blackboardData.dialoguePropertiesSaveData.IsNullOrEmpty())
            {
                return;
            }

            foreach (DialoguePropertyModel propertyModel in blackboardData.dialoguePropertiesSaveData)
            {
                IDialogueProperty property = propertyModel.PropertyType switch
                {
                    PropertyType.Bool => new BoolDialogueProperty(propertyModel, DataOwner),
                    PropertyType.Integer => new IntegerDialogueProperty(propertyModel, DataOwner),
                    PropertyType.Event => new EventDialogueProperty(propertyModel),
                    _ => throw new NotImplementedException()
                };

                AddProperty(property);

                if (property is IExpandableDialogueProperty expandableProperty)
                {
                    expandableProperty.UpdateConstantView();
                }

                BlackboardField field = (BlackboardField)propertyRows[property.Id].userData;

                field.text = property.DisplayName;
                UpdateNodesWith(property);
            }
        }

        internal void HandlePropertyRemove(IDialogueProperty deletedProperty)
        {
            if (!propertyRows.TryGetValue(deletedProperty.Id, out BlackboardRow row))
            {
                return;
            }

            row.RemoveFromHierarchy();
            properties.Remove(deletedProperty);

            Blackboard.graphView.graphElements.ForEach(element => {

                if (!ElementIsDialogueProperty(element, deletedProperty, out IPropertyNode propertyNode))
                {
                    return;
                }

                propertyRows.Remove(deletedProperty.Id);
                propertyNode.UnbindFromProperty();

                BaseNode dangerCauser = (BaseNode)propertyNode;
                DangerLogger.ErrorDanger(
                    "Deleted property created an empty node! Remove the node or convert it back to property.", dangerCauser
                );

                DangerLogger.MarkNodeDangerous(
                    dangerCauser, () => {
                        if (propertyNode.IsBoundToProperty)
                        {
                            DangerLogger.UnmarkNodeDangerous(dangerCauser);
                            return true;
                        }
                        return false;
                    }
                );
            });
        }

        internal void UpdatePropertyBinds()
        {
            foreach (IDialogueProperty property in properties)
            {
                UpdateNodesWith(property);
            }
        }

        internal void AddProperty(IDialogueProperty property, bool create = false, int index = -1)
        {
            if (propertyRows.ContainsKey(property.Id))
            {
                return;
            }

            if (create)
            {
                property.DisplayName = GetSanitizedPropertyName(property.DisplayName);
            }

            BlackboardField field;

            if (property is IDraggableProperty draggableProperty)
            {
                field = new BlackboardDraggableField(
                    (DialogueGraphView)Blackboard.graphView,
                    property.DisplayName,
                    property.PropertyType.ToString()
                )
                { userData = draggableProperty };
            }
            else
            {
                field = new BlackboardField(
                    null,
                    property.DisplayName,
                    property.PropertyType.ToString()
                )
                { userData = property };
            }

            BlackboardRow row;
            if (property is IExpandableDialogueProperty expandableProperty)
            {
                VisualElement expandedAssignValueField = CreateRowExpanded(expandableProperty);
                row = new BlackboardRow(field, expandedAssignValueField);
            }
            else
            {
                row = new BlackboardRow(field, null);
            }

            row.userData = field;

            if (index < 0)
            {
                index = propertyRows.Count;
            }

            if (index == propertyRows.Count)
            {
                section.Add(row);
            }
            else
            {
                section.Insert(index, row);
            }

            propertyRows[property.Id] = row;

            properties.Add(property);
            if (create)
            {
                row.expanded = true;
                DataOwner.Owner.RegisterCompleteObjectUndo($"Created a property {property.DisplayName}");

                field.OpenTextEditor();
            }
        }

        private string GetSanitizedPropertyName(string propertyName)
        {
            string[] existingNames = properties.Select(property => property.DisplayName).ToArray();
            return ObjectNames.GetUniqueName(existingNames, propertyName);
        }

        private void AddItemRequested(Blackboard blackboard)
        {
            var gm = new GenericMenu();
            gm.AddItem(new GUIContent("Integer"), false, () => AddProperty(new IntegerDialogueProperty(DataOwner.CreatePropertyModel(), DataOwner), true));
            gm.AddItem(new GUIContent("Bool"), false, () => AddProperty(new BoolDialogueProperty(DataOwner.CreatePropertyModel(), DataOwner), true));
            gm.AddItem(new GUIContent("Event"), false, () => AddProperty(new EventDialogueProperty(DataOwner.CreatePropertyModel()), true));
            gm.ShowAsContext();
        }

        private void EditTextRequested(Blackboard blackboard, VisualElement visualElement, string newText)
        {
            BlackboardField field = (BlackboardField)visualElement;
            IDialogueProperty property = (IDialogueProperty)field.userData;

            newText = newText.Sanitize();
            if (string.IsNullOrEmpty(newText) || newText.Equals(property.DisplayName))
            {
                return;
            }

            newText = GetSanitizedPropertyName(newText);

            DataOwner.Owner.RegisterCompleteObjectUndo($"Changed property name {property.DisplayName} to {newText}");
            property.DisplayName = newText;
            field.text = newText;
            UpdateNodesWith(property);
        }

        private void UpdateNodesWith(IDialogueProperty property)
        {
            Blackboard.graphView.graphElements.ForEach(element => {

                if (!ElementIsDialogueProperty(element, property, out IPropertyNode propertyNode))
                {
                    return;
                }

                propertyNode.BindToProperty(property);
            });
        }

        private bool ElementIsDialogueProperty(GraphElement element, IDialogueProperty property, out IPropertyNode propertyNode)
        {
            propertyNode = null;
            if (element is not IPropertyNode)
            {
                return false;
            }

            propertyNode = (IPropertyNode)element;
            string propertyId = propertyNode.PropertyId;

            if (string.IsNullOrEmpty(propertyId))
            {
                return false;
            }

            if (propertyId != property.Id)
            {
                return false;
            }

            return true;
        }

        private VisualElement CreateRowExpanded(IExpandableDialogueProperty property)
        {
            VisualElement expandedAssignValueField = new VisualElement()
                .WithFlexGrow()
                .WithHorizontalGrow();

            Label startValueLabel = new Label() { text = "Start value = " };

            IConstantViewControlCreator constantView = property.ToConstantView();
            VisualElement constantViewControl = constantView.CreateControl().WithFlexGrow();

            expandedAssignValueField.Add(startValueLabel);
            expandedAssignValueField.Add(constantViewControl);

            return expandedAssignValueField;
        }

        private void Build()
        {
            properties = new List<IDialogueProperty>();
            propertyRows = new Dictionary<string, BlackboardRow>();
            if (Blackboard.Contains(section))
                Blackboard.Remove(section);

            section = new BlackboardSection { headerVisible = false };
            Blackboard.Add(section);
        }
    }
}
