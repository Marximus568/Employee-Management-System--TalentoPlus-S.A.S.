// Dashboard Interactivity

$(document).ready(function () {
    // Sidebar Toggle
    $('#sidebarToggle').on('click', function () {
        $('.sidebar').toggleClass('collapsed');
        $('.main-content').toggleClass('expanded');
    });

    // User Dropdown Toggle
    $('.user-profile').on('click', function (e) {
        e.stopPropagation();
        $('.dropdown-menu').fadeToggle(200);
    });

    // Close dropdown when clicking outside
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.user-dropdown').length) {
            $('.dropdown-menu').fadeOut(200);
        }
    });

    // Highlight active link based on current URL (fallback if server-side fails)
    var currentPath = window.location.pathname;
    $('.nav-item').each(function () {
        var href = $(this).attr('href');
        if (currentPath.includes(href) && href !== '/Dashboard/Index') {
            $(this).addClass('active');
        } else if (currentPath === '/Dashboard/Index' && href === '/Dashboard/Index') {
            $(this).addClass('active');
        }
    });
});
