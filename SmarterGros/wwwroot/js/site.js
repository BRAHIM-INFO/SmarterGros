// ========== GLOBAL JS ==========

document.addEventListener('DOMContentLoaded', function () {
    // Set current date
    const dateEl = document.getElementById('currentDate');
    if (dateEl) {
        const now = new Date();
        const options = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' };
        dateEl.textContent = now.toLocaleDateString('ar-DZ', options);
    }

    // Sidebar toggle
    const toggleBtn = document.getElementById('sidebarToggle');
    const sidebar = document.querySelector('.sg-sidebar');
    if (toggleBtn && sidebar) {
        toggleBtn.addEventListener('click', () => {
            sidebar.classList.toggle('open');
        });
    }

    // Auto-hide alerts
    const alerts = document.querySelectorAll('.sg-alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.opacity = '0';
            alert.style.transition = 'opacity 0.5s';
            setTimeout(() => alert.remove(), 500);
        }, 4000);
    });

    // Add active nav indicator
    const navItems = document.querySelectorAll('.sg-nav-item');
    navItems.forEach(item => {
        item.addEventListener('click', function () {
            navItems.forEach(n => n.classList.remove('active'));
            this.classList.add('active');
        });
    });
});

// Global search
document.addEventListener('DOMContentLoaded', function () {
    const globalSearch = document.getElementById('globalSearch');
    if (globalSearch) {
        let timeout;
        globalSearch.addEventListener('input', function () {
            clearTimeout(timeout);
            timeout = setTimeout(() => {
                const val = this.value.trim();
                if (val.length > 2) {
                    window.location.href = `/Products/Index?search=${encodeURIComponent(val)}`;
                }
            }, 500);
        });
    }
});

// Utility functions
function formatNumber(num, decimals = 2) {
    return parseFloat(num).toLocaleString('ar-DZ', {
        minimumFractionDigits: decimals,
        maximumFractionDigits: decimals
    });
}

function showToast(message, type = 'success') {
    const toast = document.createElement('div');
    toast.className = `sg-toast sg-toast-${type}`;
    toast.innerHTML = `<i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'} me-2"></i>${message}`;
    toast.style.cssText = `
        position: fixed;
        top: 80px;
        left: 20px;
        z-index: 9999;
        background: ${type === 'success' ? '#166534' : '#991b1b'};
        color: white;
        padding: 12px 20px;
        border-radius: 10px;
        font-family: 'Cairo', sans-serif;
        font-size: 14px;
        font-weight: 600;
        box-shadow: 0 5px 20px rgba(0,0,0,0.2);
        animation: slideIn 0.3s ease;
    `;
    document.body.appendChild(toast);
    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transition = 'opacity 0.3s';
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

async function apiCall(url, method = 'GET', data = null) {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    const options = {
        method,
        headers: { 'Content-Type': 'application/json' }
    };
    if (token) options.headers['RequestVerificationToken'] = token;
    if (data) options.body = JSON.stringify(data);

    const response = await fetch(url, options);
    return await response.json();
}