$(document).ready(function () {
    $("#scheduler").kendoScheduler({
        date: new Date("2013/6/13"),
        startTime: new Date("2013/6/13 07:00 AM"),
        endTime: new Date("2013/6/13 11:00 AM"),
        height: 600,
        editable: false,
        allDaySlot: false,
        footer: false,
        dateHeaderTemplate: kendo.template("#=kendo.toString(date, 'ddd')#"),
        views: [
            { type: "week", selected: true },
        ],
        dataSource: {
            batch: true,
            transport: {
                read: {
                    url: "http://demos.telerik.com/kendo-ui/service/meetings",
                    dataType: "jsonp"
                },
                parameterMap: function (options, operation) {
                    if (operation !== "read" && options.models) {
                        return { models: kendo.stringify(options.models) };
                    }
                }
            },
            schema: {
                model: {
                    id: "meetingID",
                    fields: {
                        title: { from: "Title", defaultValue: "No title", validation: { required: true } },
                        start: { type: "date", from: "Start" },
                        end: { type: "date", from: "End" },
                    }
                }
            }
        },
        resources: [
            {
                field: "roomId",
                dataSource: [
                    { text: "Meeting Room 101", value: 1, color: "#6eb3fa" },
                    { text: "Meeting Room 201", value: 2, color: "#f58a8a" }
                ],
                title: "Room"
            }
        ]
    });
});