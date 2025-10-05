// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", function () {
    var modal = document.getElementById("confirmDeleteModal");
    var form = document.getElementById("deleteForm");
    if (!modal || !form) return;

    modal.addEventListener("show.bs.modal", function (event) {
        var trigger = event.relatedTarget;
        var actionUrl = trigger?.getAttribute("data-action");
        if (actionUrl) form.setAttribute("action", actionUrl);
    });
});