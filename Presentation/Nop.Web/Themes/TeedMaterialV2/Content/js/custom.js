$(document).ready(function () {
    $('.customer-growth-hacking.collection-item a').html('<i style="vertical-align: middle;" class="small material-icons">card_giftcard</i> &#33;Gana s&#250;per gratis!');
    let cards = $(".product-card");
    $.each(cards, function (index, element) {
        let id = $(element).data("productid");
        let equivalenceCoefficient = $(element).data("equivalencecoefficient");
        let buyingBySecondary = $(element).data("buyingbysecondary");
        let weightInterval = $(element).data("weightinterval");
        let sku = $(element).data("sku");
        let name = $(element).data("name");
        let category = $(element).data("category");
        let brand = $(element).data("brand");
        let variant = $(element).data("variant");
        let list = $(element).data("list");
        let position = $(element).data("position");
        let price = $(element).data("price");
        initialSetup(buyingBySecondary, id, equivalenceCoefficient, weightInterval);
        productImpression(sku, name, category, brand, variant, list, position, price);
    });
    $(".link-rss").hide();
    setTimeout(function () { AjaxCart.init(false, '.header-links .cart-qty', '.header-links .wishlist-qty', '#flyout-cart'); }, 500);
    //$('.header').on('mouseenter', '#topcartlink', function () {
    //    $('#flyout-cart').addClass('active');
    //});
    //$('.header').on('mouseleave', '#topcartlink', function () {
    //    $('#flyout-cart').removeClass('active');
    //});
    //$('.header').on('mouseenter', '#flyout-cart', function () {
    //    $('#flyout-cart').addClass('active');
    //});
    //$('.header').on('mouseleave', '#flyout-cart', function () {
    //    $('#flyout-cart').removeClass('active');
    //});
    $('.top-logo-container').on('mouseenter', '#topcartlink', function () {
        $('#flyout-cart').addClass('active');
    });
    $('.top-logo-container').on('mouseleave', '#topcartlink', function () {
        $('#flyout-cart').removeClass('active');
    });
    $('.top-icons-container').on('mouseenter', '#flyout-cart', function () {
        $('#flyout-cart').addClass('active');
    });
    $('.top-icons-container').on('mouseleave', '#flyout-cart', function () {
        $('#flyout-cart').removeClass('active');
    });
    $('.notify-bubble').show(400);

    $.ajax({
        url: '/admin/custompages/GetCustomPages',
        type: 'GET',
        cache: false,
        success: function (data) {
            if (data.length > 0) {
                var element = "<li class='tab-shape hide-on-med-and-down' style='border-bottom-color:" + $("button").css("background-color") + "'><a style='text-align:center;color:#fff;font-weight:bold' href='/' >" + $("#store-name").val() + "</a></li>";
                $(".header ul.left").append(element);
                $(".header .nav-wrapper.container").attr("style", "width:auto !important");
                $("#slide-out").prepend("<div style='margin-bottom:20px'></div>");
            }

            $.each(data, function (i, val) {
                var element = "<li class='tab-shape hide-on-med-and-down' style='border-bottom-color:#" + val.TabColor + "'><a style='text-align:center;font-weight:bold;color:#fff' href='/" + val.Slug + "'>" + val.PageName + "</a></li>";
                $(".header ul.left").append(element);
                $("#slide-out").prepend(element.replace("tab-shape hide-on-med-and-down", "").replace("border-bottom-color", "height:60px;background-color").replace("style='text-align:center", "style='height:60px!important;line-height:60px!important;text-align:center"));
            });

            $("#slide-out").prepend("<div style='margin-bottom:20px'></div>");
        }
    });

    // Top navbar categories slick
    setTimeout(function () {
        $('.top-menu-categories-slick').slick({
            dots: false,
            centerMode: false,
            arrows: true,
            slidesToShow: 7,
            infinite: true,
            variableWidth: true,
        });
        $('.top-menu-categories-slick').show();
    }, 500);

    // Balance checkbox
    $('.balance-checkbox input[type="checkbox"]').click(function () {
        $(this).attr('disabled', 'disabled');
        var value = $(this).prop('checked');
        $.ajax({
            type: "GET",
            url: "/ShoppingCart/SetBalanceUsageValue?value=" + value,
            success: function (data) {
                location.reload();
            },
            error: function (err) {
                console.log(err);
                location.reload();
            }
        });
    });
});

