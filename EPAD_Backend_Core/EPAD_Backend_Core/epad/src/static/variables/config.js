(function (window) {
    window.__env = window.__env || {};
    window.__env.api_host = '';
    window.__env.api_endpoint = '';
    window.__env.enableDebug = false;
    window.__env.updateUI = true;
    window.__env.uiName = 'DarkOcean';

    var req = new XMLHttpRequest();
    req.onreadystatechange = function() {
    if (this.readyState == 4 && this.status == 200) {
        var appHost = JSON.parse(this.responseText);
        window.__env.api_host = appHost.Domain;
        window.__env.api_endpoint = appHost.ApiEndpoint;
        window.__env.push_notification_url = appHost.PushNotificationUrl;
        window.__env.ezHrUrl = appHost.ezHrUrl;
    }
  };
  req.open("GET", "static/variables/app.host.json", false);
  req.send();
}(this));
