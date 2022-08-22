"use strict";
var page = require('webpage').create(),
    system = require('system'),
    resources = [],
    tStart, tEnd, address;
tStart = Date.now();
address = system.args[1];
page.settings.resourceTimeout = system.args[2];
if (system.args.length === 3) {
    page.settings.userAgent = system.args[3];
}

page.open(address, function(status) {
    if (status !== 'success')
    {
        console.error('Error: Unable to access network');
    }
    else
    {
        tEnd = Date.now();
        var html = page.evaluate(function() {
            return document.documentElement.outerHTML;
        });
        console.log('---OUTPUT---');
        console.log('ResponseCode:', resources[0].status);
        console.log('RequestStartTime:', tStart);
        console.log('RequestEndTime:', tEnd);
        console.log('---HTML---');
        console.log(html);
    }
    phantom.exit();
});

page.onResourceReceived = function (response) {
    // check if the resource is done downloading 
    if (response.stage !== "end") return;
    // apply resource filter if needed:
    if (response.headers.filter(function (header) {
        if (header.name == 'Content-Type' && header.value.indexOf('text/html') == 0) {
            return true;
        }
        return false;
    }).length > 0)
        resources.push(response);
};