setTimeout(() => {
    $(".vertical-nav").show();
}, 500);

function openMenu() {
    document.getElementById("slide-out-web").style.marginLeft = "0px";

    document.getElementById("menuCanvasNav").style.width = "100%";
    document.getElementById("menuCanvasNav").style.opacity = "0.8";

    $(".close-menu a").removeAttr("href");
    $(".close-menu a").attr("onclick", 'closeMenu()');
    $("#slide-out-web").css("overflow-y", "auto");
}

function closeMenu() {
    document.getElementById("slide-out-web").style.marginLeft = "-350px";

    document.getElementById("menuCanvasNav").style.width = "0%";
    document.getElementById("menuCanvasNav").style.opacity = "0";

    $("#logo-container").attr("href", "/");
    $("#slide-out-web").css("overflow-y", "hidden");
}

function addToCart(id, coefficient, weightInterval, shoppingCartTypeId) {
    disableElements(id);
    setTimeout(function () {
        $.ajax({
            cache: false,
            url: '/ShoppingCart/CartAjax',
            type: 'get',
            success: function (data) {
                $('#pop-cart').hide();
                $('#pop-cart').empty();
                $('#pop-cart').append(data);
                $('#pop-cart').show(400);

                var newValue = parseInt($(".current-qty-" + id).val()) + 1;
                $(".current-qty-" + id).val(newValue);
                var reload = false;
                if (window.location.pathname == '/cart')
                    reload = true;
                updatedProductType(id, coefficient, weightInterval, true, false, reload);

                enableElements(id);
            }
        });
    }, 800);

    var buyingBySecondary = $(".type-selection-" + id).val() > 0;
    var selectedProperty = $(".type-selection-" + id).val() === undefined ? null : $(".type-selection-" + id).val();
    AjaxCart.addproducttocart_catalog('/shoppingcart/AddProductToCart_Catalog?productId=' + id + '&shoppingCartTypeId=' + shoppingCartTypeId + '&quantity=1&forceredirection=false&buyingBySecondary=' + buyingBySecondary + '&selectedPropertyOption=' + selectedProperty);
    return false;
}

function removeFromCart(id, coefficient, weightInterval) {
    disableElements(id);
    setTimeout(function () {
        $.ajax({
            cache: false,
            url: '/ShoppingCart/CartAjax',
            type: 'get',
            success: function (data) {
                $('#pop-cart').hide();
                $('#pop-cart').empty();
                $('#pop-cart').append(data);
                $('#pop-cart').show(400);

                var newValue = parseInt($(".current-qty-" + id).val()) - 1;
                $(".current-qty-" + id).val(newValue);
                var reload = false;
                if (window.location.pathname == '/cart')
                    reload = true;
                updatedProductType(id, coefficient, weightInterval, true, false, reload);

                enableElements(id);
            }
        });
    }, 800);

    var buyingBySecondary = $(".type-selection-" + id).val() > 0;
    var selectedProperty = $(".type-selection-" + id).val() === undefined ? null : $(".type-selection-" + id).val();
    AjaxCart.removeproductfromcart_catalog('/shoppingcart/RemoveProductFromCart_Catalog?productId=' + id + '&buyingBySecondary=' + buyingBySecondary + '&selectedPropertyOption=' + selectedProperty);
    return false;
}

function updateCartWithSelect(id, coefficient, weightInterval, element) {
    disableElements(id);
    setTimeout(function () {
        $.ajax({
            cache: false,
            url: '/ShoppingCart/CartAjax',
            type: 'get',
            success: function (data) {
                $('#pop-cart').hide();
                $('#pop-cart').empty();
                $('#pop-cart').append(data);
                $('#pop-cart').show(400);

                var reload = false;
                if (window.location.pathname == '/cart')
                    reload = true;
                updatedProductType(id, coefficient, weightInterval, true, false, reload);
                enableElements(id);
            }
        });
    }, 800);

    var buyingBySecondary = $(".type-selection-" + id).val() > 0;
    var selectedProperty = $(".type-selection-" + id).val() === undefined ? null : $(".type-selection-" + id).val();
    var newValue = parseInt($(element).val());
    $(".current-qty-" + id).val(newValue);
    AjaxCart.removeproductfromcart_catalog('/shoppingcart/UpdateProductQty_Catalog?productId=' + id + '&buyingBySecondary=' + buyingBySecondary + '&qty=' + newValue + '&selectedPropertyOption=' + selectedProperty);
    return false;
}

