const rgb2hex = (rgb) => `#${rgb.match(/^rgb\((\d+),\s*(\d+),\s*(\d+)\)$/).slice(1).map(n => parseInt(n, 10).toString(16).padStart(2, '0')).join('')}`;

var routeColors = [
    "03a9f4", //1
    "e91e63",//2
    "607d8b",//3
    "9c27b0",//4
    "cddc39",//5
    "ffc107",//6
    "673ab7",//7
    "795548",//8
    "ff5722",//9
    "363dbd",//10
    "FF1493",//11
    "32cd32",//12
    "78909c",
    "78909c",
    "78909c",
    "78909c",
    "78909c",
    "78909c",
    "78909c",
];

function getStarColor(optimizationId) {
    var className = "";
    switch (optimizationId) {
        case 1:
            className = "optimize-color-pink";
            break;
        case 2:
            className = "optimize-color-blue";
            break;
        case 3:
            className = "optimize-color-black";
            break;
        case 4:
            className = "optimize-color-orange";
            break;
        case 5:
            className = "optimize-color-green";
            break;
        default:
            break;
    }

    var $tmp = $(`<a class="${className}"></a>`);
    $('body').append($tmp);
    $tmp.hide();
    var color = $(`.${className}`).css('color');
    $tmp.remove();
    return rgb2hex(color).replace('#', '');
}

function getOptimizationType(optimizationTypeId) {
    var optimizationElement = "";
    switch (optimizationTypeId) {
        case 1:
            optimizationElement = `<br/><br/><strong class="optimize-color-pink">► Solicitud inmediata</strong>`;
            break;
        case 2:
            optimizationElement = `<br/><br/><strong class="optimize-color-blue">► Solicitud en consideración</strong>`;
            break;
        case 3:
            optimizationElement = `<br/><br/><strong class="optimize-color-black">► Solicitud en proceso de evaluación</strong>`;
            break;
        case 4:
            optimizationElement = `<br/><br/><strong class="optimize-color-orange">► Solicitud realizada por el cliente</strong>`;
            break;
        case 5:
            optimizationElement = `<br/><br/><strong class="optimize-color-green">► Solicitud aplicable en trayecto</strong>`;
            break;
        default:
            break;
    };
    return optimizationElement;
}

let openDatePicker = () => {
    $(".datepicker").click();
};

function formatDate(n) {
    return n < 10 ? '0' + n : '' + n;
}

function done() {
    if (!this.hasEvent) {
        this.hasEvent = true;
        window.open('/Admin/Order/ExportOrdersData?selectedDate=' + $('.datepicker').val() + '&byBuyer=true&buyerId=' + $("#buyerId").val(), '_blank');
    }
}

$('.datepicker').datepicker({
    onOpen: function (e) {
        var that = this;
        that.hasEvent = false;
        this.doneBtn.addEventListener('click', done.bind(that));
    },
    onClose: function () {
        var that = this;
        this.doneBtn.removeEventListener('click', done.bind(that));
    },
    format: "dd-mm-yyyy",
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