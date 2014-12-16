$(document).ready(function () {

    $(".first").focus();

    if ($("#ReportCategory").val() == "User" || $("#ReportCategory").val() == "School") {
        $("#start_date, #end_date").hide();
    }

    $(".date").datepicker({
        dateFormat: "mm/dd/yy"
    });

    $("#ReportCategory").change(function () {
        var message = $(this).val();
        if (message == "Project" || message == "All") {
            $("#start_date, #end_date").show();
        }
        else {
            $("#start_date, #end_date").hide();
        }
    });

    $(".title-txtbox").bind("keypress keyup focus", function () {
        var maxLength = $(this).attr("maxLength");
        var textLength = $(this).val().length;
        var remaining = maxLength - textLength;

        $("#title-remaining").html(remaining + " characters remaining");
    });

    $(".summary-txtbox").bind("keypress keyup focus", function () {
        var maxLength = $(this).attr("maxLength");
        var textLength = $(this).val().length;
        var remaining = maxLength - textLength;

        $("#summary-remaining").html(remaining + " characters remaining");
    });

    $("textarea.desc-txtarea").bind("keypress keyup focus", function () {
        var maxLength = $(this).attr("maxLength");
        var textLength = $(this).val().length;
        var remaining = maxLength - textLength;

        $("#desc-remaining").text(remaining + " characters remaining");
    });

    $("textarea.just-txtarea").bind("keypress keyup focus", function () {
        var maxLength = $(this).attr("maxLength");
        var textLength = $(this).val().length;
        var remaining = maxLength - textLength;

        $("#just-remaining").html(remaining + " characters remaining");
    });

    $(".title-txtbox").blur(function () {
        $("#title-remaining").html("");
    });
    $(".summary-txtbox").blur(function () {
        $("#summary-remaining").html("");
    });
    $(".desc-txtarea").blur(function () {
        $("#desc-remaining").html("");
    });
    $(".just-txtarea").blur(function () {
        $("#just-remaining").html("");
    });

    $(".search-name").bind("keyup", function () {
        $.ajax({
            url: "/Admin/ListUser",
            data: { name: $(this).val() },
            success: function (result) {
                $(".user-list").html(result);
            },
            error: function (xhr, status) {
                alert(status);
            }
        });
    });


    $(".search-ambas").bind("keyup", function () {
        $.ajax({
            url: "/Admin/ListAmbassador",
            data: { name: $(this).val() },
            success: function (result) {
                $(".ambassador-list").html(result);
            },
            error: function (xhr, status) {
                alert(status);
            }
        });
    });


    $(".search-contr").bind("keyup", function () {
        $.ajax({
            url: "/Admin/ListContributor",
            data: { name: $(this).val() },
            success: function (result) {
                $(".contributor-list").html(result);
            },
            error: function (xhr, status) {
                alert(status);
            }
        });
    });

    $("#search_idea").autocomplete({
        source: "/Home/SearchIdea"
    });


    $(".test").click(function () {
        alert("hello");
        $(this).focus();
    });

    $("#prj-tab").addClass("grey");
    $("#clnt-opt, #report-opt").hide();

    $("#prj-tab").click(function () {
        $(this).addClass("grey");
        $("#clnt-tab, #report-tab").removeClass("grey");
        $("#idea-opt").show();
        $("#clnt-opt, #report-opt").hide();
    });

    $("#clnt-tab").click(function () {
        $(this).addClass("grey");
        $("#prj-tab, #report-tab").removeClass("grey");
        $("#clnt-opt").show();
        $("#idea-opt, #report-opt").hide();
    });

    $("#report-tab").click(function () {
        $(this).addClass("grey");
        $("#prj-tab, #clnt-tab").removeClass("grey");
        $("#report-opt").show();
        $("#idea-opt, #clnt-opt").hide();
    });

});