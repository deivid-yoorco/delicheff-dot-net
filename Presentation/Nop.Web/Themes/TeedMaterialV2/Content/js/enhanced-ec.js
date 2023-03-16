// Get current quantities to calculate better add and remove from cart.
$(document).ready(function () {
    login();
    $.ajax({
        cache: false,
        type: "GET",
        url: "/ShoppingCart/GetCurrentQuantitiesInCart",
        success: function (data) {
            $.each(data, function (i, e) {
                qtyBoxes.push({ 'id': e.sku, 'quantity': e.quantity });
            });
        },
        error: function (err) {
            console.log(err);
        },
    });
});

function htmlDecode(value) {
    return $('<div/>').html(value).text();
}

// Guide says: "It is recommended to use the following command to clear the eCommerce
// object before sending an eCommerce event to the data layer. Deleting the object will
// prevent different eCommerce events on a page from influencing each other".
const cleanPreviousEcommerce = () => {
    dataLayer.push({ ecommerce: null });
};

//// Create single impression object to reduce dataLayer push actions.
var impressions = [];
const pushImpressionsDelay = 500;
var intervalForPush;
var timer = 0;

const resetInterval = () => {
    clearInterval(intervalForPush);
    timer = 0;
    intervalForPush = setInterval(timerPlus, 1);
};

const timerPlus = () => {
    timer++;
};
// Run timer for first time to start count for impressions.
resetInterval();

var refreshIntervalId = window.setInterval(function () {
    if (timer >= pushImpressionsDelay) {
        if (impressions.length > 0) {
            $.each(impressions, function (i, e) {
                console.log(e.items.length);
                cleanPreviousEcommerce();
                dataLayer.push({
                    'event': 'view_item_list',
                    'ecommerce': {
                        'item_list_name': e.name,
                        'impressions': e.items
                    }
                });
            });
            impressions = [];
        }
        clearInterval(intervalForPush);
        clearInterval(refreshIntervalId);
    }
}, 100);
////

// Push to global intervals and reset interval so no elements are left out.
const productImpression = (id, name, category, brand, variant, list, position, price) => {
    setTimeout(function () {
        resetInterval();
        var newItem = {
            "id": htmlDecode(id),
            "name": htmlDecode(name),
            "brand": htmlDecode(brand),
            'category': htmlDecode(category),
            "variant": htmlDecode(variant),
            "position": position,
            "price": price
        }
        var listName = htmlDecode(list);
        var existingNameList = impressions.filter(function (item) {
            return item.name == listName;
        });
        if (existingNameList.length > 0)
            existingNameList[0].items.push(newItem);
        else
            impressions.push({ 'name': listName, 'items': [newItem] });
    }, 10);
};

// Add to cart.
const registerAddToCart = (id, name, category, brand, price, qty, changeLists = true) => {
    checkAndGetListQuantity(id, qty, changeLists ? '+' : '', changeLists);
    cleanPreviousEcommerce();
    var item = {
        'name': htmlDecode(name),
        'id': htmlDecode(id),
        'price': price,
        'brand': htmlDecode(brand),
        'category': htmlDecode(category),
        'quantity': qty
    };
    dataLayer.push({
        'event': 'add_to_cart',
        'ecommerce': {
            'currencyCode': 'MXN',
            'add': {
                'products': [item]
            }
        }
    });
};

// Remove from cart.
const registerRemoveFromCart = (id, name, category, brand, price, qty, changeLists = true) => {
    checkAndGetListQuantity(id, qty, changeLists ? '-' : '', changeLists);
    cleanPreviousEcommerce();
    var item = {
        'name': htmlDecode(name),
        'id': htmlDecode(id),
        'price': price,
        'brand': htmlDecode(brand),
        'category': htmlDecode(category),
        'quantity': qty
    };
    dataLayer.push({
        'event': 'remove_from_cart',
        'ecommerce': {
            'currencyCode': 'MXN',
            'remove': {
                'products': [item]
            }
        }
    });
};

// Checks from Select if quantity is either add or remove items.
var qtyBoxes = [];
const registerAddOrRemoveFromSelectToCart = (id, name, category, brand, price, element) => {
    if ($(element).length > 0) {
        var qtySelected = $(element).val();
        if (typeof qtySelected != 'undefined' && qtySelected != '') {
            var newQty = parseInt(qtySelected).toString() == 'NaN' ? -1 : parseInt(qtySelected);
            if (newQty >= 0) {
                var checked = checkAndGetListQuantity(id, newQty);
                var qtyToApply = checked.qtyToApply;
                var isAdd = checked.isAdd;
                if (isAdd)
                    registerAddToCart(id, name, category, brand, price, qtyToApply, false);
                else
                    registerRemoveFromCart(id, name, category, brand, price, qtyToApply, false);
            }
        }
    }
};

