var calendarEl = document.getElementById('calendar');

window.calendar = new FullCalendar.Calendar(calendarEl, {
    timeZone: 'UTC',
    initialView: 'dayGridMonth',
    themeSystem: 'bootstrap5',
    headerToolbar: {
        left: 'prev,next today',
        center: 'title',
        right: 'dayGridMonth,timeGridWeek,timeGridDay,listMonth'
    },
    eventDisplay: 'block',
    eventSources: [`/Event/GetEvents`, `/Event/GetEventShifts`],
    dayMaxEventRows: true,
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