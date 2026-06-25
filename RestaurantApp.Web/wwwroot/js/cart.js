function toggleCart() {
    const cartSidebar = document.getElementById('cart-sidebar');
    const cartOverlay = document.getElementById('cart-overlay');
    cartSidebar.classList.toggle('active');
    cartOverlay.classList.toggle('active');

    // Sepet açıldığında masa numarasını localStorage'dan doldur
    if (cartSidebar.classList.contains('active')) {
        loadTableNumber();
    }
}

//sepete ürün eklemek sepette listelemek için kullandığımız script
function addToCart(productId, productName) {
    fetch(`/Home/GetProductPrice?productId=${productId}`)
        .then(response => response.json())
        .then(data => {
            const productPrice = parseFloat(data.price);
            const cart = getCart();
            const existingProduct = cart.find(p => p.productId === productId);

            if (existingProduct) {
                existingProduct.quantity++;
                existingProduct.totalPrice = (parseFloat(existingProduct.totalPrice) + productPrice).toFixed(2);
            } else {
                cart.push({
                    productId,
                    name: productName,
                    price: productPrice.toFixed(2),
                    quantity: 1,
                    totalPrice: productPrice.toFixed(2)
                });
            }

            setCart(cart);
            updateCartCount();
            renderCart();
        })
        .catch(error => {
            console.error('Error fetching product price:', error);
        });
}

function removeFromCart(productId) {
    const cart = getCart();
    const productIndex = cart.findIndex(p => p.productId === productId);

    if (productIndex > -1) {
        const product = cart[productIndex];
        const productPrice = parseFloat(product.price);

        if (product.quantity > 1) {
            product.quantity--;
            product.totalPrice = (parseFloat(product.totalPrice) - productPrice).toFixed(2);
        } else {
            cart.splice(productIndex, 1);
        }
    }

    setCart(cart);
    updateCartCount();
    renderCart();
}

function increaseQuantity(productId) {
    const cart = getCart();
    const product = cart.find(p => p.productId === productId);

    if (product) {
        const productPrice = parseFloat(product.price);
        product.quantity++;
        product.totalPrice = (parseFloat(product.totalPrice) + productPrice).toFixed(2);
        setCart(cart);
        renderCart();
    }
}

async function getData() {
    const response = await fetch('/userinfo');
    const data = await response.json();
    return data;
}

// ============================================
// Masa Numarası Yönetimi
// ============================================
function getTableNumber() {
    var input = document.getElementById('table-number-input');
    return input ? parseInt(input.value) : null;
}

function saveTableNumber(tableNum) {
    localStorage.setItem('tableNumber', tableNum);
}

function loadTableNumber() {
    var input = document.getElementById('table-number-input');
    if (input) {
        var saved = localStorage.getItem('tableNumber');
        if (saved) {
            input.value = saved;
        }
    }
}

function validateTableNumber() {
    var input = document.getElementById('table-number-input');
    var errorSpan = document.getElementById('table-number-error');
    var tableNum = getTableNumber();

    if (!tableNum || isNaN(tableNum) || tableNum < 1) {
        // Hata göster
        if (input) {
            input.classList.add('input-error');
        }
        if (errorSpan) {
            errorSpan.style.display = 'block';
        }
        // Focus'u input'a ver
        if (input) {
            input.focus();
        }
        return false;
    }

    // Hata yoksa temizle
    if (input) {
        input.classList.remove('input-error');
    }
    if (errorSpan) {
        errorSpan.style.display = 'none';
    }
    return true;
}

// Input değiştiğinde hata stilini temizle ve localStorage'a kaydet
document.addEventListener('DOMContentLoaded', function () {
    var tableInput = document.getElementById('table-number-input');
    if (tableInput) {
        tableInput.addEventListener('input', function () {
            this.classList.remove('input-error');
            var errorSpan = document.getElementById('table-number-error');
            if (errorSpan) {
                errorSpan.style.display = 'none';
            }
            // Değeri localStorage'a kaydet
            if (this.value && parseInt(this.value) > 0) {
                saveTableNumber(this.value);
            }
        });

        // Sayfa yüklendiğinde masa numarasını doldur
        loadTableNumber();
    }
});

