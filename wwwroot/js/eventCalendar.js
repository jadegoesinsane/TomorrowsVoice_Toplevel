var calendarEl = document.getElementById('calendar');

window.calendar = new FullCalendar.Calendar(calendarEl, {
    initialView: 'dayGridMonth',
    themeSystem: 'bootstrap5',
    eventDisplay: 'block',
    eventSources: [`/Event/GetEvents`, `/Event/GetEventShifts`],
    dayMaxEventRows: true,
    views: {
        timeGrid: {
            dayMaxEventRows: 6
        }
    },
    eventClick: function (info) {
        if (info.event.extendedProps.isShift) {
            window.location.href = `/Shift/Details/${info.event.id}`;
        } else {
            window.location.href = `/Event/Details/${info.event.id}`;
        }
    },
    eventTimeFormat: {
        hour: 'numeric',
        minute: '2-digit',
        omitZeroMinute: true,
        meridiem: true
    }
});

document.addEventListener('DOMContentLoaded', function () {
    calendar.render();
});