function disableElements(id) {
    $(".btnCart-" + id).attr('disabled', true);
    $(".btnCart-remove-" + id).attr('disabled', true);
}

function enableElements(id, buyingBySecondary, equivalenceCoefficient, weightInterval) {
    $('.btnCart-' + id).attr('disabled', false);
    $(".btnCart-remove-" + id).attr('disabled', !(parseInt($(".current-qty-" + id).val())) > 0);
    initialSetup()
}

function updatedProductType(id, coefficient, weightInterval, shouldUpdateProduct = false, shouldUpdateSelect = false, doReload = false) {
    var newValue = 0;
    var type = ($('.checkbox-weight-' + id).is(":checked") && coefficient > 0) || weightInterval > 0 ? "gr" : "pz";
    newValue = $(".current-qty-" + id).val();

    if (shouldUpdateProduct) {
        updateProduct(id);
    }

    if (shouldUpdateSelect) {
        var select = $(".selected-qty-" + id);
        select.empty();
        for (var j = 0; j < select.length; j++) {
            var options = select[j].options;
            if ($('.checkbox-weight-' + id).is(":checked") && coefficient > 0) {
                for (var i = 0; i <= 50; i++) {
                    var value = $(".current-qty-" + id).val();
                    value = ((i * 1000) / coefficient).toFixed(2);
                    if (value >= 1000) {
                        value = (value / 1000).toFixed(2);
                        type = " kg";
                    }
                    var option = new Option(value + type, i);
                    options.add(option);
                }
            }
            else if (weightInterval > 0) {
                for (var w = 0; w <= 50; w++) {
                    var weightValue = $(".current-qty-" + id).val();
                    weightValue = (w * weightInterval).toFixed(2);
                    if (weightValue >= 1000) {
                        weightValue = (weightValue / 1000).toFixed(2);
                        type = " kg";
                    }
                    var optionWeight = new Option(weightValue + type, w);
                    options.add(optionWeight);
                }
            }
            else {
                for (var e = 0; e <= 50; e++) {
                    var newOption = new Option(e + " " + type, e);
                    options.add(newOption);
                }
            }
        }
    }

    $(".selected-qty-" + id).find('option[value=' + parseInt(newValue) + ']').prop('selected', true);
    if (doReload) {
        location.reload();
    }
}

function updatePropertySelection(id, element) {
    $(".type-selection-" + id).val($(element).val());
    updateProduct(id);
}

function updateProduct(id) {
    var buyingBySecondary = $('.checkbox-weight-' + id).is(":checked");
    var selectedProperty = $(".type-selection-" + id).val() === undefined ? null : $(".type-selection-" + id).val();

    $.ajax({
        cache: false,
        url: '/ShoppingCart/UpdateCartProductBuyingType?productId=' + id + '&buyingBySecondary=' + buyingBySecondary + '&selectedPropertyOption=' + selectedProperty,
        type: 'get'
    });
}

function addWish(id) {
    $('.add-to-wish-button-' + id).show();
    $('.mobil-add-to-wish-button-' + id).show();
    $('.add-to-cart-button-' + id).hide();
    $('.mobil-add-to-cart-button-' + id).hide();
}

function addCart(id) {
    $('#add-to-wish-button-' + id).hide();
    $('#mobil-add-to-wish-button-' + id).hide();
    $('#add-to-cart-button-' + id).show();
    $('#mobil-add-to-cart-button-' + id).show();
}

$(window).scroll(function () {
    if ($(this).scrollTop() > 200) {
        if (window.matchMedia("(min-width: 993px)").matches) {
            $('.menu-style').css({
                'display': 'none'
            });
            $('.height-top-menu').each(function () {
                this.style.setProperty('height', '5em', 'important');
            });
        }
    }
    else {
        if (window.matchMedia("(min-width: 993px)").matches) {
            $('.menu-style').css({
                'display': 'block'
            });
            $('.height-top-menu').each(function () {
                this.style.setProperty('height', '12em', 'important');
            });
        }
    }
});

