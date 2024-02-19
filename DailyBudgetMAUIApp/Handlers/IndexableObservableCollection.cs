using System.Collections.ObjectModel;
using System.Collections.Specialized;


namespace DailyBudgetMAUIApp.Handlers
{
    public class IndexableObservableCollection<T> : ObservableCollection<T> where T : IIndexable
    {
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    for (int i = 0; i < e.NewItems.Count; i++)
                        if (e.NewItems[i] is IIndexable indexableItem)
                            indexableItem.Index = e.NewStartingIndex + i;
                    break;
            }
            base.OnCollectionChanged(e);
        }
    }
}
