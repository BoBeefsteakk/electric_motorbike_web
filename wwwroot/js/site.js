/* ═══════════════════════════════════════════════════
   VINFAST WEB — GLOBAL JS
═══════════════════════════════════════════════════ */
const API_BASE = "http://localhost:5000";
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
        const userId = window.currentUserId;
        if (!userId) return;

        const res = await fetch(`${API_BASE}/api/cart/count?user_id=${encodeURIComponent(userId)}`);
        const data = await res.json();

        const badge = document.getElementById('cartBadge');
        if (badge) {
            const count = data.count || 0;
            badge.textContent = count;
            badge.style.display = count > 0 ? 'flex' : 'none';
        }
    } catch (err) {
        console.error('Badge error:', err);
    }
}

// ── Add to cart ─────────────────────────────────────
async function addToCart(productId, productType, name, price, image, colorName = null, colorValue = null) {
    try {
        const userId = window.currentUserId;

        if (!userId) {
            alert('Bạn cần đăng nhập để lưu giỏ hàng');
            return;
        }

        const res = await fetch(`${API_BASE}/api/cart/add`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                user_id: userId,
                product_id: productId,
                product_type: productType,
                name,
                price,
                image,
                quantity: 1,
                color_name: colorName,
                color_value: colorValue
            })
        });

        const data = await res.json();

        if (data.success) {
            const badge = document.getElementById('cartBadge');
            if (badge) {
                badge.textContent = data.count;
                badge.style.display = data.count > 0 ? 'flex' : 'none';
            }
            showToast('✓ Đã thêm vào giỏ hàng!');
        } else {
            showToast('Lỗi thêm giỏ hàng');
        }
    } catch (err) {
        console.error(err);
        showToast('Không kết nối được BE');
    }
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
async function updateQty(productId, productType, qty, row = null) {
    try {
        const userId = window.currentUserId;
        const res = await fetch(`${API_BASE}/api/cart/update-qty`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                user_id: userId,
                product_id: productId,
                product_type: productType,
                qty
            })
        });

        const data = await res.json();
        if (data.success) {
            if (row) {
                const itemTotalEl = row.querySelector(".item-total");
                if (itemTotalEl) {
                    itemTotalEl.innerText =
                        (data.itemTotal || 0).toLocaleString('vi-VN') + ' ₫';
                }
            }

            const grandEl = document.getElementById("grandTotal");
            if (grandEl) {
                grandEl.innerText =
                    (data.grandTotal || 0).toLocaleString('vi-VN') + ' ₫';
            }

            updateCartBadge();
        }
    } catch (err) {
        console.error("UPDATE QTY ERROR:", err);
    }
}

