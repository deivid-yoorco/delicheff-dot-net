var indexView = {
    openModal: function (title) {
        console.log(title);
        //$("#Name").empty();
        //$("#Name").append(title);
    },

    atrribute: function () {
        $.ajax({
            url: '/admin/' + indexView.controllerName() + '/DeleteAssociationPage?button=save&index=' + indexView.toDeleteId() + "&gId=" + indexView.toDeletePartnerId(),
            type: 'DELETE',
            success: function (result) {
                $('#' + indexView.toDeleteId()).remove();
            },
            error: function (error) {
                $("#error").empty();
                $("#error").append("Ocurrió un error inesperado.");
            }
        });
    }
};
