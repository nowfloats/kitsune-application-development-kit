var ViewModel;
$(document).ready(function () {
    //$("body").tooltip({
    //    selector: '[data-toggle=tooltip]'
    //});
    ViewModel = new Customizations();
    ko.applyBindings(ViewModel);
    show_loading_bar({
        pct: 100,
        delay: 3
    });
    $("td input").hover(function () {
        $(this).parents("tr").children("input").css("background", "#f3f4f6 !important");
    },
        function () {
            $(this).parents("tr").children("input").css("background", "#fbfcfd !important");
        });
});

var initApp = {
    blockUI: function () {

    },
    unblockUI: function () {

    },
    Toaster: function () {

    }
}

var Customizations = function () {
    var self = {};
    self.WebActionsData = ko.observableArray();
    self.WebActionsList = ko.observableArray();
    self.WebActionsDataByName = ko.observableArray();
    self.SkipWebActionsDataBy = ko.observable(0);
    self.DomainServerUrl = "/k-admin";
    self.selectedWebActionOption = ko.observable("");
    window.dataLoadIndex = 0;
    // Get Web Actions List Data
    self.GetWebActionsList = function () {
        $.ajax({
            url: self.DomainServerUrl + "/webaction/GetWebActionsList",
            type: "POST",
            success: function (data) {
                var res;
                try {
                    res = JSON.parse(data);
                } catch (err) {
                    res = "error";
                }
                if (res != "error" && res != undefined && res != null && res != "") {
                    //ga('send', 'event', 'WEB ACTIONS', 'List Web Actions', self.FPData().Tag, self.FPData().PaymentState);
                    //debugger;
                    console.log(res);
                    self.WebActionsList(res);
                    self.selectedWebActionOption(self.WebActionsList().WebActions[0].DisplayName);
                    self.OpenWebActionsView(self.WebActionsList().WebActions[0].Name, 0);
                    ko.utils.arrayForEach(self.WebActionsList().WebActions, function (item, index) {
                        // Get a specific web Actions Data
                        GetWebActionData(item.Name, item.DisplayName);
                    });
                    var listLength = self.WebActionsList().WebActions.length;
                    var loadingInterval = setInterval(function () {
                        if (window.dataLoadIndex == listLength) {
                            initApp.unblockUI("#web-actions-content");
                            clearInterval(loadingInterval);
                        }
                    }, 100);
                } else {
                    initApp.unblockUI("#web-actions-content");
                }
            },
            error: function (error) {
                initApp.unblockUI("#web-actions-content");
            }
        });
    }

    //function GetWebActionData(webActionName, displayName) {
    //    $.ajax({
    //        url: 'http://api.kitsune.tools/webaction/v1/' + webActionName + '/get-data',
    //        type: "GET",
    //        dataType: "JSON",
    //        headers: {
    //            'Authorization': '5812233c9ec6682dbce36860'
    //        },
    //        // data: { webActionName: webActionName, fpId: "", skip: self.SkipWebActionsDataBy()},
    //        success: function (data) {
    //            if (data != undefined && data != null && data != "") {
    //                var alreadyExists = false;
    //                var oldData = self.WebActionsDataByName();
    //                var indexLine;
    //                for (i = 0; i < oldData.length; i++) {
    //                    if (oldData[i].webActionName == webActionName) {
    //                        indexLine = i;
    //                        self.WebActionsDataByName()[i].webActionData = data.Data;
    //                        self.WebActionsDataByName()[i].DataCount = data.Data.length;
    //                        alreadyExists = true;
    //                        initApp.unblockUI("#web-actions-content");
    //                        break;
    //                    }
    //                }
    //                if (!alreadyExists) {
    //                    var webActionDescription;
    //                    for (i = 0; i < self.WebActionsList().WebActions.length; i++) {
    //                        if (self.WebActionsList().WebActions[i].Name == webActionName) {
    //                            webActionDescription = self.WebActionsList().WebActions[i].Description;
    //                            break;
    //                        }
    //                    }
    //                    self.WebActionsDataByName().push({
    //                        "webActionDisplayName": displayName,
    //                        "webActionName": webActionName,
    //                        "webActionDescription": webActionDescription,
    //                        "webActionData": data.Data,
    //                        "DataCount": data.Data.length
    //                    });
    //                    window.dataLoadIndex++;
    //                    if (window.dataLoadIndex == Math.ceil((self.WebActionsList().WebActions.length) / 2)) {
    //                        $(".blockUI.blockMsg.blockElement .loading-message.loading-message-boxed span").html("&nbsp;&nbsp;LOADING DATA...");
    //                    }
    //                    //ga('send', 'event', 'WEB ACTIONS', 'Web Action Name - "' + webActionName.toUpperCase() + '"', self.FPData().Tag, self.FPData().PaymentState);
    //                }
    //                var tempData = self.WebActionsDataByName();
    //                self.OpenWebActionName(self.OpenWebActionName());
    //                self.WebActionsDataByName([]);
    //                self.WebActionsDataByName(tempData);
    //                if (indexLine != undefined && indexLine != null) {
    //                    self.OpenWebActionsView(webActionName, indexLine);
    //                }
    //            } else {
    //                initApp.Toaster("error", "Data Unavailable", "Please try again later");
    //                initApp.unblockUI("#web-actions-content");
    //            }
    //        },
    //        error: function (error) {
    //            initApp.Toaster("error", "Data Unavailable", "Please try again later");
    //            initApp.unblockUI("#web-actions-content");
    //        }
    //    });
    //}

    function GetWebActionData(webActionName, displayName) {
        $.ajax({
            url: self.DomainServerUrl + "/webaction/GetWebActionsData?webActionName=" + webActionName,
            type: "POST",
            dataType: "JSON",
            success: function (data) {
                if (data != undefined && data != null && data != "") {
                    var alreadyExists = false;
                    var oldData = self.WebActionsDataByName();
                    var indexLine;
                    for (i = 0; i < oldData.length; i++) {
                        if (oldData[i].webActionName == webActionName) {
                            indexLine = i;
                            self.WebActionsDataByName()[i].webActionData = data.Data;
                            self.WebActionsDataByName()[i].DataCount = (data.Data != null) ? data.Data.length : null;
                            alreadyExists = true;
                            initApp.unblockUI("#web-actions-content");
                            break;
                        }
                    }
                    if (!alreadyExists) {
                        var webActionDescription;
                        for (i = 0; i < self.WebActionsList().WebActions.length; i++) {
                            if (self.WebActionsList().WebActions[i].Name == webActionName) {
                                webActionDescription = self.WebActionsList().WebActions[i].Description;
                                break;
                            }
                        }
                        self.WebActionsDataByName().push({
                            "webActionDisplayName": displayName,
                            "webActionName": webActionName,
                            "webActionDescription": webActionDescription,
                            "webActionData": data.Data,
                            "DataCount": (data.Data != null) ? data.Data.length : null
                        });
                        //debugger;
                        window.dataLoadIndex++;
                        if (window.dataLoadIndex == Math.ceil((self.WebActionsList().WebActions.length) / 2)) {
                            $(".blockUI.blockMsg.blockElement .loading-message.loading-message-boxed span").html("&nbsp;&nbsp;LOADING DATA...");
                        }
                        //ga('send', 'event', 'WEB ACTIONS', 'Web Action Name - "' + webActionName.toUpperCase() + '"', self.FPData().Tag, self.FPData().PaymentState);
                    }
                    var tempData = self.WebActionsDataByName();
                    self.OpenWebActionName(self.OpenWebActionName());
                    self.WebActionsDataByName([]);
                    self.WebActionsDataByName(tempData);
                    if (indexLine != undefined && indexLine != null) {
                        self.OpenWebActionsView(webActionName, indexLine);
                    }
                } else {
                    initApp.Toaster("error", "Data Unavailable", "Please try again later");
                    initApp.unblockUI("#web-actions-content");
                }
            },
            error: function (error) {
                initApp.Toaster("error", "Data Unavailable", "Please try again later");
                initApp.unblockUI("#web-actions-content");
            }
        });
    }

    self.GetWebActionsDataCount = function (webActionName) {
        var dataCountArray = $.grep(self.WebActionsDataByName(), function (data) {
            if (data.webActionName == webActionName) return data.DataCount;
        });
        return dataCountArray[0];
    }

    self.OpenWebActionName = ko.observable();
    self.WebActionDisplayName = ko.observable();
    self.openViewIndex = ko.observable(0);
    self.WebActionsViewData = ko.observableArray();
    self.WebActionHeaders = ko.observableArray();
    self.WebActionProps = ko.observableArray();
    self.DestroyTableCounter = ko.observable(0);
    self.addWebActionClicked = ko.observable(false);
    self.deleteClicked = ko.observable(false);
    self.deleteIndex = ko.observable();
    self.RawWebActionsData = ko.observableArray();

    // Show specific Web Actions Related Data
    self.OpenWebActionsView = function (webActionName, index) {
        //ga('send', 'event', 'WEB ACTIONS', 'Opened Web Action - "' + webActionName.toUpperCase() + '"', self.FPData().Tag, self.FPData().PaymentState);
        var selectedWebAction = $("#web-actions-cards>div:nth-child(" + (index + 1) + ")>a>div.dashboard-stat2");
        $("div.dashboard-stat2").removeClass("selected-web-action");
        selectedWebAction.addClass("selected-web-action");

        if (self.deleteClicked()) {
            var WebActionsDataArray = $.grep(self.WebActionsDataByName(), function (data) {
                if (data.webActionName == webActionName) {
                    self.OpenWebActionName(webActionName);
                    self.WebActionDisplayName(data.webActionDisplayName);
                    self.selectedWebActionOption(data.webActionDisplayName);

                    return data.webActionData.splice(self.deleteIndex(), 1);
                }
            });
            self.deleteClicked(false);
        } else {
            var WebActionsDataArray = $.grep(self.WebActionsDataByName(), function (data) {
                if (data.webActionName == webActionName) {
                    self.OpenWebActionName(webActionName);
                    self.WebActionDisplayName(data.webActionDisplayName);
                    self.selectedWebActionOption(data.webActionDisplayName);

                    return data.webActionData;
                }
            });
        }
        if (WebActionsDataArray && WebActionsDataArray.length && (WebActionsDataArray[0].DataCount != 0 || WebActionsDataArray[0].DataCount == 0)) {
            self.openViewIndex(index);
            if ($.fn.dataTable.isDataTable("#webActionsTable")) {
                $('#webActionsTable').dataTable().fnDestroy();
                $("#web-actions-table").remove();
            }

            var webActionTypesElement = webActionTypesHeader = ``;
            var webActionType1 = `
                <td data-bind="if: $index() == $root.CountForDynamicArray() - 1 , visible: $index() == $root.CountForDynamicArray() - 1">
                    <a class="btn btn-default edit-button" data-toggle="tooltip" data-placement="top" data-original-title="Edit" data-bind="click: function(data, event){$root.EditField( $parentContext.$index(), event) }"><i class="fa fa-pencil-square-o" aria-hidden="true" style="font-size: 20px;"></i></button>
                    <a class="btn btn-default save-button" data-toggle="tooltip" data-placement="top" data-original-title="Save" data-bind="click: function(data, event){ $root.SaveField($parentContext.$data, $parentContext.$index(), event) }, visible: false"><i class="fa fa-floppy-o" aria-hidden="true" style="font-size: 20px;"></i></button>
                </td>
            `;
            var webActionType2 = `
                <td data-bind="if: $index() == $root.CountForDynamicArray() - 1, visible: $index() == $root.CountForDynamicArray() - 1">
                    <a class="btn btn-default delete-button" data-toggle="tooltip" data-placement="top" data-original-title="Delete" data-bind="click: function(data, event){ $root.DeleteField($parentContext.$data, $parentContext.$index(), event) }"><i class="fa fa-trash" aria-hidden="true" style="font-size: 20px;"></i></button>
                </td>
            `;
            var webActionTypeHeader1 = `
                <th data-bind="if: $index() == $root.CountForDynamicArray() - 1, visible: $index() == $root.CountForDynamicArray() - 1"></th>
            `;
            var webActionTypeHeader2 = `
                <th data-bind="if: $index() == $root.CountForDynamicArray() - 1, visible: $index() == $root.CountForDynamicArray() - 1"></th>
            `;
            //if(data.webActionTypes.indexOf("UPDATE") > -1 && data.webActionTypes.indexOf("DELETE") > -1){
            //    webActionTypesElement = webActionType1 + webActionType2;
            //    webActionTypesHeader = webActionTypeHeader1 + webActionTypeHeader2;
            //}
            //else if(data.webActionTypes.indexOf("UPDATE") > -1 && data.webActionTypes.indexOf("DELETE") == -1){
            //    webActionTypesElement = webActionType1;
            //    webActionTypesHeader = webActionTypeHeader1;
            //}
            webActionTypesElement = webActionType1 + webActionType2;
            webActionTypesHeader = webActionTypeHeader1 + webActionTypeHeader2;

            $(".web-portlet").append(`
                <div class="table-responsive" id="web-actions-table">
                    <table id="webActionsTable" class="display" cellspacing="0">
                        <thead>
                            <tr data-bind="foreach: $root.WebActionHeaders()">
                                <th>
                                    <span data-bind="text: $root.CapitalizeHeaders($data)"></span>
                                    <span class="required" data-bind="if: $root.WebActionProps()[$index()].isRequired, visible: $root.WebActionProps()[$index()].isRequired">*</span>
                                </th>` + webActionTypesHeader + `
                            </tr>
                        </thead>
                        <tbody data-bind="foreach: {data: $root.WebActionsViewData(), afterRender: $root.PostProcessingForeach}">
                            <tr data-bind="foreach: $root.BindDynamicWebActions($data)">
                                <td>
                                    <!-- ko if: typeof $data !== 'object' -->
                                        <!-- ko if: $root.WebActionProps()[$index()].dataType == 'DATE' -->
                                            <input class="form-control form-control-inline input-medium date-picker" data-bind="value: $data, attr: {placeholder: $root.WebActionHeaders()[$index()]}" disabled>
                                        <!-- /ko -->
                                        <!-- ko if: $root.WebActionProps()[$index()].dataType != 'DATE' -->
                                            <input data-bind="value: $data, attr: {placeholder: $root.WebActionHeaders()[$index()]}" readonly/>
                                        <!-- /ko -->
                                    <!-- /ko -->
                                    <!-- ko if: typeof $data === 'object' && $data.url.toLowerCase().match(/\.(jpeg|jpg|gif|png)$/) != null -->
                                        <button class="btn btn-default btn-image" style="display: none;"><i class="fa fa-plus"></i>&nbsp; Select Image</button>
                                        <input class="input-file" style="display: none;" id="image-upload" type="file" name="upload-image" accept="image/*">
                                        <img class="img-url" data-bind="attr: {src: $data.url}" style="height: 50px; width: 50px">
                                        <a class="img-remove" data-toggle="tooltip" data-placement="top" data-original-title="Remove Image" style="display: none"><i class="fa fa-times-circle" style="font-size: 20px;color: #aaa;"></i></a>
                                        <input style="display: none;" class="input-url" data-bind="value: $data.url" readonly />
                                        <textarea data-bind="value: $data.description" placeholder="Description" readonly />
                                    <!-- /ko -->
                                    <!-- ko if: typeof $data === 'object' && $data.url.toLowerCase().match(/\.(jpeg|jpg|gif|png)$/) == null -->
                                        <button class="btn btn-default btn-link" style="display: none;"><i class="fa fa-plus"></i>&nbsp; Choose File</button>
                                        <input class="input-link" style="display: none;" id="link-upload" type="file" name="upload-link" accept="application/*">
                                        <span id="input-or" style="display:none;"><br>OR</span>
                                        <input class="input-url" data-bind="value: $data.url" placeholder="Enter URL Link" readonly/>
                                        <textarea data-bind="value: $data.description" placeholder="Description" readonly />
                                    <!-- /ko -->
                                </td>` + webActionTypesElement + `
                            </tr>
                        </tbody>
                    </table>
                </div>
            `);
            self.addWebActionClicked(false);

            var propertyList;
            self.WebActionsList().WebActions.forEach(function (item, index) {
                var keys = Object.keys(item);
                if (self.OpenWebActionName() == item.Name) {
                    propertyList = item.Properties;
                }
            });

            self.WebActionHeaders([]); // Headings for the table column
            self.WebActionProps([]); // Property names of the values to show
            self.WebActionsViewData([]); // Full Data to show within table
            var props = new Array();
            var headers = new Array();
            for (j = 0; j < propertyList.length; j++) {
                headers.push(propertyList[j].DisplayName);
                props.push({
                    propName: propertyList[j].PropertyName,
                    isRequired: propertyList[j].IsRequired,
                    dataType: propertyList[j].DataType
                });
            }

            self.RawWebActionsData(WebActionsDataArray[0].webActionData); // Putting WebActions Data to a Raw observable
            ko.utils.arrayForEach(self.RawWebActionsData(), function (itemA) {
                var i = itemA;
                var obj = {};
                ko.utils.arrayForEach(props, function (itemB) {
                    obj[itemB.propName] = i[itemB.propName];
                });
                self.WebActionsViewData().push(obj); // Putting data to the view Table
            });
            self.WebActionHeaders(headers);
            self.WebActionProps(props);
            self.WebActionsViewData(self.WebActionsViewData());
            if (WebActionsDataArray[0].DataCount == 0) {
                $("#webActionsTable").DataTable();
                if (!(self.addWebActionClicked())) {
                    $("#webActionsTable_length label").append(`
                        <button class="btn add-web-action"><i class="fa fa-plus" style="font-size: 15px;"></i>&nbsp;&nbsp;ADD</button>
                    `);
                }
                InitializeAddWebActionDataButton();
            }

            // Applying bindings on the data view for customizations
            ko.applyBindings(ViewModel, $("#web-actions-table")[0]);
            $(".web-actions-page").removeClass("hide");
            InitializeRowHover();

            // Initializing and binding event to datepicker
            InitializeDatepicker();
        }
    }

    function InitializeDatepicker() {
        $('.date-picker').datepicker().on('changeDate', function (ev) {
            $(this).datepicker('hide');
        });
    }

    function InitializeRowHover() {
        $("#web-actions-table tr").hover(
            function () { // On hovering table row
                $(this).addClass("hovered");
            },
            function () { // On Removing hover from table row
                $(this).removeClass("hovered");
            }
        );
    }

    function InitializeAddWebActionDataButton() {
        $(".add-web-action").on("click", function (e) {
            if ($(".btn.save-web-action").length == 0) {
                self.addWebActionClicked(true);
                var dArray = {},
                    cArray = {};
                var vData = {};
                if (self.WebActionsViewData().length > 0) {
                    vData = self.WebActionsViewData()[0];
                } else {
                    self.WebActionProps().forEach(function (item, index) {
                        if (item.dataType == "IMAGE" || item.dataType == "LINK") {
                            vData[item.propName] = {};
                        } else
                            vData[item.propName] = "";
                    });
                }
                for (var propName in vData) {
                    if (typeof vData[propName] === 'object') {
                        var tData = vData[propName]

                        //check for image 
                        var propertyList;
                        self.WebActionsList().WebActions.forEach(function (item, index) {
                            var keys = Object.keys(item);
                            if (self.OpenWebActionName() == item.Name) {
                                propertyList = item.Properties;
                            }
                        });
                        var isImage = false;
                        var isLink = false;
                        for (j = 0; j < propertyList.length; j++) {
                            if (propertyList[j].PropertyName == propName) {
                                if (propertyList[j].DataType == "IMAGE") {
                                    isImage = true;
                                    break;
                                }
                                if (propertyList[j].DataType == "LINK") {
                                    isLink = true;
                                    break;
                                }
                            }
                        }
                        if (isImage || isLink) {
                            if (isImage) {
                                dArray[propName] = {
                                    url: ".jpeg, .jpg, .gif or .png",
                                    description: ""
                                };
                            }
                            if (isLink) {
                                dArray[propName] = {
                                    url: "",
                                    description: ""
                                };
                            }
                        } else {
                            for (var pName in tData) {
                                cArray[pName] = "";
                            }
                            dArray[propName] = cArray;
                        }
                        //end check for image
                    } else
                        dArray[propName] = "";
                }
                self.WebActionsViewData().push(dArray);
                var swap = self.WebActionsViewData()[0],
                    slength = self.WebActionsViewData().length;
                self.WebActionsViewData()[0] = self.WebActionsViewData()[slength - 1];
                self.WebActionsViewData()[slength - 1] = swap;
                self.WebActionsViewData(self.WebActionsViewData());
                self.EditField(0, e);
                self.addWebActionClicked(false);

                // Styling Editing on row
                var trNth = "#webActionsTable tbody tr:nth-child(1)";
                $(trNth).addClass("editing");

                // Initializing and binding event to datepicker
                InitializeDatepicker();
            } else {
                initApp.Toaster("error", "Already Editing", "Please save current data");
            }
        });
    }

    self.PostProcessingForeach = function (elements, data) {
        if (this.foreach[this.foreach.length - 1] === data) {
            $("#webActionsTable").DataTable();
            if (!(self.addWebActionClicked())) {
                $("#webActionsTable_length label").append(`
                    <button class="btn add-web-action"><i class="fa fa-plus" style="font-size: 15px;"></i>&nbsp;&nbsp;ADD</button>
                `);
            }
            InitializeAddWebActionDataButton();
        }
    }

    self.CountForDynamicArray = ko.observable(0);
    self.BindDynamicWebActions = function (data) {
        var dArray = new Array();
        var i = 0;
        var propData = self.WebActionProps();
        if (propData.length > 0) {
            propData.forEach(function (item, index) {
                if (item.dataType == "LINK" || item.dataType == "IMAGE") {
                    if (data[item.propName] == undefined) {
                        data[item.propName] = {};
                        data[item.propName].url = "";
                        data[item.propName].description = "";
                    } else if (data[item.propName].url == undefined) {
                        data[item.propName].url = "";
                    } else if (data[item.propName].description == undefined) {
                        data[item.propName].description = "";
                    }
                } else if (data[item.propName] == undefined) {
                    data[item.propName] = "";
                }
                dArray.push(data[item.propName]);
            });
        }
        var length = dArray.length;
        self.CountForDynamicArray(length);
        return dArray;
    }
    self.CapitalizeHeaders = function (data) {
        var temp = data.split(/(?=[A-Z])/);
        var newA = '';
        ko.utils.arrayForEach(temp, function (item, index) {
            item = item.replace(item[0], item[0].toUpperCase());
            newA = newA + ' ' + item;
        });
        return newA;
    };

    self.editIndex = ko.observable(0);
    self.editedObjectId = ko.observable();

    self.EditField = function (index, event) {
        if ($(".btn.save-web-action").length == 0) { //(self.WebActionsViewData().length == self.RawWebActionsData().length) || self.addWebActionClicked()){
            self.editIndex(index);
            InitializeSaveButton();
            if (self.RawWebActionsData().length > 0) {
                self.editedObjectId(self.RawWebActionsData()[index]._id);
            }
            var trNth = self.addWebActionClicked() ? $("#webActionsTable tbody tr:nth-child(1)") : $($(event.target).parents("tr")[0]);
            $(trNth).addClass("editing");
            $(trNth).children("td").each(function () {
                $(this).children(".date-picker").removeAttr("disabled");
                $(".date-picker").datepicker("hide");
                $(this).children("input, textarea").removeAttr("readonly");
                $(".date-picker").attr("readonly", true);
                if ($(this).children("a").length > 1) {
                    $(this).children("a.edit-button").hide();
                    $(this).children("a.save-button").show();
                }
                if ($(this).children('.img-url').length > 0) {
                    if ($(this).children('.input-url').val() == "" || $(this).children('.input-url').val() == ".jpeg, .jpg, .gif or .png") {
                        $(this).children('.img-url').hide();
                        $(this).children(".img-remove").hide();
                        $(this).children('.btn-image').show();
                    } else {
                        $(this).children('.img-url').show();
                        $(this).children(".img-remove").show();
                        $(this).children('.btn-image').hide();
                    }
                }
                if ($(this).children('.input-url').val() == "") {
                    $(this).children("#input-or").show();
                    $(this).children('.btn-link').show();

                } else {
                    $(this).children('.btn-link').show();
                    $(this).children("#input-or").show();
                    $('.input-url').prop('readonly', false);
                }
                var parentThis = $(this);
                $(this).children('.input-file').change(function () {
                    var inputFiles = $(this)[0].files;
                    var formData = new FormData();

                    if (inputFiles.length > 0) {
                        for (var i = 0; i < inputFiles.length; i++) {
                            formData.append('file-' + i, inputFiles[i]);
                        }
                    }
                    var innerThis = $(this);
                    $.ajax({
                        url: self.DomainServerUrl + "/webaction/WebActionDataUpload",
                        type: "POST",
                        data: formData,
                        contentType: false,
                        processData: false,
                        success: function (res) {
                            if (res != null && res != "" && res != "error" && res != "no file sent") {
                                try {
                                    url = JSON.parse(res);
                                    parentThis.children('.img-url').attr("src", url);
                                    parentThis.children('.img-url').show();
                                    parentThis.children('.img-remove').show();
                                    innerThis.prev().hide();
                                    parentThis.children('.input-url').val(url);
                                    initApp.Toaster("success", "", "Image has been saved successfully");
                                } catch (ex) {
                                    initApp.Toaster("error", "Upload error", "Please try again later");
                                }
                            } else {
                                initApp.Toaster("error", "Upload error", "Please try again later");
                            }
                        },
                        error: function (err) {
                            initApp.Toaster("error", "Upload error", "Please try again later");
                        }
                    });
                })
                $(this).children('.input-link').change(function () {
                    initApp.blockUI({
                        target: window,
                        boxed: true
                    });
                    $(".blockUI.blockMsg.blockPage").css("top", "40%");
                    $(".blockUI.blockMsg.blockPage .loading-message.loading-message-boxed span").html("&nbsp;&nbsp;UPLOADING FILE...");

                    var inputFiles = $(this)[0].files;
                    var formData = new FormData();

                    if (inputFiles.length > 0) {
                        for (var i = 0; i < inputFiles.length; i++) {
                            formData.append('file-' + i, inputFiles[i]);
                        }
                    }
                    var innerThis = $(this);
                    $.ajax({
                        url: self.DomainServerUrl + "/webaction/WebActionDataUpload",
                        type: "POST",
                        data: formData,
                        contentType: false,
                        processData: false,
                        success: function (res) {
                            if (res != null && res != "" && res != "error" && res != "no file sent") {
                                try {
                                    url = JSON.parse(res);
                                    innerThis.prev().hide();
                                    parentThis.children('.input-url').val(url);
                                    parentThis.children('#input-or').hide();
                                    initApp.unblockUI();
                                    initApp.Toaster("success", "", "Link has been saved successfully");
                                } catch (ex) {
                                    initApp.unblockUI();
                                    initApp.Toaster("error", "Upload error", "Please try again later");
                                }
                            } else {
                                initApp.unblockUI();
                                initApp.Toaster("error", "Upload error", "Please try again later");
                            }
                        },
                        error: function (err) {
                            initApp.unblockUI();
                            initApp.Toaster("error", "Upload error", "Please try again later");
                        }
                    });
                })
            });
            $(trNth).children("td").each(function () {
                var inputFile = $(this).children('.input-file');
                if (inputFile.length > 0) {
                    $(this).children("textarea").focus();
                    return false;
                } else if ($(this).children("input").length > 0) {
                    $(this).children("input").focus();
                    return false;
                }

                var inputLink = $(this).children('.input-link');
                if (inputLink.length > 0) {
                    $(this).children("textarea").focus();
                    return false;
                } else if ($(this).children("input").length > 0) {
                    $(this).children("input").focus();
                    return false;
                }
            });

            // To choose image for Image type data
            $(".btn-image").on('click', function () {
                $(this).next().click();
            });
            //To choose file for Link type data
            $(".btn-link").on('click', function () {
                $(this).next().click();
            });

            // Remove Uploaded Image
            $(".img-remove").on("click", function () {
                $(this).next().val("");
                $(this).prev().prev().prev().show();
                $(this).prev().hide();
                $(this).hide();
            });

        } else {
            initApp.Toaster("error", "Already Editing", "Please save current data");
        }
    };

    function InitializeSaveButton() {
        $("#webActionsTable_length label").append(`
            <button class="btn save-web-action" style="margin-left: 15px;"><i class="fa fa-floppy-o" aria-hidden="true" style="font-weight: bold; font-size: 15px;"></i>&nbsp;&nbsp;SAVE</button>
        `);
        $(".btn.save-web-action").on("click", function () {
            self.SaveField(self.WebActionsViewData()[self.editIndex()], self.editIndex());
        });
    }

    self.SaveField = function (data, index, event) {
        var dataObject = data;
        var trNth = $($(event.target).parents("tr")[0]);
        var isEmpty = false;
        var keysData = Object.keys(data);
        var i = 0;
        $(trNth).children("td").each(function () {
            var inputExists = $(this).children("input").length > 0;
            var textareaExists = $(this).children("textarea").length > 0;
            if (inputExists) {
                if (textareaExists) {
                    if (typeof dataObject[self.WebActionProps()[i].propName] === 'object') {
                        var pValue = {
                            url: "",
                            description: ""
                        };
                        pValue["url"] = $(this).children(".input-url").val().replace(/.jpeg, .jpg, .gif or .png/g, '');
                        pValue["description"] = $(this).children("textarea").val();
                        dataObject[self.WebActionProps()[i].propName] = pValue;
                        i++;
                    }
                } else {
                    dataObject[self.WebActionProps()[i].propName] = $(this).children("input").val();
                    i++;
                }
            }
        });

        var propTemp = self.WebActionProps();
        var propLength = propTemp.length;
        for (i = 0; i < propLength; i++) { // Checking if required Info is filled or not
            if (propTemp[i].isRequired) {
                if (dataObject[propTemp[i].propName] == "" && propTemp[i].dataType != "LINK" && propTemp[i].dataType != "IMAGE") {
                    initApp.Toaster("error", "Required Field", "Please enter " + self.WebActionHeaders()[i]);
                    return;
                } else if (propTemp[i].dataType == "LINK" || propTemp[i].dataType == "IMAGE") {
                    if (dataObject[propTemp[i].propName].url == "") {
                        initApp.Toaster("error", "Required Field", "Please enter link of " + self.WebActionHeaders()[i]);
                        return;
                    } else if (dataObject[propTemp[i].propName].description == "") {
                        initApp.Toaster("error", "Required Field", "Please enter description of " + self.WebActionHeaders()[i]);
                        return;
                    }
                }
            }
        }

        initApp.blockUI({
            target: window,
            boxed: true
        });
        $(".blockUI.blockMsg.blockPage").css("top", "40%");
        $(".blockUI.blockMsg.blockPage .loading-message.loading-message-boxed span").html("&nbsp;&nbsp;SAVING...");
        self.WebActionsViewData()[index] = dataObject;
        self.WebActionsViewData(self.WebActionsViewData());

        var isEdit = (self.RawWebActionsData().length == self.WebActionsViewData().length) ? true : false;
        if (isEdit) {
            // Save Edited data to DB
            dataObject["WebsiteId"] = self.RawWebActionsData()[index].WebsiteId;
            var updateId = "{_id:'" + self.editedObjectId() + "'}";
            var dataPass = "{$set : " + JSON.stringify(dataObject) + "}";
            $.ajax({
                url: self.DomainServerUrl + "/webaction/UpdateWebActionsData",
                type: "POST",
                data: {
                    "webActionName": self.OpenWebActionName(),
                    "webActionData": JSON.stringify({
                        "Query": updateId,
                        "UpdateValue": dataPass,
                        "Multi": true
                    })
                },
                success: function (data) {
                    if (data != undefined && data != null && data != "") {
                        $('.img-url').show();
                        $('.img-remove').hide();
                        $('.btn-image').hide();
                        initApp.Toaster("success", "", "Data saved successfully");
                        $(".btn.save-web-action").remove();
                        $(trNth).removeClass("editing");
                        $(trNth).children("td").each(function () {
                            $(this).children("input, textarea").prop("readonly", true);
                            $(trNth).removeClass("editing");
                            if ($(this).children("a").length > 1) {
                                $(this).children("a.edit-button").show();
                                $(this).children("a.save-button").hide();
                            }
                        });
                        //ga('send', 'event', 'WEB ACTIONS', 'Edited Web Action Data - "' + self.OpenWebActionName().toUpperCase() + '"', self.FPData().Tag, self.FPData().PaymentState);
                        GetWebActionData(self.OpenWebActionName(), self.WebActionDisplayName());
                    } else {
                        if (data == null || data == undefined || data == "")
                            initApp.Toaster("error", "Data not saved", "Please try again later");
                        else
                            initApp.Toaster("error", "Data not saved", data);
                    }
                    initApp.unblockUI();
                },
                error: function (error) {
                    initApp.Toaster("error", "Data not saved", "Please try again later");
                    initApp.unblockUI();
                }
            });
        } else {
            // Save Added data to DB
            $.ajax({
                url: self.DomainServerUrl + "/webaction/AddWebActionsData",
                type: "POST",
                data: {
                    "webActionName": self.OpenWebActionName(),
                    "webActionData": JSON.stringify({
                        "WebsiteId": "",
                        "ActionData": dataObject
                    })
                },
                success: function (data) {
                    if (data != undefined && data != null && data != "") {
                        initApp.Toaster("success", "", "Data saved successfully");
                        $(".btn.save-web-action").remove();
                        $(trNth).children("td").each(function () {
                            $(this).children("input, textarea").prop("readonly", true);
                            $(trNth).removeClass("editing");
                            if ($(this).children("a").length > 1) {
                                $(this).children("a.edit-button").show();
                                $(this).children("a.save-button").hide();
                            }
                        });
                        //ga('send', 'event', 'WEB ACTIONS', 'Added Web Action Data - "' + self.OpenWebActionName().toUpperCase() + '"', self.FPData().Tag, self.FPData().PaymentState);
                        GetWebActionData(self.OpenWebActionName(), self.WebActionDisplayName());
                    } else {
                        initApp.Toaster("error", "Data not saved", "Please try again later");
                    }
                    initApp.unblockUI();
                },
                error: function (error) {
                    initApp.Toaster("error", "Data not saved", "Please try again later");
                    initApp.unblockUI();
                }
            });
        }
    }

    self.DeleteField = function (data, index, event) {
        initApp.blockUI({
            target: window,
            boxed: true
        });
        $(".blockUI.blockMsg.blockPage").css("top", "40%");
        $(".blockUI.blockMsg.blockPage .loading-message.loading-message-boxed span").html("&nbsp;&nbsp;DELETING...");
        if (self.RawWebActionsData().length >= self.WebActionsViewData().length) {
            self.editedObjectId(self.RawWebActionsData()[index]._id);
            var updateId = "{_id:'" + self.editedObjectId() + "'}";
            var websiteId = self.RawWebActionsData()[index].WebsiteId;
            var dataPass = "{$set : {IsArchived: true, WebsiteId: '" + websiteId + "'}}";
            $.ajax({
                url: self.DomainServerUrl + "/webaction/UpdateWebActionsData",
                type: "POST",
                data: {
                    "webActionName": self.OpenWebActionName(),
                    "webActionData": JSON.stringify({
                        "Query": updateId,
                        "UpdateValue": dataPass,
                        "Multi": true
                    })
                },
                success: function (data) {
                    if (data != undefined && data != null && data != "") {
                        $(".btn.save-web-action").remove();
                        initApp.Toaster("success", "", "Data deleted successfully");
                        //ga('send', 'event', 'WEB ACTIONS', 'Deleted Web Action Data - "' + self.OpenWebActionName().toUpperCase() + '"', self.FPData().Tag, self.FPData().PaymentState);
                        GetWebActionData(self.OpenWebActionName(), self.WebActionDisplayName());
                    } else {
                        initApp.Toaster("error", "Data not deleted", "Please try again later");
                    }
                    initApp.unblockUI();
                },
                error: function (error) {
                    initApp.Toaster("error", "Data not deleted", "Please try again later");
                    initApp.unblockUI();
                }
            });
        } else {
            DataDeleted(0);
            $(".btn.save-web-action").remove();
            GetWebActionData(self.OpenWebActionName(), self.WebActionDisplayName());
            initApp.unblockUI();
        }
    }

    function DataDeleted(index) {
        initApp.Toaster("success", "", "Data deleted successfully");
        self.WebActionsViewData().splice(index, 1);
        self.WebActionsViewData(self.WebActionsViewData());
    }

    //self.GetWebActionsList();

    return self;
}