using Chocolate4.Dialogue.Edit.Saving;
using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Edit.Entities.Sidebar;
using Chocolate4.Dialogue.Edit.Entities.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Chocolate4.Dialogue.Runtime.Utilities;
using Chocolate4.Dialogue.Edit.Search;
using Chocolate4.Dialogue.Edit.Utilities;
using Chocolate4.Dialogue.Edit.Graph.Utilities;

namespace Chocolate4.Dialogue.Edit.Entities
{
    public class DialogueEntitiesView : VisualElement, IRebuildable, ISearchable
    {
        private List<DialogueEntity> displayedEntities;
        private List<DialogueEntity> cachedDialogueEntities;
        private EntityView entityView;

        public ListView ListView { get; private set; }

        internal void Initialize()
        {
            ResolveDependencies();
        }

        public void Rebuild()
        {
            cachedDialogueEntities = entitiesData.cachedEntities.ToList();
            displayedEntities = cachedDialogueEntities;

            CreateListView();

            if (!cachedDialogueEntities.IsNullOrEmpty())
            {
                ListView.SetSelection(0);
            }

            Sort(displayedEntities);
            RebuildListView();
        }

        public EntitiesData Save()
        {
            return new EntitiesData() { cachedEntities = cachedDialogueEntities };
        }

        public void Search(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                displayedEntities = cachedDialogueEntities;
            }
            else
            {
                displayedEntities = cachedDialogueEntities.Where(entity => entity.entityName.ToLower().Contains(value)).ToList();
            }

            ListView.itemsSource = displayedEntities;

            Sort(displayedEntities);
            RebuildListView();

            ListView.ClearSelection();
            ListView.SetSelection(0);
        }

        internal DialogueEntity AddEntity()
        {
            DialogueEntity entity = ScriptableObject.CreateInstance<DialogueEntity>();
            entity.Initialize();

            string[] existingNames = cachedDialogueEntities.Select(entity => entity.entityName).ToArray();
            string name = EntitiesUtilities.GetEntityName(entity, existingNames);

            entity.entityName = name;

            cachedDialogueEntities.Add(entity);
            RebuildListView();
            return entity;
        }

        private void ResolveDependencies()
        {
            cachedDialogueEntities = new List<DialogueEntity>();
            displayedEntities = new List<DialogueEntity>();
            entityView = new EntityView();
            contentContainer.Add(entityView);
            entityView.StretchToParentSize();
        }

        private void CreateListView()
        {
            ListView = new ListView() 
            { 
                fixedItemHeight = UIStyles.ListViewItemHeight, 
                selectedIndex = 0,
                itemsSource = displayedEntities,
                makeItem = MakeListViewItem,
                bindItem = (item, index) => BindItem(item, index),
            };

            ListView.selectionChanged += ListView_selectionChanged;
        }

        private VisualElement MakeListViewItem()
        {
            return new ListViewItem();
        }

        private void ListView_selectionChanged(IEnumerable<object> selectedItems)
        {
            if (selectedItems.IsNullOrEmpty())
            {
                return;
            }

            DialogueEntity entity = (DialogueEntity)selectedItems.First();

            if (!displayedEntities.Contains(entity))
            {
                return;
            }

            RebuildListView();
            entityView.Display(entity, (updatedEntity) => ListView.RefreshItem(ListView.selectedIndex));
        }

        private void BindItem(VisualElement item, int index)
        {
            ListViewItem listViewItem = item as ListViewItem;
            DialogueEntity entity = displayedEntities[index];

            listViewItem.AddContextualMenu("Remove", _ => RemoveEntity(item, entity));

            listViewItem.Initialize(entity);
        }

        private void RemoveEntity(VisualElement item, DialogueEntity entity)
        {
            cachedDialogueEntities.Remove(entity);
            item.RemoveFromHierarchy();

            RebuildListView();
        }

        private void Sort(List<DialogueEntity> entities)
        {
            entities.Sort(delegate (DialogueEntity a, DialogueEntity b)
            {
                return a.entityName.CompareTo(b.entityName);
            });
        }

        private void RebuildListView()
        {
            ListView.Rebuild();
        }
    }
}