// ── Cart: remove item ───────────────────────────────
async function removeItem(productId, productType) {
    try {
        const userId = window.currentUserId;
        const res = await fetch(`${API_BASE}/api/cart/remove`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                user_id: userId,
                product_id: productId,
                product_type: productType
            })
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
// ── GLOBAL SMART SEARCH ──────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    const input = document.getElementById('globalSearchInput');
    const box = document.getElementById('globalSuggestionBox');
    const bar = document.getElementById('dynamicSearchBar');

    if (!input || !box || !bar) return;

    let data = [];
    let loaded = false;
    let timeout = null;

    bar.style.display = 'block';

    const normalize = (str) => {
        return (str || '')
            .toLowerCase()
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .replace(/đ/g, 'd')
            .replace(/[^a-z0-9\s]/g, ' ')
            .replace(/\s+/g, ' ')
            .trim();
    };

    const formatPrice = (price) => {
        return `${new Intl.NumberFormat('vi-VN').format(price || 0)} ₫`;
    };

    const buildLink = (item) => {
        if (item.category === 'car') return `/Cars/Detail/${item.id}`;
        if (item.category === 'motorbike') return `/Products/Detail/${item.id}`;
        if (item.category === 'accessory') return `/Accessories/Detail/${item.id}`;
        return '#';
    };

    const getCategoryTitle = (category) => {
        if (category === 'motorbike') return 'Xe máy';
        if (category === 'car') return 'Ô tô';
        if (category === 'accessory') return 'Phụ kiện';
        return 'Khác';
    };

    const detectCategory = (query) => {
        const q = normalize(query);

        if (
            q.includes('xe may') ||
            q.includes('xe dien') ||
            q.includes('motorbike')
        ) return 'motorbike';

        if (
            q.includes('o to') ||
            q.includes('oto') ||
            q.includes('car')
        ) return 'car';

        if (
            q.includes('phu kien') ||
            q.includes('accessory')
        ) return 'accessory';

        return null;
    };

    const parsePriceQuery = (query) => {
        const q = normalize(query);

        const toMoney = (n) => {
            const val = parseFloat(n);
            if (isNaN(val)) return null;
            return val * 1000000;
        };

        let min = null;
        let max = null;

        let m = q.match(/duoi\s+(\d+(\.\d+)?)/);
        if (m) {
            max = toMoney(m[1]);
            return { min, max };
        }

        m = q.match(/tren\s+(\d+(\.\d+)?)/);
        if (m) {
            min = toMoney(m[1]);
            return { min, max };
        }

        m = q.match(/tu\s+(\d+(\.\d+)?)\s+(den|toi)\s+(\d+(\.\d+)?)/);
        if (m) {
            min = toMoney(m[1]);
            max = toMoney(m[4]);
            return { min, max };
        }

        return { min, max };
    };

    const loadData = async () => {
        if (loaded) return;

        try {
            box.innerHTML = `<div class="p-3 text-muted">Đang tải...</div>`;

            const res = await fetch('/Search/GetSearchIndex');
            data = await res.json();
            loaded = true;
        } catch (e) {
            console.error('Search load error:', e);
            box.innerHTML = `<div class="p-3 text-danger">Lỗi tải dữ liệu</div>`;
        }
    };

    const score = (item, q) => {
        const query = normalize(q);
        const name = normalize(item.name);
        const categoryLabel = normalize(item.categoryLabel || '');

        let s = 0;
        if (!query) return 0;

        if (name === query) s += 1200;
        if (name.startsWith(query)) s += 800;
        if (name.includes(query)) s += 400;

        query.split(' ').forEach(word => {
            if (name.includes(word)) s += 120;
            if (categoryLabel.includes(word)) s += 40;
        });

        return s;
    };

    const search = (q) => {
        if (!q) {
            return data.slice(0, 9);
        }

        const categoryFilter = detectCategory(q);
        const priceFilter = parsePriceQuery(q);
        const nq = normalize(q);

        let result = [...data];

        if (categoryFilter) {
            result = result.filter(x => x.category === categoryFilter);
        }

        if (priceFilter.min !== null) {
            result = result.filter(x => Number(x.price) >= priceFilter.min);
        }

        if (priceFilter.max !== null) {
            result = result.filter(x => Number(x.price) <= priceFilter.max);
        }

        result = result
            .map(i => ({ ...i, _score: score(i, nq) }))
            .filter(i => {
                if (categoryFilter || priceFilter.min !== null || priceFilter.max !== null) {
                    return i._score > 0 || nq.includes('duoi') || nq.includes('tren') || nq.includes('tu');
                }
                return i._score > 0;
            })
            .sort((a, b) => b._score - a._score);

        return result.slice(0, 9);
    };

    const highlight = (text, query) => {
        if (!query) return text || '';

        const words = normalize(query).split(' ').filter(Boolean);
        let result = text || '';

        words.forEach(word => {
            if (!word) return;
            const regex = new RegExp(`(${word})`, 'ig');
            result = result.replace(regex, '<mark>$1</mark>');
        });

        return result;
    };

    const renderGrouped = (list, query = '') => {
        if (!list || list.length === 0) {
            box.innerHTML = `<div class="p-3 text-muted">Không tìm thấy sản phẩm</div>`;
            return;
        }

        const groups = {
            motorbike: [],
            car: [],
            accessory: []
        };

        list.forEach(item => {
            if (groups[item.category]) groups[item.category].push(item);
        });

        let html = '';

        Object.keys(groups).forEach(category => {
            if (groups[category].length === 0) return;

            html += `
                <div class="search-group">
                    <div class="search-group-title">${getCategoryTitle(category)}</div>
                    ${groups[category].map(i => `
                        <a href="${buildLink(i)}" class="suggestion-item no-image">
                            <div class="suggestion-info">
                                <div class="suggestion-name">${highlight(i.name, query)}</div>
                                <div class="suggestion-price">${formatPrice(i.price)}</div>
                            </div>
                        </a>
                    `).join('')}
                </div>
            `;
        });

        box.innerHTML = html;
    };

    input.addEventListener('focus', async () => {
        await loadData();
        renderGrouped(data.slice(0, 9), '');
    });

    input.addEventListener('input', function () {
        clearTimeout(timeout);
        const q = this.value.trim();

        timeout = setTimeout(async () => {
            await loadData();
            const results = search(q);
            renderGrouped(results, q);
        }, 180);
    });

    input.addEventListener('keydown', async (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();

            const q = input.value.trim();
            if (!q) return;

            await loadData();
            const results = search(q);

            if (results.length > 0) {
                window.location.href = buildLink(results[0]);
            } else {
                box.innerHTML = `<div class="p-3 text-muted">Không tìm thấy sản phẩm</div>`;
            }
        }
    });

    box.addEventListener('click', (e) => {
        const item = e.target.closest('.suggestion-item');
        if (!item) return;

        const href = item.getAttribute('href');
        if (href && href !== '#') {
            window.location.href = href;
        }
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

                const userId = window.currentUserId;

                const res = await fetch(`${API_BASE}/api/cart/update-qty`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        user_id: userId,
                        product_id: productId,
                        product_type: productType,
                        qty
                    })
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

                const userId = window.currentUserId;

                const res = await fetch(`${API_BASE}/api/cart/remove`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        user_id: userId,
                        product_id: productId,
                        product_type: productType
                    })
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
                    const userId = window.currentUserId;

                    const response = await fetch(`${API_BASE}/api/order/checkout`, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({
                            user_id: userId
                        })
                    });

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