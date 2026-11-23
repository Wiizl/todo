#if ANDROID
using AndroidX.RecyclerView.Widget;
#endif

namespace TodoLists.Services;

/// <summary>Long-press-and-drag reordering for a CollectionView whose ItemsSource is a plain List (not an ObservableCollection).</summary>
public static class DragReorder
{
    /// <param name="move">Must move the item from one index to the other in the list the CollectionView shows.
    /// The adapter is notified here, so the list must NOT raise collection-changed events for the view.</param>
    /// <param name="dropped">Called once when the finger is lifted and the order has changed.</param>
    public static void Attach(CollectionView view, Action<int, int> move, Action dropped)
    {
#if ANDROID
        if (view.Handler?.PlatformView is not RecyclerView rv) return;
        if (Attached.TryGetValue(rv, out _)) return;   // attach only once per RecyclerView
        Attached.Add(rv, new object());
        new ItemTouchHelper(new ReorderCallback(move, dropped)).AttachToRecyclerView(rv);
#endif
    }

#if ANDROID
    static readonly System.Runtime.CompilerServices.ConditionalWeakTable<RecyclerView, object> Attached = new();

    sealed class ReorderCallback : ItemTouchHelper.Callback
    {
        readonly Action<int, int> _move;
        readonly Action _dropped;
        bool _changed;

        public ReorderCallback(Action<int, int> move, Action dropped) { _move = move; _dropped = dropped; }

        public override bool IsLongPressDragEnabled => true;
        public override bool IsItemViewSwipeEnabled => false;

        public override int GetMovementFlags(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder)
            => MakeMovementFlags(ItemTouchHelper.Up | ItemTouchHelper.Down, 0);

        public override bool OnMove(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, RecyclerView.ViewHolder target)
        {
            int from = viewHolder.BindingAdapterPosition, to = target.BindingAdapterPosition;
            if (from < 0 || to < 0 || from == to) return false;

            _move(from, to);                                          // 1) update the data synchronously
            recyclerView.GetAdapter()?.NotifyItemMoved(from, to);     // 2) tell the RecyclerView right away
            _changed = true;
            return true;
        }

        public override void OnSwiped(RecyclerView.ViewHolder viewHolder, int direction) { }

        public override void OnSelectedChanged(RecyclerView.ViewHolder? viewHolder, int actionState)
        {
            base.OnSelectedChanged(viewHolder, actionState);
            if (viewHolder != null && actionState == ItemTouchHelper.ActionStateDrag)
            {
                viewHolder.ItemView.Alpha = 0.9f;
                viewHolder.ItemView.ScaleX = viewHolder.ItemView.ScaleY = 1.03f;
                try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
            }
        }

        public override void ClearView(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder)
        {
            base.ClearView(recyclerView, viewHolder);
            viewHolder.ItemView.Alpha = 1f;
            viewHolder.ItemView.ScaleX = viewHolder.ItemView.ScaleY = 1f;
            if (_changed) { _changed = false; _dropped(); }
        }
    }
#endif
}
