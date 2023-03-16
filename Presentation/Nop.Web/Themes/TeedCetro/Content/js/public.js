/*
** nopCommerce custom js functions
*/

// Clean html to show in mini cart
function CleanHtml() {

    // Check if cart exists
    if ($('#flyout-cart').length > 0) {

        // Clean elements one by one
        $('#flyout-cart .attributes').each(function (i, e) {
            $(e).html($(e).html().replace(/\[.*?\]/, ""));
        });
    }
}

// Run everytime it enters
$(document).ready(function () {
    CleanHtml();
});

function OpenWindow(query, w, h, scroll) {
    var l = (screen.width - w) / 2;
    var t = (screen.height - h) / 2;

    winprops = 'resizable=0, height=' + h + ',width=' + w + ',top=' + t + ',left=' + l + 'w';
    if (scroll) winprops += ',scrollbars=1';
    var f = window.open(query, "_blank", winprops);
}

function setLocation(url) {
    window.location.href = url;
}

function displayAjaxLoading(display) {
    if (display) {
        $('.ajax-loading').show();
    }
    else {
        $('.ajax-loading').hide('slow');
    }
}

function displayPopupNotification(message, messagetype, modal) {
    //types: success, error, warning

    if (messagetype == 'error') {
        $('#main-modal-header').html('<i class="material-icons red-text">error</i>');
    }
    else if (messagetype == 'warning') {
        $('#main-modal-header').html('<i class="material-icons orange-text">warning</i>');
    }
    else {
        $('#main-modal-header').html('<i class="material-icons green-text">check_circle</i>');
    }


    var htmlcode = '';
    if ((typeof message) == 'string') {
        htmlcode = '<p>' + message + '</p>';
    } else {
        for (var i = 0; i < message.length; i++) {
            htmlcode = htmlcode + '<p>' + message[i] + '</p>';
        }
    }


    $('#main-modal-content').html(htmlcode);

    $('#main-modal').modal('open');

}
function displayPopupContentFromUrl(url, title, modal, width) {
    var isModal = (modal ? true : false);
    var targetWidth = (width ? width : 550);
    var maxHeight = $(window).height() - 20;

    $('<div></div>').load(url)
        .dialog({
            modal: isModal,
            position: ['center', 20],
            width: targetWidth,
            maxHeight: maxHeight,
            title: title,
            close: function (event, ui) {
                $(this).dialog('destroy').remove();
            }
        });
}

function displayBarNotification(message, messagetype, timeout) {

    //types: success, error, warning
    var cssclass = 'success';
    if (messagetype == 'success') {
        message = '<i class="material-icons green-text">check_circle</i> ' + message;
        cssclass = 'success';
    }
    else if (messagetype == 'error') {
        message = '<i class="material-icons red-text">error</i> ' + message;
        cssclass = 'error';
    }
    else if (messagetype == 'warning') {
        message = '<i class="material-icons orange-text">warning</i> ' + message;
        cssclass = 'warning';
    }

    //we do not encode displayed message

    //add new notifications
    var htmlcode = '';
    if ((typeof message) == 'string') {
        htmlcode = '<p class="content notification-content">' + message + '</p>';
    } else {
        for (var i = 0; i < message.length; i++) {
            htmlcode = htmlcode + '<p class="content notification-content">' + message[i] + '</p>';
        }
    }

    if (timeout == null) { timeout = 4500; }
    M.toast({ html: htmlcode, displayLength: timeout });
}

function htmlEncode(value) {
    return $('<div/>').text(value).html();
}

function htmlDecode(value) {
    return $('<div/>').html(value).text();
}


// CSRF (XSRF) security
function addAntiForgeryToken(data) {
    //if the object is undefined, create a new one.
    if (!data) {
        data = {};
    }
    //add token
    var tokenInput = $('input[name=__RequestVerificationToken]');
    if (tokenInput.length) {
        data.__RequestVerificationToken = tokenInput.val();
    }
    return data;
};
/*
** nopCommerce ajax cart implementation
*/


