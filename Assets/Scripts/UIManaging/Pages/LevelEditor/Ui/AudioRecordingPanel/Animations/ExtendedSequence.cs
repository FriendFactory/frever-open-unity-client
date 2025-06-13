using System;
using DG.Tweening;

namespace UIManaging.Pages.LevelEditor.Ui
{
    public class ExtendedSequence: IDisposable
    {
        public bool HasPlayed { get; private set; }
        public bool IsComplete => _sequence.IsComplete();
        public Sequence Sequence => _sequence;
        
        private readonly Sequence _sequence;

        public ExtendedSequence(Sequence sequence)
        {
            _sequence = sequence;
        }
        
        public void PlayForward(bool instant = false)
        {
            HasPlayed = true;

            if (IsComplete)
            {
                _sequence.Rewind();
            }

            if (instant)
            {
                _sequence.Complete(); 
                return;
            }
            
            _sequence.PlayForward();
        }
        
        public void PlayBackwards(bool instant = false)
        {
            HasPlayed = true;
            
            if (!IsComplete)
            {
                _sequence.Complete();
            }
            
            if (instant)
            {
                _sequence.Rewind();
                return;
            }
            
            _sequence.PlayBackwards();
        }
        
        public void Complete()
        {
            _sequence.Complete();
        }

        public void Rewind()
        {
            _sequence.Rewind();
        }

        public void Dispose()
        {
            _sequence?.Kill();
        }
    }
}