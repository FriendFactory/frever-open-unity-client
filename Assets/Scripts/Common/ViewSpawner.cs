using System.Collections.Generic;
using System.Linq;
using Abstract;
using ThirdPackagesExtends.Zenject;
using UnityEngine;

#pragma warning disable CS0649

namespace Components
{
    public class ViewSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        private readonly List<GameObject> _views = new List<GameObject>();

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public GameObject Prefab => prefab;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public virtual IEnumerable<V> Spawn<T, V>(IEnumerable<T> models, bool disableAfter = true) where V : BaseContextDataView<T>
        {
            var list = new List<V>();

            for (int i = 0; i < models.Count(); i++)
            {
                var newView = GetView<T, V>(models.ElementAt(i), i);
                list.Add(newView);
            }

            if (disableAfter)
            {
                DisableAllAfterIndex(models.Count() - 1);
            }
            
            return list;
        }

        public virtual void CleanUp<T, V>(bool clearViews = true) where V : BaseContextDataView<T>
        {
            foreach (var go in _views)
            {
                var view = go.GetComponent<V>();
                
                if (view == null || !view.IsInitialized) return;
                
                view.CleanUp();
            }

            if (clearViews)
            {
                //we don't need to destroy objects here, because they will be destroyed by Enhanced Scroller package
                _views.Clear();
            }
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected void DisableAllAfterIndex(int index)
        {
            for (var i = index + 1; i < _views.Count; i++)
            {
                _views[i].SetActive(false);
            }
        }
        
        protected V GetView<T, V>(T model, int positionIndex) where V : BaseContextDataView<T>
        {
            var view = _views.Count > positionIndex 
                ? _views[positionIndex].GetComponent<V>() 
                : SpawnNew<V, T>();
            
            view.gameObject.SetActive(true);
            view.gameObject.InjectDependenciesIfNeeded();
            view.Initialize(model);
            
            return view;
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private V SpawnNew<V,T>()where V : BaseContextDataView<T>
        {
            var spawned = Instantiate(prefab, transform).GetComponent<V>();
            _views.Add(spawned.gameObject);
            return spawned;
        }
    }
}
