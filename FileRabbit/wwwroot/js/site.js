function updateTable(noanim) {
    $(".file-bar").load(window.location.href + " .file-table", function () {
        if (!noanim) {
            anix_do($("html").find(".anix"));
        } else {
            $("html").find(".anix").css("opacity", "1");
        }
        
        $("html").find("table .no-data").parent().parent().css("min-height", "100%");
        if ($("html").find("table .no-data").length > 0) {
            $(".actions .active").addClass("inactive");
            $(".actions .active").removeClass("active");
        }
    });
}
function Upload(files, folderId, token) {
    const formData = new FormData();

    for (let i = 0; i < files.length; i++) {
        let file = files[i];

        formData.append('uploads', file);
    }
    formData.append("folderId", folderId);
    formData.append("__RequestVerificationToken", token);

    $.ajax({
        type: 'post',
        url: 'Upload',
        processData: false,
        contentType: false,
        beforeSend: function () {
            UploadBeforeSubmit();
        },
        xhr: function () {
            var xhr = new window.XMLHttpRequest();
            xhr.upload.addEventListener("progress", function (evt) {
                if (evt.lengthComputable) {
                    var percentComplete = evt.loaded / evt.total;
                    percentComplete = parseInt(percentComplete * 100);
                    UploadProgress(percentComplete);
                }
            }, false);
            return xhr;
        },
        data: formData,
        success: function () {
            UploadSuccess();
        }
    });
}
$(".file-bar").on("dragover", function (event) {
    event.preventDefault();
    event.stopPropagation();
    $(".file-bar").addClass("drop");
});
$(".file-bar").on("dragleave", function (event) {
    if (event.target.className == "file-bar drop") {
        $(".file-bar").removeClass("drop");
    }
    event.preventDefault();
    event.stopPropagation();
    
});

