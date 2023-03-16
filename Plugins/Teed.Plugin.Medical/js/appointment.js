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
    var isNormal = true;
    //$('#datepicker').prop('disabled', true);
    //$('#timepicker').prop('disabled', true);
    //$('#doctors').prop('disabled', true);
    $('#reserveTimeButton').prop('disabled', true);
    initTimes();
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
            if ($("#doctorId").val() != 0 && !$("#doctors").data("kendoComboBox").value()) {
                $("#doctors").data("kendoComboBox").value($("#doctorId").val());
                GetDates();
            }
        },
        select: function (e) {
            $("#doctorId").val(this.dataItem(e.item.index()).Id);
            updateBranchesByDoctor();
            //updateAppointments();
        },
        change: function (e) {
            $('#datepicker').prop('disabled', true);
            $('#datepicker').val("");
            // $('#timepicker').prop('disabled', true);
            $('#timepicker').val("");
            $('#reserveTimeButton').prop('disabled', true);
            if (this.selectedIndex == -1) {
                $("#doctors").data("kendoComboBox").value("");
            };
            GetDates();
        }
    });

    function GetDates() {
        var postData = {
            branchId: $("#branches").val(),
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
                alert('Ocurrió un problema intentando cargar las fechas.');
            }
        });
    }


    function updateBranchesByDoctor() {
        if ($("#branchId").val() == 0) {
            getBranches($("#doctorId").val());
        }
        else if (!$('#datepicker').val()) {
            GetDates();
        }
    }

    getBranches();

    function getBranches(doctorId = 0) {
        let data = {
            doctorId: doctorId
        };
        addAntiForgeryToken(data);

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
                        data: data
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
                if ($("#branchId").val() != 0 && doctorId != 0 && !$('#datepicker').val()) {
                    GetDates();
                }
            },
            select: function (e) {
                $("#branchId").val(this.dataItem(e.item.index()).Id);
            },
            change: function (e) {
                $("#doctorId").val(0);
                $("#doctors").data("kendoComboBox").value("");
                //$('#datepicker').prop('disabled', true);
                $('#datepicker').val("");
                //$('#timepicker').prop('disabled', true);
                $('#timepicker').val("");
                $('#reserveTimeButton').prop('disabled', true);
                updateDoctors($("#branches").val());
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
                updateBranchesByDoctor();
            },
            change: function (e) {
                //$('#datepicker').prop('disabled', true);
                $('#datepicker').val("");
                //$('#timepicker').prop('disabled', true);
                $('#timepicker').val("");
                $('#reserveTimeButton').prop('disabled', true);
                if (this.selectedIndex == -1) {
                    $("#doctors").data("kendoComboBox").value("");
                };
                GetDates();
            }
        });
    }


    function updateDoctorsByDate(date) {
        console.log("updateDoctorsByDate");
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
                        url: "/Admin/Doctor/DoctorByDateListData?date=" + date,
                        type: "GET",
                        data: addAntiForgeryToken
                    }
                }
            },
            dataBound: function (e) {
                console.log("dataBound");
                var combobox = $("#doctors").data('kendoComboBox');
                if (combobox.dataSource.data().length == 0) {
                    combobox.input.attr("placeholder", "No se encontraron doctores disponibles");
                }
                if (combobox.dataSource.data().length == 1) {
                    var id = combobox.dataSource.data()[0].Id;
                    combobox.value(id);
                    $("#doctorId").val(id);
                    if ($("#branches").val() != 0 && $("#doctors").val() != 0 && $("#timepicker").val() == null) {
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
                    }
                    GetDates();
                }
                if ($("#doctorId").val() != 0 && !$("#doctors").data("kendoComboBox").value()) {
                    $("#doctors").data("kendoComboBox").value($("#doctorId").val());
                    //$("#branchId").val(@Model.BranchId);
                }
            },
            select: function (e) {
                console.log("select");
                $("#doctorId").val(this.dataItem(e.item.index()).Id);
                updateBranchesByDoctor();
            },
            change: function (e) {
                console.log("change");
                //$('#datepicker').prop('disabled', true);
                //$('#datepicker').val("");
                //$('#timepicker').prop('disabled', true);
                //$('#timepicker').val("");
                $('#reserveTimeButton').prop('disabled', true);
                if (this.selectedIndex == -1) {
                    $("#doctors").data("kendoComboBox").value("");
                };
                if ($('#timepicker').val() != 0) {
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
                }
                GetDates();
            }
        });
    }


    function updateDoctorsByDateAndTime(date, time, branchId) {
        console.log("updateDoctorsByDateAndTime");
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
                        url: "/Admin/Doctor/DoctorByTimeAndDateListData?date=" + date + "&time=" + time + "&branchId=" + branchId,
                        type: "GET",
                        data: addAntiForgeryToken
                    }
                }
            },
            dataBound: function (e) {
                var combobox = $("#doctors").data('kendoComboBox');
                if (combobox.dataSource.data().length == 0) {
                    combobox.input.attr("placeholder", "No se encontraron doctores disponibles");
                }
                if (combobox.dataSource.data().length == 1) {
                    var id = combobox.dataSource.data()[0].Id;
                    combobox.value(id);
                    $("#doctorId").val(id);
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

                }
                if ($("#doctorId").val() != 0 && !$("#doctors").data("kendoComboBox").value()) {
                    $("#doctorId").val("");
                    $("#doctors").data("kendoComboBox").value($("#doctorId").val());
                    //$("#branchId").val(@Model.BranchId);
                }
            },
            select: function (e) {
                $("#doctorId").val(this.dataItem(e.item.index()).Id);
                updateBranchesByDoctor();
            },
            change: function (e) {
                //$('#datepicker').prop('disabled', true);
                //$('#datepicker').val("");
                /*$('#timepicker').prop('disabled', true);
                $('#timepicker').val("");
                $('#reserveTimeButton').prop('disabled', true);*/
                if (this.selectedIndex == -1) {
                    $("#doctors").data("kendoComboBox").value("");
                };
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
                GetDates();
            }
        });
    }


    function updateDoctorsByTime(time) {
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
                        url: "/Admin/Doctor/DoctorByTimeListData?time=" + time,
                        type: "GET",
                        data: addAntiForgeryToken
                    }
                }
            },
            dataBound: function (e) {
                var combobox = $("#doctors").data('kendoComboBox');
                if (combobox.dataSource.data().length == 0) {
                    combobox.input.attr("placeholder", "No se encontraron doctores disponibles");
                }
                if (combobox.dataSource.data().length == 1) {
                    var id = combobox.dataSource.data()[0].Id;
                    combobox.value(id);
                    $("#doctorId").val(id);

                }
                if ($("#doctorId").val() != 0 && !$("#doctors").data("kendoComboBox").value()) {
                    //$("#doctorId").val("");
                    $("#doctors").data("kendoComboBox").value($("#doctorId").val());
                    //$("#branchId").val(@Model.BranchId);
                }
            },
            select: function (e) {
                $("#doctorId").val(this.dataItem(e.item.index()).Id);
                updateBranchesByDoctor();
            },
            change: function (e) {
                //$('#datepicker').prop('disabled', true);
                //$('#datepicker').val("");
                /*$('#timepicker').prop('disabled', true);
                $('#timepicker').val("");
                $('#reserveTimeButton').prop('disabled', true);*/
                if (this.selectedIndex == -1) {
                    $("#doctors").data("kendoComboBox").value("");
                };
                GetDates();
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


    loadDatepicker();
    function loadDatepicker() {
        $("#datepicker").datepicker("destroy");
        $('#datepicker').datepicker({
            minDate: 0,
            beforeShowDay: function (date) {
                var day = date.getDay();
                var string = jQuery.datepicker.formatDate('yy-mm-dd', date);

                if (weekDaysArray.indexOf(day) != -1) return [false];
                return [doctorlockDates.indexOf(string) == -1];
                return [holidaysArray.indexOf(string) == -1];
            }
        });
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
            change: timeSelected,
            click: null
        });
        $('#timepicker').next().find("li").removeClass("selected");
    }
    
    $('#datepicker').change(function () {
        // $('#timepicker').prop('disabled', true);
        $('#reserveTimeButton').prop('disabled', true);
        // $('#timepicker').val("");
        //if (!isNormal || $("#doctors").val() == 0) {
        //    $("#doctors").val(0);
        //}


        var date = new Date($('#datepicker').datepicker('getDate'));
        $("#modelDate").val(formatDate(date.getDate()) + "-" + formatDate(date.getMonth() + 1) + "-" + date.getFullYear());

        if ($("#branches").val() != 0 && $("#doctors").val() != 0) {
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
        } else if ($("#timepicker").val() == null && $("#doctors").val() == 0) {

            updateDoctorsByDate($("#modelDate").val());
            isNormal = false;

        } else if ($("#timepicker").val() != null && $("#doctors").val() == 0) {
            var date = $("#modelDate").val();
            var time = $("#timepicker").val();
            var branchId = $("#branches").val();
            updateDoctorsByDateAndTime(date, time, branchId);
        }

    });

    function formatDate(n) {
        return n < 10 ? '0' + n : '' + n;
    }

    function updateTimes(data) {
        // $('#timepicker').prop('disabled', false);
        $('#reserveTimeButton').prop('disabled', false);
        var time = $("#timepicker").val();
        $('#timepicker').empty();
        let optionList = document.getElementById('timepicker').options;

        for (var i = 0; i < data.length; i++) {

            if (!data[i].IsActive) {
                var option = new Option(data[i].TimeText + " (Ocupado)", data[i].TimeValue);
                option.disabled = true;
            }
            else {
                var option = new Option(data[i].TimeText, data[i].TimeValue);
            }

            //if (!data[i].IsActive) { option.disabled = true; option.TimeText = option.TimeText + " (Ocupado)"; }
            optionList.add(option);
            $("#timepicker").val(time);
        }
        uiTimePicker();
    }

    function initTimes() {
        $('#timepicker').prop('disabled', false);
        $('#reserveTimeButton').prop('disabled', false);
        //$('#timepicker').empty();
        let optionList = document.getElementById('timepicker').options;
        for (var i = 5; i < 23; i++) {

            if (i < 10) {
                var option2 = new Option("0" + i + ":00", "0" + i + ":00:00");
                var option1 = new Option("0" + i + ":30", "0" + i + ":30:00");
            } else {
                var option2 = new Option(i + ":00", i + ":00:00");
                var option1 = new Option(i + ":30", i + ":30:00");
            }

            optionList.add(option2);
            optionList.add(option1);
            //if (!data[i].IsActive) { option.disabled = true; option.TimeText = option.TimeText + " (Ocupado)"; }         
        }

        uiTimePicker();
    }

    function timeSelected() {
        if ($("#branches").val() != 0 && $("#modelDate").val() != "0" && $("#doctorId").val() == 0) {
            var date = $("#modelDate").val();
            var time = $("#timepicker").val();
            var branchId = $("#branches").val();
            updateDoctorsByDateAndTime(date, time, branchId);
        } else if ($("#doctorId").val() == 0) {
            var time = $("#timepicker").val();
            updateDoctorsByTime(time);
        }
    }
});