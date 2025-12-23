document.addEventListener('DOMContentLoaded', function() {
    // Mobile Menu Toggle
    const menuToggle = document.getElementById('mobile-menu-toggle');
    const mobileMenu = document.getElementById('mobile-menu');
    
    if (menuToggle && mobileMenu) {
        menuToggle.addEventListener('click', function() {
            mobileMenu.classList.toggle('d-none');
            mobileMenu.classList.toggle('animate-fade-in-up');
        });
    }

    // Search Toggle
    const searchToggle = document.getElementById('search-toggle');
    const searchBar = document.getElementById('search-bar');
    const searchClose = document.getElementById('search-close');

    if (searchToggle && searchBar) {
        searchToggle.addEventListener('click', function() {
            searchBar.classList.remove('d-none');
            searchToggle.classList.add('d-none');
        });
    }

    if (searchClose && searchBar && searchToggle) {
        searchClose.addEventListener('click', function() {
            searchBar.classList.add('d-none');
            searchToggle.classList.remove('d-none');
        });
    }

    // Initialize Tooltips (if any)
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});
