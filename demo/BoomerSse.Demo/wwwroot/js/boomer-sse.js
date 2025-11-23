'use strict';

const buildEventData = (el, propName, data) => {
    const dataSetValue = el.dataset[propName];

    if (!dataSetValue) {
        console.warn(`No dataset value for ${propName}`);
        return undefined;
    }

    const splitValues = dataSetValue.split('|');

    if (!splitValues || splitValues.length < 1 || !splitValues[0] || splitValues[0].length < 1) {
        console.warn(`Dataset value is invalid: ${dataSetValue}`);
        return undefined;
    }

    if (!data) {
        data = splitValues.length > 1
            ? splitValues[1]
            : null;
    }

    return {
        event: splitValues[0],
        data
    };
};

const raiseEvent = (eventData) => {
    if (!eventData) {
        return;
    }

    const url = `${window.BSSE_PATHBASE}/bsse/sub?session_id=${window.BSSE_SESSION_ID}`;

    fetch(url, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${window.BSSE_GET_TOKEN()}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(eventData),
        credentials: 'include'
    })
        .catch(error => {
            console.error(`Error posting`)
        });
};

const bindOnReady = (el) => {
    raiseEvent(
        buildEventData(el, "bsseOnready")
    );
};

const bindOnClick = (el) => {
    el.addEventListener("click", e => {
        e.preventDefault();

        raiseEvent(
            buildEventData(el, "bsseOnclick")
        );
    });

};

const bindOnSubmit = (el) => {
    el.addEventListener("submit", e => {
        e.preventDefault();

        const formData = new FormData(el);
        const formObject = {};

        formData.forEach((value, key) => {
            if (formObject.hasOwnProperty(key)) {
                if (!Array.isArray(formObject[key])) {
                    formObject[key] = [formObject[key]];
                }
                formObject[key].push(value);
            } else {
                formObject[key] = value;
            }
        });

        raiseEvent(
            buildEventData(el, "bsseOnsubmit", JSON.stringify(formObject))
        );
    });
};

const bindOnLoad = (el) => {
    el.addEventListener("load", e => {
        e.preventDefault();

        raiseEvent(
            buildEventData(el, "bsseOnload")
        );
    });
};

const bindPublishers = topLevelElement => {
    const onReadyElements = topLevelElement.querySelectorAll("[data-bsse-onready]");
    const onClickElements = topLevelElement.querySelectorAll("[data-bsse-onclick]");
    const onSubmitElements = topLevelElement.querySelectorAll("form[data-bsse-onsubmit]");
    const onLoadElements = topLevelElement.querySelectorAll("[data-bsse-onload]");

    if (topLevelElement.hasAttribute) {
        if (topLevelElement.hasAttribute("data-bsse-ready")) {
            onReadyElements.push(topLevelElement);
        }

        if (topLevelElement.hasAttribute("data-bsse-click")) {
            onClickElements.push(topLevelElement);
        }

        if (topLevelElement.hasAttribute("data-bsse-submit")) {
            onSubmitElements.push(topLevelElement);
        }

        if (topLevelElement.hasAttribute("data-bsse-onload")) {
            onLoadElements.push(topLevelElement);
        }
    }

    onReadyElements.forEach(bindOnReady);
    onClickElements.forEach(bindOnClick);
    onSubmitElements.forEach(bindOnSubmit);
    onLoadElements.forEach(bindOnLoad);
}

const openSseConnection = () => {
    const url = `${window.BSSE_PATHBASE}/bsse/sub?session_id=${window.BSSE_SESSION_ID}`;

    const eventSource = new EventSource(url, {
        withCredentials: true,
        headers: {
            'Authorization': `Bearer ${window.BSSE_GET_TOKEN()}`
        }
    });
}

const init = () => {
    window.BSSE_PATHBASE = window.BSSE_PATHBASE || '';
    window.BSSE_SESSION_ID = crypto.randomUUID();

    if (!window.BSSE_GET_TOKEN) {
        window.BSSE_GET_TOKEN = () => "";
    }

    bindPublishers(document);
    openSseConnection();
};

window.addEventListener("DOMContentLoaded", () => {
    init();
});
