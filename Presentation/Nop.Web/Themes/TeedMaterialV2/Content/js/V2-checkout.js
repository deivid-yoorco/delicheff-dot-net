var form = false;
var saveUrl = false;
var isSuccess = false;
var successUrl = false;

function saveAddressNextForm() {
    doSaveAddressNextForm();
}
function doSaveAddressNextForm() {
    $('#nextForm-button-disable').addClass('disabled');

    var firstName = $('#input-Checkout-FirstName').val();
    var lastName = $('#input-Checkout-LastName').val();
    var email = $('#input-Checkout-Email').val();
    var company = $('#input-Checkout-Company').val();
    var countryId = $('.input-Checkout-Country').val();
    var stateId = $('#state-form').val();
    var city = $('#input-Checkout-City').val();
    var address1 = $('#google-map-address').val();
    var address2 = $('#input-Checkout-Address2').val();
    var zcp = $('#input-Checkout-ZCP').val();
    var phone = $('#input-Checkout-Phone').val().replace(/\s/g, '');
    var attrAddress = $($(".attr-address-checkout")[1]).val();
    var exterior = $($(".attr-address-checkout")[0]).val();
    var interior = $('#input-Checkout-Interior').val();
    var latitude = $('#input-Latitude').val();
    var longitude = $('#input-Longitude').val();
    var woInterior = $("#checkNunInt").is(":checked");


    var valoresAceptados = /^[0-9]+$/;
    var phoneBol = false;
    if (phone.match(valoresAceptados) && phone.length == 10) {
        phoneBol = true;
    }

    if (phoneBol == false) {
        $("#phone-field-teed").show();
    }
    else {
        $("#phone-field-teed").hide();
    }

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

    if (exterior == "") {
        $("#ext-number-field-teed").show();
    }
    else {
        $("#ext-number-field-teed").hide();
    }

    if (interior == "" && !woInterior) {
        $("#int-number-field-teed").show();
    } else {
        $("#int-number-field-teed").hide();
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
        exterior == "" ||
        (interior == "" && !woInterior) ||
        interior == "" || phoneBol == false) {
        $("#general-address-error").show();
        $('#nextForm-button-disable').removeClass('disabled');
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
            address1: address1 + ", exterior " + exterior + ", interior " + interior,
            address2: address2,
            zcp: zcp,
            phone: phone,
            attrAddress: attrAddress,
            exterior: exterior,
            latitude: latitude,
            longitude: longitude
        };

        //addAntiForgeryToken(data);

        $.ajax({
            cache: false,
            url: 'Checkout/OpcSaveBillingNew',
            type: 'POST',
            data: datadir,
            success: function () {
                $.ajax({
                    cache: false,
                    url: 'Checkout/ShippingMethodNew',
                    type: 'GET',
                    success: function (data) {
                        var m = data.ShippingMethods.length == 1 ? 12 : data.ShippingMethods.length == 2 ? 6 : 4;
                        for (var i = 0; i < data.ShippingMethods.length; i++) {
                            var str = '<li class="col s12 m' + m + '" style="margin-bottom:15px;"><div class="method-name"><label>' +
                                '<input id="shippingoption_' + i + '" class="with-gap" value="' + data.ShippingMethods[i].Name + '___' +
                                data.ShippingMethods[i].ShippingRateComputationMethodSystemName + '/' + data.ShippingMethods[i].Fee + '" ' +
                                'name="shippingoption" type="radio" checked/>' + '<span id="shippingoption__' + i + '">' + data.ShippingMethods[i].Name +
                                ' (' + data.ShippingMethods[i].Fee + ')' + '</span></label></div></li>';
                            $('#option-shipping-container').append(str);
                        }
                        $('#shipping-method-buttons-container').show();
                        disableTimes();
                        $('.first-step-checkout').hide();
                        scrollTop();
                        autoSelectDate();
                    },
                    error: function () {
                        alert('Error de conexión');
                        scrollTop();
                    }
                });
            },
            error: function () {
                alert('Error al guardar');
                scrollTop();
            }
        });
    }
}

function selectAddressNext() {
    doSelectAddressNext();
}
function doSelectAddressNext() {
    $(".verifying-address").show();
    $(".verifying-address").addClass("active");
    $("#selected-address-error").hide();
    $('#next-button-disable').addClass('disabled');
    var postalCode = $("#billing-address-select-new option:selected").data("postalcode");
    $.ajax({
        cache: false,
        url: '/api/app/VerifyPostalCodeRegion?postalcode=' + postalCode,
        type: 'GET',
        success: function (data) {
            console.log("resultado de verificacion", data);
            if (data) {
                var addressId = $('#billing-address-select-new').val();
                $.ajax({
                    cache: false,
                    url: 'Checkout/AddressEdit?addressId=' + addressId,
                    type: 'GET',
                    success: function (response) {
                        doSelectAddressSave(response)
                    },
                    error: function () {
                        alert('Error al continuar');
                    }
                });
            }
            else {
                console.log(postalCode);
                $('#next-button-disable').removeClass('disabled');
                $(".verifying-address").hide();
                $(".verifying-address").removeClass("active");
                $("#selected-address-error").show();
                return;
            }
        },
        error: function () {
            alert('Error de conexión');
        }
    });
}

