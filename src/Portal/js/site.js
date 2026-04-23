// ===================== 全局组件 =====================

// 1. 汉堡菜单（移动端导航）
(function () {
    const hamburger = document.getElementById('hamburger');
    const navLinks = document.getElementById('navLinks');
    if (hamburger && navLinks) {
        hamburger.addEventListener('click', function () {
            navLinks.classList.toggle('open');
        });
        // 点击导航链接后自动关闭菜单
        navLinks.querySelectorAll('a').forEach(function (link) {
            link.addEventListener('click', function () {
                navLinks.classList.remove('open');
            });
        });
    }
})();

// 2. 返回顶部按钮（全局）
(function () {
    const btt = document.getElementById('btt');
    if (btt) {
        window.addEventListener('scroll', function () {
            btt.classList.toggle('vis', window.scrollY > 200);
        });
        btt.addEventListener('click', function () {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    }
})();

// 3. 平滑滚动（针对所有以 # 开头的内部锚点）
(function () {
    document.querySelectorAll('a[href^="#"]').forEach(function (anchor) {
        anchor.addEventListener('click', function (e) {
            const targetId = this.getAttribute('href');
            if (targetId === '#') return;
            const targetElement = document.querySelector(targetId);
            if (targetElement) {
                e.preventDefault();
                targetElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
        });
    });
})();

// 4. 其他全局初始化（例如：给当前页面导航高亮）
(function () {
    const currentPath = window.location.pathname;
    document.querySelectorAll('.nav-links a').forEach(function (link) {
        if (link.getAttribute('href') === currentPath) {
            link.classList.add('active');
        } else {
            link.classList.remove('active');
        }
    });
})();