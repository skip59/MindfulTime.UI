var currentEvent;

var formatDate = function formatDate(date) {
    return date === null ? '' : moment(date).format("YYYY-MM-DD HH:mm");
};

var fpStartTime = flatpickr("#StartTime", {
    enableTime: true,
    dateFormat: "Y-m-d H:i"
});

var fpEndTime = flatpickr("#EndTime", {
    enableTime: true,
    dateFormat: "Y-m-d H:i"
});

$('#calendar').fullCalendar({
    defaultView: 'month',
    height: 'parent',
    header: {
        left: 'prev,next today',
        center: 'title',
        right: 'month,agendaWeek,agendaDay'
    },
    monthNames: ['Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь', 'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь'],
    monthNamesShort: ['Янв.', 'Фев.', 'Март', 'Апр.', 'Май', 'Июнь', 'Июль', 'Авг.', 'Сент.', 'Окт.', 'Ноя.', 'Дек.'],
    dayNames: ['Воскресенье', 'Понедельник', 'Вторник', 'Среда', 'Четверг', 'Пятница', 'Суббота'],
    dayNamesShort: ['ВС', 'ПН', 'ВТ', 'СР', 'ЧТ', 'ПТ', 'СБ'],
    buttonText: {
        prev: 'Назад',
        next: 'Вперед',
        prevYear: 'Назад',
        nextYear: 'Вперед',
        today: 'Сегодня',
        month: 'Месяц',
        week: 'Неделя',
        day: 'День'
    },
    eventRender: function eventRender(event, $el) {
        $el.qtip({
            content: {
                title: event.title,
                text: event.description
            },
            hide: {
                event: 'unfocus'
            },
            show: {
                solo: true
            },
            position: {
                my: 'top left',
                at: 'bottom left',
                viewport: $('#calendar-wrapper'),
                adjust: {
                    method: 'shift'
                }
            }
        });
    },
    events: '/WorkSpace/GetCalendarEvents',
    eventClick: updateEvent,
    selectable: true,
    select: addEvent
});

/**
 * Calendar Methods
 **/

function updateEvent(event, element) {
    currentEvent = event;
    if ($(this).data("qtip")) $(this).qtip("hide");
    $('#eventModalLabel').html('Редактирование задачи');
    $('#eventModalSave').html('Изменить');
    $('#EventTitle').val(event.title);
    $('#Description').val(event.description);
    $('#isNewEvent').val(false);
    var start = formatDate(event.start);
    var end = formatDate(event.end);
    fpStartTime.setDate(start);
    fpEndTime.setDate(end);
    $('#StartTime').val(start);
    $('#EndTime').val(end);

    if (event.allDay) {
        $('#AllDay').prop('checked', 'checked');
    } else {
        $('#AllDay')[0].checked = false;
    }

    $('#eventModal').modal('show');
}

function addEvent(start, end) {
    $('#eventForm')[0].reset();
    $('#eventModalLabel').html('Новая задача');
    $('#eventModalSave').html('Создать');
    $('#isNewEvent').val(true);
    start = formatDate(start);
    end = formatDate(end);
    fpStartTime.setDate(start);
    fpEndTime.setDate(end);
    $('#eventModal').modal('show');
}

/**
 * Modal
 **/

$('#eventModalSave').click(function () {
    var title = $('#EventTitle').val();
    var description = $('#Description').val();
    var startTime = moment($('#StartTime').val(), "YYYY-MM-DD HH:mm");
    var endTime = moment($('#EndTime').val(), "YYYY-MM-DD HH:mm");
    var isAllDay = $('#AllDay').is(":checked");
    var isNewEvent = $('#isNewEvent').val() === 'true';
    var priority = $('#Priority').val();
    var complexity = $('#Complexity').val();
    var importance = $('#Importance').val();
    var hasDependencies = $('#HasDependencies').is(":checked");
    var uid = $('#uid').val();

    if (startTime > endTime) {
        alert('Дата начала не должна быть больше даты окончания задачи.');
        return;
    } else if ((!startTime.isValid() || !endTime.isValid()) && !isAllDay) {
        alert('Пожалуйста выберите дату начала и окончания задачи.');
        return;
    }

    var event = {
        title: title,
        description: description,
        isAllDay: isAllDay,
        startTime: startTime.toISOString(),
        endTime: endTime.toISOString(),
        priority: priority,
        complexity: complexity,
        importance: importance,
        hasDependencies: hasDependencies,
        uid: uid
    };

    if (isNewEvent) {
        sendAddEvent(event);
    } else {
        sendUpdateEvent(event);
    }
});

function sendAddEvent(event) {
    axios({
        method: 'post',
        url: '/WorkSpace/AddEvent',
        data: {
            "Title": event.title,
            "Description": event.description,
            "Start": event.startTime,
            "End": event.endTime,
            "AllDay": event.isAllDay,
            "Priority": event.priority,
            "Complexity": event.complexity,
            "Importance": event.importance,
            "UserId": event.uid

        }
    }).then(function (res) {
        var { message, eventId } = res.data;

        if (message === '') {
            var newEvent = {
                start: event.startTime,
                end: event.endTime,
                allDay: event.isAllDay,
                title: event.title,
                description: event.description,
                eventId: eventId,
                priority: priority,
                complexity: complexity,
                importance: importance,
                hasDependencies: hasDependencies,
                uid: uid
            };
            $('#calendar').fullCalendar('renderEvent', newEvent);
            $('#calendar').fullCalendar('unselect');
            $('#eventModal').modal('hide');
        } else {
            alert(`Неизвестная ошибка: ${message}`);
        }
    }).catch(function (err) {
        alert(`Неизвестная ошибка: ${err}`);
    });
}

function sendUpdateEvent(event) {
    axios({
        method: 'post',
        url: '/WorkSpace/UpdateEvent',
        data: {
            "EventId": currentEvent.eventId,
            "Title": event.title,
            "Description": event.description,
            "Start": event.startTime,
            "End": event.endTime,
            "AllDay": event.isAllDay,
            "Priority": event.priority,
            "Complexity": event.complexity,
            "Importance": event.importance,
            "UserId": event.uid
        }
    }).then(function (res) {
        var message = res.data.message;

        if (message === '') {
            currentEvent.title = event.title;
            currentEvent.description = event.description;
            currentEvent.start = event.startTime;
            currentEvent.end = event.endTime;
            currentEvent.allDay = event.isAllDay;
            $('#calendar').fullCalendar('updateEvent', currentEvent);
            $('#eventModal').modal('hide');
        } else {
            alert(`Неизвестная ошибка: ${message}`);
        }
    }).catch(function (err) {
        alert(`Неизвестная ошибка: ${err}`);
    });
}

$('#deleteEvent').click(function () {
    if (confirm(`Вы действительно хотите удалить задачу "${currentEvent.title}"?`)) {
        axios({
            method: 'post',
            url: '/WorkSpace/DeleteEvent',
            data: {
                "EventId": currentEvent.eventId
            }
        }).then(function (res) {
            var message = res.data.message;

            if (message === '') {
                $('#calendar').fullCalendar('removeEvents', currentEvent._id);
                $('#eventModal').modal('hide');
            } else {
                alert(`Неизвестная ошибка: ${message}`);
            }
        }).catch(function (err) {
            alert(`Неизвестная ошибка: ${err}`);
        });
    }
});

$('#AllDay').on('change', function (e) {
    if (e.target.checked) {
        $('#EndTime').val('');
        fpEndTime.clear();
        this.checked = true;
    } else {
        this.checked = false;
    }
});

$('#EndTime').on('change', function () {
    $('#AllDay')[0].checked = false;
});