function saveAddressSave() {
    doSelectAddressSave();
}
function doSelectAddressSave(response) {
    var datadir = {
        firstName: response.firstName,
        lastName: response.lastName,
        email: response.email,
        company: response.company,
        countryId: response.countryId,
        stateId: response.stateId,
        city: response.city,
        address1: response.address1,
        address2: response.address2,
        zcp: response.zcp,
        phone: response.phone,
        attrAddress: response.attrAddress
    };

    $.ajax({
        cache: false,
        url: 'Checkout/OpcSaveBillingNew',
        type: 'POST',
        data: datadir,
        success: function () {
            $.ajax({
                cache: false,
                url: 'Checkout/ShippingMethodNew',
                type: 'GET',
                success: function (data) {
                    var m = data.ShippingMethods.length === 1 ? 12 : data.ShippingMethods.length == 2 ? 6 : 4;
                    for (var i = 0; i < data.ShippingMethods.length; i++) {
                        var str = '<li class="col s12 m' + m + '" style="margin-bottom:15px;"><div class="method-name"><label>' +
                            '<input id="shippingoption_' + i + '" class="with-gap" value="' + data.ShippingMethods[i].Name + '___' +
                            data.ShippingMethods[i].ShippingRateComputationMethodSystemName + '/' + data.ShippingMethods[i].Fee + '" ' +
                            'name="shippingoption" type="radio" checked/>' + '<span id="shippingoption__' + i + '">' + data.ShippingMethods[i].Name +
                            ' (' + data.ShippingMethods[i].Fee + ')' + '</span></label></div></li>';
                        $('#option-shipping-container').append(str);
                    }
                    $('#shipping-method-buttons-container').show();
                    disableTimes();
                    $('.first-step-checkout').hide();

                    $(".verifying-address").hide();
                    $(".verifying-address").removeClass("active");

                    scrollTop();

                    autoSelectDate();
                },
                error: function () {
                    alert('Error de conexión');
                    $(".verifying-address").hide();
                    $(".verifying-address").removeClass("active");
                }
            });
        },
        error: function () {
            alert('Error al guardar');
            $(".verifying-address").hide();
            $(".verifying-address").removeClass("active");
        }
    });
}

function shippingOptionsSave() {
    doshippingOptionsSave();
}
function doshippingOptionsSave() {
    $('#shipping-button-disable').addClass('disabled');

    var methods = document.getElementsByName('shippingoption');
    if (methods.length == 0) {
        alert('Your order cannot be completed at this time as there is no shipping methods available for it. Please make necessary changes in your shipping address.');
        return false;
    }

    var data = "";
    for (var i = 0; i < methods.length; i++) {
        if (methods[i].checked) {
            data = methods[i].value;
            break;
            //return true;
        }
    }

    data = data.split('/');

    if (data[0] != 'Terrestre___Shipping.FixedOrByWeight') {
        document.getElementById('calculate-shipping').innerHTML = data[1];

        var sub = $('#calculate-subtotal').text();
        var nSub = Number(sub.replace(/[^0-9\.-]+/g, ""));

        //var tax = $('#calculate-tax').text();
        //var nTax = Number(tax.replace(/[^0-9\.-]+/g, ""));

        var shi = $('#calculate-shipping').text();
        var nShi = Number(shi.replace(/[^0-9\.-]+/g, ""));

        var dis = $('#calculate-total-discount').text();
        if (dis == '') dis = "0";
        var nDis = Number(dis.replace(/[^0-9\.-]+/g, ""));

        var nTotal = nSub + /*nTax +*/ nShi - nDis;
        var total = nTotal.toFixed(2).toString().split("").reverse().join("").replace(/(\d{3}(?!$))/g, '$1,');

        document.getElementById('calculate-total').innerHTML = '$' + total.split("").reverse().join("");
        document.getElementById('calculate-total').style.fontWeight = "bold";
    }

    var url = 'Checkout/SelectShippingMethodNew?shippingoption=' + data[0] + '&orderdate=' + $(".datepicker").val();

    var orderTotal = $('#calculate-total').text();
    var nOrderTotal = Number(orderTotal.replace(/[^0-9\.-]+/g, ""));

    var selectedDate = $(".datepicker").val();
    var selectedHour = $("#timepicker option:selected").val();

    if (!selectedDate) {
        $("#shipping-date-validation").show();
    }
    else {
        $("#shipping-date-validation").hide();
    }

    if (!selectedHour || selectedHour === "0") {
        $("#shipping-hour-validation").show();
    }
    else {
        $("#shipping-hour-validation").hide();
    }

    if (!selectedDate || !selectedHour || selectedHour === "0") {
        $('#shipping-button-disable').removeClass('disabled');
    }
    else {
        $.ajax({
            cache: false,
            url: url,
            type: 'POST',
            //data: data,
            success: function () {
                $.ajax({
                    cache: false,
                    url: 'Checkout/PaymentMethodNew?orderTotal=' + nOrderTotal,
                    type: 'GET',
                    success: function (data) {
                        $('#shipping-method-buttons-container').hide();
                        options(data, nOrderTotal);
                        $('#checkout-step-payment-method').show();
                        $('#payment-method-buttons-container').show();

                        scrollTop();
                    },
                    error: function () {
                        alert('Error de conexión payment');
                    }
                });
                $(".date-error").hide();
            },
            error: function (data) {
                $(".date-error").hide();
                if (data.responseText == "DateError") {
                    $(".date-error").show();
                    $('#shipping-button-disable').removeClass('disabled');
                } else {
                    alert('Error de conexión shipping');
                    $('#shipping-button-disable').removeClass('disabled');
                }
            }
        });
    }
}

