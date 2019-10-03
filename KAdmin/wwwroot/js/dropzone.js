Dropzone.options.myAwesomeDropzone = false;
Dropzone.autoDiscover = false;

var uploadModalCloseBtn = document.getElementById('closeUploadModal');
var uploadModalOpenBtn = document.getElementById('uploadFileInit');

var imageProcessrorCloseBtn = document.getElementById('closeImageProcessor');
var imageProcessrorOpenBtn = document.getElementById('openImageProcessor');

var myDropzone = new Dropzone("#drop", {
    url: isWebaction ? WebactionsEndpoints.uploadFile : Endpoints.UploadImageForSchema,
    previewTemplate: document.querySelector('#previewTemplate').innerHTML,
    autoProcessQueue: false,
    previewsContainer: '#drpz-data',
    //clickable: true,
    //timeout: 300,
    maxFiles: 1,
    acceptedFiles: 'image/*',
    maxFilesize: 2
});

myDropzone.on("addedfile", function (file) {
    let drop = document.getElementById('drop');
    let placeholder = document.getElementById('image-placeholder');
    drop.style.display = 'none';
    placeholder.style.display = 'flex';
    vm.setIsImageForUploading(file.type.startsWith('image'));

    // even when multiple files selection is diabled dropzone allow muliple file drag and drop
    // hence needed to be removed manually
    if (myDropzone.files.length > myDropzone.options.maxFiles) {
        myDropzone.removeFile(file);
        return false;
    }

});

myDropzone.on("success", function (file, response) {
    try {
        let link = response.replace(/\"/g, "");

        if (link && link.length <= 0) {
            new Error("link not valid");
        }

        myDropzone.removeFile(file);

        let placeholder = document.getElementById('image-placeholder');
        let drop = document.getElementById('drop');
        let uploadModalCloseBtn = document.getElementById('closeUploadModal');

        uploadModalCloseBtn.click();

        placeholder.style.display = 'none';
        drop.style.display = 'flex';
        vm.showLoader(true);
        imageProcessrorCloseBtn.click();

        myDropzone.removeAllFiles();
        toastr.success('image upload successfull', 'success');
        vm.updateLinkOfImgaeObject(link);
    }
    catch (err) {
        vm.setIsErrorInUploading(true);
        toastr.error("Error", "error while uploading image.");
        vm.showLoader(false);
    }
    
});

myDropzone.on("error", function (file, errorMessage, error) {
    console.log(file, errorMessage, error);
    vm.setIsErrorInUploading(true);
    toastr.error("Error", errorMessage);
})

myDropzone.on("canceled", function (file, error) {
    console.log('-------------');
    console.log(file);
    console.log(error);
})


// always executed wheather ajax was successfull or not
myDropzone.on('completed', function (file) {
    vm.showLoader(false);
});

function processFile() {
    uploadModalCloseBtn.click();

    var file = myDropzone.getQueuedFiles()[0];
    var imageContainer = document.getElementById('img-container');
    var image = document.createElement('img');

    // need to clear it every time else canvas of previous image will cause issues
    imageContainer.innerHTML = "";
    imageContainer.appendChild(image);

    image.id = 'imgz';
    image.src = file.dataURL;

    caman = null;
    caman = Caman('#imgz')
    vm.showLoader(true);

    setTimeout(function () {
        vm.showLoader(false);
        imageProcessrorOpenBtn.click();
    }, 1000)

}

function uploadFileAfterModifications() {
    vm.showLoader(true);
    var file = myDropzone.getAcceptedFiles()[0];
    var base64string = caman.toBase64();
    var newFile = dataURLtoFile(base64string, file.name);

    // uplad needs to be added as dropzone requires it for uploading
    newFile.upload = file.upload;
    newFile.upload.total = newFile.size;

    // uploads the new processed file
    if (isWebaction) {
        myDropzone.options.url = myDropzone.options.url.replace("{name}", vm.currentWebactionName);
    }
    myDropzone.processFile(newFile);
}

// returns File Object from the dataUrl
function dataURLtoFile(dataurl, filename) {
    var arr = dataurl.split(','), mime = arr[0].match(/:(.*?);/)[1],
        bstr = atob(arr[1]), n = bstr.length, u8arr = new Uint8Array(n);
    while (n--) {
        u8arr[n] = bstr.charCodeAt(n);
    }
    return new File([u8arr], filename, { type: mime });
}

function cancelUpload() {
    myDropzone.removeAllFiles();

    let placeholder = document.getElementById('image-placeholder');
    let drop = document.getElementById('drop');

    placeholder.style.display = 'none';
    drop.style.display = 'flex';

    vm.setIsErrorInUploading(false);
}

var backToUpload = function backFromImageManipulationModalToUploadModalTo() {

    imageProcessrorCloseBtn.click();

    setTimeout(function () {
        uploadModalOpenBtn.click();
    }, 500);

    //uploadModalOpenBtn.click();

}


function closeActiveModals() {
    uploadModalCloseBtn.click();
    imageProcessrorCloseBtn.click();
}