function RenderActions(RenderActionstring) {
    $("#OpenDialog").load(RenderActionstring);
};

(function () {
    if (typeof window.sessionExpiry !== 'undefined' && window.sessionExpiry) {
        var expiry = new Date(window.sessionExpiry);
        var timerDiv = document.getElementById('session-timer');
        var hasRedirected = false;
        var lastActivityTime = Date.now();
        var activityThrottleMs = 30000; // Only send activity update every 30 seconds

        // Activity event handler
        function handleActivity() {
            var now = Date.now();

            // Only send activity update if enough time has passed since last update
            if (now - lastActivityTime > activityThrottleMs) {
                lastActivityTime = now;

                // Send activity ping to server to reset session timer
                fetch('/Account/ResetSessionTimer', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    }
                })
                    .then(response => response.json())
                    .then(data => {
                        if (data.success && data.newExpiry) {
                            // Update expiry time with new value from server
                            expiry = new Date(data.newExpiry);
                            console.log('Session timer reset, new expiry:', expiry);
                        }
                    })
                    .catch(error => {
                        console.log('Activity tracking error:', error);
                    });
            }
        }

        // Listen for user activity events
        document.addEventListener('click', handleActivity);
        document.addEventListener('keypress', handleActivity);
        document.addEventListener('scroll', handleActivity);
        document.addEventListener('mousemove', handleActivity);

        function updateTimer() {
            var now = new Date();
            var diff = expiry - now;
            if (diff <= 0) {
                timerDiv.textContent = "Session expired";

                if (!hasRedirected) {
                    hasRedirected = true;
                    window.location.href = '/Account/LoginDb?timeout=true';
                }
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