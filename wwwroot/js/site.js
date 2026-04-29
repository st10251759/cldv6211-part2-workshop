// ============================================================
// MediBook Global JavaScript
// ============================================================

// ── Navbar scroll effect ──
window.addEventListener('scroll', function () {
    const navbar = document.querySelector('.medibook-navbar');
    if (navbar) {
        if (window.scrollY > 50) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
    }

    // Scroll to top button visibility
    const scrollBtn = document.getElementById('scrollTopBtn');
    if (scrollBtn) {
        if (window.scrollY > 300) {
            scrollBtn.classList.add('visible');
        } else {
            scrollBtn.classList.remove('visible');
        }
    }
});

// ── Scroll to top ──
function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// ── Active nav link highlight ──
document.addEventListener('DOMContentLoaded', function () {
    const currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll('.medibook-navbar .nav-link').forEach(link => {
        const href = link.getAttribute('href');
        if (href && currentPath.startsWith(href.toLowerCase()) && href !== '/') {
            link.classList.add('active');
        } else if (href === '/' && currentPath === '/') {
            link.classList.add('active');
        }
    });
});

// ── Intersection Observer for fade-in animations ──
document.addEventListener('DOMContentLoaded', function () {
    const fadeElements = document.querySelectorAll('.fade-in-up');
    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                setTimeout(() => {
                    entry.target.classList.add('visible');
                }, index * 100);
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1 });
    fadeElements.forEach(el => observer.observe(el));
});

// ── Card Carousel ──
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.card-carousel-wrapper').forEach(wrapper => {
        const track = wrapper.querySelector('.card-carousel-track');
        const items = wrapper.querySelectorAll('.card-carousel-item');
        const prevBtn = wrapper.parentElement.querySelector('.carousel-prev');
        const nextBtn = wrapper.parentElement.querySelector('.carousel-next');
        const dotsContainer = wrapper.parentElement.querySelector('.carousel-dots');

        if (!track || items.length === 0) return;

        let currentIndex = 0;
        let autoplayTimer;
        const visibleCount = () => {
            if (window.innerWidth < 576) return 1;
            if (window.innerWidth < 992) return 2;
            return 3;
        };
        const maxIndex = () => Math.max(0, items.length - visibleCount());

        // Build dots
        if (dotsContainer) {
            dotsContainer.innerHTML = '';
            for (let i = 0; i <= maxIndex(); i++) {
                const dot = document.createElement('div');
                dot.className = 'carousel-dot' + (i === 0 ? ' active' : '');
                dot.addEventListener('click', () => goTo(i));
                dotsContainer.appendChild(dot);
            }
        }

        function updateDots() {
            if (!dotsContainer) return;
            dotsContainer.querySelectorAll('.carousel-dot').forEach((dot, i) => {
                dot.classList.toggle('active', i === currentIndex);
            });
        }

        function goTo(index) {
            currentIndex = Math.max(0, Math.min(index, maxIndex()));
            const itemWidth = items[0].offsetWidth + 24; // gap
            track.style.transform = `translateX(-${currentIndex * itemWidth}px)`;
            updateDots();
        }

        function next() { goTo(currentIndex < maxIndex() ? currentIndex + 1 : 0); }
        function prev() { goTo(currentIndex > 0 ? currentIndex - 1 : maxIndex()); }

        if (nextBtn) nextBtn.addEventListener('click', () => { next(); resetAutoplay(); });
        if (prevBtn) prevBtn.addEventListener('click', () => { prev(); resetAutoplay(); });

        function startAutoplay() {
            autoplayTimer = setInterval(next, 4000);
        }
        function resetAutoplay() {
            clearInterval(autoplayTimer);
            startAutoplay();
        }

        startAutoplay();
        window.addEventListener('resize', () => goTo(0));
    });
});