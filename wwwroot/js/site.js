/* ═══════════════════════════════════════════════════
   VINFAST WEB — GLOBAL JS
═══════════════════════════════════════════════════ */

document.addEventListener('DOMContentLoaded', () => {

    // ── Navbar scroll effect ──────────────────────────
    const navbar = document.getElementById('navbar');
    if (navbar) {
        window.addEventListener('scroll', () => {
            navbar.classList.toggle('scrolled', window.scrollY > 40);
        });
    }

    // ── Hamburger menu ────────────────────────────────
    const hamburger = document.getElementById('hamburger');
    const navLinks = document.getElementById('navLinks');
    if (hamburger && navLinks) {
        hamburger.addEventListener('click', () => {
            hamburger.classList.toggle('open');
            navLinks.classList.toggle('open');
        });
        // Dropdown toggle trên mobile
        navLinks.querySelectorAll('.nav-item.has-dropdown > .nav-link').forEach(link => {
            link.addEventListener('click', e => {
                if (window.innerWidth < 768) {
                    e.preventDefault();
                    link.closest('.nav-item').classList.toggle('open');
                }
            });
        });
    }

    // ── Highlight active nav link ─────────────────────
    const path = window.location.pathname.toLowerCase();
    document.querySelectorAll('.nav-link').forEach(a => {
        const href = a.getAttribute('href')?.toLowerCase();
        if (href && href !== '/' && path.startsWith(href)) {
            a.classList.add('active');
        }
    });

    // ── Cart badge ────────────────────────────────────
    updateCartBadge();

    // ── Fade-up on scroll ─────────────────────────────
    const fadeEls = document.querySelectorAll('[data-fade]');
    if (fadeEls.length) {
        const observer = new IntersectionObserver(entries => {
            entries.forEach(e => {
                if (e.isIntersecting) {
                    e.target.classList.add('fade-up');
                    observer.unobserve(e.target);
                }
            });
        }, { threshold: 0.12 });
        fadeEls.forEach(el => observer.observe(el));
    }

});

// ── Cart badge update ───────────────────────────────
async function updateCartBadge() {
    try {
        const res = await fetch('/Cart/Count');
        const data = await res.json();
        const badge = document.getElementById('cartBadge');
        if (badge) {
            badge.textContent = data.count;
            badge.style.display = data.count > 0 ? 'flex' : 'none';
        }
    } catch { }
}

// ── Add to cart ─────────────────────────────────────
async function addToCart(productId, productType, name, price, image) {
    try {
        const res = await fetch('/Cart/Add', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId, productType, name, price, image, quantity: 1 })
        });
        const data = await res.json();
        if (data.success) {
            const badge = document.getElementById('cartBadge');
            if (badge) { badge.textContent = data.count; badge.style.display = 'flex'; }
            showToast('✓ Đã thêm vào giỏ hàng!');
        }
    } catch { showToast('Lỗi khi thêm vào giỏ hàng'); }
}

// ── Toast ───────────────────────────────────────────
function showToast(msg, duration = 2800) {
    const t = document.getElementById('toast');
    if (!t) return;
    t.textContent = msg;
    t.classList.add('show');
    setTimeout(() => t.classList.remove('show'), duration);
}

// ── Copy voucher code ───────────────────────────────
function copyCode(code) {
    navigator.clipboard.writeText(code).then(() => {
        showToast(`📋 Đã sao chép mã: ${code}`);
    }).catch(() => {
        showToast(`Mã voucher: ${code}`);
    });
}

// ── Cart: update qty ────────────────────────────────
async function updateQty(productId, productType, qty) {
    try {
        const res = await fetch('/Cart/UpdateQty', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId, productType, qty })
        });
        const data = await res.json();
        if (data.success) {
            const badge = document.getElementById('cartBadge');
            if (badge) { badge.textContent = data.count; badge.style.display = data.count > 0 ? 'flex' : 'none'; }
            // Update UI
            const itemTotalEl = document.querySelector(`[data-item-total="${productId}-${productType}"]`);
            if (itemTotalEl) itemTotalEl.textContent = data.itemTotal;
            const grandEl = document.getElementById('grandTotal');
            if (grandEl) grandEl.textContent = data.grandTotal;
            if (qty <= 0) location.reload();
        }
    } catch { }
}

