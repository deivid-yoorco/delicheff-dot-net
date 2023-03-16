$(document).ready(function () {

    $.datepicker.regional['es'] = {
        closeText: 'Cerrar',
        prevText: '< Ant',
        nextText: 'Sig >',
        currentText: 'Hoy',
        monthNames: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
        monthNamesShort: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'],
        dayNames: ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'],
        dayNamesShort: ['Dom', 'Lun', 'Mar', 'Mié', 'Juv', 'Vie', 'Sáb'],
        dayNamesMin: ['Do', 'Lu', 'Ma', 'Mi', 'Ju', 'Vi', 'Sá'],
        weekHeader: 'Sm',
        dateFormat: 'dd-mm-yy',
        firstDay: 1,
        isRTL: false,
        showMonthAfterYear: false,
        yearSuffix: ''
    };
    $.datepicker.setDefaults($.datepicker.regional['es']);

    var holidaysArray = [""];
    var weekDaysArray = [""];
    var doctorlockDates = [""];
    $('#datepicker').prop('disabled', false);
    $('#timepicker').prop('disabled', false);
    $('#doctors').prop('disabled', false);

    $("#doctors").kendoComboBox({
        placeholder: "Selecciona al responsable..."
    });

    $("#branches").kendoComboBox({
        placeholder: "Selecciona la sucursal...",
        dataTextField: "Branch",
        dataValueField: "Id",
        filter: "contains",
        autoBind: true,
        minLength: 1,
        dataSource: {
            type: "json",
            transport: {
                read: {
                    url: "/Admin/Appointment/BranchListData",
                    type: "POST",
                    data: addAntiForgeryToken
                }
            }
        },
        dataBound: function (e) {
            var combobox = $("#branches").data('kendoComboBox');
            if (combobox.dataSource.data().length == 1) {
                var id = combobox.dataSource.data()[0].Id;
                combobox.value(id);
                $("#branchId").val(id);
            }
            if ($("#branchId").val() != 0 && !$("#branches").data("kendoComboBox").value()) {
                $("#branches").data("kendoComboBox").value($("#branchId").val());
                //$("#branchId").val(@Model.BranchId);
            }
        },
        select: function (e) {
            $("#branchId").val(this.dataItem(e.item.index()).Id);
        },
        change: function (e) {
            $("#doctorId").val(0);
            $("#doctors").data("kendoComboBox").value("");
            $('#datepicker').prop('disabled', true);
            $('#datepicker').val("");
            $('#timepicker').prop('disabled', true);
            $('#timepicker').val("");
            updateDoctors($("#branches").val());
        }
    });

    $("#doctors").kendoComboBox({
        placeholder: "Selecciona al responsable...",
        dataTextField: "Doctor",
        dataValueField: "Id",
        filter: "contains",
        autoBind: true,
        minLength: 1,
        dataSource: {
            type: "json",
            transport: {
                read: {
                    url: "/Admin/Doctor/DoctorByBranchListData?branchId=" + $("#branchId").val(),
                    type: "GET",
                    data: addAntiForgeryToken
                }
            }
        },
        dataBound: function (e) {
            var combobox = $("#doctors").data('kendoComboBox');
            if (combobox.dataSource.data().length == 1) {
                var id = combobox.dataSource.data()[0].Id;
                combobox.value(id);
                $("#doctorId").val(id);
            }
            if ($("#doctorId").val() != 0 && !$("#doctors").data("kendoComboBox").value()) {
                $("#doctors").data("kendoComboBox").value($("#doctorId").val());
                //$("#branchId").val(@Model.BranchId);
            }
        },
        select: function (e) {
            $("#doctorId").val(this.dataItem(e.item.index()).Id);
        },
        change: function (e) {
            $('#datepicker').prop('disabled', true);
            $('#datepicker').val("");
            $('#timepicker').prop('disabled', true);
            $('#timepicker').val("");
            if (this.selectedIndex == -1) {
                $("#doctors").data("kendoComboBox").value("");
            };
            var postData = {
                branchId: $("#branches").val(),
                doctorId: $("#doctors").val()
            };
            addAntiForgeryToken(postData);
            $.ajax({
                cache: false,
                type: "POST",
                url: "/Admin/Appointment/GetDates",
                data: postData,
                success: function (data) {
                    updateDisableDates(data);
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    alert('Ocurrió un error.');
                }
            });
        }
    });

    GetDates();

    function loadDatepicker() {
        $('#datepicker').datepicker({
            minDate: 0,
            defaultDate: new Date($("#datepicker").val()),
            beforeShowDay: function (date) {
                var day = date.getDay();
                var string = jQuery.datepicker.formatDate('yy-mm-dd', date);

                if (weekDaysArray.indexOf(day) != -1) return [false];
                return [doctorlockDates.indexOf(string) == -1];
                return [holidaysArray.indexOf(string) == -1];
            }
        });
    }

    GetTimes();

    function GetTimes() {
        //var date = new Date($('#datepicker').datepicker('getDate'));
        var date = new Date($("#datepicker").val());
        $("#modelDate").val(formatDate(date.getDate()) + "-" + formatDate(date.getMonth() + 1) + "-" + date.getFullYear());
        console.log($("#modelDate").val());

        var postData = {
            doctorId: $("#doctorId").val(),
            branchId: $("#branchId").val(),
            selectedDate: $("#modelDate").val()
        };
        addAntiForgeryToken(postData);
        $.ajax({
            cache: false,
            type: "POST",
            url: "/Admin/Appointment/GetTimes",
            data: postData,
            success: function (data) {
                updateTimes(data);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert('Ocurrió un error cargando las horas disponibles.');
            }
        });
    }

    function GetDates() {
        $("#datepicker").val(jQuery.datepicker.formatDate('mm/dd/yy', new Date($("#modelDate").val())));
        var postData = {
            branchId: $("#branchId").val(),
            doctorId: $("#doctorId").val()
        };
        addAntiForgeryToken(postData);
        $.ajax({
            cache: false,
            type: "POST",
            url: "/Admin/Appointment/GetDates",
            data: postData,
            success: function (data) {
                updateDisableDates(data);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert('Ocurrió un error.');
            }
        });
    }

    function updateDoctors(branchId) {
        $('#doctors').prop('disabled', false);
        $('#doctors').val("");
        $("#doctors").kendoComboBox({
            placeholder: "Selecciona al responsable...",
            dataTextField: "Doctor",
            dataValueField: "Id",
            filter: "contains",
            autoBind: true,
            minLength: 1,
            dataSource: {
                type: "json",
                transport: {
                    read: {
                        url: "/Admin/Doctor/DoctorByBranchListData?branchId=" + branchId,
                        type: "GET",
                        data: addAntiForgeryToken
                    }
                }
            },
            dataBound: function (e) {
                if ($("#doctorId").val() != 0 && !$("#doctors").data("kendoComboBox").value()) {
                    $("#doctors").data("kendoComboBox").value($("#doctorId").val());
                    //$("#branchId").val(@Model.BranchId);
                }
            },
            select: function (e) {
                $("#doctorId").val(this.dataItem(e.item.index()).Id);
            },
            change: function (e) {
                $('#datepicker').prop('disabled', true);
                $('#datepicker').val("");
                $('#timepicker').prop('disabled', true);
                $('#timepicker').val("");
                if (this.selectedIndex == -1) {
                    $("#doctors").data("kendoComboBox").value("");
                };
                var postData = {
                    branchId: $("#branches").val(),
                    doctorId: $("#doctors").val()
                };
                addAntiForgeryToken(postData);
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: "/Admin/Appointment/GetDates",
                    data: postData,
                    success: function (data) {
                        updateDisableDates(data);
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        alert('Ocurrió un error.');
                    }
                });
            }
        });
    }

    function updateDisableDates(data) {
        holidaysArray = data[0];
        weekDaysArray = data[1];
        doctorlockDates = data[2];
        $('#datepicker').prop('disabled', false);
        loadDatepicker();
    }

    function uiTimePicker() {
        $('#timepicker').next().remove()
        $('#timepicker').ui_choose({
            itemWidth: null,
            skin: '',
            multi: true,
            active: 'selected',
            full: false,
            colNum: null,
            dataKey: 'ui-choose',
            change: null,
            click: null
        });
        //$('#timepicker').next().find("li").removeClass("selected");
    }

    $('#datepicker').change(function () {
        $('#timepicker').prop('disabled', true);
        $('#timepicker').val("");

        var date = new Date($('#datepicker').datepicker('getDate'));
        $("#modelDate").val(formatDate(date.getDate()) + "-" + formatDate(date.getMonth() + 1) + "-" + date.getFullYear());

        var postData = {
            doctorId: $("#doctors").val(),
            branchId: $("#branches").val(),
            selectedDate: $("#modelDate").val()
        };
        addAntiForgeryToken(postData);
        $.ajax({
            cache: false,
            type: "POST",
            url: "/Admin/Appointment/GetTimes",
            data: postData,
            success: function (data) {
                updateTimes(data);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert('Ocurrió un error cargando las horas disponibles.');
            }
        });
    });

    function formatDate(n) {
        return n < 10 ? '0' + n : '' + n;
    }

    function updateTimes(data) {
        var selectedTimes = $("#timepickerValue>option").map(function() { return $(this).val(); }).get();
        $('#timepicker').prop('disabled', false);
        cleanTimes();
        let optionList = document.getElementById('timepicker').options;
        for (var i = 0; i < data.length; i++) {
            var hour = $("#appointmentHour").val() < 10 ? "0" + $("#appointmentHour").val() : $("#appointmentHour").val();
            var minute = $("#appointmentMinute").val() < 10 ? "0" + $("#appointmentMinute").val() : $("#appointmentMinute").val();
            if (selectedTimes.indexOf(data[i].TimeValue) > -1) {
                var option = new Option(data[i].TimeText, data[i].TimeValue);
                option.selected = true;
            }
            else if (!data[i].IsActive && (data[i].TimeValue != $("#timepickerValue").val())) {
                var option = new Option(data[i].TimeText + " (Ocupado)", data[i].TimeValue);
                option.disabled = true;
            }
            else {
                var option = new Option(data[i].TimeText, data[i].TimeValue);
            }
            
            optionList.add(option);
        }
        uiTimePicker();
    }

    function cleanTimes() {
        $("#timepicker").find('option').remove();
    }
});