function savePaymentSave(orderNote = null) {
    doSavePaymentSave(orderNote);
}

const hidePaymentElements = () => {
    $("#address-info-container").hide();
    $("#confirm-button-disable").show();
    $("#paypal-plus").hide();
    $("#pplusContinueButton").hide();
    $('#loading-confirm').removeClass('active');
    $('#payment-stripe-form').hide();
    $('#payment-netpay-form-container').hide();
    $("#third-step-select-billing-address").hide();
    $('#payment-visa-form').hide();
};

function doSavePaymentSave(orderNote) {
    var payment = $('#payment-method-select-container').val();
    $('#confirm-button-disable').addClass('disabled');
    $('#loading-confirm').addClass('active');

    if (payment === '') {
        alert('Debes seleccionar un metodo de pago.');
        return false;
    }

    var points = false;
    var url = 'Checkout/SelectPaymentMethodNew?paymentmethod=' + payment + '&UseRewardPoints=' + points;
    $.ajax({
        cache: false,
        url: url,
        type: 'POST',
        success: function () {
            orderConfirmSave(orderNote);
        },
        error: function () {
            $('#confirm-button-disable').removeClass('disabled');
            $('#loading-confirm').removeClass('active');
            alert('Error de conexión. Por favor vuelve a intentarlo o contáctanos para ayudarte a solucionar el problema.');
        }
    });
}

function orderConfirmSave(orderNote) {
    doOrderConfirmSave(orderNote);
}
function doOrderConfirmSave(orderNote) {
    var formatedDate = "";
    if ($(".datepicker").val()) {
        formatedDate = $(".datepicker").datepicker().val();
        //var date = $(".datepicker").datepicker()[0].M_Datepicker.date;
        //formatedDate = formatDate(date.getDate()) + "-" + formatDate(date.getMonth() + 1) + "-" + date.getFullYear();
    }

    if ($('#checkbox-billing').is(':checked')) {
        if ($('#billing-address-select-new').val()) {
            console.log(1);
            var data = $('#billing-address-select-new').val();
            $.ajax({
                cache: false,
                url: 'Checkout/AddressEdit?addressId=' + data,
                type: 'GET',
                success: function (response) {
                    var datadir = {
                        parameter: $('#payment-method-select-container option:selected').attr('id'),
                        messagegift: $('#message-gift').val(),
                        signsgift: $('#name-signs-gift').val(),
                        firstName: response.firstName,
                        lastName: response.lastName,
                        email: response.email,
                        company: response.company,
                        countryId: response.countryId,
                        stateId: response.stateId,
                        city: response.city,
                        address1: response.address1,
                        address2: response.address2,
                        zcp: response.zcp,
                        phone: response.phone,
                        cardName: $('#input-name').val(),
                        cardNumber: $('#input-card').val(),
                        cardMonth: $('#input-month').val(),
                        cardYear: $('#input-year').val(),
                        cardCvv: $('#input-cvv').val(),
                        shippingDate: formatedDate,
                        shippingHour: $("#timepicker option:selected").val(),
                        orderNote: orderNote
                    };
                    sendData(datadir);
                },
                error: function () {
                    alert('Error al continuar');
                }
            });
        }
        else {
            var dir = {
                parameter: $('#payment-method-select-container option:selected').attr('id'),
                messagegift: $('#message-gift').val(),
                signsgift: $('#name-signs-gift').val(),
                firstName: $('#input-Checkout-FirstName').val(),
                lastName: $('#input-Checkout-LastName').val(),
                email: $('#input-Checkout-Email').val(),
                countryId: $('.input-Checkout-Country').val(),
                stateId: $('.input-Checkout-State').val(),
                city: $('#input-Checkout-City').val(),
                address1: $('#google-map-address').val() + " exterior " + $($(".attr-address-checkout")[0]).val() + " interior " + $('#input-Checkout-Interior').val(),
                address2: $('#input-Checkout-Address2').val(),
                zcp: $('#input-Checkout-ZCP').val(),
                phone: $('#input-Checkout-Phone').val().replace(/\s/g, ''),
                cardName: $('#input-name').val(),
                cardNumber: $('#input-card').val(),
                cardMonth: $('#input-month').val(),
                cardYear: $('#input-year').val(),
                cardCvv: $('#input-cvv').val(),
                shippingDate: formatedDate,
                shippingHour: $("#timepicker option:selected").val(),
                orderNote: orderNote
            };
            sendData(dir);
        }
    }
    else {
        var body = {
            parameter: $('#payment-method-select-container option:selected').attr('id'),
            messagegift: $('#message-gift').val(),
            signsgift: $('#name-signs-gift').val(),
            firstName: $('#input-card-FirstName').val(),
            lastName: $('#input-card-LastName').val(),
            email: $('#input-card-Email').val(),
            countryId: $('.input-card-Country').val(),
            stateId: $('.input-card-State').val(),
            city: $('#input-card-City').val(),
            address1: $('#google-map-address').val() + " exterior " + $($(".attr-address-checkout")[0]).val() + " interior " + $('#input-Checkout-Interior').val(),
            address2: $('#input-card-Address2').val(),
            zcp: $('#input-card-ZCP').val(),
            phone: $('#input-card-Phone').val(),
            cardName: $('#input-name').val(),
            cardNumber: $('#input-card').val(),
            cardMonth: $('#input-month').val(),
            cardYear: $('#input-year').val(),
            cardCvv: $('#input-cvv').val(),
            shippingDate: formatedDate,
            shippingHour: $("#timepicker option:selected").val(),
            orderNote: orderNote
        };
        sendData(body);
    }
}

