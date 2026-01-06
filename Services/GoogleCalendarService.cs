using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using GoogleCalendarIntegration.Models;
using System.IO;
using System.Text.Json;

namespace GoogleCalendarIntegration.Services
{
    public class GoogleCalendarService
    {
        private readonly IConfiguration _configuration;
        private readonly string[] Scopes = { CalendarService.Scope.Calendar };
        private readonly string ApplicationName = "Google Calendar Integration";

        public GoogleCalendarService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private async Task<CalendarService> GetCalendarService()
        {
            var clientId = _configuration["Google:ClientId"];
            var clientSecret = _configuration["Google:ClientSecret"];

            // In a production app, you should implement proper token storage and retrieval
            // For this example, we'll use a new token each time
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                Scopes,
                "user",
                System.Threading.CancellationToken.None);

            return new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        public async Task<IList<CalendarEvent>> GetUpcomingEvents(int maxResults = 10)
        {
            var service = await GetCalendarService();
            var request = service.Events.List("primary");
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = maxResults;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            var events = await request.ExecuteAsync();
            var result = new List<CalendarEvent>();

            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    result.Add(new CalendarEvent
                    {
                        Id = eventItem.Id,
                        Summary = eventItem.Summary,
                        Description = eventItem.Description,
                        Start = eventItem.Start.DateTime ?? DateTime.Parse(eventItem.Start.Date),
                        End = eventItem.End.DateTime ?? DateTime.Parse(eventItem.End.Date),
                        Location = eventItem.Location,
                        Status = eventItem.Status,
                        CalendarId = "primary"
                    });
                }
            }

            return result;
        }

        public async Task<CalendarEvent> CreateEvent(CalendarEvent newEvent)
        {
            var service = await GetCalendarService();

            var eventToCreate = new Google.Apis.Calendar.v3.Data.Event
            {
                Summary = newEvent.Summary,
                Description = newEvent.Description,
                Location = newEvent.Location,
                Start = new EventDateTime
                {
                    DateTime = newEvent.Start,
                    TimeZone = "UTC"
                },
                End = new EventDateTime
                {
                    DateTime = newEvent.End,
                    TimeZone = "UTC"
                }
            };

            var request = service.Events.Insert(eventToCreate, newEvent.CalendarId ?? "primary");
            var createdEvent = await request.ExecuteAsync();

            return new CalendarEvent
            {
                Id = createdEvent.Id,
                Summary = createdEvent.Summary,
                Description = createdEvent.Description,
                Start = createdEvent.Start.DateTime ?? DateTime.Parse(createdEvent.Start.Date),
                End = createdEvent.End.DateTime ?? DateTime.Parse(createdEvent.End.Date),
                Location = createdEvent.Location,
                Status = createdEvent.Status,
                CalendarId = newEvent.CalendarId ?? "primary"
            };
        }

        public async Task<bool> DeleteEvent(string eventId, string calendarId = "primary")
        {
            try
            {
                var service = await GetCalendarService();
                await service.Events.Delete(calendarId, eventId).ExecuteAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
