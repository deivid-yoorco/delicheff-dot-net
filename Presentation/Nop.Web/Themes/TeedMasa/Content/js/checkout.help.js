var selectAddress = {
    next: function () {
        $("#selected-address-error").hide();
        var postalCode = $("#billing-address-select-new option:selected").data("postalcode");
        checkPostalCode(postalCode)
            .done(function (result) {
                if (typeof result == 'undefined') {
                    console.log(postalCode);
                    $("#selected-address-error").show();
                    return;
                }

                $('#next-button-disable').addClass('disabled');
                var data = $('#billing-address-select-new').val();
                $.ajax({
                    cache: false,
                    url: 'Checkout/AddressEdit?addressId=' + data,
                    type: 'GET',
                    success: selectAddress.save,
                    error: function () {
                        alert('Error al continuar');
                    }
                });
            })
            .fail(function (result) {
                console.log(result);
                isDone = true;
                finalCpResult = false;
            });
    },
    save: function (response) {
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
                        console.log(data.ShippingMethods[i]);
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
                        $('.first-step-checkout').hide();
                    },
                    error: function () {
                        alert('Error de conexión');
                    }
                });
            },
            error: function () {
                alert('Error al guardar');
            }
        });
    }
};

let toggleTimeOptions = (disable, useCase) => {
    switch (useCase) {
        case 1:
            $("option[value='1:00 PM - 3:00 PM']").prop("disabled", disable);
            $("option[value='3:00 PM - 5:00 PM']").prop("disabled", disable);
            $("option[value='7:00 PM - 9:00 PM']").prop("disabled", disable);
            break;
        case 2:
            if (disable) { disableHolidayTimes(); }
            $("option[value='5:00 PM - 7:00 PM']").prop("disabled", disable);
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

let disableHolidayTimes = () => {
    $("#timepicker").prop("disabled", true);
    $('#loading-time').show();
    $('#loading-time').addClass('active');

    $.ajax({
        url: '/Order/CheckHolidayTimes?date=' + $(".datepicker").val(),
        method: 'GET',
        success: (data) => {
            $("#timepicker").prop("disabled", false);
            let activateTime1 = data[0] < 5;
            let activateTime2 = data[1] < 5;
            $("option[value='1:00 PM - 3:00 PM']").prop("disabled", !activateTime1);
            $("option[value='3:00 PM - 5:00 PM']").prop("disabled", !activateTime2);
            $('#loading-time').removeClass('active');
            $('#loading-time').hide();
        },
        error: (error) => {
            console.log(JSON.stringify(error));
            $('#loading-time').removeClass('active');
            $('#loading-time').hide();
        }
    });
};

var saveAddress = {
    nextForm: function () {
        var stop = true;
        var zcp = document.getElementById("input-Checkout-ZCP").value;
        if (zcp == "") {
            $("#zcp-field-teed").show();
        }
        else {
            $("#zcp-field-teed").hide();
            if (zcp.length == 5) {
                onPlaceChanged(zcp);
            }
            else {
                $("#zcp-field-teed").show();
            }
        }

        function onPlaceChanged(zcp) {
            isDone = false;
            finalCpResult = false;
            $("#postal-code-error").hide();
            isPostalCodeValid(zcp);
            var final = finalCpResult;
            if (final) {
                $("#address-error").hide();
                stop = false;
            }
            else {
                $("#address-error").show();
            }
        }

        function isPostalCodeValid(zcp) {
            var postalCode = zcp;
            console.log(postalCode, "CP 1");
            if (postalCode != "") {
                checkPostalCode(postalCode).done(function (result) {
                    if (typeof result == 'undefined')
                        return false;
                    else
                        return true;
                });
            }
            else {
                $("#postal-code-error").show();
                isDone = true;
                finalCpResult = false;
                return false;
            }
        }

        if (stop == false) {

            $('#nextForm-button-disable').addClass('disabled');
            console.log("si entra");
            var firstName = $('#input-Checkout-FirstName').val();
            var lastName = $('#input-Checkout-LastName').val();
            var email = $('#input-Checkout-Email').val();
            var phone = document.getElementById("input-Checkout-Phone").value; //= $('#input-Checkout-Phone').val();
            //var company = $('#input-Checkout-Company').val();//usualmente no se usa
            var stateId = $('.input-Checkout-State').val();
            //var countryId = $('.input-Checkout-Country').val();//usualmente no se usa
            var city = document.getElementById("input-Checkout-City").value; //$('#input-Checkout-City').val();
            var address1 = $('#input-Checkout-Address1').val();//calle
            var exterior = $('#address_attribute_2').val();
            var address2 = $('#input-Checkout-Address2').val();//Colonia
            var interior = $('#input-Checkout-Interior').val();
            var attrAddress = $($(".attr-address-checkout")[1]).val();
            var attrAddress2 = $($(".attr-address-checkout")[0]).val()
   



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

            //if (countryId == "") {
            //    $("#countryId-field-teed").show();
            //}
            //else {
            //    $("#countryId-field-teed").hide();
            //}

            if (stateId == "") {
                $("#stateId-field-teed").show();
            }
            else {
                $("#stateId-field-teed").hide();
            }

            if (city == "") {
                $("#city-field-teed").show();
            }
            else {
                $("#city-field-teed").hide();
            }

            if (address1 == "") {
                $("#address1-field-teed").show();
            }
            else {
                $("#address1-field-teed").hide();
            }

            if (exterior == "") {
                $("#numberExt").show();
            }
            else {
                $("#numberExt").hide();
            }

            if (address2 == "") {
                $("#Address2-field-teed").show();
            }
            else {
                $("#Address2-field-teed").hide();
            }

            if (interior == "") {
                $('#input-Checkout-Interior').val("S/N");
                interior = "S/N";
            }

            function validateEmail(email) {
                var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
                return re.test(String(email).toLowerCase());
            }
            if (!validateEmail(email) ||
                firstName == "" ||
                lastName == "" ||
                email == "" ||
                stateId == "" ||
                city == "" ||
                address1 == "" ||
                exterior == "" ||
                zcp == "" ||
                phoneBol == false) {
                $("#general-address-error").show();
                $('#nextForm-button-disable').removeClass('disabled');
            }
            else {
                $("#general-address-error").hide();
                var datadir = {
                    firstName: firstName,
                    lastName: lastName,
                    email: email,
                    phone: phone,
                    stateId: stateId,
                    city: city,
                    address1: address1 + " exterior " + exterior + " interior " + interior,
                    address2: address2,
                    zcp: zcp,
                    attrAddress: attrAddress,
                    attrAddress2: attrAddress2
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
                                $('.first-step-checkout').hide();
                            },
                            error: function () {
                                alert('Error de conexión');
                            }
                        });
                    },
                    error: function () {
                        alert('Error al guardar');
                    }
                });
            }
        }
    }
};

