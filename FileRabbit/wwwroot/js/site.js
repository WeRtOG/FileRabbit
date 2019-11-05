// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$('form.addfolder').ajaxForm(function (response) {
    var newtr = "<tr ondblclick=\"window.location.href = '/Folder/Watch?folderId=' + '" + response.id + "'\">";
    console.log(response);
    newtr += '<td class="icon"><i class="material-icons">folder</i></td><td>' + response.elemName + '</td><td>' + response.lastModified + '</td><td>-</td>';
    $(".file-table").append(newtr);
    $("#folderCreate").modal("hide");
});
$('form.upload').ajaxForm(function (response) {
    var newtr = "<tr ondblclick=\"window.location.href = '/Folder/Watch?folderId=' + '" + response.id + "'\">";
    console.log(response);
    //newtr += '<td class="icon"><i class="material-icons">folder</i></td><td>' + response.elemName + '</td><td>' + response.lastModified + '</td><td>-</td>';
    //$(".file-table").append(newtr);
    //$("#folderCreate").modal("hide");
});
//[{"id":"21ac6039-8ee7-4c32-a894-8ac6e2178f01","isFolder":false,"type":1,"elemName":"Компьютерная графика.docx","lastModified":"05.11.2019","size":{"item1":225.4,"item2":1}}]
$(".file-table tr[ondblclick]").click(function () {
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
//alert(1);