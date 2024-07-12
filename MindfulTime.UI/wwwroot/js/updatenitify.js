document.addEventListener('DOMContentLoaded', function () {
    const sendMessageCheck = document.getElementById('sendMessageCheck');
    const notificationOptions = document.getElementById('notificationOptions');
    const notifyTelegram = document.getElementById('notifyTelegram');
    const telegramIdInput = document.getElementById('tid');

    sendMessageCheck.addEventListener('change', function () {
        if (this.checked) {
            notificationOptions.style.display = 'block';
        } else {
            notificationOptions.style.display = 'none';
        }
    });

    if (sendMessageCheck.checked) {
        notificationOptions.style.display = 'block';
    }
});

$(function () {
    $('#updateUserSettings').on('submit', function (event) {
        event.preventDefault();

        if ($('#notifyTelegram').is(':checked') && !$('#tid').val()) {
            alert('Пожалуйста, введите ваш Telegram ID для получения уведомлений через Telegram.');
            return;
        }

        const umail = $('#userEmail').val();
        const uid = '00000000-0000-0000-0000-000000000000';
        const tid = $('#tid').val();
        const notificationOption = true;

        $.ajax({
            url: '/WorkSpace/UpdateUser',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                Id: uid,
                Email: umail,
                TelegramId: tid,
                IsSendMessage: notificationOption
            }),
            success: function (result) {
                location.reload();
            }
        });
    });
});
