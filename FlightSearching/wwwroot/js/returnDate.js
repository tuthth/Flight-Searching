function enableReturnDate() {
    document.getElementById("return-date").disabled = false;
    setValue();
}
function disableReturnDate() {
    document.getElementById("return-date").disabled = true;
    setValue();
}
function setValue() {
    document.getElementById("return-date").value = null;
}