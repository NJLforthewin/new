document.addEventListener("DOMContentLoaded", function () {
    var roomSelect = document.getElementById("RoomId");
    var priceInput = document.getElementById("PricePerNight");
    var totalPriceInput = document.getElementById("TotalPrice");
    var checkInInput = document.getElementById("CheckInDate");
    var checkOutInput = document.getElementById("CheckOutDate");

    var roomPrices = JSON.parse(document.getElementById("RoomPricesData").textContent);

    function updatePrice() {
        var selectedRoomId = roomSelect.value;
        if (selectedRoomId && roomPrices[selectedRoomId]) {
            priceInput.value = roomPrices[selectedRoomId];
            calculateTotalPrice();
        } else {
            priceInput.value = "";
            totalPriceInput.value = "";
        }
    }

    function calculateTotalPrice() {
        var checkInDate = new Date(checkInInput.value);
        var checkOutDate = new Date(checkOutInput.value);
        if (checkInDate && checkOutDate && checkOutDate > checkInDate) {
            var totalDays = (checkOutDate - checkInDate) / (1000 * 60 * 60 * 24);
            totalPriceInput.value = (totalDays * parseFloat(priceInput.value)).toFixed(2);
        } else {
            totalPriceInput.value = "";
        }
    }

    roomSelect.addEventListener("change", updatePrice);
    checkInInput.addEventListener("change", calculateTotalPrice);
    checkOutInput.addEventListener("change", calculateTotalPrice);
});
