'use strict';

window.addEventListener("DOMContentLoaded", () => {
    //const url = `${window.BSSE_PATHBASE}/bsse/sub?session_id=${window.BSSE_SESSION_ID}`;

    //const eventSource = new EventSource(url, {
    //    withCredentials: true,
    //    headers: {
    //        'Authorization': `Bearer ${window.BSSE_GET_TOKEN()}`
    //    }
    //});
});

window.publishClientEvent = (e) => {
    e.preventDefault();
};

window.defaultGetToken = () => '';