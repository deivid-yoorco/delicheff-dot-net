$(document).ready(function () {
    $('#timepicker').change(function () {
        $('#shipping-hour-validation').hide();
        $('#shipping-date-validation').hide();
    });
});

function createDatePicker(enableThisDatesOnly = []) {
    $('.datepicker').datepicker({
        onClose: function (date) {
            disableTimes();
        },
        format: "dd-mm-yyyy",
        disableDayFn: function (date) {
            if (enableThisDatesOnly.indexOf(date.toDateString()) > -1) {
                if (date.getDay() === 1)
                    return true;
                else
                    return false;
            }
            else if (enableThisDatesOnly.length > 1)
                return true;
            else {
                if (date.getDay() === 1 &&
                    new Date(2021, 11, 27).toDateString() !== date.toDateString() &&
                    new Date(2021, 12, 3).toDateString() !== date.toDateString())
                    return true;
                else
                    return false;
            }
        },
        minDate: new Date(Date.now()),
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
}

var selectAddress = {
    next: function () {
        $('#next-button-disable').addClass('disabled');
        var data = $('#billing-address-select-new').val();
        $.ajax({
            cache: false,
            url: 'Checkout/AddressEdit?addressId=' + data,
            type: 'GET',
            success: this.save,
            error: function () {
                alert('Error al continuar');
                $('#nextForm-button-disable').removeClass('disabled');
            }
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
        }

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
                        $('#nextForm-button-disable').removeClass('disabled');
                    }
                });
            },
            error: function () {
                alert('Error al guardar');
                $('#nextForm-button-disable').removeClass('disabled');
            }
        });
    }
}

var saveAddress = {
    nextForm: function (allProductsNoShipping) {
        $('#nextForm-button-disable').addClass('disabled');

        var firstName = $('#input-Checkout-FirstName').val();
        var lastName = $('#input-Checkout-LastName').val();
        var email = $('#input-Checkout-Email').val();
        var company = $('#input-Checkout-Company').val();
        var countryId = $('.input-Checkout-Country').val();
        var stateId = $('.input-Checkout-State').val();
        var city = $('#input-Checkout-City').val();
        var address1 = $('#input-Checkout-Address1').val();
        var address2 = $('#input-Checkout-Address2').val();
        var zcp = $('#input-Checkout-ZCP').val();
        var phone = $('#input-Checkout-Phone').val();
        var attrAddress = $('.attr-address-checkout').val();

        if (!allProductsNoShipping) {
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
            else if (!phoneNumberIsValid(phone)) {
                $("#validate-phone-field-teed").show();
                $("#phone-field-teed").hide();
            }
            else {
                $("#phone-field-teed").hide();
                $("#validate-phone-field-teed").hide();
            }

            function validateEmail(email) {
                var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
                return re.test(String(email).toLowerCase());
            }

            function phoneNumberIsValid(phoneNumber) {
                return phoneNumber.length === 10;
            };
        }

        if (!allProductsNoShipping && (!validateEmail(email) ||
            firstName == "" ||
            lastName == "" ||
            email == "" ||
            countryId == "0" ||
            stateId == "0" ||
            city == "" ||
            address1 == "" ||
            zcp == "" ||
            phone == "")) {
            $('#nextForm-button-disable').removeClass('disabled');
        }
        else {
            var datadir = {
                firstName: firstName,
                lastName: lastName,
                email: email,
                company: company,
                countryId: countryId,
                stateId: stateId,
                city: city,
                address1: address1,
                address2: address2,
                zcp: zcp,
                phone: phone,
                attrAddress: attrAddress,
                allProductsNoShipping: allProductsNoShipping
            }

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
                            $('#extra-time').hide();
                            $('#extra-time').removeClass('active');
                            if (typeof data != 'undefined' && data != null) {
                                var m = data.ShippingMethods.length == 1 ? 12 : data.ShippingMethods.length == 2 ? 6 : 4;
                                for (var i = 0; i < data.ShippingMethods.length; i++) {
                                    var str = '<li class="col s12 m' + m + '" style="margin-bottom:15px;"><div class="method-name"><label>' +
                                        '<input id="shippingoption_' + i + '" class="with-gap" value="' + data.ShippingMethods[i].Name + '___' +
                                        data.ShippingMethods[i].ShippingRateComputationMethodSystemName + '/' + data.ShippingMethods[i].Fee + '" ' +
                                        'name="shippingoption" type="radio" checked/>' + '<span id="shippingoption__' + i + '">' + data.ShippingMethods[i].Name +
                                        ' (' + data.ShippingMethods[i].Fee + ')' + '</span></label></div></li>';
                                    $('#option-shipping-container').append(str);
                                }
                            } else {

                            }
                            $('#shipping-method-buttons-container').show();
                            $('.first-step-checkout').hide();
                        },
                        error: function () {
                            alert('Error de conexión');
                            $('#nextForm-button-disable').removeClass('disabled');
                        }
                    });
                },
                error: function () {
                    alert('Error al guardar');
                    $('#nextForm-button-disable').removeClass('disabled');
                }
            });
        }
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
                    PaymentAlert.paymentAlert();
                    $("#third-step-billing-new-address-form").hide();
                    checkIfBillingHasEmptyData();
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

