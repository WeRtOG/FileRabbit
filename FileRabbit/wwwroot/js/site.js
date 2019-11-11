// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function sortTable(n, table) {
    var rows, switching, i, x, y, shouldSwitch, dir, switchcount = 0;
    switching = true;
    //Set the sorting direction to ascending:
    dir = "asc";
    /*Make a loop that will continue until
    no switching has been done:*/
    while (switching) {
        //start by saying: no switching is done:
        switching = false;
        rows = table.rows;
        /*Loop through all table rows (except the
        first, which contains table headers):*/
        for (i = 1; i < (rows.length - 1); i++) {
            //start by saying there should be no switching:
            shouldSwitch = false;
            /*Get the two elements you want to compare,
            one from current row and one from the next:*/
            x = rows[i].getElementsByTagName("TD")[n];
            y = rows[i + 1].getElementsByTagName("TD")[n];
            /*check if the two rows should switch place,
            based on the direction, asc or desc:*/
            if (dir == "asc") {
                if (x.innerHTML.toLowerCase() > y.innerHTML.toLowerCase()) {
                    //if so, mark as a switch and break the loop:
                    shouldSwitch = true;
                    break;
                }
            } else if (dir == "desc") {
                if (x.innerHTML.toLowerCase() < y.innerHTML.toLowerCase()) {
                    //if so, mark as a switch and break the loop:
                    shouldSwitch = true;
                    break;
                }
            }
        }
        if (shouldSwitch) {
            /*If a switch has been marked, make the switch
            and mark that a switch has been done:*/
            rows[i].parentNode.insertBefore(rows[i + 1], rows[i]);
            switching = true;
            //Each time a switch is done, increase this count by 1:
            switchcount++;
        } else {
            /*If no switching has been done AND the direction is "asc",
            set the direction to "desc" and run the while loop again.*/
            if (switchcount == 0 && dir == "asc") {
                dir = "asc";
                switching = true;
            }
        }
    }
    var folders = [];
    $(table).find("tr").each(function (i) {
        var isfolder = $(this).attr("data-isfolder") === "True" || $(this).attr("data-isfolder") === undefined;

        if (isfolder) {
            folders.push($(this));
        }
    });
    $(table).prepend(folders);
    console.log(folders);
}

var sizeTypes = [
    "B",
    "KB",
    "MB",
    "GB",
    "TB"
];
var iconTypes = [
    "insert_drive_file",
    "insert_drive_file",
    "image",
    "movie",
    "music_note",
    "folder",
    "insert_drive_file"
];
function AddFileRow(element) {
    var newtr = '<tr class="drive-row" data-isfolder="False" data-id="' + element.id + '"><td class="icon"><i class="material-icons">' + iconTypes[element.type] + '</i></td><td>' + element.elemName + '</td><td>' + element.lastModified + '</td><td>' + element.size.item1 + ' ' + sizeTypes[element.size.item2] + '</td></tr>';
    $(".file-table").append(newtr);
    sortTable(1, $(".file-table")[0]);
}
function AddFolderRow(element) {
    var newtr = '<tr class="drive-row" data-isfolder="True" data-id="' + element.id + '">';
    newtr += '<td class="icon"><i class="material-icons">folder</i></td><td>' + element.elemName + '</td><td>' + element.lastModified + '</td><td>-</td>';
    $(".file-table").append(newtr);
    sortTable(1, $("body").find(".file-table")[0]);
}
$(function () {
    sortTable(1, $("body").find(".file-table")[0]);
});
function Upload(files, folderId, token) {
    const formData = new FormData();

    for (let i = 0; i < files.length; i++) {
        let file = files[i];

        formData.append('uploads', file);
    }
    formData.append("folderId", folderId);
    formData.append("__RequestVerificationToken", token);

    fetch("Upload", {
        method: 'POST',
        body: formData,
    }).then(response => {
        response.json().then(function (data) {
            data.forEach(function (element) {
                AddFileRow(element);
            });
        });
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
    AddFolderRow(response);
    $("#folderCreate").modal("hide");
});
$('form.upload').ajaxForm(function (response) {
    response.forEach(function (element) {
        AddFileRow(element);
    });
    
    $("#fileUpload").modal("hide");
});
//[{"id":"21ac6039-8ee7-4c32-a894-8ac6e2178f01","isFolder":false,"type":1,"elemName":"Компьютерная графика.docx","lastModified":"05.11.2019","size":{"item1":225.4,"item2":1}}]
$("html").on("click", ".file-table tr.drive-row", function () {
    $(this).toggleClass("selected");

    var count = $(".file-table tr.selected").length;
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
});
$("html").on("dblclick", ".file-table tr.drive-row", function () {
    var isfolder = $(this).attr("data-isfolder") === "True";
    var fId = $(this).attr("data-id");
    if (isfolder) {
        window.location.href = location.protocol + '//' + location.host + location.pathname + '?folderId=' + fId;
    } else {
        window.location.href = location.protocol + '//' + location.host + '/Folder/Download?fileId=' + fId;
    }
});
//alert(1);