function saveNewAddress() {
    $('#save-address-btn').addClass('disabled');

    var firstName = $('#input-Checkout-FirstName').val();
    var lastName = $('#input-Checkout-LastName').val();
    var email = $('#input-Checkout-Email').val();
    var company = $('#input-Checkout-Company').val();
    var countryId = $('.input-Checkout-Country').val();
    var stateId = $('.input-Checkout-State').val();
    var city = $('#input-Checkout-City').val();
    var address1 = $('#google-map-address').val();
    var address2 = $('#input-Checkout-Address2').val();
    var zcp = $('#input-Checkout-ZCP').val();
    var phone = $('#input-Checkout-Phone').val();
    var attrAddress = $('.attr-address-checkout').val();
    var interior = $('#input-Checkout-Interior').val();

    var latitude = $('#input-Latitude').val();
    var longitude = $('#input-Longitude').val();

    if (firstName == "") {
        $("#first-name-field-teed").show();
    }
    else {
        $("#first-name-field-teed").hide();
    }

    if (lastName == "") {
        $("#last-name-field-teed").show();
    }
    else {
        $("#last-name-field-teed").hide();
    }

    if (interior == "") {
        $("#interior-field-teed").show();
    }
    else {
        $("#interior-field-teed").hide();
    }

    if (email == "") {
        $("#email-field-teed").show();
    }
    else if (!validateEmail(email)) {
        $("#validate-email-field-teed").show();
        $("#email-field-teed").hide();
    }
    else {
        $("#email-field-teed").hide();
        $("#validate-email-field-teed").hide();
    }

    if (countryId == "0") {
        $("#countryId-field-teed").show();
    }
    else {
        $("#countryId-field-teed").hide();
    }

    if (stateId == "0") {
        $("#stateId-field-teed").show();
    }
    else {
        $("#stateId-field-teed").hide();
    }

    if (address1 == "") {
        $("#address1-field-teed").show();
    }
    else {
        $("#address1-field-teed").hide();
    }

    if (zcp == "") {
        $("#zcp-field-teed").show();
    }
    else {
        $("#zcp-field-teed").hide();
    }

    if (phone == "") {
        $("#phone-field-teed").show();
    }
    else {
        $("#phone-field-teed").hide();
    }

    function validateEmail(email) {
        var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return re.test(String(email).toLowerCase());
    }

    if (!validateEmail(email) ||
        firstName == "" ||
        lastName == "" ||
        email == "" ||
        countryId == "0" ||
        stateId == "0" ||
        city == "" ||
        address1 == "" ||
        zcp == "" ||
        phone == "" ||
        interior == "") {
        $("#general-address-error").show();
        $('#save-address-btn').removeClass('disabled');
    }
    else {
        $("#general-address-error").hide();
        var datadir = {
            firstName: firstName,
            lastName: lastName,
            email: email,
            company: company,
            countryId: countryId,
            stateId: stateId,
            city: city,
            address1: address1 + " interior " + interior,
            address2: address2,
            zcp: zcp,
            phone: phone,
            attrAddress: attrAddress,
            latitude: latitude,
            longitude: longitude
        };

        //addAntiForgeryToken(data);

        $.ajax({
            cache: false,
            url: '/Customer/SaveNewAddress',
            type: 'POST',
            data: datadir,
            success: function (response) {
                window.location = response.redirect
            },
            error: function () {
                $('#save-address-btn').removeClass('disabled');
                alert('Error al guardar');
            }
        });
    }
}

function handleProductTypeCheckboxClick(element) {
    var $box = $(element);
    if ($box.is(":checked")) {
        var group = "input:checkbox[name='" + $box.attr("name") + "']";
        $(group).prop("checked", false);
        $("." + $box.attr("class")).prop("checked", true);
    }
    else if ($box.not(":checked")) {
        e.preventDefault();
        return false;
    }
    else {
        $box.prop("checked", false);
    }
}

