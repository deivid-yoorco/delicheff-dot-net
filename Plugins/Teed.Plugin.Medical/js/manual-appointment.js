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
   /* $('#datepicker').prop('disabled', true);
    $('#timepicker').prop('disabled', true);
    $('#doctors').prop('disabled', true);
    $('#reserveTimeButton').prop('disabled', true);*/
    updateTimes();
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
                    url: "/Admin/Visit/DoctorListData",
                    type: "POST",
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
            if (!$("#doctorId").val()) {
                $("#doctors").data("kendoComboBox").value($("#doctorId").val());
            }
        },
        select: function (e) {
            $("#doctorId").val(this.dataItem(e.item.index()).Id);
            //updateAppointments();
        },
        change: function (e) {
            if (this.selectedIndex == -1) {
                $("#doctors").data("kendoComboBox").value("");
            }
           
        }
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
            //$("#doctorId").val(0);
            //$("#doctors").data("kendoComboBox").value("");
            //$('#datepicker').prop('disabled', true);
            $('#datepicker').val("");
            //$('#timepicker').prop('disabled', true);
            $('#timepicker').val("");
            //$('#reserveTimeButton').prop('disabled', true);
            //updateDoctors($("#branches").val());
        }
    });

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
                $('#reserveTimeButton').prop('disabled', true);
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
        $('#datepicker').prop('disabled', false);
    }

    $('#datepicker').datepicker({
        minDate: 0,
        beforeShowDay: function (date) {
            var day = date.getDay();
            var string = jQuery.datepicker.formatDate('yy-mm-dd', date);

            if (weekDaysArray.indexOf(day) != -1) return [false];
            return [holidaysArray.indexOf(string) == -1];
        }
    });

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
        $('#timepicker').next().find("li").removeClass("selected");
    }

    $('#datepicker').change(function () {
        //$('#timepicker').prop('disabled', true);
       // $('#reserveTimeButton').prop('disabled', true);
        //$('#timepicker').val("");

        var date = new Date($('#datepicker').datepicker('getDate'));
        $("#modelDate").val(formatDate(date.getDate()) + "-" + formatDate(date.getMonth() + 1) + "-" + date.getFullYear());

        var postData = {
            doctorId: $("#doctors").val(),
            branchId: $("#branches").val(),
            selectedDate: $("#modelDate").val()
        };
        addAntiForgeryToken(postData);
        /*$.ajax({
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
        });*/
    });

    function formatDate(n) {
        return n < 10 ? '0' + n : '' + n;
    }

    function updateTimes() {
        $('#timepicker').prop('disabled', false);
        $('#reserveTimeButton').prop('disabled', false);
        $('#timepicker').empty();
        //let optionList = document.getElementById('timepicker').options;
        //var c = 1;
        //for (var i = 5; i < 23; i++) {
           
        //    if (i < 10) {
        //        if (c % 2 == 0) {
        //            var option = new Option("0" + i + ":30", "0" + i + ":30:00");
        //        } else {
        //            var option = new Option("0" + i + ":00", "0" + i + ":00:00");
        //        }
        //    } else {
        //        if (c % 2 == 0) {
        //            var option = new Option(i + ":30", "0" + i + ":30:00");
        //        } else {
        //            var option = new Option(i + ":00", "0" + i + ":00:00");
        //        }
        //    }
        //    c++;

        //    //if (!data[i].IsActive) { option.disabled = true; option.TimeText = option.TimeText + " (Ocupado)"; }
        //    optionList.add(option);
        //}

        let optionList = document.getElementById('timepicker').options;
        for (var i = 5; i < 23; i++) {

            if (i < 10) {
                var option6 = new Option("0" + i + ":00", "0" + i + ":00:00");
                var option5 = new Option("0" + i + ":10", "0" + i + ":10:00");
                var option4 = new Option("0" + i + ":20", "0" + i + ":20:00");
                var option3 = new Option("0" + i + ":30", "0" + i + ":30:00");
                var option2 = new Option("0" + i + ":40", "0" + i + ":40:00");
                var option1 = new Option("0" + i + ":50", "0" + i + ":50:00");
            } else {
                var option6 = new Option(i + ":00", i + ":00:00");
                var option5 = new Option(i + ":10", i + ":10:00");
                var option4 = new Option(i + ":20", i + ":20:00");
                var option3 = new Option(i + ":30", i + ":30:00");
                var option2 = new Option(i + ":40", i + ":40:00");
                var option1 = new Option(i + ":50", i + ":50:00");
            }
            optionList.add(option6);
            optionList.add(option5);
            optionList.add(option4);
            optionList.add(option3);
            optionList.add(option2);
            optionList.add(option1);
        }
        uiTimePicker();
    }
});