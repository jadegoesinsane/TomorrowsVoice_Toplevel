import { computePosition, flip, shift, offset, arrow } from 'https://cdn.jsdelivr.net/npm/@floating-ui/dom@1.6.13/+esm';

const brightColours = {
    "Blue": "#467ECE",
    "Purple": "#9944bc",
    "Red": "#d3162b",
    "Brown": "#804205",
    "Magenta": "#aa394f"
};

const lightColours = {
    "Pink": "#F6CBDF",
    "Green": "#D7E3C0",
    "Yellow": "#f5e0ac",
    "Light Blue": "#BFD6E9",
    "Light Purple": "#d8cbe7"
};

function getTextColour(colour) {
    if (Object.values(brightColours).includes(colour)) {
        return "#FFFFFF";
    } else if (Object.values(lightColours).includes(colour)) {
        return "#000000";
    } else {
        return "#000000";
    }
}

var calendarEl = document.getElementById('timetable');
var id = document.getElementById('ID').value;
window.calendar = new FullCalendar.Calendar(calendarEl, {
    initialView: 'timeGridWeek',
    themeSystem: 'bootstrap5',
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
    },
    events: `/Event/GetShifts?id=${id}`,
    eventTimeFormat: {
        hour: 'numeric',
        minute: '2-digit',
        omitZeroMinute: true,
        meridiem: true
    },
    eventDrop: function (eventDropInfo) {
        const newEvent = eventDropInfo.event;
        const oldEvent = eventDropInfo.oldEvent;

        const newRecurring = eventDropInfo.event._def.recurringDef;
        const oldRecurring = eventDropInfo.oldEvent._def.recurringDef;

        if (newRecurring != null && oldRecurring != null) {
            if (newEvent.start.getDay() != oldEvent.start.getDay()) {
                const daysOfWeek = newRecurring.typeData.daysOfWeek;
                const shiftedDaysOfWeek = daysOfWeek.map(day => (day + newEvent.start.getDay() - oldEvent.start.getDay() + 7) % 7);
                newEvent._def.recurringDef.typeData.daysOfWeek = shiftedDaysOfWeek;
            }
            if (newEvent.start.getTime() != oldEvent.start.getTime()) {
                const startTime = newEvent.start.getHours() * 3600000 + newEvent.start.getMinutes() * 60000;
                const endTime = newEvent.end.getHours() * 3600000 + newEvent.end.getMinutes() * 60000;

                newEvent._def.recurringDef.typeData.startTime = { years: 0, months: 0, days: 0, milliseconds: startTime };
                newEvent._def.recurringDef.typeData.endTime = { years: 0, months: 0, days: 0, milliseconds: endTime };
            }
            refresh();
        }
    }
});

document.addEventListener('DOMContentLoaded', function () {
    calendar.render();
});

function selected(info) {
    const backgroundColour = brightColours["Blue"]; // Default color
    const textColour = getTextColour(backgroundColour);

    calendar.addEvent({
        start: info.start,
        end: info.end,
        allDay: info.allDay,
        volunteersNeeded: null,
        backgroundColor: backgroundColour,
        borderColor: backgroundColour,
        textColor: textColour,
        extendedProps: {
            daysOfWeek: [],
            note: "",
            location: ""
        }
    });
}