function orderConfirmNextStep(response) {
    doOrderConfirmNextStep(response);
}
function doOrderConfirmNextStep(response) {
    if (response.orderId) {
        registerPayment(response.orderId);
    }
    //if (response.update_section) {
    //    window.location = OrderConfirm.successUrl;
    //}
    if (response.error) {
        $('#confirm-button-disable').removeClass('disabled');
        $('#loading-confirm').removeClass('active');
        if ((typeof response.message) === 'string') {
            alert(response.message);
        } else {
            alert(response.message.join("\n"));
        }

        return false;
    }

    if (response.orderId && !checkoutPaymentIsDone) {
        var refreshIntervalId = window.setInterval(function () {
            if (checkoutPaymentIsDone) {
                finalDoneStep(response);
                clearInterval(refreshIntervalId);
            }
        }, 100);
    } else
        finalDoneStep(response);
}

function finalDoneStep(response) {
    if (response.redirect) {
        location.href = response.redirect;
        return;
    }

    if (response.success) {
        window.location = successUrl;
    }
}

function orderConfirmInit(saveUrlF, successUrlF) {
    saveUrl = saveUrlF;
    successUrl = successUrlF;
}

function toggleTimeOptions(disable, useCase) {
    switch (useCase) {
        case 1:
            $("option[value='1:00 PM - 3:00 PM']").prop("disabled", disable);
            $("option[value='3:00 PM - 5:00 PM']").prop("disabled", disable);
            $("option[value='7:00 PM - 9:00 PM']").prop("disabled", disable);
            break;
        case 2:
            if (disable) { disableHolidayTimes(); }
            $("option[value='7:00 PM - 9:00 PM']").prop("disabled", disable);
            break;
        default:
            break;
    }

    if (!disable) {
        $("option[value='1:00 PM - 3:00 PM']").prop("disabled", disable);
        $("option[value='3:00 PM - 5:00 PM']").prop("disabled", disable);
        $("option[value='5:00 PM - 7:00 PM']").prop("disabled", disable);
        $("option[value='7:00 PM - 9:00 PM']").prop("disabled", disable);
    }
};

function disableHolidayTimes() {
    $("#timepicker").prop("disabled", true);
    $('#loading-time').show();
    $('#loading-time').addClass('active');

    $.ajax({
        url: '/Order/CheckHolidayTimes?date=' + $(".datepicker").val(),
        method: 'GET',
        success: function (data) {
            $("#timepicker").prop("disabled", false);
            var activateTime1 = data[0] < 14;
            var activateTime2 = data[1] < 48;
            var activateTime3 = data[2] < 56;
            $("option[value='1:00 PM - 3:00 PM']").prop("disabled", !activateTime1);
            $("option[value='3:00 PM - 5:00 PM']").prop("disabled", !activateTime2);
            $("option[value='5:00 PM - 7:00 PM']").prop("disabled", !activateTime3);
            $('#loading-time').removeClass('active');
            $('#loading-time').hide();
        },
        error: function (error) {
            console.log(JSON.stringify(error));
            $('#loading-time').removeClass('active');
            $('#loading-time').hide();
        }
    });
};

