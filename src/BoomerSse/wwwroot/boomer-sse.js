'use strict';

const bsse = {
    publishClientEvent: (e) => {
        e.preventDefault();
        bsse.publishClientEventUsingDataset(e.target.dataset);
    },
    publishClientEventUsingDataset: (dataset) => {
        const eventName = dataset.bsseEvent;
        if (!eventName) {
            return;
        }

        const eventMessage = dataset.bsseMessage;
        const eventData = dataset.bsseData;

        fetch('/bsse/pub?session_id=' + BSSE_SESSION_ID, {
            method: 'POST',
            withCredentials: true,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${BSSE_GET_TOKEN()}`
            },
            body: JSON.stringify({
                event: eventName,
                message: eventMessage,
                data: eventData
            })
        })
        .catch(error => {
            console.error('Error publishing client event: ', error);
        });
    },
    triggerOnAppears: (root) => {
        const appearElements = root.querySelectorAll('[onappear]');        

        appearElements.forEach((el) => {
            bsse.publishClientEventUsingDataset(el.dataset);
        });
    },
    connectToEventSource: () => {
        const url = `${BSSE_PATH_BASE}/bsse/sub?session_id=${BSSE_SESSION_ID}`;

        const eventSource = new EventSource(url, {
            withCredentials: true,
            headers: {
                'Authorization': `Bearer ${BSSE_GET_TOKEN()}`
            }
        });

        eventSource.onmessage = (message) => {
            console.log("Received message: ", message);
        };
    },
    defaultGetToken: () => ''
};

addEventListener("DOMContentLoaded", () => {
    bsse.connectToEventSource();
    bsse.triggerOnAppears(document);
});