// ============================================
// Sipariş Kaydetme
// ============================================
async function saveCart() {
    const cart = getCart();

    if (cart.length === 0) {
        alert('Sepetiniz boş!');
        return;
    }

    // Masa numarası kontrolü - zorunlu
    if (!validateTableNumber()) {
        return;
    }

    var tableNumber = getTableNumber();
    saveTableNumber(tableNumber); // localStorage'a kaydet

    var userInfo = await getData();

    if (!userInfo || !userInfo.id) {
        console.error("User ID not found");
        return;
    }

    // Sipariş verisini masa numarasıyla birlikte gönder
    var orderData = {
        cart: cart,
        tableNumber: tableNumber
    };

    fetch(`SaveOrder/${userInfo.id}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(orderData)
    })
        .then(response => {
            if (!response.ok) {
                return response.json().then(error => {
                    console.error('Error details:', error.details);
                    throw new Error(error.message);
                });
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                alert('Sipariş başarıyla kaydedildi! Masa: ' + tableNumber);
                toggleCart();
                localStorage.removeItem('cart'); // Önce sepeti temizle
                updateCartCount(); // Sonra sayacı güncelle (0 olacak)
                renderCart(); // Sepet listesini boşalt
                // Masa numarası localStorage'da kalır, sonraki siparişte dolu olur
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Sipariş kaydedilirken hata oluştu: ' + error.message);
        });
}

function getCart() {
    return JSON.parse(localStorage.getItem('cart')) || [];
}

function setCart(cart) {
    localStorage.setItem('cart', JSON.stringify(cart));
}

function updateCartCount() {
    const cart = getCart();
    const cartCount = cart.reduce((total, product) => total + product.quantity, 0);
    var badges = document.querySelectorAll('.cart-count-badge');
    badges.forEach(el => el.innerText = cartCount);
}

function renderCart() {
    const cartItemsElement = document.getElementById('cart-items');
    const cartTotalElement = document.getElementById('cart-total');
    const cart = getCart();

    if (!cartItemsElement) return;

    cartItemsElement.innerHTML = '';

    let totalAmount = 0;

    cart.forEach(product => {
        const itemElement = document.createElement('li');
        itemElement.className = 'list-group-item d-flex justify-content-between align-items-center';
        itemElement.dataset.productId = product.productId;
        itemElement.innerHTML = `
            <span>${product.name} - ${parseFloat(product.price).toFixed(2)}₺ x ${product.quantity}</span>
            <div>
                <button class="btn btn-sm btn-danger" onclick="removeFromCart(${product.productId})">-</button>
                <button class="btn btn-sm btn-success" onclick="increaseQuantity(${product.productId})">+</button>
            </div>
        `;
        cartItemsElement.appendChild(itemElement);
        totalAmount += parseFloat(product.totalPrice);
    });

    if (cartTotalElement) {
        cartTotalElement.innerText = `Toplam: ${totalAmount.toFixed(2)} ₺`;
    }
}

document.querySelectorAll('.add-to-cart').forEach(button => {
    button.addEventListener('click', () => {
        const productId = parseInt(button.getAttribute('data-id'));
        const productName = button.getAttribute('data-name');

        addToCart(productId, productName);
    });
});

var cartBtn = document.getElementById('cart-btn');
if (cartBtn) {
    cartBtn.addEventListener('click', () => {
        document.getElementById('cart-sidebar').classList.toggle('active');
        document.getElementById('cart-overlay').classList.toggle('active');
        renderCart();
    });
}

var closeCartBtn = document.querySelector('.close-cart');
if (closeCartBtn) {
    closeCartBtn.addEventListener('click', () => {
        document.getElementById('cart-sidebar').classList.remove('active');
        document.getElementById('cart-overlay').classList.remove('active');
    });
}

window.addEventListener('load', function () {
    var cartCountEl = document.getElementById('cart-count');
    if (cartCountEl) {
        updateCartCount();
    }
});
