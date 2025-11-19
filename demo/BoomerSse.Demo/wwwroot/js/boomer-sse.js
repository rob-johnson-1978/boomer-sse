'use strict';

const getEventData = (el, propName) => {
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

    const data = splitValues.length > 1
        ? splitValues[1]
        : null;

    return {
        event: splitValues[0],
        data
    };
};

const bindOnLoad = (el) => {
    const eventData = getEventData(el, "bsseOnload");
    console.log(eventData);
};

const bindOnClick = (el) => {
    const eventData = getEventData(el, "bsseOnclick");
    console.log(eventData);
};

const bindOnSubmit = (el) => {
    const eventData = getEventData(el, "bsseOnsubmit");
    console.log(eventData);
};

const init = () => {

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
