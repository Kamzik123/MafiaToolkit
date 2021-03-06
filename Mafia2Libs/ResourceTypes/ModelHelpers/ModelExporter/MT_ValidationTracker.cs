using System;
using System.Collections.Generic;
using Utils.Extensions;

namespace ResourceTypes.ModelHelpers.ModelExporter
{
    public enum MT_MessageType
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    /*
     *  Object which tracks errors logged by the validation process.
     */
    public class MT_ValidationTracker
    {
        private Dictionary<IValidator, List<string>> Messages;

        public MT_ValidationTracker()
        {
            Messages = new Dictionary<IValidator, List<string>>();
        }

        public void Setup(IValidator ValidationObject)
        {
            if(!Messages.ContainsKey(ValidationObject))
            {
                Messages.Add(ValidationObject, new List<string>());
            }
        }

        public void AddMessage(IValidator CallerObject, MT_MessageType MessageType, string Format, params object[] Args)
        {
            string Message = string.Format(Format, Args);
            AddMessage(CallerObject, MessageType, Message);
        }

        public void AddMessage(IValidator CallerObject, MT_MessageType MessageType, string Text)
        {
            string FinalMessage = string.Format("{0} - {1}", MessageType.ToString(), Text);

            Messages.TryGet(CallerObject)?.Add(FinalMessage);
        }

        public bool IsObjectValid(IValidator ObjectToCheck)
        {
            List<string> ObjectMsgs = Messages.TryGet(ObjectToCheck);

            return ObjectMsgs.Count == 0;
        }

        public List<string> GetObjectMessages(IValidator ObjectToCheck)
        {
            return Messages.TryGet(ObjectToCheck);
        }
    }
}
