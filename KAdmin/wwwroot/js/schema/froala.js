(function FroalaEditorWrapper(global) {
    var maxFileSize = 2;
    /**
     * for more information on error codes please reffer : https://www.froala.com/wysiwyg-editor/docs/concepts/image/upload
     */
    var errorInImageUpload = {
        "ImageMaxFileSizeExceeded": {
            code: 5,
            message: "Maximum file size allowed is " + maxFileSize + " MB."
        }
    }

    function getConfiguration() {
        return {
            key: "lB5B1B4D1uF2C1F1H2A10C1D6A1D6B4hZSZGUSXYSMZb1JGZ==",
            toolbarButtons: [
                'fullscreen', '|',
                'bold',
                'italic',
                'underline',
                'strikeThrough',
                'fontSize',
                'color', '|',
                'paragraphFormat',
                'paragraphStyle',
                'quote',
                'align',
                'formatUL',
                'formatOL',
                'outdent',
                'indent', '|',
                'insertLink',
                'insertImage',
                'insertVideo',
                //'insertFile',
                'insertTable',
                'html',
                '-',
                'undo',
                'redo', '|',
                'specialCharacters',
                //'emoticons',
                'insertHR', '|',
                'superscript',
                'subscript'
            ],

            imageEditButtons: ['imageReplace',
                'imageAlign',
                'imageRemove', '|',
                'imageLink',
                'linkOpen',
                'linkEdit',
                'linkRemove',
                '-',
                'imageDisplay', 
                'imageStyle',
                'imageAlt',
                'imageSize'],

            fontSize: ['8', '10', '12', '14', '18', '30', '60', '96'],
            fontSizeSelection: true,

            // colors
            colorsBackground: [
                '#15E67F', '#E3DE8C', '#D8A076', '#D83762', '#76B6D8', 'REMOVE',
                '#1C7A90', '#249CB8', '#4ABED9', '#FBD75B', '#FBE571', '#FFFFFF'
            ],
            colorsStep: 6,
            colorsText: [
                '#15E67F', '#E3DE8C', '#D8A076', '#D83762', '#76B6D8', 'REMOVE',
                '#1C7A90', '#249CB8', '#4ABED9', '#FBD75B', '#FBE571', '#FFFFFF'
            ],

            // image upload settings
            imageUploadURL: CookieHelper.isAliCloud() ?
                'https://kadmin.getkitsune-alicloud.com/k-admin/ManageWebsiteContent/SaveUploadedFileV2'
                : 'ManageWebsiteContent/SaveUploadedFile',
            imageMaxSize: (maxFileSize * 1024 * 1024),
            imageUploadParams: { froala: true },
            requestHeaders: { DeveloperId: vm.customerData.developerId, WebsiteId: vm.customerData.websiteId },
            useClasses: false,
            heightMax: 'calc(40vh)',
            heightMin: 200,
            videoInsertButtons: ['videoBack', '|', 'videoByURL', 'videoEmbed'],
            imageInsertButtons: ['imageBack', '|', 'imageUpload', 'imageByURL']
        };
    };

    global.FroalaEditor = function (elementId) {
        var $element = $('#' + elementId)

        function initialize() {
            var editorConfiguration = getConfiguration();
            $element.froalaEditor(editorConfiguration ? editorConfiguration : null);
        }

        function setTextToFlora(content) {
            return $element.froalaEditor('html.set', content);
        }

        function getTextFromFlora() {
            return $element.froalaEditor('html.get');
        }

        // image error handling
        $element.on('froalaEditor.image.error', function (e, editor, error, response) {

            if (error.code == errorInImageUpload.ImageMaxFileSizeExceeded.code) {
                var $popup = editor.popups.get('image.insert');
                var $layer = $popup.find('.fr-image-progress-bar-layer');
                $layer.find('h3').text(errorInImageUpload.ImageMaxFileSizeExceeded.message);
            }

        });

        var publicAPI = {
            initialize: initialize,
            setContent: setTextToFlora,
            getContent: getTextFromFlora
        }

        return publicAPI;
    };

})(window);
