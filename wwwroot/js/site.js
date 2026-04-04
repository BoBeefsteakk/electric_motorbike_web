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