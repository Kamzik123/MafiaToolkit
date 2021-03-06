using System.Diagnostics;

namespace ResourceTypes.ModelHelpers.ModelExporter
{
    public abstract class IValidator
    {
        private MT_ValidationTracker OurTrackerObject;

        public bool ValidateObject(MT_ValidationTracker TrackerObject)
        {
            // Temporarily set our reference
            OurTrackerObject = TrackerObject;

            TrackerObject.Setup(this);

            // Kill our reference
            OurTrackerObject = null;

            return InternalValidate(TrackerObject);
        }

        public void AddMessage(MT_MessageType MessageType, string Format, params object[] Args)
        {
            Debug.Assert(OurTrackerObject == null, "TrackerObject shouldn't be invalid.");
            OurTrackerObject.AddMessage(this, MessageType, Format, Args);
        }

        public void AddMessage(MT_MessageType MessageType, string Text)
        {
            Debug.Assert(OurTrackerObject == null, "TrackerObject shouldn't be invalid.");
            OurTrackerObject.AddMessage(this, MessageType, Text);
        }

        protected abstract bool InternalValidate(MT_ValidationTracker TrackerObject);

    }
}
