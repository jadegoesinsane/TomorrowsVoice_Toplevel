import { computePosition, flip, shift, offset, arrow } from 'https://cdn.jsdelivr.net/npm/@floating-ui/dom@1.6.13/+esm';

const brightColors = ["#467ECE", "#9944bc", "#d3162b", "#804205", "#aa394f"];
const pastelColors = ["#F6CBDF", "#D7E3C0", "#f5e0ac", "#BFD6E9", "#d8cbe7"];

var calendarEl = document.getElementById('timetable');

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
    events: `/Event/GetShifts?id=${id}`
});

document.addEventListener('DOMContentLoaded', function () {
    calendar.render();
});

function selected(info) {
    calendar.addEvent({
        start: info.start,
        end: info.end,
        allDay: info.allDay,
        volunteersNeeded: null,
        backgroundColor: '#467ECE',
        borderColor: '#467ECE',
        textColor: '#FFFFFF',
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
                    <option value="#467ECE" ${info.event.backgroundColor === "#467ECE" ? 'selected' : ''} default>Blue</option>
                    <option value="#9944bc" ${info.event.backgroundColor === "#9944bc" ? 'selected' : ''}>Purple</option>
                    <option value="#d3162b" ${info.event.backgroundColor === "#d3162b" ? 'selected' : ''}>Red</option>
                    <option value="#804205" ${info.event.backgroundColor === "#804205" ? 'selected' : ''}>Brown</option>
                    <option value="#aa394f" ${info.event.backgroundColor === "#aa394f" ? 'selected' : ''}>Magenta</option>
                </optgroup>
                <optgroup label="Pastel">
                    <option value="#F6CBDF" ${info.event.backgroundColor === "#F6CBDF" ? 'selected' : ''}>Pink</option>
                    <option value="#D7E3C0" ${info.event.backgroundColor === "#D7E3C0" ? 'selected' : ''}>Green</option>
                    <option value="#f5e0ac" ${info.event.backgroundColor === "#f5e0ac" ? 'selected' : ''}>Yellow</option>
                    <option value="#BFD6E9" ${info.event.backgroundColor === "#BFD6E9" ? 'selected' : ''}>Blue</option>
                    <option value="#d8cbe7" ${info.event.backgroundColor === "#d8cbe7" ? 'selected' : ''}>Purple</option>
                </optgroup>
            </select>
        </div>
    </div>
    <div class="g-1 row">
        <i class="bi bi-person col-md-1"></i>
        <div class="col-md-11">
            <input type="number" id="volunteersNeeded" placeholder="Volunteers needed" class="form-control" value="${info.event.extendedProps.volunteersNeeded}">
        </div>
    </div>
    <div class="g-1 row">
        <i class="bi bi-clock col-md-1"></i>
        <div class="col-md-11">
            <input type="date" id="date" class="form-control" value="${info.event.startStr.substring(0, 10)}">
        </div>
        <div class="col-md-5 offset-1">
            <input type="time" id="start" class="form-control" value="${info.event.startStr.substring(11, 16)}">
        </div>
        <i class="bi bi-arrow-right col-md-1"></i>
        <div class="col-md-5">
            <input type="time" id="end" class="form-control" value="${info.event.endStr.substring(11, 16)}">
        </div>
    </div>
    <div class="g-1 row">
        <i class="bi bi-geo-alt col-md-1"></i>
        <div class="col-md-11">
            <input type="text" id="location" placeholder="Location" class="form-control" value="${info.event.extendedProps.location}">
        </div>
    </div>
    <div class="g-1 row">
        <i class="bi bi-arrow-counterclockwise col-md-1"></i>
        <div class="col-md-11">
            <select name="dow" id="cboDow" class="form-control" multiple>
                <option value="never" ${daysOfWeek.length === 0 ? 'selected' : ''}>Don't repeat</option>
                <option value="0" ${daysOfWeek.includes(0) ? 'selected' : ''}>Sunday</option>
                <option value="1" ${daysOfWeek.includes(1) ? 'selected' : ''}>Monday</option>
                <option value="2" ${daysOfWeek.includes(2) ? 'selected' : ''}>Tuesday</option>
                <option value="3" ${daysOfWeek.includes(3) ? 'selected' : ''}>Wednesday</option>
                <option value="4" ${daysOfWeek.includes(4) ? 'selected' : ''}>Thursday</option>
                <option value="5" ${daysOfWeek.includes(5) ? 'selected' : ''}>Friday</option>
                <option value="6" ${daysOfWeek.includes(6) ? 'selected' : ''}>Saturday</option>
            </select>
        </div>
    </div>
    <div class="g-1 row">
        <i class="bi bi-card-text col-md-1"></i>
        <div class="col-md-11">
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
        const repeat = Array.from(document.getElementById('cboDow').selectedOptions).map(option => option.value);

        info.event.setProp('title', title);
        info.event.setProp('backgroundColor', colour);
        info.event.setProp('borderColor', colour);

        if (brightColors.includes(colour)) {
            info.event.setProp('textColor', 'white');
        } else if (pastelColors.includes(colour)) {
            info.event.setProp('textColor', 'black');
        }

        info.event.setStart(new Date(`${date}T${start}`).toISOString());
        info.event.setEnd(new Date(`${date}T${end}`).toISOString());

        info.event.setExtendedProp('volunteersNeeded', volunteersNeeded);
        info.event.setExtendedProp('location', location);
        info.event.setExtendedProp('note', note);

        if (repeat != "never") {
            if (info.event.groupId != null && info.event.daysOfWeek != null)
                return;
            const bg = info.event.backgroundColor;
            const txt = info.event.textColor;
            info.event.remove();
            const daysOfWeek = Array.from(repeat).map(day => parseInt(day));

            calendar.addEvent({
                groupId: 'shift-' + Date.now(),
                title: title,
                startTime: start,
                endTime: end,
                allDay: false,
                extendedProps: {
                    volunteersNeeded: volunteersNeeded,
                },
                backgroundColor: bg,
                borderColor: bg,
                textColor: txt,
                daysOfWeek: daysOfWeek
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