
function setReturnDateMin() {
    var departDateInput = document.getElementById('depart-date');
    var returnDateInput = document.getElementById('return-date');

    // Get the selected depart date
    var departDate = new Date(departDateInput.value);

    // Set the minimum value of the return date input
    returnDateInput.min = formatDate(departDate);
}
function formatDate(date) {
    var year = date.getFullYear();
    var month = ('0' + (date.getMonth() + 1)).slice(-2);
    var day = ('0' + date.getDate()).slice(-2);
    return year + '-' + month + '-' + day;
}
