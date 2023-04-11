using Domain.Common;
using Domain.Entities.Documents;

namespace Domain.Events
{
    public class TextPublishedEvent : BaseEvent
    {
        public TextPublishedEvent(Document document)
        {
            Document = document;
        }

        public Document Document { get; }
    }
}
