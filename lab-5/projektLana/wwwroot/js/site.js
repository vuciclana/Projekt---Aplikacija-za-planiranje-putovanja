// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Initialize Flatpickr for all datetime pickers
document.addEventListener('DOMContentLoaded', function() {
    if (window.jQuery && window.jQuery.validator) {
        window.jQuery.validator.methods.number = function(value, element) {
            return this.optional(element) || /^-?\d+(?:[.,]\d+)?$/.test(value.trim());
        };

        window.jQuery.validator.methods.range = function(value, element, param) {
            const normalizedValue = Number(value.replace(',', '.'));
            return this.optional(element) || (normalizedValue >= param[0] && normalizedValue <= param[1]);
        };
    }

    const userAutocomplete = document.querySelectorAll('.user-autocomplete');

    userAutocomplete.forEach(function(wrapper) {
        const input = wrapper.querySelector('.user-autocomplete-input');
        const hidden = wrapper.querySelector('.user-autocomplete-value');
        const results = wrapper.querySelector('.user-autocomplete-results');
        const url = wrapper.getAttribute('data-autocomplete-url');
        let debounceId = null;

        if (!input || !hidden || !results || !url) {
            return;
        }

        function clearResults() {
            results.innerHTML = '';
            results.classList.remove('show');
        }

        function renderResults(items) {
            results.innerHTML = '';

            if (!items || items.length === 0) {
                clearResults();
                return;
            }

            items.forEach(function(item) {
                const button = document.createElement('button');
                button.type = 'button';
                button.className = 'list-group-item list-group-item-action user-autocomplete-item';
                button.textContent = item.text;
                button.dataset.userId = item.id;
                button.dataset.userText = item.text;
                results.appendChild(button);
            });

            results.classList.add('show');
        }

        function fetchUsers(term) {
            const requestUrl = url + '?term=' + encodeURIComponent(term);
            fetch(requestUrl, { headers: { 'Accept': 'application/json' } })
                .then(function(response) { return response.ok ? response.json() : []; })
                .then(function(data) { renderResults(data); })
                .catch(function() { clearResults(); });
        }

        input.addEventListener('input', function() {
            const term = input.value.trim();
            hidden.value = '';
            if (debounceId) {
                clearTimeout(debounceId);
            }
            if (term.length < 2) {
                clearResults();
                return;
            }
            debounceId = setTimeout(function() { fetchUsers(term); }, 250);
        });

        results.addEventListener('click', function(event) {
            const target = event.target;
            if (!target || !target.dataset.userId) {
                return;
            }
            input.value = target.dataset.userText;
            hidden.value = target.dataset.userId;
            hidden.dispatchEvent(new Event('change', { bubbles: true }));
            clearResults();

            if (window.jQuery && window.jQuery.validator) {
                window.jQuery(hidden).valid();
            }
        });

        document.addEventListener('click', function(event) {
            if (!wrapper.contains(event.target)) {
                clearResults();
            }
        });
    });

    const tripRangeForms = document.querySelectorAll('form[data-trip-range="true"]');

    tripRangeForms.forEach(function(form) {
        const destinationInput = form.querySelector('.user-autocomplete-value');
        const inputsAttr = form.getAttribute('data-trip-range-inputs') || '';
        const url = form.getAttribute('data-trip-range-url');
        const fieldNames = inputsAttr.split(',').map(function(value) { return value.trim(); }).filter(Boolean);
        const fieldInputs = fieldNames
            .map(function(name) { return form.querySelector('[name="' + name + '"]'); })
            .filter(Boolean);

        if (!destinationInput || !url || fieldInputs.length === 0) {
            return;
        }

        let tripStart = null;
        let tripEnd = null;

        function formatTripDate(date) {
            if (!date) {
                return '';
            }
            return date.toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: 'numeric' });
        }

        function parsePickerValue(value) {
            if (!value) {
                return null;
            }
            const parsed = new Date(value.replace(' ', 'T'));
            return Number.isNaN(parsed.getTime()) ? null : parsed;
        }

        function setValidationMessage(input, message) {
            const span = form.querySelector('[data-valmsg-for="' + input.name + '"]');
            if (!span) {
                return;
            }

            if (message) {
                input.dataset.tripRangeError = message;
            } else {
                input.dataset.tripRangeError = '';
            }

            const activeMessage = input.dataset.sequenceError || input.dataset.tripRangeError || '';
            input.classList.toggle('is-invalid', Boolean(activeMessage));
            span.textContent = activeMessage;
        }

        function validateTripRange() {
            let isValid = true;

            fieldInputs.forEach(function(input) {
                if (!tripStart || !tripEnd) {
                    setValidationMessage(input, '');
                    return;
                }

                const value = parsePickerValue(input.value);
                if (!value) {
                    setValidationMessage(input, '');
                    return;
                }

                if (value < tripStart || value > tripEnd) {
                    const rangeText = formatTripDate(tripStart) + ' - ' + formatTripDate(tripEnd);
                    setValidationMessage(input, 'Date must be within the trip date range (' + rangeText + ').');
                    isValid = false;
                } else {
                    setValidationMessage(input, '');
                }
            });

            return isValid;
        }

        function fetchTripRange(destinationId) {
            if (!destinationId) {
                tripStart = null;
                tripEnd = null;
                validateTripRange();
                return;
            }

            const requestUrl = url + '?id=' + encodeURIComponent(destinationId);
            fetch(requestUrl, { headers: { 'Accept': 'application/json' } })
                .then(function(response) { return response.ok ? response.json() : null; })
                .then(function(data) {
                    if (!data || !data.start || !data.end) {
                        tripStart = null;
                        tripEnd = null;
                        return;
                    }

                    tripStart = new Date(data.start);
                    tripEnd = new Date(data.end);
                    validateTripRange();
                })
                .catch(function() {
                    tripStart = null;
                    tripEnd = null;
                });
        }

        form.addEventListener('submit', function(event) {
            if (!validateTripRange()) {
                event.preventDefault();
            }
        });

        fieldInputs.forEach(function(input) {
            input.addEventListener('blur', validateTripRange);
            input.addEventListener('change', validateTripRange);
        });

        destinationInput.addEventListener('change', function() {
            fetchTripRange(destinationInput.value);
        });

        if (destinationInput.value) {
            fetchTripRange(destinationInput.value);
        }
    });

    const tripSearchInputs = document.querySelectorAll('[data-ajax-search="true"]');

    tripSearchInputs.forEach(function(input) {
        const url = input.getAttribute('data-search-url');
        const targetSelector = input.getAttribute('data-search-target');
        const target = targetSelector ? document.querySelector(targetSelector) : null;
        let debounceId = null;

        if (!url || !target) {
            return;
        }

        function runSearch(term) {
            const separator = url.includes('?') ? '&' : '?';
            const requestUrl = url + separator + 'term=' + encodeURIComponent(term);
            fetch(requestUrl, { headers: { 'Accept': 'text/html' } })
                .then(function(response) { return response.ok ? response.text() : ''; })
                .then(function(markup) {
                    if (markup) {
                        target.innerHTML = markup;
                    }
                })
                .catch(function() { });
        }

        input.addEventListener('input', function() {
            const term = input.value.trim();
            if (debounceId) {
                clearTimeout(debounceId);
            }
            debounceId = setTimeout(function() { runSearch(term); }, 250);
        });
    });

    if (typeof flatpickr !== 'undefined') {
        const datetimePickers = document.querySelectorAll('.datetimepicker');
        const prefersCroatian = navigator.language && navigator.language.toLowerCase().startsWith('hr');
        const locale = prefersCroatian && flatpickr.l10ns && flatpickr.l10ns.hr
            ? flatpickr.l10ns.hr
            : 'default';

        datetimePickers.forEach(function(picker) {
            if (picker && !picker._flatpickrInitialized) {
                picker._flatpickrInitialized = true;

                flatpickr(picker, {
                    enableTime: true,
                    dateFormat: 'Y-m-d H:i',
                    time_24hr: true,
                    minuteIncrement: 1,
                    locale: locale
                });
            }
        });
    }
});

