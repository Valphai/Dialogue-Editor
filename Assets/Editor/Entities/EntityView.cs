using Chocolate4.Dialogue.Edit.Utilities;
using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Edit.Entities.Utilities;
using Chocolate4.Dialogue.Edit.Graph.Utilities;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Chocolate4.Dialogue.Edit.Entities
{
    public class EntityView : VisualElement
    {
        private DialogueEntity currentEntity;
        private string lastPropPath;
        private Image previewImage;
        private Action<DialogueEntity> onValueChanged;

        public EntityView()
        {
            this
                .WithVerticalGrow()
                .WithMarginLeft(EntitiesConstants.MarginBig)
                .WithMarginRight(EntitiesConstants.MarginSmall)
                .WithMarginTop(EntitiesConstants.MarginSmall)
                .WithMarginBot(EntitiesConstants.MarginSmall);
        }

        internal void Display(DialogueEntity entity, Action<DialogueEntity> onValueChanged)
        {
            Clear();

            this.onValueChanged = onValueChanged;
            currentEntity = entity;

            SerializedObject serializedEntity = new SerializedObject(entity);
            SerializedProperty property = serializedEntity.GetIterator();

            lastPropPath = string.Empty;
            while (property.Next(enterChildren: true))
            {
                bool alreadyDrawn = property.propertyPath.Contains(lastPropPath);
                if (!string.IsNullOrEmpty(lastPropPath) && alreadyDrawn)
                {
                    continue;
                }

                lastPropPath = property.propertyPath;
                if (lastPropPath.Contains("m_") || lastPropPath.Contains(nameof(DialogueEntity.Id)))
                {
                    continue;
                }

                PropertyField propertyField = new PropertyField(property);
                propertyField.BindProperty(property);

                if (lastPropPath.Contains(nameof(DialogueEntity.entityImage)))
                {
                    HandlePreviewImage(propertyField);
                    continue;
                }

                propertyField.RegisterValueChangeCallback(OnSourceValueChanged);
                Add(propertyField);
            }
        }

        private void HandlePreviewImage(PropertyField propertyField)
        {
            VisualElement previewContainer = new VisualElement()
                .WithHorizontalGrow();

            propertyField.WithFlexGrow();
            propertyField.style.alignSelf = Align.FlexEnd;

            previewImage = new Image();
            previewImage
                .WithWidth(EntitiesConstants.ImageWidth)
                .WithHeight(EntitiesConstants.ImageHeight)
                .WithBackgroundColor(UIStyles.DefaultDarkerColor);

            propertyField.RegisterValueChangeCallback(OnPreviewSourceValueChanged);

            previewContainer.Add(propertyField);
            previewContainer.Add(previewImage);
            Add(previewContainer);
        }

        private void OnSourceValueChanged(SerializedPropertyChangeEvent evt)
        {
            onValueChanged?.Invoke(currentEntity);
        }

        private void OnPreviewSourceValueChanged(SerializedPropertyChangeEvent evt)
        {
            previewImage.image = EntitiesUtilities.GetEntityImage(currentEntity);
            onValueChanged?.Invoke(currentEntity);
        }
    }
}