function initialSetup(buyingBySecondary, id, equivalenceCoefficient, weightInterval) {
    if (buyingBySecondary === "True") {
        $('.checkbox-unit-' + id).prop('checked', false);
        $('.checkbox-weight-' + id).prop('checked', true);
    }
    else {
        $('.checkbox-unit-' + id).prop('checked', true);
        $('.checkbox-weight-' + id).prop('checked', false);
    }
    updatedProductType(id, equivalenceCoefficient, weightInterval, false, true);

    setTimeout(function () {
        enableElements(id);
    }, 500);
}

function menuHover(categoryId) {
    $("#container-subcategory-" + categoryId).removeClass("min-bottom");
    var bottomSubcategoryContainer = $(window).height() - ($("#container-subcategory-" + categoryId).position().top + $("#container-subcategory-" + categoryId).outerHeight(true));
    if (bottomSubcategoryContainer < 0) {
        $("#container-subcategory-" + categoryId).addClass("min-bottom");
        $("#container-subcategory-" + categoryId).css('top', 'auto');
    } else {
        $("#container-subcategory-" + categoryId).css('top', $("#parentCategory-" + categoryId).position().top + 30);
    }
};

function redirectCategory() {
    var page = $('#select-categories').val();
    if (page != 0) {
        window.location.href = window.location.origin + "/" + page;
    }
}

function ondBodyLoad() {
    checkZipCode();
    popupModal();
}

//Modals for zip code and popups
var zipCode;
function checkZipCode() {
    setTimeout(function () {
        $('.send-email').addClass('disabled');
        $("#checkZipCode").appendTo(".page-footer");
        $('.spinner-layer').css('border-color', $('.teed-primary').css('background-color'));
        $('#checkZipCode').modal({
            dismissible: true,
            onCloseStart: function () {
                document.body.style = '';
            }
        });
        $("#postal-code").on('keyup', function (e) {
            if (e.key === 'Enter' || e.keyCode === 13) {
                $('.verify-zip-code').click();
            }
        });
        $("#email-for-notification").on('keyup', function (e) {
            if (e.key === 'Enter' || e.keyCode === 13) {
                $('.send-email').click();
            }
        });
        var color =
            $('.teed-primary').length > 0 ?
                $('.teed-primary').css('background-color') :
                $('.teed-primary-text').length > 0 ?
                    $('.teed-primary-text').css('color') :
                    "#444";
        $('.done-email span, .zip-code-ok span')
            .css('color', color);

        var zipCodeModalShowCountCookie = getCookie("zipCodeModalShowCount");
        if (typeof zipCodeModalShowCountCookie != 'undefined' && zipCodeModalShowCountCookie != "") {
            var count = 1;
            if (zipCodeModalShowCountCookie == "1")
                count = 2;
            else if (zipCodeModalShowCountCookie == "2")
                count = 3;
            else if (zipCodeModalShowCountCookie == "3") {
                count = 3;
                document.cookie = "notShowZipCodeModal=true; max-age=" + 30 * 24 * 60 * 60 + ";";
            }
            document.cookie = "zipCodeModalShowCount=" + count + "; max-age=" + 30 * 24 * 60 * 60 + ";";
        } else
            document.cookie = "zipCodeModalShowCount=0; max-age=" + 30 * 24 * 60 * 60 + ";";
        var notShowZipCodeModalCookie = getCookie("notShowZipCodeModal");
        if (typeof notShowZipCodeModalCookie != 'undefined' && notShowZipCodeModalCookie != "") {
            if (notShowZipCodeModalCookie == "false") {
                $('#checkZipCode').modal('open');
                $('#postal-code').focus();
            }
        } else {
            $('#checkZipCode').modal('open');
            $('#postal-code').focus();
        }
        $('#checkZipCode').css("background-color", "white !important");
        if ($(window).width() <= 600) {
            $('#checkZipCode .modal-content').append($('#checkZipCode p.left'));
        }
        $('#email-for-notification').on('keyup change', function (e) {
            if (isEmail($(this).val()))
                $('.send-email').removeClass('disabled');
            else
                $('.send-email').addClass('disabled');
        });
        $('#checkZipCode .modal-close').click(function () {
            if ($('#zipCodeHide').prop('checked'))
                document.cookie = "notShowZipCodeModal=true; max-age=" + 30 * 24 * 60 * 60 + ";";
        });
    }, 1000);
}
function changeModalBool() {
    zipCode = $('#postal-code').val();
    $('#zip-code-avilable').hide();
    $('#zip-code-unavilable').hide();
    $('.done-email').hide();
    $('.err-email').hide();
    $('.email-input').show();
    if (isEmail($('#email-for-notification').val()))
        $('.send-email').removeClass('disabled');
    else
        $('.send-email').addClass('disabled');
    if (zipCode != null && zipCode.trim() != "") {
        $('#checkZipCode .preloader-wrapper').show();
        $('.verify-zip-code').addClass('disabled');
        $.ajax({
            processData: false,
            contentType: false,
            type: "GET",
            url: "/Api/App/VerifyPostalCodeRegion?postalCode=" + zipCode,
            success: function (data) {
                if (data == true) {
                    $('#zip-code-avilable').show();
                    $('#checkZipCode .modal-close').addClass('absolute-button');
                }
                else {
                    $('#zip-code-unavilable').show();
                    $('#email-for-notification').focus();
                }
                $('.zip-code-first').hide();
                $('#checkZipCode .preloader-wrapper').hide();
                $('.verify-zip-code').removeClass('disabled');
                document.cookie = "notShowZipCodeModal=true; max-age=" + 30 * 24 * 60 * 60 + ";";
            },
            error: function (err) {
                console.log(err);
            }
        });
    }
}
function sendEmail() {
    if ($('.send-email.disabled').length < 1) {
        var email = $('#email-for-notification').val();
        if (email != null && email.trim() != "") {
            $('#checkZipCode .preloader-wrapper').show();
            $('.send-email').addClass('disabled');
            var body = {
                PostalCode: zipCode,
                Email: email
            };
            $.ajax({
                //cache: false,
                data: JSON.stringify(body),
                contentType: "application/json",
                //processData: false,
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                type: "POST",
                url: "/Api/App/PostalCodeNotificationRequest",
                success: function (data) {
                    $('.done-email').show();
                    document.cookie = "notShowZipCodeModal=true; max-age=" + 30 * 24 * 60 * 60 + ";";
                    $('#checkZipCode .preloader-wrapper').hide();
                    $('.error-unavilable').hide();
                },
                error: function (err) {
                    console.log(err);
                    $('.err-email span').text(err.responseJSON);
                    $('.err-email').show();
                    $('#checkZipCode .preloader-wrapper').hide();
                    $('.send-email').removeClass('disabled');
                }
            });
        }
    }
}

