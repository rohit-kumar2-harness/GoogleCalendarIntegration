using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GoogleCalendarIntegration.Services;
using GoogleCalendarIntegration.Models;

namespace GoogleCalendarIntegration.Controllers
{
    public class CalendarController : Controller
    {
        private readonly GoogleCalendarService _calendarService;

        public CalendarController(GoogleCalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _calendarService.GetUpcomingEvents();
            return View(events);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CalendarEvent model)
        {
            if (ModelState.IsValid)
            {
                await _calendarService.CreateEvent(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string eventId, string calendarId = "primary")
        {
            var result = await _calendarService.DeleteEvent(eventId, calendarId);
            return RedirectToAction("Index");
        }
    }
}
