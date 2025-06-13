using EnhancedUI.EnhancedScroller;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using ThirdPackagesExtends.Zenject;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    internal abstract class EventsTimelineView<T> : MonoBehaviour, IEnhancedScrollerDelegate where T : EventTimelineItemView
    {
        private const float CELL_OFFSET = -3f;
        private const float SCROLLER_OFFSET = 0f;
        private const float TWEEN_TIME = 0.1f;
        
        [SerializeField] private float _cellSize = 128f;
        [SerializeField] private EnhancedScrollerCellView _cellViewPrefab;
        [SerializeField] protected EnhancedScroller _enhancedScroller;
       
        [Inject] protected ILevelManager LevelManager;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        protected abstract EventsTimelineModel TimelineModel { get; }
        protected PostRecordEditorPageModel EditorPageModel { get; private set; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        [Inject]
        [UsedImplicitly]
        private void Construct(PostRecordEditorPageModel editorPageModel)
        {
            EditorPageModel = editorPageModel;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _enhancedScroller.Delegate = this;
            _enhancedScroller.cellViewInstantiated = OnCellViewInstantiated;
            _enhancedScroller.cellViewVisibilityChanged += CellViewVisibilityChanged;
            TimelineModel.Initialized += OnInitialized;
        }
        
        protected virtual void OnDestroy()
        {
            TimelineModel.Initialized -= OnInitialized;
            _enhancedScroller.cellViewVisibilityChanged -= CellViewVisibilityChanged;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Show()
        {
            gameObject.SetActive(true);
            OnShown();
        }
        
        public virtual void Hide()
        {
            gameObject.SetActive(false);
            OnHidden();
        }

        public void RemoveEventViews()
        {
            _enhancedScroller.ClearAll();
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return TimelineModel?.EventTimelineItemModels?.Length ?? 0;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _cellSize;
        }

        public virtual EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = _enhancedScroller.GetCellView(_cellViewPrefab);
            var eventItemView = cellView.GetComponent<T>();
            var eventThumbnail = cellView.GetComponentInChildren<EventThumbnail>();

            if (eventThumbnail != null)
            {
                eventThumbnail.gameObject.InjectDependenciesIfNeeded();
            }
            
            eventItemView.Initialize(TimelineModel.EventTimelineItemModels[dataIndex]);
            cellView.gameObject.name = $"{_cellViewPrefab.name}{dataIndex}";
            
            return cellView;
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected abstract void OnCellViewInstantiated(EnhancedScroller enhancedScroller, EnhancedScrollerCellView cellView);

        protected virtual void CellViewVisibilityChanged(EnhancedScrollerCellView cellView)
        {
        }
        
        protected void JumpTo(int eventIndex)
        {
            //workaround for shifting event thumbnails in preview timeline
            //for proper shifting to next event we use SCROLLER_OFFSET, to last event - 0
            var isLastEvent = LevelManager.CurrentLevel.GetLastEvent().LevelSequence == eventIndex;
            _enhancedScroller.JumpToDataIndex(eventIndex, SCROLLER_OFFSET, isLastEvent ? 0 : CELL_OFFSET,
                                              true, EnhancedScroller.TweenType.linear, TWEEN_TIME);
        }

        protected virtual void OnShown()
        {
        }
        
        protected virtual void OnHidden()
        {
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnInitialized()
        {
            _enhancedScroller.ReloadData();
        }
    }
}
