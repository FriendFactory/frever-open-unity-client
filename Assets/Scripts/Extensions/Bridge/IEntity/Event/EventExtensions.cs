using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.Common.Files;
using Event = Models.Event;

namespace Extensions
{
    public static partial class EventExtensions 
    {
        private static void ThrowExceptionIfEventIsNull(Event ev)
        {
            if (ev == null) throw new ArgumentNullException(nameof(ev));
        }

        public static bool HasFaceAnimation(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetFaceAnimations().Any();
        }

        public static bool IsGroupFocus(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.TargetCharacterSequenceNumber == -1;
        }
        
        public static void SetFiles(this Event ev, FileInfo[] files)
        {
            ThrowExceptionIfEventIsNull(ev);
            
            if (ev.Files == null)
            {
                ev.Files = new List<FileInfo>();
            }
            
            ev.Files.Clear();
            ev.Files.AddRange(files);
        }
    }
}