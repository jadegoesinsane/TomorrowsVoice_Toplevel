import { computePosition, flip, shift, offset } from 'https://cdn.jsdelivr.net/npm/@floating-ui/dom@1.6.13/+esm';

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
    }
});

document.addEventListener('DOMContentLoaded', function () {
    calendar.render();
});

function selected(info) {
    console.log(info);
    calendar.addEvent({
        start: info.start,
        end: info.end,
        allDay: info.allDay,
        title: 'New Shift',
        volunteersNeeded: 5,
        extendedProps: {
            daysOfWeek: []
        }
    });
}

function clicked(info) {
    const tooltip = document.querySelector('#tooltip');
    const daysOfWeek = info.event._def.recurringDef ? info.event._def.recurringDef.typeData.daysOfWeek : [];
    tooltip.innerHTML = `
        <div class="col-md-6">
            <div class="form-floating">
                <input type="text" id="title" class="form-control" value="${info.event.title}">
                <label for="title" class="control-label">Title</label>
            </div>
        </div>
        <div class="col-md-3 offset-3">
            <div class="form-floating">
                <input type="number" id="volunteersNeeded" class="form-control" value="${info.event.extendedProps.volunteersNeeded}">
                <label for="volunteersNeeded" class="control-label">Volunteers</label>
            </div>
        </div>
        <div class="col-md-12"></div>
        <div class="col-md-5">
            <div class="form-floating">
                <input type="date" id="date" class="form-control" value="${info.event.startStr.substring(0, 10)}">
                <label for="date" class="control-label">Date</label>
            </div>
        </div>
        <div class="col-md-3 offset-1">
            <div class="form-floating">
                <input type="time" id="start" class="form-control" value="${info.event.startStr.substring(11, 16)}">
                <label for="start" class="control-label">Start</label>
            </div>
        </div>
        <div class="col-md-3">
            <div class="form-floating">
                <input type="time" id="end" class="form-control" value="${info.event.endStr.substring(11, 16)}">
                <label for="end" class="control-label">End</label>
            </div>
        </div>
        <div class="col-md-12"></div>
        <div class="col-md-6" class="control-label">
            Repeat this task?
        </div>
        <div class="col-md-6">
            <select name="dow" id="cboDow" class="form-control" multiple>
                <option value="never" ${daysOfWeek.length === 0 ? 'selected' : ''}>Never</option>
                <option value="0" ${daysOfWeek.includes(0) ? 'selected' : ''}>Sunday</option>
                <option value="1" ${daysOfWeek.includes(1) ? 'selected' : ''}>Monday</option>
                <option value="2" ${daysOfWeek.includes(2) ? 'selected' : ''}>Tuesday</option>
                <option value="3" ${daysOfWeek.includes(3) ? 'selected' : ''}>Wednesday</option>
                <option value="4" ${daysOfWeek.includes(4) ? 'selected' : ''}>Thursday</option>
                <option value="5" ${daysOfWeek.includes(5) ? 'selected' : ''}>Friday</option>
                <option value="6" ${daysOfWeek.includes(6) ? 'selected' : ''}>Saturday</option>
            </select>
        </div>
        <div class="col-md-12"></div>
        <div class="col-md-6" class="control-label">
            Select a colour
        </div>
        <div class="col-md-6">
        
        
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
        <div class="col-md-auto">
           <button id="btnSave" class="btn btn-sm btn-outline-success">Save</button>
        </div>
        <div class="col-md-auto">
            <button id="btnDelete" class="btn btn-sm btn-outline-danger">Delete</button>
        </div>
    `;
    computePosition(info.el, tooltip, {
        placement: 'right',
        middleware: [
            flip(),
            shift({ padding: 5 }),
            offset(10)
        ]
    }).then(({ x, y }) => {
        Object.assign(tooltip.style, {
            left: `${x}px`,
            top: `${y}px`,
            display: 'flex'
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
                        daysOfWeek: shift.daysOfWeek
                    }
                });
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