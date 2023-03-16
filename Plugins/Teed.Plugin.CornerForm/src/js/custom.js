function onCornerFormSubmit() {
    let answer = $("textarea:visible.form-answer").val();
    if (!answer) return;
    $(".loading-container").show();

    $(".send-button").prop('disabled', true);
    $(".cancel-button").prop('disabled', true);
    $(".success-msg").hide();
    $(".error-msg").hide();

    let body = {
        AnswerText: answer
    };

    $.ajax({
        url: '/CornerFormAnswer/SubmitAnswer',
        method: 'POST',
        data: body,
        success: (data) => {
            $("textarea:visible.form-answer").val('');
            $(".success-msg").show();
            $(".loading-container").hide();
            $(".send-button").prop('disabled', false);
            $(".cancel-button").prop('disabled', false);
        },
        error: (error) => {
            $(".loading-container").hide();
            $(".send-button").prop('disabled', false);
            $(".cancel-button").prop('disabled', false);
            $(".error-msg").show();
        }
    });
}

function openCornerForm() {
    document.getElementById("myCornerForm").style.display = "block";
}

function closeCornerForm() {
    $(".success-msg").hide();
    $(".error-msg").hide();
    $(".loading-container").hide();
    document.getElementById("myCornerForm").style.display = "none";
}