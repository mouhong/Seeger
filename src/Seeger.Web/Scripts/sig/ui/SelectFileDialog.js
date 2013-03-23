﻿(function ($) {

    var sig = window.sig = window.sig || {};
    sig.ui = sig.ui || {};

    sig.ui.SelectFileDialog = function (options) {
        var _this = this;
        var _dialog = new sig.ui.Dialog();
        var _fileManager = null;
        var _isInited = false;
        var _options = {
            multi: false,
            folder: '/Files',
            filter: null,
            width: 700,
            height: 500,
            modal: false
        };

        if (options) {
            _options = $.extend(true, _options, options);
        }

        this.init = function () {
            if (_isInited) return;

            _dialog.init({
                width: _options.width,
                height: _options.height,
                modal: _options.modal,
                buttons: [
                    {
                        text: sig.GlobalResources.get('Cancel'),
                        click: function () {
                            _this.close();
                        }
                    },
                    {
                        text: sig.GlobalResources.get('OK'),
                        click: function () {
                            var files = _fileManager.selectedFiles();
                            if (files.length === 0) {
                                alert(sig.GlobalResources.get('Please select a file'));
                            } else {
                                if (_options.onOK) {
                                    _options.onOK.apply(_this, [files]);
                                }
                            }
                        }
                    }
                ]
            });

            var fileManagerId = '__filemanager_' + new Date().getTime();
            _dialog.html('<div id="' + fileManagerId + '"></div>');

            _fileManager = new sig.ui.FileManager({
                element: $('#' + fileManagerId),
                aspNetAuth: _options.aspNetAuth,
                folder: _options.folder,
                filter: _options.filter,
                allowMultiSelect: _options.multi,
                allowSelectFolder: false
            });
            _fileManager.init();
            _fileManager.grid().maxHeight(350);

            _isInited = true;
        }

        this.open = function () {
            _this.init();
            _dialog.open();
        }

        this.close = function () {
            _dialog.close();
        }
    };

})(jQuery);