function popupModal() {
    setTimeout(function () {
        $("#popupModal").appendTo(".page-footer");
        $('#popupModal').modal({
            dismissible: true,
            onCloseStart: function () {
                document.body.style = '';
            }
        });
        $.ajax({
            processData: false,
            contentType: false,
            type: "GET",
            url: "/Api/Popup/GetPopupData?isFirstTime=false&mobileOnly=false",
            success: function (data) {
                if (data.length > 0) {
                    var popup = data[0];
                    var idForCookie = popup.mobile.substring(popup.mobile.indexOf('id=') + 3, popup.mobile.length);

                    var notShowPopupCookie = getCookie("notShowPopup" + idForCookie);
                    if (typeof notShowPopupCookie != 'undefined' && notShowPopupCookie != "") {
                        if (notShowPopupCookie == "false") {
                            setAndShowPopup(popup, idForCookie);
                        }
                    } else {
                        setAndShowPopup(popup, idForCookie);
                    }
                }
            },
            error: function (err) {
                console.log(err);
            }
        });
    }, 1000);
}
function isEmail(email) {
    var regex = /^([a-zA-Z0-9_.+-])+\@(([a-zA-Z0-9-])+\.)+([a-zA-Z0-9]{2,4})+$/;
    return regex.test(email);
}
function resetZipCodeCookie() {
    document.cookie = "notShowZipCodeModal=false; max-age=" + 30 * 24 * 60 * 60 + ";";
    location.reload(true);
}
function setAndShowPopup(popup, idForCookie) {
    $('#popupModal img.mobile-popup').attr('src', '/Api' + popup.mobile);
    $('#popupModal img.desktop-popup').attr('src', '/Api' + popup.desktop);
    $('#popupModal').modal('open');
    document.cookie = "notShowPopup" + idForCookie + "=true; max-age=" + 1 * 24 * 60 * 60 + ";";
}
function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
}
//