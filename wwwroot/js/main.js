function RenderActions(RenderActionstring) {
    $("#OpenDialog").load(RenderActionstring);
};

(function () {
    if (typeof window.sessionExpiry !== 'undefined' && window.sessionExpiry) {
        var expiry = new Date(window.sessionExpiry);
        var timerDiv = document.getElementById('session-timer');
        function updateTimer() {
            var now = new Date();
            var diff = expiry - now;
            if (diff <= 0) {
                timerDiv.textContent = "Session expired";
                // Optionally, redirect to logout/login
                // window.location.href = '/Account/Login';
            } else {
                var mins = Math.floor(diff / 60000);
                var secs = Math.floor((diff % 60000) / 1000);
                timerDiv.textContent = "Session expires in " + mins + "m " + secs + "s";
            }
        }
        updateTimer();
        setInterval(updateTimer, 1000);
    }
})();