function validateTimes(dateValue) {
    $('#order-minimum-error-message span').html('');
    $('#order-minimum-error-message').hide();
    $("#timepicker").prop("disabled", true);
    $('#loading-time').show();
    $('#loading-time').addClass('active');
    $("#time-disabled-error").hide();
    $('#shipping-button-disable').removeClass('disabled');

    $('#date-msg').hide();
    $('#date-msg .msg').hide();

    if (dateValue) {
        $.ajax({
            url: '/Order/GetOrderMinimumPedidoCheck?date=' + dateValue,
            method: 'GET',
            success: function (msg) {
                if (msg === '' || typeof msg === 'undefined' || msg === null || isImpersonating)
                    $.ajax({
                        url: '/Order/GetWebTimes?date=' + dateValue,
                        method: 'GET',
                        success: function (data) {
                            $("#timepicker").prop("disabled", false);

                            $("#timepicker").empty();
                            $("#timepicker").html(data.optionValue);

                            $('#loading-time').removeClass('active');
                            $('#loading-time').hide();

                            if (data.anyDisabled) {
                                $("#time-disabled-error").show();
                            }

                            $("#timepicker").val("0");
                            setDateMsg();
                        },
                        error: function (error) {
                            console.log(JSON.stringify(error));
                            $('#loading-time').removeClass('active');
                            $('#loading-time').hide();
                        }
                    });
                else {
                    $('#loading-time').removeClass('active');
                    $('#loading-time').hide();
                    $('#shipping-button-disable').addClass('disabled');

                    $('#order-minimum-error-message span').html(msg);
                    $('#order-minimum-error-message').show();
                }
            },
            error: function (error) {
                console.log(JSON.stringify(error));
                $('#loading-time').removeClass('active');
                $('#loading-time').hide();
            }
        });
    } else {
        console.log('datepicker value empty, not performing Ajax');
        $('#loading-time').removeClass('active');
        $('#loading-time').hide();
    }
};

function disableTimes() {
    $('#shipping-button-disable').addClass('disabled');
    toggleTimeOptions(false);
    validateTimes($(".datepicker").val());
};

var day = 0;
var month = 0;
var year = 0;
var canSelectDate = false;
function autoSelectDate() {

    $('.datepicker').click();
    $('.datepicker-cancel').click();
    //
    var d = new Date();
    month = d.getMonth() + 1;
    day = d.getDate();
    year = d.getFullYear();
    //
    checkInnerDates();
    while (!canSelectDate) {
        checkInnerDates();
    }
    $('.datepicker').click();
    $('.datepicker-calendar-container tbody td[data-day="' + day + '"] button').click();
    $('.datepicker-cancel').click();
    $('.datepicker').val((day < 10 ? '0' : '') + day + '-' + (month < 10 ? '0' : '') + month + '-' + year);
    toggleTimeOptions(true);
    validateTimes($(".datepicker").val());
    setDateMsg();
};

function setDateMsg() {
    $('#date-msg').hide();
    $('#date-msg .msg').hide();
    var tomorrowString = tomorrowDate();
    var selectedDate = $('.datepicker').val();

    if (selectedDate === tomorrowString) {
        $('#date-msg #tomorrow').show();
    } else {
        $('#date-msg #after').show();
    }
    $('#date-msg .date-string').text(getWeekDay(selectedDate));
    $('#date-msg #time').text($('#timepicker').val()?.replace('-', 'a'));
    if ($('#timepicker').val() !== "" && $('#timepicker').val() !== null)
        $('#date-msg').show();
};

function tomorrowDate() {
    var myDate = new Date();
    myDate.setDate(myDate.getDate() + 1);
    // format a date
    var dt = myDate.getDate() + '-' + ("0" + (myDate.getMonth() + 1)).slice(-2) + '-' + myDate.getFullYear();
    return dt;
}

function getWeekDay(dateString) {
    var from = dateString.split("-");
    var date = new Date(from[2], from[1] - 1, from[0]);
    var options = {
        weekday: 'long',
        month: 'long',
        day: 'numeric'
    };
    return date.toLocaleDateString('es-MX', options).replace(',', '');
}

function jsUcfirst(string) {
    return string.charAt(0).toUpperCase() + string.slice(1);
}

function checkInnerDates() {
    if (day + 1 > $('.datepicker-calendar-container tbody')
        .find('td[data-day]').last().data('day')) {
        day = 1;
        $('.datepicker').click();
        $('.month-next').click();
        $('.datepicker-cancel').click();
        if (month + 1 > 12) {
            month = 1;
            year++;
        } else
            month++;
    } else
        day++;
    canSelectDate = !$('.datepicker-calendar-container tbody')
        .find('td[data-day="' + day + '"]').hasClass('is-disabled');
};

