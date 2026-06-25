// Admin Panel - Genel JS işlemleri
(function () {
    // feather ikonlarını yükle (eğer CDN üzerinden gelmiyorsa)
    if (typeof feather !== 'undefined') {
        feather.replace();
    }

    // Silme işlemleri için onay diyaloğu
    document.querySelectorAll('[data-confirm]').forEach(function (el) {
        el.addEventListener('click', function (e) {
            var msg = el.getAttribute('data-confirm') || 'Bu kaydı silmek istediğinize emin misiniz?';
            if (!confirm(msg)) {
                e.preventDefault();
            }
        });
    });

    // Kategori tablosundaki Sil butonlarına otomatik onay ekle
    document.querySelectorAll('a.btn-danger').forEach(function (btn) {
        if (btn.href && btn.href.includes('DeleteCategory')) {
            btn.setAttribute('data-confirm', 'Bu kategoriyi silmek istediğinize emin misiniz? İlişkili ürünler de etkilenebilir.');
            btn.addEventListener('click', function (e) {
                var msg = btn.getAttribute('data-confirm');
                if (!confirm(msg)) {
                    e.preventDefault();
                }
            });
        }
        if (btn.href && btn.href.includes('DeleteProduct')) {
            btn.setAttribute('data-confirm', 'Bu ürünü silmek istediğinize emin misiniz?');
            btn.addEventListener('click', function (e) {
                var msg = btn.getAttribute('data-confirm');
                if (!confirm(msg)) {
                    e.preventDefault();
                }
            });
        }
    });
})();
