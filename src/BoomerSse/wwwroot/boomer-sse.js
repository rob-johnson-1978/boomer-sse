'use strict';

window.bsse = {
    publishClientEvent: (e) => {
        e.preventDefault();
        window.bsse.publishClientEventUsingDataset(e.target.dataset);
    },    
    publishClientEventUsingDataset: (dataset) => {
        const eventName = dataset.bsseEvent;
        if (!eventName) {
            return;
        }

        const eventMessage = dataset.bsseMessage;
        const eventData = dataset.bsseData;

        console.log(eventName, eventMessage, eventData);
        // todo: POST to server
    },
    triggerOnAppears: (root) => {
        const appearElements = root.querySelectorAll('[onappear]');
        console.log(appearElements);

        appearElements.forEach((el) => {
            window.bsse.publishClientEventUsingDataset(el.dataset);
        });
    },
    defaultGetToken: () => ''
};

window.addEventListener("DOMContentLoaded", () => {
    window.bsse.triggerOnAppears(window.document);

    //const url = `${window.BSSE_PATHBASE}/bsse/sub?session_id=${window.BSSE_SESSION_ID}`;

    //const eventSource = new EventSource(url, {
    //    withCredentials: true,
    //    headers: {
    //        'Authorization': `Bearer ${window.BSSE_GET_TOKEN()}`
    //    }
    //});
});