function options(data, orderTotal) {
    var str = '';
    $('#payment-method-select-container').empty().html(' ');
    if (!$.isArray(data) && !(orderTotal > 0) && balanceActive) {
        $('#payment-method-select-container-div').hide();
        $('#payment-method-select-container').hide();
        $('#PaymentIsZero').show();
    } else {
        for (var i = data.PaymentMethods.length - 1; i >= 0; i--) {
            if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.CashOnDelivery" && $("#order-total").val() < 6000) {
                //str += '<optgroup label="Pago en efectivo (contra entrega)">';
                str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
            }
            else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.Replacement" && $("#show-replacement-payment-method").val()) {
                //str += '<optgroup label="Reposición">';
                str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
            }
            else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.Benefits" && $("#show-replacement-payment-method").val()) {
                //str += '<optgroup label="Reposición">';
                str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
            }
            else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.CardOnDelivery") {
                //str += '<optgroup label="Pago con tarjeta (contra entrega)">';
                str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
            }
            else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.PaypalPlus") {
                //str += '<optgroup label="Pago con tarjeta">';
                str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
            }
            else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.PayPalStandard") {
                //str += '<optgroup label="Pago con PayPal">';
                str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
            }
            //else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.MercadoPago") {
            //    str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
            //}
            else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.Stripe") {
                //str += '<optgroup label="Pago con PayPal">';
                str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
            }
            else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.Visa") {
                //str += '<optgroup label="Pago con PayPal">';
                str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
            }
            else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.MercadoPagoQr") {
                //str += '<optgroup label="Pago con PayPal">';
                str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
            }
            //else {
            //    str += '<optgroup label="Pago con tarjeta"><option selected value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">Pago con tarjeta (Crédito/Débito)</option></optgroup>';
            //}
        }
        $('#payment-method-select-container').append(str);
        $('select').formSelect();
    }
    paymentAlert();
}

function paymentAlert() {
    hidePaymentElements();
    var payElement = $('#payment-method-select-container').val();
    var str = '';
    if (payElement === 'Payments.CashOnDelivery') {
        str = '';
        $('#pay-card').hide();
        $('#PaymentAlert').empty().html(' ');
        $('#PaymentAlert').show();
        str += '<label>Podrás realizar el pago en efectivo cuando te entreguemos tu pedido.</label>';
        $('#PaymentAlert').append(str);
    }
    else if (payElement === 'Payments.PaypalPlus') {
        str = '';
        $('#pay-card').hide();
        $('#PaymentAlert').empty().html(' ');
        $('#PaymentAlert').show();
        str += '<label>Ingresa la información solicitada para pagar con tu tarjeta de crédito o débito.</label>';
        $('#PaymentAlert').append(str);
        buildPPlusIframe();
    }
    else if (payElement === 'Payments.MercadoPago') {
        str = '';
        $('#pay-card').hide();
        $('#PaymentAlert').empty().html(' ');
        $('#PaymentAlert').show();
        str += '<label>Serás redirigido a la página de MercadoPago para que hagas el pago con tu tarjeta de crédito / débito o en efectivo. Al finalizar regresarás de manera automática a nuestro sitio.</label>';
        $('#PaymentAlert').append(str);
    }
    else if (payElement === 'Payments.Stripe') {
        buildStripeIframe();
    }
    else if (payElement === 'Payments.Visa') {
        requireThirdStepBillingAddress();
        buildVisaForm();
        if (!$("#third-step-billing-address-select-new").val()) {
            thirdStepNewAddress(true);
        }
    }
    else if (payElement === 'Payments.CardOnDelivery') {
        str = '';
        $('#pay-card').hide();
        $('#PaymentAlert').empty().html(' ');
        $('#PaymentAlert').show();
        str += '<label>Podrás realizar el pago utilizando tu tarjeta de crédito o débito cuando te entreguemos tu pedido.</label>';
        $('#PaymentAlert').append(str);
    }
    else if (payElement === 'Payments.MercadoPagoQr') {
        str = '';
        $('#pay-card').hide();
        $('#PaymentAlert').empty().html(' ');
        $('#PaymentAlert').show();
        str += '<label>Deberás utilizar la app de Mercadopago para escanear el código QR que te mostrará el repartidor cuando te entreguemos tu pedido y así confirmar el pago.</label>';
        $('#PaymentAlert').append(str);
    }
    else if (payElement === 'Payments.PayPalStandard') {
        str = '';
        $('#pay-card').hide();
        $('#PaymentAlert').empty().html(' ');
        $('#PaymentAlert').show();
        str += '<label>Serás redirigido a la página de Paypal para que hagas el pago con tu tarjeta de crédito / débito. Al finalizar regresarás de manera automática a nuestro sitio.</label>';
        $('#PaymentAlert').append(str);
    }
    else if (payElement === 'Payments.PagoFacil') {
        $('#PaymentAlert').hide();
        $('#pay-card').show();
        $('.billing-address').hide();
    }
    else {
        $('#PaymentAlert').hide();
        $('#pay-card').hide();
        $('.billing-address').hide();
    }
}

