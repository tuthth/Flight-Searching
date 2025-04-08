
function checkLocations() {
    // Get the selected values of 'from' and 'to' select elements
    var fromValue = document.getElementById('from').value;
    var toValue = document.getElementById('to').value;

    // Check if both selected values are the same
    if (fromValue === toValue) {
        alert("The 'from' and 'to' locations cannot be the same.");
        document.getElementById('to').value = "";
    }
}