function options(data) {
    var str = '';
    $('#payment-method-select-container').empty().html(' ');
    for (var i = data.PaymentMethods.length - 1; i >= 0; i--) {
        if (data.PaymentMethods[i].PaymentMethodSystemName == "Payments.PayPalStandard") {
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
        }
        else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.Replacement" && $("#show-replacement-payment-method").val()) {
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
        }
        else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.PaypalPlus") {
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
        }
        else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.MercadoPago") {
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
        }
        else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.Stripe") {
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
        }
        else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.NetPay") {
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
        }
        else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.ManualTransfer") {
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
        }
        else {
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option>';
        }
    }
    $('#payment-method-select-container').append(str);
    $('select').formSelect();

    PaymentAlert.paymentAlert();
}

function checkIfBillingHasEmptyData() {
    if ($('.billing-address-block-info').length > 0
        && !$('#third-step-billing-new-address-form').is(':visible')) {
        let firstName = $('.current-billing-firstName').text();
        let lastName = $('.current-billing-lastName').text();
        let email = $('.current-billing-email').text();
        let address1 = $('.current-billing-address1').text();
        let city = $('.current-billing-city').text();
        let zipPostalCode = $('.current-billing-zipPostalCode').text();
        let phoneNumber = $('.current-billing-phoneNumber').text();

        if (isNullOrEmpty(firstName)
            || isNullOrEmpty(lastName)
            || isNullOrEmpty(email)
            || isNullOrEmpty(address1)
            || isNullOrEmpty(city)
            || isNullOrEmpty(zipPostalCode)
            || isNullOrEmpty(phoneNumber)) {
            $('#empty-billing-info-error').show();
            $('#payments-container').hide();
            $('#confirm-button-disable').hide();
            thirdStepNewAddress(true);
        } else {
            $('#empty-billing-info-error').hide();
            $('#payments-container').show();
            $('#confirm-button-disable').show();
        }
    } else {
        if ($('#third-step-billing-address-select-new option').length < 1 && $('#third-step-billing-new-address-form').is(':visible')) {
            $('#empty-billing-info-error').show();
            $('#payments-container').hide();
            $('#confirm-button-disable').hide();
            thirdStepNewAddress(true);
        } else if ($('#third-step-billing-new-address-form').is(':visible')) {
            $('#empty-billing-info-error').show();
            $('#payments-container').hide();
            $('#confirm-button-disable').hide();
        } else {
            $('#empty-billing-info-error').hide();
            $('#payments-container').show();
            $('#confirm-button-disable').show();
        }
    }
}