var thirdStepValidateAddressBilling = {
    formBillingAddress: function () {
        $('#third-step-billingForm-button-disable').addClass('disabled');

        var firstName = $('#input-card-FirstName').val();
        var lastName = $('#input-card-LastName').val();
        var email = $('#input-card-email').val();
        //var company = $('#input-card-Company').val();
        var countryId = $('.input-card-Country').val();
        var stateId = $('.input-card-State').val();
        var city = $('#input-card-City').val();
        var address1 = $('#input-card-Address1').val();
        var address2 = $('#input-card-Address2').val();
        var zcp = $('#input-card-ZCP').val();
        var phone = $('#input-card-Phone').val();
        //var attrAddress = $('.attr-address-checkout').val();

        if (firstName == "") {
            $(".third-step-first-name-field-teed").show();
        }
        else {
            $(".third-step-first-name-field-teed").hide();
        }

        if (lastName == "") {
            $(".third-step-last-name-field-teed").show();
        }
        else {
            $(".third-step-last-name-field-teed").hide();
        }

        if (email == "") {
            $(".third-step-email-field-teed").show();
        }
        else if (!validateEmail(email)) {
            $(".third-step-validate-email-field-teed").show();
            $(".third-step-email-field-teed").hide();
        }
        else {
            $(".third-step-email-field-teed").hide();
            $(".third-step-validate-email-field-teed").hide();
        }

        if (countryId == "0") {
            $(".third-step-countryId-field-teed").show();
        }
        else {
            $(".third-step-countryId-field-teed").hide();
        }

        if (stateId == "0") {
            $(".third-step-stateId-field-teed").show();
        }
        else {
            $(".third-step-stateId-field-teed").hide();
        }

        if (city == "") {
            $(".third-step-city-field-teed").show();
        }
        else {
            $(".third-step-city-field-teed").hide();
        }

        if (address1 == "") {
            $(".third-step-address1-field-teed").show();
        }
        else {
            $(".third-step-address1-field-teed").hide();
        }

        if (zcp == "") {
            $(".third-step-zcp-field-teed").show();
        }
        else {
            $(".third-step-zcp-field-teed").hide();
        }

        if (phone == "") {
            $(".third-step-phone-field-teed").show();
        }
        else if (!phoneNumberIsValid(phone)) {
            $(".third-step-validate-phone-field-teed").show();
            $(".third-step-phone-field-teed").hide();
        }
        else {
            $(".third-step-phone-field-teed").hide();
            $(".third-step-validate-phone-field-teed").hide();
        }

        function validateEmail(email) {
            var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
            return re.test(String(email).toLowerCase());
        }

        function phoneNumberIsValid(phoneNumber) {
            return phoneNumber.length === 10;
        };

        if (!phoneNumberIsValid(phone) ||
            !validateEmail(email) ||
            firstName == "" ||
            lastName == "" ||
            email == "" ||
            countryId == "0" ||
            stateId == "0" ||
            city == "" ||
            address1 == "" ||
            zcp == "" ||
            phone == "") {
            $('#third-step-billingForm-button-disable').removeClass('disabled');
        }
        else {
            var datadir = {
                firstName: firstName,
                lastName: lastName,
                email: email,
                countryId: countryId,
                stateId: stateId,
                city: city,
                address1: address1,
                zcp: zcp,
                phone: phone,
            }

            $.ajax({
                cache: false,
                url: 'Checkout/ThirdStepSaveBillingAddress',
                type: 'POST',
                data: datadir,
                success: function (newAddress) {
                    let address = $.parseJSON(newAddress);
                    let newAddressString = address.firstName + " " + address.lastName + ", " + address.address1 + ", " + address.city + ", " + address.stateProvinceName + " " + address.zipPostalCode + ", " + address.countryName;

                    let option = `<option data-firstName="${address.firstName}"
                                            data-lastName="${address.lastName}"
                                            data-email="${address.email}"
                                            data-countryName="${address.countryName}"
                                            data-stateProvinceName="${address.stateProvinceName}"
                                            data-address1="${address.address1}"
                                            data-city="${address.city}"
                                            data-zipPostalCode="${address.zipPostalCode}"
                                            data-phoneNumber="${address.phoneNumber}"
                                            value="${address.id}">
                                        ${newAddressString}
                                    </option>`
                    $("#third-step-billing-address-select-new").append(option);
                    $("#third-step-billing-address-select-new").val(address.id);
                    hidePaymentElements();
                    paymentAlert();
                    $("#third-step-billing-new-address-form").hide();
                },
                error: function (e) {
                    console.log('ERROR SAVING BILLING ADDRESS', e);
                    alert('Error al guardar la dirección');
                    $('#nextForm-button-disable').removeClass('disabled');
                }
            });
        }
    }
}

function formatDate(n) {
    return n < 10 ? '0' + n : '' + n;
}

function sendData(dir) {
    $.ajax({
        cache: false,
        url: 'Checkout/OpcConfirmOrderNew/',
        data: dir,
        type: 'post',
        success: function (response) {
            orderConfirmNextStep(response);
        },
        error: function () {
            alert('Error de conexión. Por favor vuelve a intentarlo o contáctanos para ayudarte a solucionar el problema.');
        }
    });
}

