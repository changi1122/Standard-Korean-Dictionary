using System;
using System.Collections;
using System.Collections.Specialized;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Interop;

namespace 표준국어대사전.Models
{
    /// <summary>
    /// ItemsSource에 바인딩할 컬렉션을 감싸는 어댑터.
    /// </summary>
    /// <remarks>
    /// UWP(.NET + CsWinRT) 모드에서는 ObservableCollection&lt;T&gt;, List&lt;T&gt; 같은 BCL 컬렉션이
    /// XAML에 넘어갈 때 IBindableVector를 노출하지 못해 ItemsSource 설정이 E_INVALIDARG로 실패한다.
    /// (CsWinRT 소스 생성기가 제네릭 BCL 컬렉션과 그 파생 클래스의 vtable을 만들지 않음)
    /// 이 클래스는 제네릭 기반 타입 없이 IBindableObservableVector를 직접 구현하므로 생성기가 vtable을 만든다.
    /// 원본이 INotifyCollectionChanged를 구현하면 변경 사항을 VectorChanged로 전달한다.
    /// </remarks>
    public sealed partial class BindableVector : IBindableObservableVector
    {
        private readonly IList source;

        public BindableVector(IList source)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            if (source is INotifyCollectionChanged notifier)
                notifier.CollectionChanged += OnSourceCollectionChanged;
        }

        /// <summary>감싸고 있는 원본 컬렉션</summary>
        public IList Source => source;

        public event BindableVectorChangedEventHandler VectorChanged;

        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add when e.NewStartingIndex >= 0:
                    for (int i = 0; i < e.NewItems.Count; i++)
                        Raise(CollectionChange.ItemInserted, e.NewStartingIndex + i);
                    break;
                case NotifyCollectionChangedAction.Remove when e.OldStartingIndex >= 0:
                    for (int i = 0; i < e.OldItems.Count; i++)
                        Raise(CollectionChange.ItemRemoved, e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace when e.NewStartingIndex >= 0:
                    for (int i = 0; i < e.NewItems.Count; i++)
                        Raise(CollectionChange.ItemChanged, e.NewStartingIndex + i);
                    break;
                default:
                    Raise(CollectionChange.Reset, 0);
                    break;
            }
        }

        private void Raise(CollectionChange change, int index)
        {
            VectorChanged?.Invoke(this, new VectorChangedEventArgs(change, (uint)index));
        }

        public object this[int index] { get => source[index]; set => source[index] = value; }
        public bool IsFixedSize => source.IsFixedSize;
        public bool IsReadOnly => source.IsReadOnly;
        public int Count => source.Count;
        public bool IsSynchronized => source.IsSynchronized;
        public object SyncRoot => source.SyncRoot;
        public int Add(object value) => source.Add(value);
        public void Clear() => source.Clear();
        public bool Contains(object value) => source.Contains(value);
        public void CopyTo(Array array, int index) => source.CopyTo(array, index);
        public IEnumerator GetEnumerator() => source.GetEnumerator();
        public int IndexOf(object value) => source.IndexOf(value);
        public void Insert(int index, object value) => source.Insert(index, value);
        public void Remove(object value) => source.Remove(value);
        public void RemoveAt(int index) => source.RemoveAt(index);

        private sealed partial class VectorChangedEventArgs : IVectorChangedEventArgs
        {
            public VectorChangedEventArgs(CollectionChange change, uint index)
            {
                CollectionChange = change;
                Index = index;
            }

            public CollectionChange CollectionChange { get; }
            public uint Index { get; }
        }
    }
}
