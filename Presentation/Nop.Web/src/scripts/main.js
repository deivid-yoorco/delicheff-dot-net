$(document).ready(function () {
    $('.sidenav').sidenav();
    $('.parallax').parallax();
    $('.modal').modal();
    $('.tooltipped').tooltip();

    var selects = document.querySelectorAll('select');
    var selectsInstance = M.FormSelect.init(selects, {});

    var collapsible = document.querySelectorAll('.collapsible');
    if (collapsible) {
        var collapsibleInstance = M.Collapsible.init(collapsible, {});
    }

    var carousel = document.querySelectorAll('.main.carousel.carousel-slider');
    if (carousel) {
        var carouselInstance = M.Carousel.init(
            carousel,
            { fullWidth: true, indicators: true });
    }

    var parallax = document.querySelectorAll('.parallax');
    if (parallax) {
        var parallaxInstance = M.Parallax.init(
            parallax,
            { height: "250px" });
    }

    var menuDropdowns = document.querySelectorAll('.top-menu-dropdown-trigger');
    var menuDropdownsInstance = [];
    if (menuDropdowns) {
        for (var i = 0; i < menuDropdowns.length; i++) {
            if (!isEmpty(menuDropdowns[i])) {
                menuDropdownsInstance[i] = M.Dropdown.init(
                    menuDropdowns[i],
                    { hover: true, constrainWidth: false, coverTrigger: false });
            }
        }
    }
   

});

function isEmpty(obj) {
    for (var prop in obj) {
        if (obj.hasOwnProperty(prop))
            return false;
    }

    return JSON.stringify(obj) === JSON.stringify({});
}