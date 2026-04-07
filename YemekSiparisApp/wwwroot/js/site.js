// YemekSiparis - Site JavaScript

// Navbar scroll efekti
window.addEventListener('scroll', () => {
    const nav = document.querySelector('.navbar');
    if (nav) {
        nav.style.boxShadow = window.scrollY > 20
            ? '0 4px 20px rgba(0,0,0,0.4)'
            : 'none';
    }
});

// Alert otomatik kapanma
document.querySelectorAll('.alert').forEach(alert => {
    setTimeout(() => {
        alert.style.transition = 'opacity 0.5s ease';
        alert.style.opacity = '0';
        setTimeout(() => alert.remove(), 500);
    }, 4000);
});

// Form submit loading durumu
document.querySelectorAll('form').forEach(form => {
    form.addEventListener('submit', () => {
        const btn = form.querySelector('[type="submit"]');
        if (btn) {
            btn.disabled = true;
            btn.textContent = 'Lütfen bekleyin...';
        }
    });
});
