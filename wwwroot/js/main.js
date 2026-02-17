function RenderActions(RenderActionstring) {
    $("#OpenDialog").load(RenderActionstring);
};

// Global activity tracking - works on ALL pages
(function () {
    var lastActivityTime = 0; // Start at 0 so first activity fires immediately
    var activityThrottleMs = 30000; // Send activity update every 30 seconds

    // Activity event handler
    function handleActivity() {
        var now = Date.now();

        // Only send activity update if enough time has passed since last update
        if (now - lastActivityTime > activityThrottleMs) {
            console.log('Activity detected, resetting session timer...');

            // Send activity ping to server to reset session timer
            fetch('/Account/ResetSessionTimer', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                }
            })
                .then(response => response.json())
                .then(data => {
                    console.log('Session reset response:', data);
                    if (data.success && data.newExpiry) {
                        // IMPORTANT: Reset the lastActivityTime AFTER successful reset
                        lastActivityTime = Date.now();

                        // Update the global expiry time
                        window.sessionExpiry = data.newExpiry;
                        console.log('Session timer reset, new expiry:', data.newExpiry);

                        // Trigger custom event to update timer display if it exists
                        window.dispatchEvent(new CustomEvent('sessionReset', { detail: { newExpiry: data.newExpiry } }));
                    } else {
                        console.log('Session reset failed or no active session');
                    }
                })
                .catch(error => {
                    console.log('Activity tracking error:', error);
                    // Don't update lastActivityTime on error, allow retry sooner
                });
        } else {
            var secondsUntilNext = Math.ceil((activityThrottleMs - (now - lastActivityTime)) / 1000);
            console.log('Activity throttled, next reset available in ' + secondsUntilNext + ' seconds');
        }
    }

    // Listen for user activity events on ENTIRE document
    document.addEventListener('click', handleActivity);
    document.addEventListener('keypress', handleActivity);
    document.addEventListener('scroll', handleActivity);
    document.addEventListener('mousemove', handleActivity);
})();

// Timer display - only runs if session exists
(function () {
    if (typeof window.sessionExpiry !== 'undefined' && window.sessionExpiry) {
        var expiry = new Date(window.sessionExpiry);
        var timerDiv = document.getElementById('session-timer');
        var hasRedirected = false;

        console.log('Timer initialized, expiry:', expiry);

        // Listen for session reset events
        window.addEventListener('sessionReset', function (event) {
            expiry = new Date(event.detail.newExpiry);
            console.log('Timer display updated with new expiry:', expiry);
        });

        function updateTimer() {
            if (!timerDiv) return; // Safety check

            var now = new Date();
            var diff = expiry - now;

            if (diff <= 0) {
                timerDiv.textContent = "Session expired";

                if (!hasRedirected) {
                    hasRedirected = true;
                    console.log('Session expired, redirecting to login...');
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