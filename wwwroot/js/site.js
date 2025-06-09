// Hatırlatıcı Yönetim Sistemi
$(document).ready(function () {
    initializeReminderSystem();
});

function initializeReminderSystem() {
    // Event Delegation kullanarak dinamik elementler için
    $(document)
        .on('click', '.complete-btn', handleCompleteReminder)
        .on('click', '.delete-btn', handleDeleteReminder);

    // Bildirim sistemini başlat
    initializeNotificationSystem();
}

// Hatırlatıcı Tamamlama İşlemi
function handleCompleteReminder() {
    const id = $(this).data('id');
    const $listItem = $(this).closest('.list-group-item');

    showLoading($listItem);

    $.post(`/Reminders/Complete/${id}`)
        .done(function () {
            updateUIAfterComplete($listItem);
            showToast('success', 'Hatırlatıcı tamamlandı olarak işaretlendi');
        })
        .fail(function () {
            showToast('error', 'İşlem sırasında hata oluştu');
        })
        .always(function () {
            hideLoading($listItem);
        });
}

// Hatırlatıcı Silme İşlemi
function handleDeleteReminder() {
    const id = $(this).data('id');
    const $listItem = $(this).closest('.list-group-item');

    if (!confirm('Bu hatırlatıcıyı silmek istediğinize emin misiniz?')) return;

    showLoading($listItem);

    $.post(`/Reminders/Delete/${id}`)
        .done(function () {
            $listItem.fadeOut(300, function () {
                $(this).remove();
                showToast('success', 'Hatırlatıcı başarıyla silindi');
            });
        })
        .fail(function () {
            showToast('error', 'Silme işlemi başarısız');
        })
        .always(function () {
            hideLoading($listItem);
        });
}

// Bildirim Sistemi
function initializeNotificationSystem() {
    checkReminders();
    setInterval(checkReminders, 60000); // Her 1 dakikada bir kontrol

    // Sayfa görünür olduğunda kontrol et
    document.addEventListener('visibilitychange', function () {
        if (!document.hidden) checkReminders();
    });
}

function checkReminders() {
    $.get("/Reminders/GetUpcoming")
        .done(function (data) {
            if (data && data.length > 0) {
                data.forEach(reminder => {
                    showBrowserNotification(reminder.title, reminder.description);
                    showToast('warning', `Hatırlatıcı: ${reminder.title}`);
                });
            }
        })
        .fail(function () {
            console.error('Hatırlatıcılar alınırken hata oluştu');
        });
}

// Tarayıcı Bildirimleri
function showBrowserNotification(title, message) {
    if (!("Notification" in window)) return;

    const showNotification = () => {
        const notification = new Notification(title, {
            body: message,
            icon: '/images/notification-icon.png'
        });

        notification.onclick = function () {
            window.focus();
            this.close();
        };
    };

    if (Notification.permission === "granted") {
        showNotification();
    }
    else if (Notification.permission !== "denied") {
        Notification.requestPermission().then(permission => {
            if (permission === "granted") showNotification();
        });
    }
}

// Yardımcı Fonksiyonlar
function showLoading($element) {
    $element.css('opacity', '0.6').find('button').prop('disabled', true);
    $element.append('<div class="spinner-border spinner-border-sm text-primary ms-2"></div>');
}

function hideLoading($element) {
    $element.css('opacity', '').find('button').prop('disabled', false);
    $element.find('.spinner-border').remove();
}

function showToast(type, message) {
    const toast = $(`
        <div class="toast show align-items-center text-white bg-${type} border-0 position-fixed bottom-0 end-0 m-3">
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `);

    $('body').append(toast);

    setTimeout(() => {
        toast.remove();
    }, 5000);
}

// Sayfa yenileme olmadan UI güncelleme
function updateUIAfterComplete($listItem) {
    $listItem
        .addClass('bg-light')
        .find('.complete-btn').remove();

    $listItem.find('h5')
        .addClass('text-decoration-line-through text-muted')
        .append('<span class="badge bg-success ms-2">Tamamlandı</span>');
}