function options(data) {
    var str = '';
    $('#payment-method-select-container').empty().html(' ');
    for (var i = data.PaymentMethods.length - 1; i >= 0; i--) {
        if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.CashOnDelivery") {
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
        }
        else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.Replacement" && $("#show-replacement-payment-method").val()) {
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
        }
        else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.CardOnDelivery") {
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
        }
        else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.PaypalPlus") {
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
        }
        else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.PayPalStandard") {
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
        }
        else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.MercadoPago") {
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
        }
        else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.Stripe") {
            //str += '<optgroup label="Pago con PayPal">';
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
        }
        //else {
        //    str += '<optgroup label="Pago con tarjeta"><option selected value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">Pago con tarjeta (Crédito/Débito)</option></optgroup>';
        //}
    }
    $('#payment-method-select-container').append(str);
    $('select').formSelect();

    PaymentAlert.paymentAlert();
}

var PaymentAlert = {
    paymentAlert: function () {
        $("#confirm-button-disable").show();
        $("#paypal-plus").hide();
        $("#pplusContinueButton").hide();
        $('#loading-confirm').removeClass('active');
        $('#payment-stripe-form').hide();
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
        else if (payElement === 'Payments.Stripe') {
            buildStripeIframe();
        }
        else if (payElement === 'Payments.CardOnDelivery') {
            str = '';
            $('#pay-card').hide();
            $('#PaymentAlert').empty().html(' ');
            $('#PaymentAlert').show();
            str += '<label>Podrás realizar el pago utilizando tu tarjeta de crédito o débito cuando te entreguemos tu pedido.</label>';
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
        else if (payElement === 'Payments.MercadoPago') {
            str = '';
            $('#pay-card').hide();
            $('#PaymentAlert').empty().html(' ');
            $('#PaymentAlert').show();
            str += '<label>Serás redirigido a la página de MercadoPago para que hagas el pago. Al finalizar regresarás de manera automática a nuestro sitio.</label>';
            $('#PaymentAlert').append(str);
        }
        else {
            $('#PaymentAlert').hide();
            $('#pay-card').hide();
            $('.billing-address').hide();
        }

        //if ($('#payment-method-select-container').val() == "Payments.MercadoPago") {
        //    if ($('.mp-mercadopago-checkout-wrapper').length < 1) {
        //        $.ajax({
        //            cache: false,
        //            url: '/Admin/PaymentMercadoPago/GetPreference',
        //            type: 'GET',
        //            success: function (preference) {
        //                DoMercadoPagoButton(preference)
        //            },
        //            error: function (err) {
        //                console.log(err);
        //                $.ajax({
        //                    cache: false,
        //                    url: '/PaymentMercadoPago/GetPreference',
        //                    type: 'GET',
        //                    success: function (preference) {
        //                        DoMercadoPagoButton(preference)
        //                    },
        //                    error: function (err) {
        //                        console.log(err);
        //                    }
        //                });
        //            }
        //        });
        //    } else {
        //        $('#MercadoPagoButton').show();
        //    }
        //    $('button[onclick="SavePayment.save()"]').hide();
        //} else {
        //    $('#MercadoPagoButton').hide();
        //    $('button[onclick="SavePayment.save()"]').show();
        //}
    }
};

//function DoMercadoPagoButton(preference) {
//    $('button[onclick="SavePayment.save()"]').after('' +
//        '<form id="MercadoPagoButton" asp-action="MercadoPagoModal" method="POST" style="">' +
//        '<script src="https://www.mercadopago.com.mx/integrations/v1/web-payment-checkout.js" data-preference-id="' +
//        preference.Id + '" data-button-label="' +
//        'CONFIRMAR" data-elements-color="#000000">' +
//        '</script>' +
//        '<button type="submit" class="mercadopago-button" formmethod="post" style="background-color: rgb(0, 0, 0);">CONFIRMAR</button>' +
//        '</form>');

//    $('.page-footer').first().before('' +
//        '<div class="mp-mercadopago-checkout-wrapper" style="z-index: 2147483647; display: block; background: rgba(0, 0, 0, 0.7); border: 0px; overflow: hidden; visibility: visible; margin: 0px; padding: 0px; position: fixed; left: 0px; top: 0px; width: 100%; opacity: 1; height: 100%; transition: opacity 220ms ease-in 0s;">' +
//        '< svg class= "mp-spinner" viewBox = "25 25 50 50" >' +
//        '<circle class="mp-spinner-path" cx="50" cy="50" r="20" fill="none" stroke-miterlimit="10"></circle>' +
//        '</svg >' +
//        '<iframe id="mercadopago-checkout" src="https://www.mercadopago.com.mx/checkout/v1/modal/?preference-id=' +
//        preference.Id + '" width="100%" height="100%" style="z-index:1;display:block;position:fixed;left:0;top:0;" frameborder="0" transition="height 2s ease"></iframe>' +
//        '</div >');
//}

var ShippingOptions = {
    save: function () {
        console.log('Test');
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
            

            var sub = $('#calculate-subtotal').text();
            var nSub = Number(sub.replace(/[^0-9\.-]+/g, ""));

            if (data[1] == '$0.00') {
                var shi = "0";
                var nShi = Number(shi.replace(/(-)[^0-9\.-]+/g, ""));
            }
            else {
                document.getElementById('calculate-shipping').innerHTML = data[1];
                var shi = $('#calculate-shipping').text();
                var nShi = Number(shi.replace(/[^0-9\.-]+/g, ""));
            }

            //var tax = $('#calculate-tax').text();
            //var nTax = Number(tax.replace(/[^0-9\.-]+/g, ""));

            var dis = $('#calculate-total-discount').text();
            if (dis == '') dis = "0";
            var nDis = Number(dis.replace(/(-)[^0-9\.-]+/g, ""));

            var nTotal = nSub + /*nTax +*/ nShi - nDis;
            var total = nTotal.toFixed(2).toString().split("").reverse().join("").replace(/(\d{3}(?!$))/g, '$1,');

            document.getElementById('calculate-total').innerHTML = '$' + total.split("").reverse().join("");
            document.getElementById('calculate-total').style.fontWeight = "bold";
            console.log("----", nTotal, "+++++",  total.toString());
        }

        //var url = 'Checkout/SelectShippingMethodNew?shippingoption=' + data[0] + '&orderdate=' + $(".datepicker").val();
        var url = 'Checkout/SelectShippingMethodNew?shippingoption=' + data[0] + '&orderdate=';

        var orderTotal = $('#calculate-total').text();
        var nOrderTotal = Number(orderTotal.replace(/[^0-9\.-]+/g, ""));

        var selectedDate = $(".datepicker").val();

        if (!selectedDate) {
            $("#shipping-date-validation").show();
        }
        else {
            $("#shipping-date-validation").hide();
        }

        //if (!selectedDate) {
        //    $('#shipping-button-disable').removeClass('disabled');
        //}
        //else {

        //}
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
                        options(data);
                        $('#checkout-step-payment-method').show();
                        $('#payment-method-buttons-container').show();
                    },
                    error: function () {
                        alert('Error de conexión payment');
                    }
                });
            },
            error: function (data) {
                    alert('Error de conexión shipping');
            }
        });
    }
}

