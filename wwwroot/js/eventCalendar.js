var calendarEl = document.getElementById('calendar');

window.calendar = new FullCalendar.Calendar(calendarEl, {
    initialView: 'dayGridMonth',
    themeSystem: 'bootstrap5',
    eventDisplay: 'block',
    events: `/Event/GetEvents`,
    eventClick: function (info) {
        window.location.href = `/Event/Details/${info.event.id}`;
    }
});

document.addEventListener('DOMContentLoaded', function () {
    calendar.render();
});