// ── Cart: remove item ───────────────────────────────
async function removeItem(productId, productType) {
    try {
        const res = await fetch('/Cart/Remove', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId, productType })
        });
        const data = await res.json();
        if (data.success) location.reload();
    } catch { }
}

// ── VF Navbar scroll ────────────────────────────────
const vfNavbar = document.getElementById('navbar');
if (vfNavbar) {
    window.addEventListener('scroll', () => {
        vfNavbar.classList.toggle('scrolled', window.scrollY > 40);
    });
}

// ── VF Hamburger ─────────────────────────────────────
const vfHamburger = document.getElementById('hamburger');
const vfNavLinks = document.getElementById('navLinks');
if (vfHamburger && vfNavLinks) {
    vfHamburger.addEventListener('click', () => {
        vfHamburger.classList.toggle('open');
        vfNavLinks.classList.toggle('open');
    });
    // Mobile dropdown toggle
    vfNavLinks.querySelectorAll('.vf-nav-item.has-drop > .vf-nav-link').forEach(link => {
        link.addEventListener('click', e => {
            if (window.innerWidth < 768) {
                e.preventDefault();
                link.closest('.vf-nav-item').classList.toggle('open');
            }
        });
    });
}
// ── Modern Global Search ──────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    const searchIconBtn = document.getElementById('searchIconBtn');
    const searchDropdownMenu = document.getElementById('dynamicSearchBar');
    const globalInput = document.getElementById('globalSearchInput');
    const closeSearchBtn = document.getElementById('closeSearchBtn');
    const globalSuggestBox = document.getElementById('globalSuggestionBox');

    let searchTimeout = null;

    if (!searchIconBtn || !searchDropdownMenu) return;

    // 👉 MỞ SEARCH
    searchIconBtn.addEventListener('click', (e) => {
        e.preventDefault();

        searchDropdownMenu.style.display = 'block';
        searchIconBtn.classList.add('hidden-icon');
        globalInput.focus();

        fetchSuggestions(''); // 🔥 load gợi ý luôn khi mở
    });

    // 👉 ĐÓNG SEARCH
    closeSearchBtn.addEventListener('click', (e) => {
        e.preventDefault();

        searchDropdownMenu.style.display = 'none';
        searchIconBtn.classList.remove('hidden-icon');
        globalInput.value = '';
        globalSuggestBox.innerHTML = '';
    });

    // 👉 CLICK NGOÀI
    document.addEventListener('click', (e) => {
        if (!searchDropdownMenu.contains(e.target) && !searchIconBtn.contains(e.target)) {
            searchDropdownMenu.style.display = 'none';
            searchIconBtn.classList.remove('hidden-icon');
        }
    });

    // 👉 FETCH DATA (UI đẹp như bản cũ)
    const fetchSuggestions = async (query) => {
        try {
            globalSuggestBox.innerHTML = `
                <div class="text-center py-3 text-muted">
                    <i class="fas fa-spinner fa-spin"></i> Đang tải...
                </div>
            `;

            const res = await fetch(`/Search/GetSuggestions?q=${encodeURIComponent(query)}`);
            const data = await res.json();

            if (!data || data.length === 0) {
                globalSuggestBox.innerHTML = `
                    <div class="text-center py-3 text-muted">
                        Không tìm thấy sản phẩm
                    </div>
                `;
                return;
            }

            

            // 🔥 items UI đẹp
            const itemsHtml = data.map(i => {
                let link = `/Products/Details/${i.id}`;
                if (i.category === 'car') link = `/Cars/Details/${i.id}`;
                if (i.category === 'motorbike') link = `/Motorbikes/Details/${i.id}`;
                if (i.category === 'accessory') link = `/Accessories/Details/${i.id}`;

                return `
                    <a href="${link}" class="suggestion-item">
                        <img src="${i.imageUrl}" alt="${i.name}">
                        <div class="suggestion-info">
                            <div class="suggestion-name">${i.name}</div>
                            <div class="suggestion-price">
                                ${new Intl.NumberFormat('vi-VN').format(i.price)} ₫
                            </div>
                        </div>
                    </a>
                `;
            }).join('');

            globalSuggestBox.innerHTML = itemsHtml;

        } catch (e) {
            globalSuggestBox.innerHTML = `
                <div class="text-center py-3 text-danger">
                    Lỗi tải dữ liệu
                </div>
            `;
        }
    };

    // 👉 INPUT SEARCH (debounce)
    globalInput.addEventListener('input', function () {
        clearTimeout(searchTimeout);
        const query = this.value.trim();

        searchTimeout = setTimeout(() => {
            fetchSuggestions(query);
        }, 300);
    });
});
// Giỏ hàng
document.addEventListener("DOMContentLoaded", () => {

    // 1. Cập nhật số lượng
    const cartTable = document.getElementById("cartTable");
    if (cartTable) {
        cartTable.addEventListener("change", async (e) => {
            if (e.target.classList.contains("qty-input")) {
                const row = e.target.closest("tr");
                const productId = parseInt(row.dataset.id);
                const productType = row.dataset.type;
                const qty = parseInt(e.target.value);

                if (qty < 1) return;

                const res = await fetch("/Cart/UpdateQty", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ productId, productType, qty })
                });

                const data = await res.json();
                if (data.success) {
                    row.querySelector(".item-total").innerText = data.itemTotal;
                    document.getElementById("grandTotal").innerText = data.grandTotal;
                    updateCartBadge();
                }
            }
        });

        // 2. Xóa sản phẩm
        cartTable.addEventListener("click", async (e) => {
            const btn = e.target.closest(".btn-remove");
            if (btn) {
                const row = btn.closest("tr");
                const productId = parseInt(row.dataset.id);
                const productType = row.dataset.type;

                const res = await fetch("/Cart/Remove", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ productId, productType })
                });

                const data = await res.json();
                if (data.success) {
                    row.style.opacity = '0';
                    setTimeout(() => {
                        row.remove();
                        if (document.querySelectorAll("#cartTable tr").length === 0) {
                            location.reload(); // Reload để hiện giao diện "Giỏ hàng trống"
                        }
                    }, 300);
                    updateCartBadge();
                }
            }
        });
    }

    // 3. Xóa tất cả
    const clearBtn = document.getElementById("clearCart");

    if (clearBtn) {

        // Tạo modal xác nhận
        const modal = document.createElement("div");
        modal.id = "confirmModal";

        modal.innerHTML = `
        <div class="confirm-box">
            <h3>Xóa giỏ hàng</h3>
            <p>Bạn có chắc muốn xóa toàn bộ giỏ hàng?</p>

            <div class="confirm-buttons">
                <button id="cancelClear">Hủy</button>
                <button id="confirmClear">Xóa</button>
            </div>
        </div>
    `;

        document.body.appendChild(modal);

        clearBtn.addEventListener("click", () => {
            modal.style.display = "flex";
        });

        document.getElementById("cancelClear").addEventListener("click", () => {
            modal.style.display = "none";
        });

        document.getElementById("confirmClear").addEventListener("click", async () => {

            const res = await fetch("/Cart/Clear", {
                method: "POST"
            });

            if (res.ok) {
                location.reload();
            }
        });
    }

    // 4. Hàm cập nhật Badge (Số lượng hiện trên icon giỏ hàng ở Navbar)
    async function updateCartBadge() {
        const res = await fetch("/Cart/Count");
        const data = await res.json();
        const badge = document.getElementById("cartBadge");
        if (badge) {
            badge.innerText = data.count;
            badge.style.display = data.count > 0 ? "flex" : "none";
        }
    }
    // 5. Thanh toán
    const btnCheckout = document.querySelector('.btn-checkout');

    if (btnCheckout) {
        // Thêm tham số 'e' vào function để bắt sự kiện
        btnCheckout.addEventListener('click', async (e) => {
            e.preventDefault(); // Chặn hành vi mặc định (load lại trang nếu lỡ bọc trong form)

            const result = await Swal.fire({
                title: 'Xác nhận thanh toán?',
                text: "Đơn hàng của bạn sẽ được xử lý ngay lập tức!",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#1a1a1a',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Đồng ý',
                cancelButtonText: 'Hủy'
            });

            if (result.isConfirmed) {
                // Hiện loading
                Swal.fire({
                    title: 'Đang xử lý...',
                    didOpen: () => { Swal.showLoading(); },
                    allowOutsideClick: false
                });

                try {
                    const response = await fetch('/Order/Checkout', { method: 'POST' });
                    const data = await response.json();

                    if (data.success) {
                        // --- ĐÂY LÀ ĐOẠN XÓA GIAO DIỆN GIỎ HÀNG NGAY LẬP TỨC ---
                        const cartTable = document.getElementById('cartTable');
                        if (cartTable) cartTable.innerHTML = ''; // Xóa sạch danh sách SP

                        const grandTotal = document.getElementById('grandTotal');
                        if (grandTotal) grandTotal.innerText = '0 ₫'; // Đưa tổng tiền về 0

                        const cartBadge = document.getElementById('cartBadge');
                        if (cartBadge) {
                            cartBadge.innerText = '0';
                            cartBadge.style.display = 'none'; // Ẩn số lượng trên thanh menu
                        }
                        // ---------------------------------------------------------

                        // Báo thành công 1.5s rồi tự động chuyển sang trang Success
                        await Swal.fire({
                            icon: 'success',
                            title: 'Thành công!',
                            text: 'Đơn hàng của bạn đã được đặt.',
                            timer: 1500,
                            showConfirmButton: false
                        });

                        // ĐIỀU HƯỚNG TỚI TRANG THÀNH CÔNG
                        window.location.href = '/Order/Success';
                    } else {
                        Swal.fire('Lỗi!', data.message, 'error');
                    }
                } catch (error) {
                    console.error("Checkout Error: ", error);
                    Swal.fire('Thất bại!', 'Không thể kết nối đến máy chủ.', 'error');
                }
            }
        });
    }
    // Tìm nút Thanh toán theo class bạn đã đặt ở View Index của Cart
    const btnClear = document.getElementById('clearCart');

    if (btnClear) {
        btnClear.addEventListener('click', async () => {
            const confirmClear = await Swal.fire({
                title: 'Bạn có chắc chắn?',
                text: "Toàn bộ sản phẩm sẽ bị xóa khỏi giỏ hàng!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33', // Nút xóa nên để màu đỏ cho nguy hiểm
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Xóa hết',
                cancelButtonText: 'Giữ lại'
            });

            if (confirmClear.isConfirmed) {
                // Hiện loading tránh trường hợp mạng chậm
                Swal.fire({
                    title: 'Đang dọn dẹp...',
                    didOpen: () => { Swal.showLoading(); },
                    allowOutsideClick: false
                });

                try {
                    const response = await fetch('/Cart/Clear', { method: 'POST' });
                    if (response.ok) {
                        location.reload();
                    } else {
                        Swal.fire('Lỗi!', 'Không thể xóa giỏ hàng lúc này.', 'error');
                    }
                } catch (error) {
                    console.error("Clear Cart Error: ", error);
                    Swal.fire('Thất bại!', 'Lỗi kết nối máy chủ.', 'error');
                }
            }
        });
    }
});
document.addEventListener("DOMContentLoaded", () => {
    const cartTable = document.getElementById("cartTable");
    const selectAllItems = document.getElementById("selectAllItems");
    const selectedCount = document.getElementById("selectedCount");
    const selectedSubtotal = document.getElementById("selectedSubtotal");
    const grandTotal = document.getElementById("grandTotal");
    const btnCheckout = document.querySelector(".btn-checkout");

    function formatCurrency(value) {
        return new Intl.NumberFormat("vi-VN", {
            style: "currency",
            currency: "VND"
        }).format(value);
    }

    function updateSelectedSummary() {
        const checkedItems = document.querySelectorAll(".checkout-item:checked");
        let total = 0;

        checkedItems.forEach(item => {
            const row = item.closest("tr");
            const qtyInput = row.querySelector(".qty-input");
            const price = parseFloat(row.dataset.price || 0);
            const qty = parseInt(qtyInput?.value || 1);

            total += price * qty;
        });

        if (selectedCount) selectedCount.innerText = checkedItems.length;
        if (selectedSubtotal) selectedSubtotal.innerText = formatCurrency(total);
        if (grandTotal) grandTotal.innerText = formatCurrency(total);

        const allItems = document.querySelectorAll(".checkout-item");
        if (selectAllItems) {
            selectAllItems.checked = allItems.length > 0 && checkedItems.length === allItems.length;
        }
    }

    if (selectAllItems) {
        selectAllItems.addEventListener("change", function () {
            document.querySelectorAll(".checkout-item").forEach(item => {
                item.checked = this.checked;
            });
            updateSelectedSummary();
        });
    }

    document.addEventListener("change", function (e) {
        if (e.target.classList.contains("checkout-item") || e.target.classList.contains("qty-input")) {
            updateSelectedSummary();
        }
    });

    if (btnCheckout) {
        btnCheckout.addEventListener("click", async (e) => {
            e.preventDefault();

            const selectedItems = [];
            document.querySelectorAll(".checkout-item:checked").forEach(item => {
                const row = item.closest("tr");
                selectedItems.push({
                    productId: parseInt(row.dataset.id),
                    productType: row.dataset.type,
                    quantity: parseInt(row.querySelector(".qty-input").value || 1)
                });
            });

            if (selectedItems.length === 0) {
                Swal.fire({
                    icon: "warning",
                    title: "Chưa chọn sản phẩm",
                    text: "Vui lòng chọn ít nhất một sản phẩm để thanh toán."
                });
                return;
            }

            const result = await Swal.fire({
                title: "Xác nhận thanh toán?",
                text: `Bạn đang chọn ${selectedItems.length} sản phẩm để thanh toán.`,
                icon: "question",
                showCancelButton: true,
                confirmButtonColor: "#0d6efd",
                cancelButtonColor: "#6c757d",
                confirmButtonText: "Thanh toán ngay",
                cancelButtonText: "Hủy"
            });

            if (!result.isConfirmed) return;

            Swal.fire({
                title: "Đang xử lý...",
                didOpen: () => Swal.showLoading(),
                allowOutsideClick: false
            });

            try {
                const response = await fetch("/Order/CheckoutSelected", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({ items: selectedItems })
                });

                const data = await response.json();

                if (data.success) {
                    await Swal.fire({
                        icon: "success",
                        title: "Thành công!",
                        text: "Đơn hàng của bạn đã được tạo.",
                        timer: 1400,
                        showConfirmButton: false
                    });

                    window.location.href = "/Order/Success";
                } else {
                    Swal.fire("Lỗi!", data.message || "Không thể thanh toán.", "error");
                }
            } catch (error) {
                console.error(error);
                Swal.fire("Thất bại!", "Không thể kết nối đến máy chủ.", "error");
            }
        });
    }

    updateSelectedSummary();
});