var AjaxCart = {
    loadWaiting: false,
    usepopupnotifications: true,
    topcartselector: '',
    topwishlistselector: '',
    flyoutcartselector: '',

    init: function (usepopupnotifications, topcartselector, topwishlistselector, flyoutcartselector) {
        this.loadWaiting = false;
        this.usepopupnotifications = usepopupnotifications;
        this.topcartselector = topcartselector;
        this.topwishlistselector = topwishlistselector;
        this.flyoutcartselector = flyoutcartselector;
    },

    setLoadWaiting: function (display) {
        displayAjaxLoading(display);
        this.loadWaiting = display;
    },

    //add a product to the cart/wishlist from the catalog pages
    addproducttocart_catalog: function (urladd) {
        if (this.loadWaiting != false) {
            return;
        }
        this.setLoadWaiting(true);

        $.ajax({
            cache: false,
            url: urladd,
            type: 'post',
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },

    //remove a product from the cart/wishlist from the catalog pages
    removeproductfromcart_catalog: function (urlremove) {
        if (this.loadWaiting != false) {
            return;
        }
        this.setLoadWaiting(true);

        $.ajax({
            cache: false,
            url: urlremove,
            type: 'get',
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },

    //add a product to the cart/wishlist from the product details page
    addproducttocart_details: function (urladd, formselector) {
        if (this.loadWaiting != false) {
            return;
        }
        this.setLoadWaiting(true);

        $.ajax({
            cache: false,
            url: urladd,
            data: $(formselector).serialize(),
            type: 'post',
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },

    //add a product to compare list
    addproducttocomparelist: function (urladd) {
        if (this.loadWaiting != false) {
            return;
        }
        this.setLoadWaiting(true);

        $.ajax({
            cache: false,
            url: urladd,
            type: 'post',
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },

    success_process: function (response) {
        if (response.updatetopcartsectionhtml) {
            $(AjaxCart.topcartselector).html(response.updatetopcartsectionhtml.replace(/\D/g, '').trim());
        }
        if (response.updatetopwishlistsectionhtml) {
            $(AjaxCart.topwishlistselector).html(response.updatetopwishlistsectionhtml);
        }
        if (response.updateflyoutcartsectionhtml) {
            $(AjaxCart.flyoutcartselector).replaceWith(response.updateflyoutcartsectionhtml);
            CleanHtml();
        }
        if (response.message) {
            //display notification
            if (response.success == true) {
                //success
                if (AjaxCart.usepopupnotifications == true) {
                    displayPopupNotification(response.message, 'success', true);
                }
                else {
                    if (!response.message.includes("cart")) {
                        //specify timeout for success messages
                        displayBarNotification(response.message, 'success', 5500);
                    } else {
                        displayBarNotification(response.message, 'success', 3000);
                        $('.flyout-cart').fadeIn().delay(4000).fadeOut(1000, function () {
                            $(this).css("display", "");
                        });
                    }
                }
            }
            else {
                //error
                if (AjaxCart.usepopupnotifications == true) {
                    displayPopupNotification(response.message, 'error', true);
                }
                else {
                    //no timeout for errors
                    displayBarNotification(response.message, 'error', 10000);
                }
            }
            return false;
        }
        if (response.redirect) {
            location.href = response.redirect;
            return true;
        }
        return false;
    },

    resetLoadWaiting: function () {
        AjaxCart.setLoadWaiting(false);
    },

    ajaxFailure: function () {
        alert('Failed to add the product. Please refresh the page and try one more time.');
    }
};

// CETRO
let changeElementType = (elm ,newType) => {
    var attrs = {};
    $.each(elm[0].attributes, function (idx, attr) {
        attrs[attr.nodeName] = attr.nodeValue;
    });
    elm.replaceWith(function () {
        return $("<" + newType + "/>", attrs).append($(this).contents());
    });
}
$(document).ready(function () {
    // Move category buttons to logo container and single button for tickets at the end
    var $logoContainer = $("#parent-logo-container").parent();
    var $tickets = $logoContainer.find('ul li a[href="/categorias/boletos"]').parent();
    var $anchor = $tickets.find('a');
    $anchor.html('Compra tus boletos');
    $tickets.attr('style', 'float: right !important; background-color: white !important; font-weight: bold !important;');
    $tickets.addClass('tickets-button');
    $anchor.attr('style', 'color: black !important;');
    var $navContainer = $logoContainer.find('ul').clone();
    $logoContainer.find('ul li').not('.tickets-button').remove();
    $navContainer.find('.tickets-button').remove();
    $navContainer.addClass('left');
    $navContainer.removeClass('right');
    $navContainer.css('margin-left', '135px');
    $navContainer.appendTo('#parent-logo-container');

    // Move user, search and cart icons to single button container
    var $icons = $('#main-icon-container ul.right li').clone();
    var $ticketsParent = $('.tickets-button').parent();
    $ticketsParent.append($icons);
    $ticketsParent.find('#topcartlink').attr('id', 'topcartlinkdup');

    // Re-add functionality to cart button
    $('#topcartlinkdup').mouseenter(function () {
        $('#flyout-cart').addClass('active');
    });
    $('#topcartlinkdup').mouseleave(function () {
        $('#flyout-cart').removeClass('active');
    });

    // Mobile nav changes
    $('#slide-out a').css('color', 'white');
    $('#slide-out').css('background-color', 'black');
    var $ticketsMobile = $("#slide-out").find('a[href="/categorias/boletos"]').parent();
    var $anchorMobile = $ticketsMobile.find('a');
    $anchorMobile.html('Compra tus boletos');
    $anchorMobile.css('color', 'black');
    $ticketsMobile.attr('style', 'background-color: white !important; font-weight: bold !important;');

    //Add new cart elements for double update when adding to cart
    AjaxCart.topcartselector += ', #topcartlinkdup #pop-cart';

    //Footer changes
    $('.page-footer .row .col').first().prepend($('#logo-container').clone());
    $('.page-footer #logo-container').attr('id', 'logo-container-footer');
    $('.page-footer #logo-container-footer img').css('margin-top', '15px');

    //Apply CSS
    $('.table-row .col').each(function (i, e) {
        var allAreImages = $(e).find(':not(img)').length === 0;
        if (allAreImages)
            $(e).css('text-align', 'center');
        $(e).find('ul').each(function (indx, ul) {
            changeElementType($(ul), "ol");
        });
    });
});
////////

window.onload = function () {
    if (document.querySelector('.grid') !== null) {
        var $parent = $('.grid');
        var $allImages = $parent.find('img').clone(true, true);
        $parent.children().remove();
        $allImages.each(function (i, e) {
            $(e).removeAttr('width');
            $(e).removeAttr('height');
            $parent.append(e);
        });
        var options =
        {
            srcNode: 'img',             // grid items (class, node)
            margin: '5px',              // margin in pixel, default: 0px
            width: '250px',             // grid item width in pixel, default: 220px
            max_width: '',              // dynamic gird item width if specified, (pixel)
            resizable: true,            // re-layout if window resize
            transition: 'all 0.5s ease' // support transition for CSS3, default: all 0.5s ease
        }
        document.querySelector('.grid').gridify(options);
    }
}