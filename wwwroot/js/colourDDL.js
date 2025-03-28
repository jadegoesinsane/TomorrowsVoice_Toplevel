function formatColours(colour) {
    if (!colour.id) {
        return colour.text;
    }
    var $colour = $(`<span><span class="dot form-check-input ` + colour.text.replace(/\s+/g, '').toLowerCase() + `"></span> ` + colour.text + `</span>`);
    return $colour;
};

$(document).ready(function () {
    $('.bg-select').select2({
        templateResult: formatColours,
        minimumResultsForSearch: Infinity
    });
    $('.select2-container').addClass('form-control');
});