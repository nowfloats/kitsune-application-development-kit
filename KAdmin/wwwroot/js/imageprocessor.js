var caman = null;
var jcrop_api = null;

var rotation = 0;

function applyFilters() {
    caman.revert(false);

    $('.slider').each(function () {
        var op = $(this).attr('id');
        var value = $(this).data('val');

        if (value === 0) {
            return;
        }

        caman[op](value);
    });
}

function resetFilters() {
    $('.slider').each(function () {
        var op = $(this).attr('id');

        $('#' + op).slider('option', 'value', $(this).attr('data-val'));
    });
}

$('.slider').each(function () {
    var op = $(this).attr('id');

    $('#' + op).slider({
        min: $(this).data('min'),
        max: $(this).data('max'),
        val: $(this).data('val'),
        change: function (e, ui) {
            $('#v-' + op).html(ui.value);
            $(this).data('val', ui.value);

            if (e.originalEvent === undefined) {
                return;
            }

            applyFilters();
            caman.render();
        }
    });
});

$('#rotate-cw').click(function () {
    rotation += 90;
    caman.rotate(90);
    applyFilters();
    caman.render();
});

$('#rotate-ccw').click(function () {
    rotation -= 90;
    caman.rotate(-90);
    applyFilters();
    caman.render();
});

$('#resize').click(function () {
    caman.resize({
        width: $('#resize-w').val(),
        height: $('#resize-h').val()
    });
    applyFilters();
    caman.render();
});

$('#crop').click(function () {
    caman.crop(
        $('#crop-w').val(),
        $('#crop-h').val(),
        $('#crop-x').val(),
        $('#crop-y').val()
    );
    applyFilters();
    caman.render();
});

$('.preset').click(function () {
    imageRenderingLoader(true);
    resetFilters();
    var preset = $(this).data('preset');
    caman.revert(true);
    caman[preset]();
    caman.render();
});

$('#resetcanvas').click(function () {

    // sets resize height and width to zero
    $('#newWidth').val(0);
    $('#newHeight').val(0);


    removeCropper();
    jcrop_api = null;
    caman.reset();
    imageRenderingLoader(true);
    caman.render();
    resetFilters();
});

function imageRenderingLoader(show) {
    var imageContainerForFilters = document.querySelector('#image-processor-container');
    if (show) {
        showLoader(imageContainerForFilters, true);
    } else {
        removeLoader(null, true);
    }
}
Caman.Event.listen("renderFinished", function () {

    var canvas = document.getElementById('imgz');

    imageRenderingLoader(false);
});


// cropping function
function processImageForCropping() {
    var $canvas = $('#imgz');
    jcrop_api = $.Jcrop($canvas, {
        allowSelect: true,
        allowMove: true,
        allowResize: true
    });
}

function removeCropper() {
    if (jcrop_api) {
        jcrop_api.release();
        jcrop_api.disable();
    }
}

function updateCropCordinatesInCaman() {
    if (jcrop_api) {
        var imgz = document.getElementById('imgz');
        var cordinates = jcrop_api.tellScaled();
        var scaleRatioH = 0;
        var scaleRatioW = 0;

        var { height, width, clientHeight, clientWidth } = imgz;
        var { w, h, x, y } = cordinates;

        if (height > clientHeight || width > clientWidth) {
            // find how much image has scaled across height and width and calculate new co-ordinates
            scaleRatioH = (height / clientHeight);
            scaleRatioW = (width / clientWidth);

            w = parseInt(w * scaleRatioW);
            h = parseInt(h * scaleRatioH);
            x = parseInt(x * scaleRatioW);
            y = parseInt(y * scaleRatioH);

        }

        if (w && h && x && y) {
            caman.crop(
                w,
                h,
                x,
                y);
            caman.render()
        }
        removeCropper();
    }
}

// resizing functions

function resizeImage() {
    let width = parseInt(document.getElementById('newWidth').value);
    let height = parseInt(document.getElementById('newHeight').value);
    if (height && width) {
        imageRenderingLoader(true);
        caman.resize({
            width: width,
            height: height
        });
        caman.render();
    }
    // add toastr for error
}