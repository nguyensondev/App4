using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App4
{
    public class XViewEventArgs : EventArgs
    {
        public readonly string EventName;
        public readonly int EventIndex;
        public readonly string EventDesc;
        public readonly object CastObject;
        public XViewEventArgs(string eventName, int eventIndex)
        {
            EventName = eventName;
            EventIndex = eventIndex;
        }

        public XViewEventArgs(string eventName, object castObject)
        {
            EventName = eventName;
            CastObject = castObject;
        }

        public XViewEventArgs(string eventName, int eventIndex, string desc)
        {
            EventName = eventName;
            EventIndex = eventIndex;
            EventDesc = desc;
        }
    }

}
