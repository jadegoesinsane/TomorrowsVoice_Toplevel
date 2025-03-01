try {
    var calendarEl = document.getElementById('calendar');
    var calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        themeSystem: 'bootstrap5',
        //events: '/Event/GetShifts',
        //eventDisplay: 'block',
        headerToolbar: {
            start: 'title',
            center: '',
            end: 'prev,next today'
        },
        displayEventTime: false,
        navLinks: true,
     /*   navLinkDayClick: function (date) { window.location.href = '/Event/GetShifts?date=' + date.toISOString().split('T')[0] },*/
        //dateClick: function (info) { dateModal(info) }
    });
    calendar.render();
    fetch('/Event/GetShifts')
        .then(response => response.json())
        .then(events => {
            events.forEach(event => {
                calendar.addEvent({
                    title: event.title,
                    start: event.start,
                    end: event.end,
                    display: event.display,
                    groupId: event.groupId,
                    borderColor: event.borderColor,
                    textColor: event.textColor,
                    backgroundColor: event.backgroundColor,
                    url: event.url
                });
            });
        })
        .catch(error => console.log('Error getting events:', error));
    fetch('/Event/GetEvents')
        .then(response => response.json())
        .then(events => {
            events.forEach(event => {
                calendar.addEvent({
                    title: event.title,
                    start: event.start,
                    end: event.end,
                    display: event.display,
                    groupId: event.groupId,
                    borderColor: event.borderColor,
                    textColor: event.textColor,
                    backgroundColor: event.backgroundColor,
                    url: event.url
                });
            });
        })
        .catch(error => console.log('Error getting events:', error));
}
catch (e)
{
    console.log(e)
}


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