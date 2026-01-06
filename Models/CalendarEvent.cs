using System;

namespace GoogleCalendarIntegration.Models
{
    public class CalendarEvent
    {
        public string Id { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public string CalendarId { get; set; }
    }
}
