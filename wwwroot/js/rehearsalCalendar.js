var calendarEl = document.getElementById('calendar');
var calendar = new FullCalendar.Calendar(calendarEl, {
    initialView: 'dayGridMonth',
    themeSystem: 'bootstrap5',
    events: '/Rehearsal/GetRehearsals',
    eventDisplay: 'block',
    headerToolbar: {
        start: 'title',
        center: '',
        end: 'prev,next today'
    },
    navLinks: true,
    navLinkDayClick: function (date) { window.location.href = '/Rehearsal/SetRehearsal?date=' + date.toISOString().split('T')[0] },
    //dateClick: function (info) { dateModal(info) }
});
calendar.render();
/*document.addEventListener('DOMContentLoaded', function () {
    
});*/

/*function dateModal (date)
{
    var dates = [];
    calendar.getEvents().forEach(function (event) {
        if (date.date.toDateString() === event.start.toDateString())
        {
            dates += event;
        }
    });    

    $('#exampleModal .modal-body').text('Selected Date: ' + date);
    $('#exampleModal').modal('show')
}*/