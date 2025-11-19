'use strict';

const buildEventData = (el, propName, data) => {
    const dataSetValue = el.dataset[propName];

    if (!dataSetValue) {
        console.warn(`No dataset value for ${propName}`);
        return undefined;
    }

    const splitValues = dataSetValue.split('|');

    if (!splitValues || splitValues.length < 1 || !splitValues[0] || splitValues[0].length < 1) {
        console.warn(`Dataset value is invalid ${dataSetValue}`);
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
    console.log(eventData);
    // todo: POST data to endpoint configured via middleware
};

const bindOnLoad = (el) => {
    el.addEventListener("load", e => {
        e.preventDefault();

        raiseEvent(
            buildEventData(el, "bsseOnload")
        );
    });
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
            buildEventData(el, "bsseOnsubmit", formObject)
        );
    });
};

const init = () => {
    window.BSSE_PATHBASE = window.BSSE_PATHBASE || '';
    window.BSSE_SESSION_ID = crypto.randomUUID();

    const onLoadElements = document.querySelectorAll("[data-bsse-onload]");
    const onClickElements = document.querySelectorAll("[data-bsse-onclick]");
    const onSubmitElements = document.querySelectorAll("form[data-bsse-onsubmit]");

    onLoadElements.forEach(bindOnLoad);
    onClickElements.forEach(bindOnClick);
    onSubmitElements.forEach(bindOnSubmit);
};

window.addEventListener("DOMContentLoaded", () => {
    init();
});