const checkAndGetListQuantity = (id, newQty, removeOrAddToOirignal = '', changeLists = true) => {
    var isAdd = true;
    var qtyToApply = 0;

    var current = qtyBoxes.filter(function (item) {
        return item.id == id;
    });
    if (current.length > 0) {
        if (removeOrAddToOirignal != '') {
            if (removeOrAddToOirignal == '-')
                newQty = current[0].quantity - newQty;
            else if (removeOrAddToOirignal == '+')
                newQty = newQty + current[0].quantity;
        }
        var currentQty = current[0].quantity;
        if (changeLists) {
            if (currentQty > newQty) {
                qtyToApply = currentQty - newQty;
                isAdd = false;
            } else if (currentQty < newQty) {
                qtyToApply = newQty - currentQty;
                isAdd = true;
            }
            changeQuantity(id, newQty);
        } else
            qtyToApply = currentQty;
    } else {
        qtyToApply = newQty;
        qtyBoxes.push({ 'id': id, 'quantity': newQty });
    }
    return {
        'isAdd': isAdd,
        'qtyToApply': qtyToApply
    };
};

// Serach term in search bar.
const searchContent = (searchTerm) => {
    cleanPreviousEcommerce();
    dataLayer.push({
        'event': 'search',
        'ecommerce': {
            'search_term': searchTerm
        }
    });
};

// Sign up.
const signUp = () => {
    cleanPreviousEcommerce();
    dataLayer.push({
        'event': 'sign_up',
        'ecommerce': {
            'method': 'Web'
        }
    });
};

// Login checks if sessions was attempted succesfully, if so it sends.
const login = () => {
    let data = sessionStorage.getItem('loginAttempt');
    if (data != null && $('.validation-summary-errors').length < 1) {
        cleanPreviousEcommerce();
        dataLayer.push({
            'event': 'login',
            'ecommerce': {
                'method': 'Web'
            }
        });
    }
    sessionStorage.removeItem('loginAttempt');
};

const viewCart = (cart) => {
    let items = createItemsListFromCart(cart);
    cleanPreviousEcommerce();
    dataLayer.push({
        'event': 'view_cart',
        'ecommerce': {
            'currencyCode': 'MXN',
            'checkout': {
                'products': items
            }
        }
    });
};

// Create item list.
const createItemsListFromCart = (cart) => {
    let items = [];
    console.log(cart);
    for (var i = 0; i < cart.length; i++) {
        var product = cart[i];
        var item = {
            "id": htmlDecode(product.Sku),
            "name": htmlDecode(product.ProductName),
            "quantity": product.Quantity,
            "category": htmlDecode(product.Category),
            "price": parseFloat(product.SubTotal.replace('$', '')),
            "isRewardItem": evalueIsRewardItem(product.IsRewardItem)
        };
        items.push(item);
    }
    return items;
};


const evalueIsRewardItem = (isRewardItem) => {
    if (typeof isRewardItem === "undefined") {
        isRewardItem = false;
    }
    return isRewardItem;
}

// First step is entering onepagecheckout.
const checkoutFirstStep = (cart) => {
    let items = createItemsListFromCart(cart);
    cleanPreviousEcommerce();
    dataLayer.push({
        'event': 'begin_checkout',
        'ecommerce': {
            'currencyCode': 'MXN',
            'checkout': {
                'products': items
            }
        }
    });
};

// Sends shipping info, first button in checkout.
const checkoutSecondStep = (cart) => {
    let items = createItemsListFromCart(cart);
    cleanPreviousEcommerce();
    dataLayer.push({
        'event': 'add_shipping_info',
        'ecommerce': {
            'currencyCode': 'MXN',
            'checkout': {
                'products': items
            },
            'shipping_tier': 'Ground',
        }
    });
};

// Sends payment method selected.
const checkoutThirdStep = (items, paymentMethod) => {
    cleanPreviousEcommerce();
    dataLayer.push({
        'event': 'add_payment_info',
        'ecommerce': {
            'currencyCode': 'MXN',
            'checkout': {
                'products': items
            },
            'payment_type': paymentMethod,
        }
    });
};

// Sends purchase and third step checkout events (checks coupons to send too).
var checkoutPaymentIsDone = false;
const checkoutPayment = (cart, total, paymentMethod, orderId) => {
    let items = createItemsListFromCart(cart);
    checkoutThirdStep(items, paymentMethod);
    var coupons = '';
    $.ajax({
        cache: false,
        type: "GET",
        url: "/ShoppingCart/GetCouponsOfOrder?orderId=" + orderId,
        success: function (data) {
            coupons = data;
        },
        error: function (err) {
            console.log(err);
        },
        complete: function () {
            cleanPreviousEcommerce();
            dataLayer.push({
                'event': 'purchase',
                'ecommerce': {
                    'purchase': {
                        'actionField': {
                            'id': orderId,
                            'affiliation': paymentMethod,
                            'revenue': total.OrderTotalValue,
                            'shipping': total.ShippingValue,
                            'coupon': coupons,
                            'currency': 'MXN',
                        },
                        'products': items
                    }
                }
            });
            setTimeout(function () { checkoutPaymentIsDone = true; }, 500);
        }
    });
};

// Replace old quantity with new.
function changeQuantity(id, quantity) {
    for (var i in qtyBoxes) {
        if (qtyBoxes[i].id == id) {
            qtyBoxes[i].quantity = quantity;
            break;
        }
    }
}