function isNullOrEmpty(value) {
    if (typeof value == 'undefined'
        || value == 'undefined'
        || value == ""
        || value.trim() == ""
        || value == null) {
        return true;
    }
    else
        return false;
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
        <strong>Nombre (como aparece en la tarjeta): </strong><span class="current-billing-firstName">${firstName}</span><br />
        <strong>Apellido (como aparece en la tarjeta): </strong><span class="current-billing-lastName">${lastName}</span><br />
        <strong>Correo electrónico: </strong><span class="current-billing-email">${email}</span><br />
        <strong>País: </strong><span class="current-billing-countryName">${countryName}</span><br />
        <strong>Estado: </strong><span class="current-billing-stateProvinceName">${stateProvinceName}</span><br />
        <strong>Calle y número: </strong><span class="current-billing-address1">${address1}</span><br />
        <strong>Colonia: </strong><span class="current-billing-city">${city}</span><br />
        <strong>Código postal: </strong><span class="current-billing-zipPostalCode">${zipPostalCode}</span><br />
        <strong>Teléfono: </strong><span class="current-billing-phoneNumber">${phoneNumber}</span><br />
    </div>
    `
    $("#address-info-container").append(element);
    checkIfBillingHasEmptyData();
};

const hidePaymentElements = () => {
    $("#address-info-container").hide();
    $("#confirm-button-disable").show();
    $("#paypal-plus").hide();
    $("#pplusContinueButton").hide();
    $('#loading-confirm').removeClass('active');
    $('#payment-stripe-form').hide();
    $('#payment-netpay-form-container').hide();
    $("#third-step-select-billing-address").hide();
};

var PaymentAlert = {
    paymentAlert: function () {
        hidePaymentElements();

        var payElement = $('#payment-method-select-container').val();
        if (payElement == 'Payments.PayPalStandard') {
            var str = '';
            $('#pay-card').hide();
            $('#PaymentAlert').empty().html(' ');
            $('#PaymentAlert').show();
            str += '<label>Serás redirigido a la página de Paypal para que hagas el pago con tu tarjeta de crédito / débito. Al finalizar regresarás de manera automática a nuestro sitio.</label>';
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
        else if (payElement === 'Payments.ManualTransfer') {
            str = '';
            $('#pay-card').hide();
            $('#PaymentAlert').empty().html(' ');
            $('#PaymentAlert').show();
            str += '<label>Te enviaremos la información para que puedas realizar el pago a tu correo electrónico.</label>';
            $('#PaymentAlert').append(str);
        }
        else if (payElement == 'Payments.PagoFacil') {
            $('#PaymentAlert').hide();
            $('#pay-card').show();
            $('.billing-address').hide();
        }
        else if (payElement == 'Payments.MercadoPago') {
            var str = '';
            $('#pay-card').hide();
            $('#PaymentAlert').empty().html(' ');
            $('#PaymentAlert').show();
            str += '<label>Serás redirigido a la página de Mercadopago para que termines tu proceso de pago. Al finalizar, regresarás de manera automática a nuestro sitio.</label>';
            $('#PaymentAlert').append(str);
        }
        else if (payElement === 'Payments.Stripe') {
            requireThirdStepBillingAddress();
            buildStripeIframe();
        }
        else if (payElement === 'Payments.NetPay') {
            requireThirdStepBillingAddress();
            buildNetPayForm();
        }
        else {
            $('#PaymentAlert').empty().html(' ');
        }
    }
}

var ShippingOptions = {
    save: function (allProductsNoShipping) {
        $('#shipping-hour-validation').hide();
        $('#shipping-date-validation').hide();

        if ($('.datepicker').val() == ''
            || $('.datepicker').val() == null
            || typeof $('.datepicker').val() == 'undefined') {
            $('#shipping-date-validation').show();
        }
        else if ($('#timepicker').val() == '0'
            || $('#timepicker').val() == null) {
            $('#shipping-hour-validation').show();
        }
        else {
            $('#shipping-button-disable').addClass('disabled');

            var methods = document.getElementsByName('shippingoption');
            if (!allProductsNoShipping) {
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
            }

            var url = !allProductsNoShipping ? 'Checkout/SelectShippingMethodNew?shippingoption=' + data[0] :
                "Checkout/SelectShippingMethodNew";

            var orderTotal = $('#calculate-total').text();
            var nOrderTotal = Number(orderTotal.replace(/[^0-9\.-]+/g, ""));

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
                error: function () {
                    alert('Error de conexión shipping');
                }
            });
        }
    }
}

var SavePayment = {
    save: function (orderNote = null) {
        var payment = $('#payment-method-select-container').val();

        if (payment === '') {
            alert('Debes seleccionar un método de pago.');
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
                alert('Error de conexión');
            }
        });
    }
}

function sendData(dir) {
    $.ajax({
        cache: false,
        url: 'Checkout/OpcConfirmOrderNew/',
        data: dir,
        type: 'post',
        success: function (response) {
            OrderConfirm.nextStep(response)
        },
        error: function () {
            alert('Error de conexión');
        }
    });
}

function getThirdStepBillingAddress() {
    var thirdStepBillingAddress = null;
    if ($('#third-step-select-billing-address').css('display') != 'none') {
        var test = $('#third-step-billing-address-select-new').val();
        if ($('#third-step-billing-address-select-new').val()) {
            console.log(1);
            thirdStepBillingAddress = {
                id: $('#third-step-billing-address-select-new').val()
            }
        }
        else {
            console.log(2);
            thirdStepBillingAddress = {
                firstName: $('#input-card-FirstName').val(),
                lastName: $('#input-card-LastName').val(),
                email: $('#input-card-email').val(),
                countryId: $('.input-card-Country').val(),
                stateProvinceId: $('.input-card-State').val(),
                city: $('#input-card-City').val(),
                address1: $('#input-card-Address1').val(),
                address2: $('#input-card-Address2').val(),
                zcp: $('#input-card-ZCP').val(),
                phone: $('#input-card-Phone').val(),
            }
        }
    }
    return thirdStepBillingAddress;
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
        }
        $('#confirm-button-disable').addClass('disabled');
        $('#loading-confirm').addClass('active');

        var thirdStepBillingAddress = getThirdStepBillingAddress();

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
                            orderNote: orderNote,
                            thirdStepBillingAddress: thirdStepBillingAddress,
                            shippingDate: formatedDate,
                            shippingHour: $("#timepicker option:selected").val(),
                        }
                        sendData(datadir);
                    },
                    error: function () {
                        alert('Error al continuar');
                    }
                });
            }
            else {
                console.log(2);
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
                    address1: $('#input-Checkout-Address1').val(),
                    address2: $('#input-Checkout-Address2').val(),
                    zcp: $('#input-Checkout-ZCP').val(),
                    phone: $('#input-Checkout-Phone').val(),
                    cardName: $('#input-name').val(),
                    cardNumber: $('#input-card').val(),
                    cardMonth: $('#input-month').val(),
                    cardYear: $('#input-year').val(),
                    cardCvv: $('#input-cvv').val(),
                    orderNote: orderNote,
                    thirdStepBillingAddress: thirdStepBillingAddress,
                    shippingDate: formatedDate,
                    shippingHour: $("#timepicker option:selected").val(),
                }
                sendData(dir);
            }
        }
        else {
            console.log(3);
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
                address1: $('#input-card-Address1').val(),
                address2: $('#input-card-Address2').val(),
                zcp: $('#input-card-ZCP').val(),
                phone: $('#input-card-Phone').val(),
                cardName: $('#input-name').val(),
                cardNumber: $('#input-card').val(),
                cardMonth: $('#input-month').val(),
                cardYear: $('#input-year').val(),
                cardCvv: $('#input-cvv').val(),
                orderNote: orderNote,
                thirdStepBillingAddress: thirdStepBillingAddress,
                shippingDate: formatedDate,
                shippingHour: $("#timepicker option:selected").val(),
            }
            sendData(body);
        }
    },
    saveV2: function (orderNote = null) {
        $('#confirm-button-disable').addClass('disabled');
        $('#loading-confirm').addClass('active');

        var body = {
            messagegift: $('#message-gift').val(),
            signsgift: $('#name-signs-gift').val(),
            orderNote: orderNote
        };

        $.ajax({
            cache: false,
            url: 'Checkout/CreateOrderV2/',
            data: body,
            type: 'post',
            success: function (response) {
                OrderConfirm.nextStep(response)
            },
            error: function () {
                alert('Error de conexión, por favor contacta a un administrador.');
            }
        });
    },

    nextStep: function (response) {
        //if (response.update_section) {
        //    window.location = OrderConfirm.successUrl;
        //}
        if (response.error) {
            if ((typeof response.message) == 'string') {
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

function validateTimes(dateValue, manualTime = null, enableThisTimesOnly = []) {
    $('#shipping-hour-validation').hide();
    $('#shipping-date-validation').hide();
    $("#timepicker").prop("disabled", true);
    $('#loading-time').show();
    $('#loading-time').addClass('active');
    $("#time-disabled-error").hide();

    $('#date-msg').hide();
    $('#date-msg .msg').hide();

    if (dateValue) {
        $.ajax({
            url: '/Schedule/GetWebTimes?date=' + dateValue,
            method: 'GET',
            success: function (data) {
                $("#timepicker").prop("disabled", false);

                $("#timepicker").empty();
                $("#timepicker").html(data.optionValue);

                $('#loading-time').removeClass('active');
                $('#loading-time').hide();

                $("#timepicker").val("0");
                if (manualTime != null)
                    $("#timepicker").val(manualTime);
                if (enableThisTimesOnly.length > 0) {
                    $('#timepicker option').each(function (i, e) {
                        var value = $(e).attr('value').replaceAll(/\ \[.*?\]/g, '');
                        if (enableThisTimesOnly.indexOf(value) < 0)
                            $(e).remove();
                    });
                }
                if (data.anyDisabled)
                    $('#time-disabled-error').show();
                else
                    $('#time-disabled-error').hide();
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

function disableTimes(manualTime, enableThisTimesOnly = []) {
    validateTimes($(".datepicker").val(), manualTime, enableThisTimesOnly);
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
    validateTimes($(".datepicker").val());
};