'use strict';

const bsse = {

    connected: false,
    maxWaitTime: 5000,
    checkInterval: 100,

    publishClientEvent: (e) => {
        e.preventDefault();
        bsse.publishClientEventUsingDataset(e.target.dataset);
    },
    publishClientEventUsingDataset: async (dataset) => {
        let waitedTime = 0;

        while (!bsse.connected && bsse.waitedTime < bsse.maxWaitTime) {
            await new Promise(resolve => setTimeout(resolve, checkInterval));
            waitedTime += checkInterval;
        }

        if (!bsse.connected) {
            console.error('EventSource not connected after timeout. Page will not work.');
            return;
        }

        const eventName = dataset.bsseEvent;
        if (!eventName) {
            return;
        }

        const eventMessage = dataset.bsseMessage;
        const eventData = dataset.bsseData;

        try {
            await fetch(`${BSSE_PATH_BASE}/bsse/pub?session_id=${BSSE_SESSION_ID}`, {
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
            });
        }
        catch (error) {
            console.error('Error publishing client event: ', error);
        }
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

        bsse.connected = true;

        console.log("Connected to EventSource at ", url);
    },
    defaultGetToken: () => ''
};

addEventListener("DOMContentLoaded", () => {
    bsse.connectToEventSource();
    bsse.triggerOnAppears(document);
});