$(".file-bar").on("drop", function (e) {
    $(".file-bar").removeClass("drop");
    console.log('File(s) dropped');

    // Prevent default behavior (Prevent file from being opened)
    e.preventDefault();

    if (e.originalEvent.dataTransfer && e.originalEvent.dataTransfer.files.length) {
        e.preventDefault();
        e.stopPropagation();
        /*UPLOAD FILES HERE*/
        Upload(e.originalEvent.dataTransfer.files, $("form.addfolder [name=folderId]").val(), $("form.addfolder [name=__RequestVerificationToken]").val());
    }
});
$('form.addfolder').ajaxForm(function (response) {
    updateTable();
    $("#folderCreate").modal("hide");
});
function UploadBeforeSubmit() {
    $("html").append('<div class="progress-bar-window hide"><div class="info"></div><div class="bar"><div class="item"></div></div></div>');
    setTimeout(function () {
        $("html").find(".progress-bar-window").removeClass("hide");
    }, 20);
    $("#fileUpload").modal("hide");
}
function UploadProgress(percentComplete) {
    if (percentComplete == 100) {
        text = "Processing in progress...";
    } else {
        text = "Uploading " + percentComplete + "%";
    }
    $("html").find(".progress-bar-window .info").text(text);
    $("html").find(".progress-bar-window .item").width(percentComplete + "%");
}
function UploadSuccess() {
    updateTable();
    $("html").find(".progress-bar-window").addClass("hide");
    setTimeout(function () {
        $("html").find(".progress-bar-window").detach();
    }, 300);
}
$('form.upload').ajaxForm({
    beforeSubmit: function () {
        UploadBeforeSubmit();
    },
    uploadProgress: function (event, position, total, percentComplete) {
        UploadProgress(percentComplete);
    }, 
    success: function(response) {
        UploadSuccess();
    }
});
$('form.renameitem').ajaxForm(function (response) {
    updateTable();
    $("#renameWindow").modal("hide");
});
var $_GET = {};
$(function () {
    $("table .no-data").parent().parent().css("min-height", "100%");
    document.location.search.replace(/\??(?:([^=]+)=([^&]*)&?)/g, function () {
        function decode(s) {
            return decodeURIComponent(s.split("+").join(" "));
        }

        $_GET[decode(arguments[1])] = decode(arguments[2]);
    });
})
function ActionBarLogic() {
    var count = $(".file-table tr.selected").length;
    if (count == 1) {
        $(".renameitem .elementId").attr("value", $(".file-table tr.drive-row.selected").attr("data-id"));
        $(".renameitem .isFolder").attr("value", $(".file-table tr.drive-row.selected").attr("data-isfolder"));
        $(".renameitem [name=newName]").attr("value", $($(".file-table tr.drive-row.selected").find("td")[1]).text().replace(/\.[^/.]+$/, ""));
    }
    if (count > 0) {
        $(".actions .inactive").addClass("active");
        $(".actions .inactive").removeClass("inactive");
        if (count > 1) {
            $(".actions .rename").addClass("inactive");
            $(".actions .rename").removeClass("active");
        }
    } else {
        $(".actions .active").addClass("inactive");
        $(".actions .active").removeClass("active");
    }
}
//[{"id":"21ac6039-8ee7-4c32-a894-8ac6e2178f01","isFolder":false,"type":1,"elemName":"Компьютерная графика.docx","lastModified":"05.11.2019","size":{"item1":225.4,"item2":1}}]
$("html").on("click", ".file-table tr.drive-row", function () {
    $(this).toggleClass("selected");

    ActionBarLogic();
});
function ShareSelected() {
    var foldersId = [];
    $(".file-table tr.drive-row.selected[data-isfolder=True]").each(function () {
        foldersId.push($(this).attr("data-id"));
    });

    var filesId = [];
    $(".file-table tr.drive-row.selected[data-isfolder=False]").each(function () {
        filesId.push($(this).attr("data-id"));
    });

    $.ajax({
        type: 'POST',
        url: '/Folder/Share',
        data: { currFolderId: $_GET['folderId'], foldersId: foldersId, filesId: filesId, openAccess: true },

        traditional: true,
        success: function (response) {
            var url = "https://localhost:44350/Folder/Watch?folderId=" + response;
            bootbox.alert('Ваша ссылка готова:<br><input value="' + url + '"/>');
            updateTable(true);
        }
    });
}
function DeleteSelected() {
    var count = $(".file-table tr.selected").length;
    if (count == 0) return;
    bootbox.confirm({
        message: "Are you sure?",
        buttons: {
            confirm: {
                label: 'Yes',
            },
            cancel: {
                label: 'No',
            }
        },
        callback: function (result) {
            if (result) {
                var foldersId = [];
                $(".file-table tr.drive-row.selected[data-isfolder=True]").each(function () {
                    foldersId.push($(this).attr("data-id"));
                });

                var filesId = [];
                $(".file-table tr.drive-row.selected[data-isfolder=False]").each(function () {
                    filesId.push($(this).attr("data-id"));
                });

                $.ajax({
                    type: 'POST',
                    url: '/Folder/Delete',
                    data: { foldersId: foldersId, filesId: filesId },

                    traditional: true,
                    success: function (response) {
                        updateTable();

                    }
                });
            }
        }
    });
}
$("html").on("click", ".action-bar .delete.active", function () {
    DeleteSelected();
});
$("html").on("click", ".action-bar .share.active", function () {
    ShareSelected();
});
function DownloadSelected() {
    var foldersId = [];
    $(".file-table tr.drive-row.selected[data-isfolder=True]").each(function () {
        foldersId.push($(this).attr("data-id"));
    });

    var filesId = [];
    $(".file-table tr.drive-row.selected[data-isfolder=False]").each(function () {
        filesId.push($(this).attr("data-id"));
    });

    var request_params = "?currFolderId=" + $_GET['folderId'];
    if (foldersId.length > 0) {
        foldersId.forEach(function (value) {
            request_params += "&foldersId=" + value;
        });
    }
    if (filesId.length > 0) {
        filesId.forEach(function (value) {
            request_params += "&filesId=" + value;
        });
    }
    //alert(location.protocol + '//' + location.host + '/Folder/DownloadMultiple' + request_params);
    window.location.href = location.protocol + '//' + location.host + '/Folder/DownloadMultiple' + request_params;
}
$("html").on("click", ".action-bar .download.active, [data-action=download]", function () {
    var count = $(".file-table tr.selected").length;

    if (count == 1) {
        var isfolder = $(".file-table tr.selected").attr("data-isfolder") === "True";
        var fId = $(".file-table tr.selected").attr("data-id");
        if (isfolder) {
            DownloadSelected();
        } else {
            window.location.href = location.protocol + '//' + location.host + '/Folder/Download?fileId=' + fId;
        }
    } else if (count > 1) {
        DownloadSelected();
    }
});
$("html").on("keydown", ".file-table", function (e) {
    if (e.ctrlKey) {
        if (e.keyCode == 65 || e.keyCode == 97) { // 'A' or 'a'
            e.preventDefault();
            $(".file-table").find(".drive-row").addClass("selected");
            ActionBarLogic();
        }
    }
    if (e.keyCode == 46) {
        //Delete
        DeleteSelected();
    }
});
$("html").on("dblclick", ".file-table tr.drive-row", function () {
    $("html").find(".file-table tr.selected").removeClass("selected");
    $(this).addClass("selected");
    ActionBarLogic();

    var isfolder = $(this).attr("data-isfolder") === "True";
    var type = $(this).attr("data-type");
    var fId = $(this).attr("data-id");
    var filename = $(this).find(".filename").text();
    var ext = filename.split(".")[1];

    if (isfolder) {
        window.location.href = location.protocol + '//' + location.host + location.pathname + '?folderId=' + fId;
    } else {
        var viewURL = location.protocol + '//' + location.host + '/Folder/DisplayFile?fileId=' + fId;
        var appendView = "";

        switch (type) {
            case "Image":
                appendView = '<img src="' + viewURL + '"/>';
                break;
            case "Video":
                appendView = '<video autoplay controls src="' + viewURL + '"></video>';
                break;
            case "Audio":
                appendView = '<audio autoplay controls src="' + viewURL + '"></audio>';
                break;
            default:
                switch (ext) {
                    case "pdf":
                        appendView = '<iframe src="' + viewURL + '"/>';
                        break;
                    default:
                        appendView = '<div class="no-data"><div><h1>No preview available</h1><button data-action="download">Download</button></div></div>';
                        break;
                }
                break;
        }
        $("html").append('<div class="file-view hide"><div class="wrap"><div class="close-button"><i class="material-icons">close</i></div>' + appendView + '</div></div>');
        setTimeout(function () {
            $("html").find(".file-view").removeClass("hide");
        }, 10);
    }
});
$("html").on("click", ".file-view .close-button", function () {
    $("html").find(".file-view").addClass("hide");
    setTimeout(function () {
        $("html").find(".file-view").detach();
    }, 300);
});
//alert(1);