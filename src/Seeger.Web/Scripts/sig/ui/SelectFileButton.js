﻿(function ($) {

    var sig = window.sig = window.sig || {};
    sig.ui = sig.ui || {};

    sig.ui.SelectFileButton = {
        init: function (button, options) {
            $(button).each(function () {
                var $button = $(this);

                var filter = $button.data('filter');
                var aspNetAuth = $button.data('aspNetAuth');
                var closeDialogOnOk = $button.data('close-dialog-on-ok');
                var multi = $button.data('multi');
                var folder = $button.data('folder');
                var bucketId = $button.data('bucket-id');

                if (closeDialogOnOk === undefined) {
                    closeDialogOnOk = true;
                }

                var dialogOptions = {
                    bucketId: bucketId,
                    folder: folder,
                    filter: filter,
                    multi: multi,
                    aspNetAuth: aspNetAuth,
                    allowMultiSelect: false,
                    onOK: function (files) {
                        var updateTarget = $button.data('update-target');
                        if (updateTarget) {
                            var updateTargetAttr = $button.data('update-target-attr') || 'value';
                            $(updateTarget).attr(updateTargetAttr, files[0].publicUri);
                        }

                        if (closeDialogOnOk) {
                            $button.data('SelectFileDialog').close();
                        }
                    }
                };

                if (options) {
                    dialogOptions = $.extend(true, dialogOptions, options);
                }

                var dialog = new sig.ui.SelectFileDialog(dialogOptions);

                $button.data('SelectFileDialog', dialog);

                $button.click(function () {
                    $(this).data('SelectFileDialog').open();
                    return false;
                });
            });
        }
    };

    $(function () {
        sig.ui.SelectFileButton.init($('[data-toggle="select-file"]'));
    });

})(jQuery);