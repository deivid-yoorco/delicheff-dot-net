
var selectAddress = {
    next: function () {
        $('#next-button-disable').addClass('disabled');

        if ($("#PickUpInStore").is(":checked")) {

            let data = {
                firstName: $("#input-name-pickup").val(),
                lastName: $("#input-lastname-pickup").val(),
                email: $("#input-email-pickup").val(),
                phone: $("#input-phone-pickup").val(),
                pickUpInStore: true,
                pickUpPoint: $("#pickup-points-select").val()
            };

            this.save(data);
        }
        else {
            var data = $('#billing-address-select-new').val();
            $.ajax({
                cache: false,
                url: 'Checkout/AddressEdit?addressId=' + data,
                type: 'GET',
                success: this.save,
                error: function () {
                    alert('Error al continuar');
                }
            });
        }
    },
    save: function (response) {
        if (response) {
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
                pickUpInStore: response.pickUpInStore,
                pickUpPoint: response.pickUpPoint
            };

            $.ajax({
                cache: false,
                url: 'Checkout/OpcSaveBillingNew',
                type: 'POST',
                data: datadir,
                success: function () {
                    if (response.pickUpInStore) {
                        loadPaymentOptions();
                    }
                    else {
                        loadShippingMethods();
                    }
                },
                error: function () {
                    alert('Error al guardar');
                }
            });
        }
        else {
            loadShippingMethods();
        }
    }
};

let loadShippingMethods = () => {
    $.ajax({
        cache: false,
        url: 'Checkout/ShippingMethodNew',
        type: 'GET',
        success: function (data) {
            var m = data.ShippingMethods.length === 1 ? 12 : data.ShippingMethods.length === 2 ? 6 : 4;
            for (var i = 0; i < data.ShippingMethods.length; i++) {
                var checked = i === 0 ? "checked" : "";
                var str = '<li class="col s12 m' + m + '" style="margin-bottom:15px;"><div class="method-name"><label>' +
                    '<input onclick="shippingMethodSelected(this)" id="shippingoption_' + i + '" class="with-gap" value="' + data.ShippingMethods[i].Name + '___' +
                    data.ShippingMethods[i].ShippingRateComputationMethodSystemName + '/' + data.ShippingMethods[i].Fee + '" ' +
                    'name="shippingoption" type="radio" ' + checked + '/>' + '<span id="shippingoption__' + i + '">' + data.ShippingMethods[i].Name + ' ' + data.ShippingMethods[i].Description +
                    ' (' + (data.ShippingMethods[i].Fee === '$0.00' ? 'GRATIS' : data.ShippingMethods[i].Fee) + ')' + '</span></label></div></li>';
                $('#option-shipping-container').append(str);
            }
            $('#shipping-method-buttons-container').show();
            $('.first-step-checkout').hide();
            if (data.ShippingMethods.length > 1) {
                shippingMethodSelected($("#shippingoption_0"));
            }
        },
        error: function () {
            alert('Error de conexión');
        }
    });
};

var saveAddress = {
    nextForm: function () {
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
            phone == "") {
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
                attrAddress: attrAddress
            };

            //addAntiForgeryToken(data);

            $.ajax({
                cache: false,
                url: 'Checkout/OpcSaveBillingNew',
                type: 'POST',
                data: datadir,
                success: function () {
                    loadShippingMethods();
                },
                error: function () {
                    alert('Error al guardar');
                }
            });
        }
    }
};

function options(data) {
    var str = '';
    $('#payment-method-select-container').empty().html(' ');
    for (var i = data.PaymentMethods.length - 1; i >= 0; i--) {
        if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.CashOnDelivery") {
            str += '<optgroup label="Pago en efectivo (contra entrega)">';
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option></optgroup>';
        }
        else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.Replacement" && $("#show-replacement-payment-method").val()) {
            str += '<optgroup label="Reposición">';
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option></optgroup>';
        }
        else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.CardOnDelivery") {
            str += '<optgroup label="Pago con tarjeta (contra entrega)">';
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option></optgroup>';
        }
        else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.PaypalPlus") {
            str += '<optgroup label="Pago con tarjeta">';
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option></optgroup>';
        }
        else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.PayPalStandard") {
            str += '<optgroup label="Pago con PayPal">';
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option></optgroup>';
        }
        else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.MercadoPago") {
            str += '<optgroup label="Pago con MercadoPago">';
            str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].LogoUrl + '">' + data.PaymentMethods[i].Name + '</option></optgroup>';
        }
        else if (data.PaymentMethods[i].PaymentMethodSystemName === "Payments.ComproPago") {
            str += '<optgroup label="Pago en efectivo">';
            for (var j = 0; j < data.PaymentMethods[i].PaymentChildrens.length; j++) {
                str += '<option value="' + data.PaymentMethods[i].PaymentMethodSystemName + '" data-icon="' + data.PaymentMethods[i].PaymentChildrens[j].ProviderImage + '" id="' + data.PaymentMethods[i].PaymentChildrens[j].SystemName + '">' + data.PaymentMethods[i].PaymentChildrens[j].PublicName + '</option>';
            }
            str += '</optgroup>';
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
            str += '<label>Serás redirigido a la página de MercadoPago para que hagas el pago con tu tarjeta de crédito / débito o en efectivo. Al finalizar regresarás de manera automática a nuestro sitio.</label>';
            $('#PaymentAlert').append(str);
        }
        else if (payElement === 'Payments.ComproPago') {
            str = '';
            $('#pay-card').hide();
            $('#PaymentAlert').empty().html(' ');
            $('#PaymentAlert').show();
            str += '<label>Recibirás un correo electrónico con la información para realizar el pago en efectivo en el establecimiento seleccionado.</label>';
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
};

var ShippingOptions = {
    save: function () {
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

        var url = 'Checkout/SelectShippingMethodNew?shippingoption=' + data[0];
        
        $.ajax({
            cache: false,
            url: url,
            type: 'POST',
            //data: data,
            success: function () {
                loadPaymentOptions();
            },
            error: function () {
                alert('Error de conexión shipping');
            }
        });
    }
};

const loadPaymentOptions = () => {

    var orderTotal = $('#calculate-total').text();
    var nOrderTotal = Number(orderTotal.replace(/[^0-9\.-]+/g, ""));

    $.ajax({
        cache: false,
        url: 'Checkout/PaymentMethodNew?orderTotal=' + nOrderTotal,
        type: 'GET',
        success: function (data) {
            $('.first-step-checkout').hide();            
            $('#shipping-method-buttons-container').hide();
            options(data);
            $('#checkout-step-payment-method').show();
            $('#payment-method-buttons-container').show();
        },
        error: function () {
            alert('Error de conexión payment');
        }
    });
};

var SavePayment = {
    save: function (orderNote = null) {
        var payment = $('#payment-method-select-container').val();

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

var OrderConfirm = {
    form: false,
    saveUrl: false,
    isSuccess: false,

    init: function (saveUrl, successUrl) {
        this.saveUrl = saveUrl;
        this.successUrl = successUrl;
    },

    save: function (orderNote = null) {
        $('#confirm-button-disable').addClass('disabled');
        $('#loading-confirm').addClass('active');

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
                            orderNote: orderNote
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
                    orderNote: orderNote
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
                orderNote: orderNote
            }
            sendData(body);
        }
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