function createAddressBox() {
    const billingSelect = $("#third-step-billing-address-select-new option:selected");
    let firstName = billingSelect.data("firstname");
    let lastName = billingSelect.data("lastname");
    let email = billingSelect.data("email");
    let countryName = billingSelect.data("countryname");
    let stateProvinceName = billingSelect.data("stateprovincename");
    let address1 = billingSelect.data("address1");
    let city = billingSelect.data("city");
    let zipPostalCode = billingSelect.data("zippostalcode");
    let phoneNumber = billingSelect.data("phonenumber");

    $("#address-info-container").empty();
    let element = `
    <div style="text-align: left" class="billing-address-block-info">
        <strong>Nombre (como aparece en la tarjeta): </strong><span>${firstName}</span><br />
        <strong>Apellido (como aparece en la tarjeta): </strong><span>${lastName}</span><br />
        <strong>Correo electrónico: </strong><span>${email}</span><br />
        <strong>País: </strong><span>${countryName}</span><br />
        <strong>Estado: </strong><span>${stateProvinceName}</span><br />
        <strong>Calle y número: </strong><span>${address1}</span><br />
        <strong>Colonia: </strong><span>${city}</span><br />
        <strong>Código postal: </strong><span>${zipPostalCode}</span><br />
        <strong>Teléfono: </strong><span>${phoneNumber}</span><br />
    </div>
    `
    $("#address-info-container").append(element);
};

//function checkAddress(address, postalCode) {
//    if (address !== undefined) {
//        var addressParsed = address.replace(" ", "").toUpperCase();
//        return postalCode === "03810" && addressParsed.includes("MONTECITO38") || (addressParsed.includes("MONTECITO") && addressParsed.includes("WORLDTRADE"));
//    }
//    return false;
//}

//function checkPostalCode(postalCode) {
//    var validPostalCodes = $("#postal-codes").val().replace(' ', '').split(',');
//    if (validPostalCodes.indexOf(postalCode.toString()) >= 0) {
//        return true;
//    }
//    return false;
//}

function scrollTop() {
    try {
        $('html, body').animate({
            scrollTop: $("#order-total").offset().top
        }, 500);
    } catch (e) {
        console.log('error at scroll top');
        console.log(e);
    }
}

var datepicker;
$(document).ready(function () {
    datepicker = $('.datepicker').datepicker({
        onClose: function (date) {
            disableTimes();
            $('.modal-overlay-clone').remove();
        },
        format: "dd-mm-yyyy",
        onOpen: function () {
            setTimeout(function () {
                if ($('.datepicker-modal:visible').length > 0) {
                    var $overlay = $('.modal-overlay').clone();
                    $('.modal-overlay').remove();
                    $overlay.attr('class', 'modal-overlay-clone');
                    $('.datepicker-modal').after($overlay);

                    var refreshIntervalId = window.setInterval(function () {
                        if ($('.datepicker-modal:visible').length < 1) {
                            clearInterval(refreshIntervalId);
                            $('.modal-overlay-clone').remove();
                        }
                    }, 100);
                }
            }, 500);
        },
        disableDayFn: function (date) {
            if (date.getDay() === 0 ||
                (date.getFullYear() === 2022 && date.getMonth() === 1 && date.getDate() === 7) ||
                (date.getFullYear() === 2022 && date.getMonth() === 2 && date.getDate() === 21) ||
                (date.getFullYear() === 2022 && date.getMonth() === 8 && date.getDate() === 16) ||
                (date.getFullYear() === 2022 && date.getMonth() === 10 && date.getDate() === 21) ||
                (date.getFullYear() === 2022 && date.getMonth() === 3 && date.getDate() === 15)) {
                return true;
            }
            else {
                return false;
            }
        },
        minDate: new Date(Date.now() + 24 * 60 * 60 * 1000),
        i18n: {
            months: [
                'Enero',
                'Febrero',
                'Marzo',
                'Abril',
                'Mayo',
                'Junio',
                'Julio',
                'Agosto',
                'Septiembre',
                'Octubre',
                'Noviembre',
                'Diciembre'
            ],
            monthsShort: [
                'Ene',
                'Feb',
                'Mar',
                'Abr',
                'May',
                'Jun',
                'Jul',
                'Ago',
                'Sep',
                'Oct',
                'Nov',
                'Dic'
            ],
            weekdays: [
                'Domingo',
                'Lunes',
                'Martes',
                'Miércoles',
                'Jueves',
                'Viernes',
                'Sábado'
            ],
            weekdaysShort: [
                'Dom',
                'Lun',
                'Mar',
                'Mie',
                'Jue',
                'Vie',
                'Sáb'
            ],
            weekdaysAbbrev: ['D', 'L', 'M', 'Mi', 'J', 'V', 'S'],
            cancel: "Cancelar"
        }
    });
});