let ordersData = []; // Global değişken olarak tanımla
let currentOrderId = null; // Düzenlenen siparişin ID'sini tutacak değişken

async function getData() {
    const response = await fetch('/userinfo');
    const data = await response.json(); // Veriyi JSON formatına çeviriyor
    return data; // JSON verisini döndürüyor
}

async function toggleOrders() {
    const ordersSidebar = document.getElementById('orders-sidebar');
    const ordersOverlay = document.getElementById('orders-overlay');
    ordersSidebar.classList.toggle('active');
    ordersOverlay.classList.toggle('active');

    var userInfo = await getData();


    if (ordersSidebar.classList.contains('active')) {
        fetch(`GetTodaySales/${userInfo.id}`)
            .then(response => response.json())
            .then(data => {
                console.log(data); // Gelen veriyi kontrol etmek için
                ordersData = data; // Sipariş verilerini global değişkene ata
                const ordersItemsElement = document.getElementById('orders-items');
                ordersItemsElement.innerHTML = '';


                let grandTotal = 0;
                let totalItems = 0;

                data.forEach(order => {
                    grandTotal += order.totalPrice;
                    order.products.forEach(p => {
                        totalItems += p.quantity;
                    });

                    const orderElement = document.createElement('li');
                    orderElement.className = 'list-group-item';
                    orderElement.innerHTML = `
                                                        <div>
                                                            <h5>Masa No: ${order.tableNumber}</h5>
                                                            <p>Tarih: ${new Date(order.saleDate).toLocaleString()}</p>
                                                            <p>Toplam Tutar: ${order.totalPrice.toFixed(2)} ₺</p>
                                                            <table class="table table-bordered">
                                                                <thead>
                                                                    <tr>
                                                                        <th>Ürün Adı</th>
                                                                        <th>Ürün Fiyatı</th>
                                                                        <th>Adet</th>
                                                                        <th>Toplam Tutar</th>
                                                                    </tr>
                                                                </thead>
                                                                <tbody>
                                                                    ${order.products.map(p => `
                                                                        <tr>
                                                                            <td>${p.name}</td>
                                                                            <td>${p.productPrice.toFixed(2)} ₺</td>
                                                                            <td>${p.quantity}</td>
                                                                            <td>${p.productTotalPrice.toFixed(2)} ₺</td>
                                                                        </tr>
                                                                    `).join('')}
                                                                </tbody>
                                                            </table>
                                                            ${userInfo.email == 'admin@domain.com' ? `<button id="editButton" class="btn btn-sm btn-warning" onclick="openEditModal(${order.id})">Düzenle</button>` : ''}

                                                           
                                                        </div>
                                                    `;
                    ordersItemsElement.appendChild(orderElement);
                });

                // Toplamları ekrana yazdır
                const grandTotalElement = document.getElementById('orders-grand-total');
                const totalItemsElement = document.getElementById('orders-total-items');
                if (grandTotalElement) grandTotalElement.innerText = grandTotal.toFixed(2) + ' ₺';
                if (totalItemsElement) totalItemsElement.innerText = totalItems + ' adet';
            })
            .catch(error => {
                console.error('Error fetching today sales:', error);
            });
    }
}

// Modal penceresini açacak fonksiyon
function openEditModal(orderId) {
    currentOrderId = orderId; // Düzenlenen siparişin ID'sini kaydet
    const order = ordersData.find(o => o.id === orderId);

    const editOrderForm = document.getElementById('editOrderForm');
    editOrderForm.innerHTML = '';

    order.products.forEach((p, index) => {
        const formGroup = document.createElement('div');
        formGroup.className = 'mb-3';
        formGroup.innerHTML = `
                                            <label class="form-label">${p.name}</label>
                                            <input type="number" class="form-control" name="quantity-${index}" value="${p.quantity}" data-product-id="${p.productId}">
                                        `;
        editOrderForm.appendChild(formGroup);
    });

    $('#editOrderModal').modal('show');
}

// Değişiklikleri kaydedecek fonksiyon
function saveOrderChanges() {
    const editOrderForm = document.getElementById('editOrderForm');
    const formData = new FormData(editOrderForm);

    const updatedProducts = Array.from(formData.entries()).map(([name, value]) => ({
        productId: editOrderForm.querySelector(`[name="${name}"]`).dataset.productId,
        quantity: value
    }));

    fetch('/Home/EditOrder', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            orderId: currentOrderId,
            products: updatedProducts
        })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert('Değişiklikler kaydedildi');
                $('#editOrderModal').modal('hide');
                toggleOrders();
            } else {
                alert('Hata: ' + data.message);
            }
        })
        .catch(error => console.error('Error:', error));
}