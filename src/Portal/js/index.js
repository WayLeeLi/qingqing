// site.js - 全局交互脚本
(function () {
    // 汉堡菜单
    const hamburger = document.getElementById('hamburger');
    const navLinks = document.getElementById('navLinks');
    if (hamburger && navLinks) {
        hamburger.addEventListener('click', () => navLinks.classList.toggle('open'));
        navLinks.querySelectorAll('a').forEach(a => a.addEventListener('click', () => navLinks.classList.remove('open')));
    }

    // 平滑滚动
    document.querySelectorAll('a[href^="#"]').forEach(a => {
        a.addEventListener('click', e => {
            const target = document.querySelector(a.getAttribute('href'));
            if (target) {
                e.preventDefault();
                target.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
        });
    });

    // 滚动显示浮動按鈕
    const backTopBtn = document.querySelector('.float-btn-top');
    if (backTopBtn) {
        window.addEventListener('scroll', () => {
            if (window.scrollY > 400) backTopBtn.classList.add('visible');
            else backTopBtn.classList.remove('visible');
        });
        backTopBtn.addEventListener('click', () => window.scrollTo({ top: 0, behavior: 'smooth' }));
    }

    // 滚动动画 (简单的淡入)
    const observer = new IntersectionObserver(entries => {
        entries.forEach(e => {
            if (e.isIntersecting) {
                e.target.style.opacity = '1';
                e.target.style.transform = 'translateY(0)';
            }
        });
    }, { threshold: 0.1 });
    document.querySelectorAll('.service-card, .story-img-frame, .story-grid > div:last-child').forEach(el => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(28px)';
        el.style.transition = 'opacity .75s ease, transform .75s ease';
        observer.observe(el);
    });
})();