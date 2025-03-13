document.addEventListener("DOMContentLoaded", function () {
    let roomSelect = document.getElementById("RoomId");
    let checkIn = document.getElementById("CheckInDate");
    let checkOut = document.getElementById("CheckOutDate");
    let pricePerNightField = document.getElementById("PricePerNight");
    let totalPriceField = document.getElementById("TotalPrice");

    // Load room prices dynamically from ViewBag
    let roomPrices = JSON.parse(document.getElementById("RoomPricesData").textContent);

    function updateRoomPrice() {
        let roomId = roomSelect.value;
        if (roomId && roomPrices[roomId]) {
            pricePerNightField.value = roomPrices[roomId];
        } else {
            pricePerNightField.value = "";
        }
        calculateTotal();
    }

    function calculateTotal() {
        let pricePerNight = parseFloat(pricePerNightField.value);
        let startDate = new Date(checkIn.value);
        let endDate = new Date(checkOut.value);

        if (!isNaN(pricePerNight) && checkIn.value && checkOut.value) {
            let days = (endDate - startDate) / (1000 * 60 * 60 * 24);
            totalPriceField.value = days > 0 ? (days * pricePerNight).toFixed(2) : "0.00";
        } else {
            totalPriceField.value = "";
        }
    }

    roomSelect.addEventListener("change", updateRoomPrice);
    checkIn.addEventListener("change", calculateTotal);
    checkOut.addEventListener("change", calculateTotal);
});
