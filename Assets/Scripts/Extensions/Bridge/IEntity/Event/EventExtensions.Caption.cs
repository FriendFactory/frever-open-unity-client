using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Level.Full;
using Models;

namespace Extensions
{
    /// <summary>
    ///     Caption related
    /// </summary>
    public static partial class EventExtensions
    {
        public static CaptionFullInfo GetCaption(this Event ev, long id)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.Caption?.FirstOrDefault(x=>x.Id == id);
        }
        
        public static void SetCaptions(this Event ev, ICollection<CaptionFullInfo> caption)
        {
            ThrowExceptionIfEventIsNull(ev);
            ev.RemoveCaptions();
            if(caption == null) return;
            
            foreach (var captionFullInfo in caption)
            {
                ev.Caption.Add(captionFullInfo);
            }
        }

        public static void RemoveCaptions(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            ev.Caption.Clear();
        }

        public static bool HasCaption(this Event ev)
        {
            return ev.Caption != null && ev.Caption.Any();
        }

        public static int GetCaptionsCount(this Event ev)
        {
            return !ev.HasCaption() ? 0 : ev.Caption.Count;
        }
    }
}