function clicked(info) {
    const tooltip = document.querySelector('#tooltip');
    const daysOfWeek = info.event._def.recurringDef ? info.event._def.recurringDef.typeData.daysOfWeek : [];
    tooltip.innerHTML = `
    <div class="g-1 row">
        <div class="col-md-7 offset-1">
            <input type="text" id="title" placeholder="Title" class="form-control" value="${info.event.title}">
        </div>
        <div class="col-md-3 offset-1">
            <select name="colour" id="cboColour" class="form-control">
                <optgroup label="Bright">
                    ${Object.entries(brightColours).map(([name, value]) => `
                    <option value="${value}" ${info.event.backgroundColor === value ? 'selected' : ''}>${name}</option>
                    `).join('')}
                </optgroup>
                <optgroup label="Pastel">
                    ${Object.entries(lightColours).map(([name, value]) => `
                    <option value="${value}" ${info.event.backgroundColor === value ? 'selected' : ''}>${name}</option>
                    `).join('')}
                </optgroup>
            </select>
        </div>
    </div>
    <div class="g-1 row">
        <i class="bi bi-person col-1 col-form-label"></i>
        <div class="col">
            <input type="number" id="volunteersNeeded" placeholder="Volunteers needed" class="form-control" value="${info.event.extendedProps.volunteersNeeded}">
        </div>
    </div>
    <div class="g-1 row">
        <i class="bi bi-clock col-1 col-form-label"></i>
        <div class="col">
            <input type="date" id="date" class="form-control" value="${info.event.startStr.substring(0, 10)}">
        </div>
        <div class="col">
            <input type="time" id="start" class="form-control" value="${info.event.startStr.substring(11, 16)}">
        </div>
        <i class="bi bi-arrow-right col col-form-label"></i>
        <div class="col">
            <input type="time" id="end" class="form-control" value="${info.event.endStr.substring(11, 16)}">
        </div>
    </div>
    <div class="g-1 row">
        <i class="bi bi-geo-alt col-1 col-form-label"></i>
        <div class="col">
            <input type="text" id="location" placeholder="Location" class="form-control" value="${info.event.extendedProps.location}">
        </div>
    </div>
    <div class="g-1 row">
        <i class="bi bi-arrow-counterclockwise col-1 col-form-label"></i>
        <div class="col-md-11">
            <div class="btn-group" role="group" aria-label="Repeat on these days">
                <input type="checkbox" class="btn-check" id="btn-sunday" autocomplete="off" ${daysOfWeek.includes(0) ? 'checked' : ''}>
                <label class="btn btn-light" for="btn-sunday">Sun</label>

                <input type="checkbox" class="btn-check" id="btn-monday" autocomplete="off" ${daysOfWeek.includes(1) ? 'checked' : ''}>
                <label class="btn btn-light" for="btn-monday">Mon</label>

                <input type="checkbox" class="btn-check" id="btn-tuesday" autocomplete="off" ${daysOfWeek.includes(2) ? 'checked' : ''}>
                <label class="btn btn-light" for="btn-tuesday">Tue</label>

                <input type="checkbox" class="btn-check" id="btn-wednesday" autocomplete="off" ${daysOfWeek.includes(3) ? 'checked' : ''}>
                <label class="btn btn-light" for="btn-wednesday">Wed</label>

                <input type="checkbox" class="btn-check" id="btn-thursday" autocomplete="off" ${daysOfWeek.includes(4) ? 'checked' : ''}>
                <label class="btn btn-light" for="btn-thursday">Thu</label>

                <input type="checkbox" class="btn-check" id="btn-friday" autocomplete="off" ${daysOfWeek.includes(5) ? 'checked' : ''}>
                <label class="btn btn-light" for="btn-friday">Fri</label>

                <input type="checkbox" class="btn-check" id="btn-saturday" autocomplete="off" ${daysOfWeek.includes(6) ? 'checked' : ''}>
                <label class="btn btn-light" for="btn-saturday">Sat</label>
            </div>
            <!--<select name="dow" id="cboDow" class="form-control" multiple>
                <option value="never" ${daysOfWeek.length === 0 ? 'selected' : ''}>Don't repeat</option>
                <option value="0" ${daysOfWeek.includes(0) ? 'selected' : ''}>Sunday</option>
                <option value="1" ${daysOfWeek.includes(1) ? 'selected' : ''}>Monday</option>
                <option value="2" ${daysOfWeek.includes(2) ? 'selected' : ''}>Tuesday</option>
                <option value="3" ${daysOfWeek.includes(3) ? 'selected' : ''}>Wednesday</option>
                <option value="4" ${daysOfWeek.includes(4) ? 'selected' : ''}>Thursday</option>
                <option value="5" ${daysOfWeek.includes(5) ? 'selected' : ''}>Friday</option>
                <option value="6" ${daysOfWeek.includes(6) ? 'selected' : ''}>Saturday</option>
            </select>--!>
        </div>
    </div>
    <div class="g-1 row">
        <i class="bi bi-card-text col-1 col-form-label"></i>
        <div class="col">
            <textarea rows="1" style="width: 100%;" id="note" placeholder="Notes">${info.event._def.extendedProps.note}</textarea>
        </div>
    </div>
    <div class="d-flex justify-content-evenly">
        <button id="btnDelete" class="btn btn-sm btn-outline-danger">Delete</button>
        <button id="btnSave" class="btn btn-sm btn-outline-success">Save</button>
    </div>
    `;
    computePosition(info.el, tooltip, {
        placement: 'right',
        middleware: [
            offset(20),
            flip(),
            shift({ padding: 5 })
        ]
    }).then(({ x, y, }) => {
        Object.assign(tooltip.style, {
            left: `${x}px`,
            top: `${y}px`,
            display: 'block'
        });
    });

    document.getElementById('btnSave').addEventListener('click', function (e) {
        e.preventDefault();
        const title = document.getElementById('title').value;
        const date = document.getElementById('date').value;
        const start = document.getElementById('start').value;
        const end = document.getElementById('end').value;
        const colour = document.getElementById('cboColour').value;

        const volunteersNeeded = document.getElementById('volunteersNeeded').value;
        const location = document.getElementById('location').value;
        const note = document.getElementById('note').value;

        const repeat = [];
        if (document.getElementById('btn-sunday').checked) repeat.push(0);
        if (document.getElementById('btn-monday').checked) repeat.push(1);
        if (document.getElementById('btn-tuesday').checked) repeat.push(2);
        if (document.getElementById('btn-wednesday').checked) repeat.push(3);
        if (document.getElementById('btn-thursday').checked) repeat.push(4);
        if (document.getElementById('btn-friday').checked) repeat.push(5);
        if (document.getElementById('btn-saturday').checked) repeat.push(6);

        info.event.setProp('title', title);
        info.event.setProp('backgroundColor', colour);
        info.event.setProp('borderColor', colour);
        info.event.setProp('textColor', getTextColour(colour))

        info.event.setStart(new Date(`${date}T${start}`).toISOString());
        info.event.setEnd(new Date(`${date}T${end}`).toISOString());

        info.event.setExtendedProp('volunteersNeeded', volunteersNeeded);
        info.event.setExtendedProp('location', location);
        info.event.setExtendedProp('note', note);

        if (repeat.length > 0) {
            if (info.event.groupId != null && info.event.daysOfWeek != null)
                return;
            const bg = info.event.backgroundColor;
            const txt = info.event.textColor;
            info.event.remove();

            calendar.addEvent({
                groupId: 'shift-' + Date.now(),
                title: title,
                startTime: start,
                endTime: end,
                allDay: false,
                extendedProps: {
                    volunteersNeeded: volunteersNeeded,
                    location: location,
                    note: note,
                },
                backgroundColor: bg,
                borderColor: bg,
                textColor: txt,
                daysOfWeek: repeat
            });
        }

        hideTooltip();
    });
    document.getElementById('btnDelete').addEventListener('click', function (e) {
        e.preventDefault();
        hideTooltip();
        info.event.remove();
    })
    function onClickOutside(event) {
        if (!tooltip.contains(event.target) && !info.el.contains(event.target)) {
            hideTooltip();
            document.removeEventListener('click', onClickOutside);
        }
    }

    document.addEventListener('click', onClickOutside);
}

function loadShifts(id) {
    fetch(`/Event/GetShifts?id=${id}`)
        .then(response => response.json())
        .then(shifts => {
            shifts.forEach(shift => {
                console.log(shift);
                calendar.addEvent({
                    id: shift.id,
                    title: shift.title,
                    start: shift.start,
                    end: shift.end,
                    allDay: shift.allDay,
                    backgroundColor: shift.backgroundColor,
                    borderColor: shift.borderColor,
                    extendedProps: {
                        volunteersNeeded: shift.volunteersNeeded,
                        daysOfWeek: shift.daysOfWeek,
                        location: shift.Location
                    }
                });
                console.log(calendar);
            });
        })
        .catch(error => console.error('Error loading shifts:', error));
}

function showTooltip() {
    tooltip.style.display = 'block';
}

function hideTooltip() {
    tooltip.style.display = '';
}

function refresh() {
    const viewStart = calendar.view.currentStart;
    const viewName = calendar.view.type;
    calendar.changeView(viewName, viewStart);
}