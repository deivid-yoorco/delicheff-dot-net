/*
** nopCommerce custom js functions
*/



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