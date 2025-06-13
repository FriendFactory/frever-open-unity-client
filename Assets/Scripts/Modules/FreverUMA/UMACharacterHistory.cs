using System.Collections.Generic;

namespace Modules.FreverUMA
{
    public class UMACharacterHistory : LinkedList<string>
    {
        private readonly int _size;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public UMACharacterHistory(int size) 
        {
            _size = size;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Push(string recipe) 
        {
            if(Last != null && Last.Value.Equals(recipe)) 
                return;

            AddLast(recipe);
           
            if (Count > _size) 
                RemoveFirst();
        }

        public string Peek() 
        {
            return Last.Value;
        }

        public string Pop() 
        {
            if (Count <= 0) 
                return null;

            var last = Last.Value;
            RemoveLast();
            return last;

        }
    }
}

