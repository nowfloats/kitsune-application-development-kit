var KModal = Vue.component('k-modal', {

    template: "#modal-template",

    name: "KModal",
 
    props: {
        "cancelAction": {
            type: Function
        },
        "saveAction": {
            type: Function
        },
        "size": {
            type: String
        },
        "modalTitle": {
            type: String
        },
        "saveText": {
            type: String
        },
        "cancelText": {
            type: String
        },
        "isSaveDisabled": {
            type: Boolean
        },
        "closeModal": {
            type: Function
        },
        "mountedCallback": {
            type: Function
        }
    },

    methods: {
        dismissModal: function () {
            this.closeModal();
        },

        preventEvents: function (event) {
            if (event.target.getAttribute("for") == 'image-file-input' || event.target.type == 'file') {
                event.stopPropagation();
                return;
            }

            event.preventDefault();
            event.stopPropagation();
            
        },

        keyupHandler: function () {
            if (event.keyCode == 27) {
                this.closeModal();
            }
        }
    },

    mounted: function () {
        if (this.mountedCallback) {
            this.mountedCallback();
        }
        window.addEventListener('keyup', this.keyupHandler);
    },

    destroyed: function () {
        window.removeEventListener('keyup', this.keyupHandler);
    }

});