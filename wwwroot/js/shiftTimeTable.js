var calendarEl = document.getElementById('timetable');
var calendar = new FullCalendar.Calendar(calendarEl, {
    initialView: 'timeGridWeek',
    themeSystem: 'bootstrap5',
    /*  events: '/Rehearsal/GetRehearsals',*/
    editable: true,
    selectable: true,
    select: function (info) {
        selected(info);
    },
    validRange: {
        start: document.getElementById('startDate').value,
        end: document.getElementById('endDate').value
    },
    droppable: true,
    eventDisplay: 'block',
    headerToolbar: {
        left: 'prev,next',
        center: 'title',
        right: 'timeGridWeek,timeGridDay',
    },
    eventClick: function (info) {
        clicked(info);
    }
});

document.addEventListener('DOMContentLoaded', function () {
    calendar.render();
});

function rangeCheck() {
    calendar.setOption('validRange', {
        start: document.getElementById('startDate').value,
        end: document.getElementById('endDate').value
    });
}

function selected(info) {
    console.log(info);

    calendar.addEvent({
        start: info.start,
        end: info.end,
        allDay: info.allDay,
        title: 'New Shift',
        volunteersNeeded: 5
    });
}

function clicked(info) {
    console.log(info);
    alert('Event: ' + info.event.title);
}