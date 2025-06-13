using System;
using System.Threading.Tasks;

namespace UIManaging.Pages.Common.TabsManager
{
    public class TabModel
    {
        public event Action OnNameChangedEvent;

        public readonly int Index;
        public readonly bool ShowUpdateMarker;
        public readonly bool ContainsNew;
        public readonly bool ShowCount;
        public readonly Func<Task<int>> CountProvideFunc;
        public string Name { get; private set; }
        public bool Enabled { get; set; } = true;

        public TabModel(int index, string name)
        {
            Index = index;
            Name = name;
        }
        
        public TabModel(int index, string name, bool showCount, Func<Task<int>> countProvideFunc): this(index, name)
        {
            ShowCount = showCount;
            CountProvideFunc = countProvideFunc;
        }

        public TabModel(int index, string name, bool showUpdateMarker, bool containsNew) : this(index, name)
        {
            ShowUpdateMarker = showUpdateMarker;
            ContainsNew = containsNew;
        }

        public void SetName(string name)
        {
            Name = name;
            OnNameChangedEvent?.Invoke();
        }

        public override string ToString()
        {
            return $"{nameof(Index)}: {Index}, {nameof(Name)}: {Name}";
        }
    }
}