function formatDate(n) {
    return n < 10 ? '0' + n : '' + n;
}

var SavePayment = {
    save: function (orderNote = null) {
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
                OrderConfirm.save(orderNote);
            },
            error: function () {
                $('#confirm-button-disable').removeClass('disabled');
                $('#loading-confirm').removeClass('active');
                alert('Error de conexión. Por favor vuelve a intentarlo o contáctanos para ayudarte a solucionar el problema.');
            }
        });
    }
};

function sendData(dir) {
    $.ajax({
        cache: false,
        url: 'Checkout/OpcConfirmOrderNew/',
        data: dir,
        type: 'post',
        success: function (response) {
            OrderConfirm.nextStep(response);
        },
        error: function () {
            alert('Error de conexión. Por favor vuelve a intentarlo o contáctanos para ayudarte a solucionar el problema.');
        }
    });
}

var OrderConfirm = {
    form: false,
    saveUrl: false,
    isSuccess: false,

    init: function (saveUrl, successUrl) {
        this.saveUrl = saveUrl;
        this.successUrl = successUrl;
    },

    save: function (orderNote = null) {
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
                    address1: $('#google-map-address').val() + " interior " + $('#input-Checkout-Interior').val(),
                    address2: $('#input-Checkout-Address2').val(),
                    zcp: $('#input-Checkout-ZCP').val(),
                    phone: $('#input-Checkout-Phone').val(),
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
                address1: $('#google-map-address').val() + " interior " + $('#input-Checkout-Interior').val(),
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
    },

    nextStep: function (response) {
        //if (response.update_section) {
        //    window.location = OrderConfirm.successUrl;
        //}
        if (response.error) {
            if ((typeof response.message) === 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        if (response.redirect) {
            location.href = response.redirect;
            return;
        }

        if (response.success) {
            window.location = OrderConfirm.successUrl;
        }
    }
};

function checkAddress(address, postalCode) {
    if (address !== undefined) {
        var addressParsed = address.replace(" ", "").toUpperCase();
        return postalCode === "03810" && addressParsed.includes("MONTECITO38") || (addressParsed.includes("MONTECITO") && addressParsed.includes("WORLDTRADE"));
    }
    return false;
}

function checkPostalCode(postalCode) {
    return $.ajax({
        cache: false,
        url: 'Admin/ShippingByAddress/CheckDatesOfPc?postalCode=' + postalCode,
        type: 'GET',
        async: false,
        success: function (dates) {
            if (typeof dates != 'undefined') {
                if (dates.length > 0) {
                    var dateToday = new Date();
                    var datess = $(".datepicker").datepicker({
                        minDate: dateToday,
                        format: "dd-mm-yyyy",
                        //onSelect: function (selectedDate) {
                        //    var option = this.id == "from" ? "minDate" : "maxDate",
                        //        instance = $(this).data("datepicker"),
                        //        date = $.datepicker.parseDate(instance.settings.dateFormat || $.datepicker._defaults.dateFormat, selectedDate, instance.settings);
                        //    datess.not(this).datepicker("option", option, date);
                        //},
                        disableDayFn: function (date) {
                            if (dates.includes(date.getDay())) // getDay() returns a value from 0 to 6, 1 represents Monday
                                return false;
                            else
                                return true;
                        }
                    });
                    isDone = true;
                    finalCpResult = true;
                    console.log(1);
                } else {
                    console.log(1.2);
                    isDone = true;
                    finalCpResult = false;
                }
            } else {
                console.log(2.2);
                isDone = true;
                finalCpResult = false;
            }
        },
        error: function () {
            isDone = true;
            finalCpResult = false;
            alert('Error al continuar en chequeo de Código Postal');
        }
    });
}

$(document).ready(function () {
    $('.datepicker').datepicker({
        onClose: function (date) {
            var isHoliday = $(".datepicker").val() === '24-12-2019' || $(".datepicker").val() === '31-12-2019';
            if (isHoliday) {
                toggleTimeOptions(true, 2);
            }
        },
        format: "dd-mm-yyyy",
        disableDayFn: function (date) {
            if (date.getDay() === 0 && !(date.getFullYear() === 2020 && date.getMonth() === 0 && date.getDate() === 12)) {
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

    // MercadoPago color and text
    if ($('button[onclick="SavePayment.save()"]').css('background-color').includes('rgb')) {
        var rgb = $('button[onclick="SavePayment.save()"]').css('background-color').replace(/[^0-9]/g, '');
        var finalRgb = fullColorHex(rgb[0], rgb[1], rgb[2]);
        $('#MercadoPagoButton script').attr('data-elements-color', finalRgb);
        $('#MercadoPagoButton button').css('background-color', finalRgb);
    } else {
        $('#MercadoPagoButton script').attr('data-elements-color', $('button[onclick="SavePayment.save()"]').css('background-color'));
        $('#MercadoPagoButton button').css('data-elements-color', $('button[onclick="SavePayment.save()"]').css('background-color'));
    }

    $('#MercadoPagoButton script').attr('data-button-label', $('button[onclick="SavePayment.save()"]').text().toUpperCase());
});

var rgbToHex = function (rgb) {
    var hex = Number(rgb).toString(16);
    if (hex.length < 2) {
        hex = "0" + hex;
    }
    return hex;
};

var fullColorHex = function (r, g, b) {
    var red = rgbToHex(r);
    var green = rgbToHex(g);
    var blue = rgbToHex(b);
    return '#' + red + green + blue;
};