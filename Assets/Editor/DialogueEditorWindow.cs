using Chocolate4.Dialogue.Edit.Asset;
using Chocolate4.Dialogue.Edit.Graph.Utilities.DangerLogger;
using Chocolate4.Dialogue.Edit.Tree;
using Chocolate4.Dialogue.Edit.Utilities;
using Chocolate4.Dialogue.Edit.Graph.Utilities;
using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Runtime.Utilities;
using Chocolate4.Dialogue.Runtime.Entities;
using Chocolate4.Dialogue.Edit.Entities;
using Chocolate4.Dialogue.Edit.Graph;
using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocolate4.Dialogue.Edit
{
    public class DialogueEditorWindow : EditorWindow
    {
        public static DialogueEditorWindow Window { get; private set; }

        private TwoPaneSplitView splitView;
        private VisualElement mainSplitView;
        private VisualElement subTwoPanelView;
        private VisualElement subPanelSituationsContent;
        private VisualElement subPanelEntitiesContent;

        private bool hasInitialized;
        private Button saveButton;

        internal DialogueAssetManager DialogueAssetManager { get; private set; }
        public DialogueEntitiesView EntitiesView { get; private set; }
        internal DialogueGraphView GraphView { get; private set; }
        public DialogueTreeView DialogueTreeView { get; private set; }

        [SerializeField]
        private int selectedColumnIndex = 0;
        private int SelectedColumnIndex
        {
            get => selectedColumnIndex;
            set
            {
                if (selectedColumnIndex == value)
                {
                    return;
                }

                selectedColumnIndex = value;
                OpenSelectedView();
            }
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            string path = AssetDatabase.GetAssetPath(instanceId);
            if (!path.EndsWith(FilePathConstants.Extension, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            var obj = EditorUtility.InstanceIDToObject(instanceId);
            DialogueEditorAsset asset = obj as DialogueEditorAsset;
            if (asset == null)
            {
                return false;
            }

            Window = OpenEditor(asset, instanceId);

            return true;
        }

        public static DialogueEditorWindow OpenEditor(DialogueEditorAsset asset, int instanceId)
        {
            DialogueEditorWindow window = GetWindow<DialogueEditorWindow>();
            window.titleContent = new GUIContent("DialogueEditorWindow");
            window.SetAsset(asset, instanceId);

            window.Show();
            window.Focus();

            return window;
        }

        private void SetAsset(DialogueEditorAsset asset, int instanceId)
        {
            if (asset == null)
                return;

            string path = AssetDatabase.GetAssetPath(instanceId);

            EntitiesHolder entitiesDatabase = AssetDatabase.LoadAssetAtPath<EntitiesHolder>(path);

            DialogueAssetManager = new DialogueAssetManager(asset, instanceId, entitiesDatabase);
            PostInitialize();
        }

        private void OnEnable()
        {
            Window = this;
            if (!hasInitialized)
            {
                return;
            }

            Initialize();
        }

        private void OnDisable()
        {
            if (!hasInitialized)
            {
                return;
            }

            DialogueTreeView.OnTreeItemRenamed -= DialogueAssetManager.DialogueTreeView_OnTreeItemRenamed;
            DialogueTreeView.OnTreeItemRenamed -= GraphView.DialogueTreeView_OnTreeItemRenamed;
            DialogueTreeView.OnSituationSelected -= GraphView.DialogueTreeView_OnSituationSelected;
            DialogueTreeView.OnTreeItemRemoved -= GraphView.DialogueTreeView_OnTreeItemRemoved;


            DangerLogger.Clear();
            StoreData();
        }

        private void TrySave()
        {
            GraphView.ValidateForSave();

            if (DangerLogger.IsEditorInDanger())
            {
                return;
            }

            EntitiesData entitiesData = EntitiesView.Save();
            DialogueAssetManager.Save(entitiesData);
        }

        private void StoreData()
        {
            EntitiesData entitiesData = EntitiesView.Save();
            DialogueAssetManager.Store(entitiesData);
        }

        private void Initialize()
        {
            CreateElements();

            Rebuild();

            CreatePanels();

            OpenSelectedView();
        }

        private void PostInitialize()
        {
            Initialize();
            hasInitialized = true;
        }

        private void CreateElements()
        {
            EntitiesView = new DialogueEntitiesView();
            EntitiesView.Initialize();

            EntitiesView.WithFlexGrow().WithFlexShrink(1f);

            mainSplitView = new VisualElement();
            subTwoPanelView = new VisualElement();
            subPanelSituationsContent = new VisualElement().WithFlexGrow().WithFlexShrink(1f);
            subPanelEntitiesContent = new VisualElement().WithFlexGrow().WithFlexShrink(1f);

            GraphView = new DialogueGraphView();
            GraphView.Initialize();
            GraphView.WithFlexGrow();
            DialogueTreeView = new DialogueTreeView();

            DialogueTreeView.OnTreeItemRenamed += GraphView.DialogueTreeView_OnTreeItemRenamed;
            DialogueTreeView.OnSituationSelected += GraphView.DialogueTreeView_OnSituationSelected;
            DialogueTreeView.OnTreeItemRemoved += GraphView.DialogueTreeView_OnTreeItemRemoved;

            DialogueTreeView.OnTreeItemRenamed += DialogueAssetManager.DialogueTreeView_OnTreeItemRenamed;
            DialogueTreeView.OnTreeItemAdded += DialogueAssetManager.DialogueTreeView_OnTreeItemAdded;
        }

        private void DialogueTreeView_OnTreeItemAdded(DialogueTreeItem obj)
        {
            throw new NotImplementedException();
        }

        private void Rebuild()
        {
            DialogueAssetManager.Rebuild(DialogueTreeView, GraphView, EntitiesView);
        }

        private void CreatePanels()
        {
            rootVisualElement.Clear();

            AddSplitView();
            AddToolbar();
            AddHeaderColumns();
            AddGraphHeaderButtons();
            AddEntitiesHeaderButtons();
            AddTreeView();
            AddListView();
        }

        private void AddSplitView()
        {
            splitView = new TwoPaneSplitView(1, GraphConstants.TreeViewWindowWidth, TwoPaneSplitViewOrientation.Horizontal);

            splitView.Add(mainSplitView);
            splitView.Add(subTwoPanelView);

            mainSplitView.Add(GraphView);
            mainSplitView.Add(EntitiesView);

            rootVisualElement.Add(splitView);
        }

        private void AddHeaderColumns()
        {
            IMGUIContainer container = new IMGUIContainer(() => {

                EditorGUILayout.BeginHorizontal();

                SelectedColumnIndex = GUILayout.Toolbar(SelectedColumnIndex, new string[] { "Situations", "Entities" });

                EditorGUILayout.EndHorizontal();
            });

            subTwoPanelView.Add(container);

            subTwoPanelView.Add(subPanelSituationsContent);
            subTwoPanelView.Add(subPanelEntitiesContent);

            SelectSituationView();
        }

        private void AddTreeView()
        {
            subPanelSituationsContent.Add(DialogueTreeView.TreeView);
        }

        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();

            ToolbarSearchField searchField = new ToolbarSearchField();
            searchField
                .WithFlexGrow()
                .WithFlexShrink(1f)
                .WithMinWidth(GraphConstants.SaveButtonWidth);

            searchField.RegisterValueChangedCallback(OnSearch);

            saveButton = new Button() { text = "Save"}
                .WithOnClick(TrySave);
            saveButton.WithMaxWidth(GraphConstants.SaveButtonWidth);

            toolbar.Add(searchField);
            toolbar.Add(saveButton);

            subTwoPanelView.Add(toolbar);
        }

        private void OnSearch(ChangeEvent<string> evt)
        {
            string value = evt.newValue;

            if (mainSplitView.Contains(EntitiesView))
            {
                EntitiesView.Search(value);
            }
            else
            {
                DialogueTreeView.Search(value);
            }
        }

        private void AddListView()
        {
            subPanelEntitiesContent.Add(EntitiesView.ListView);
        }

        private void AddEntitiesHeaderButtons()
        {
            Action onClickAdd = () => EntitiesView.AddEntity();
            Action onClickSort = () => EntitiesView.Search(string.Empty);

            VisualElement buttonsContainer = new VisualElement().WithHorizontalGrow();
            VisualElementBuilder.AddHeaderButtons(onClickAdd, "New Entity", buttonsContainer);

            Button sortButton = buttonsContainer
                .WithButton(string.Empty);

            sortButton.Add(new Image() { image = EditorGUIUtility.IconContent("TreeEditor.Refresh").image });

            sortButton
                .WithOnClick(onClickSort)
                .WithMinWidth(GraphConstants.InsertButtonWidth);

            buttonsContainer.Add(sortButton);

            subPanelEntitiesContent.Add(buttonsContainer);
        }

        private void AddGraphHeaderButtons()
        {
            Action onClickAdd = () => DialogueTreeView.AddTreeItem(TreeViewConstants.DefaultSituationName);

            VisualElement buttonsContainer = new VisualElement().WithHorizontalGrow();
            VisualElementBuilder.AddHeaderButtons(onClickAdd, TreeViewConstants.DefaultSituationName, buttonsContainer);
            subPanelSituationsContent.Add(buttonsContainer);
        }

        private void SelectEntityView()
        {
            SelectSplitPanelView(
                EntitiesView, subPanelEntitiesContent,
                GraphView, subPanelSituationsContent
            );
        }

        private void SelectSituationView()
        {
            SelectSplitPanelView(
                GraphView, subPanelSituationsContent,
                EntitiesView, subPanelEntitiesContent
            );
        }

        private void SelectSplitPanelView(
            VisualElement view, VisualElement subPanel,
            VisualElement otherView, VisualElement otherSubPanel
        )
        {
            if (mainSplitView.Contains(otherView))
            {
                mainSplitView.Remove(otherView);
            }
            mainSplitView.Insert(0, view);

            view.style.display = DisplayStyle.Flex;
            subPanel.style.display = DisplayStyle.Flex;

            otherView.style.display = DisplayStyle.None;
            otherSubPanel.style.display = DisplayStyle.None;
        }

        private void OpenSelectedView()
        {
            if (selectedColumnIndex == GraphConstants.SituationViewIndex)
            {
                SelectSituationView();
            }
            else if (selectedColumnIndex == GraphConstants.EntityViewIndex)
            {
                SelectEntityView();
            }
        }
    }
}
