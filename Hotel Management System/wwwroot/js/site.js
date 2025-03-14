document.addEventListener("DOMContentLoaded", function () {
    var roomSelect = document.getElementById("RoomId");
    var priceInput = document.getElementById("PricePerNight");
    var totalPriceInput = document.getElementById("TotalPrice");
    var checkInInput = document.getElementById("CheckInDate");
    var checkOutInput = document.getElementById("CheckOutDate");
    var bookingForm = document.getElementById("bookingForm");

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

 
    bookingForm.addEventListener("submit", function (event) {
        event.preventDefault(); 

        var totalAmount = parseFloat(totalPriceInput.value);
        if (!totalAmount || totalAmount <= 0) {
            alert("Invalid booking details. Please check your dates and room selection.");
            return;
        }

        fetch("/Payment/CreatePaymentIntent", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ amount: totalAmount })
        })
            .then(response => response.json())
            .then(data => {
                if (data.paymentUrl) {
                    window.location.href = data.paymentUrl; 
                } else {
                    alert("Payment failed. Please try again.");
                }
            })
            .catch(error => {
                console.error("Payment error:", error);
                alert("An error occurred during payment processing.");
            });
    });
});
