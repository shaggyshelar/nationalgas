module.exports = function (callback) {
    var jsreport = require('jsreport-core')();

    jsreport.init().then(function () {
        return jsreport.render({
            template: {
                content: '<h1>Hello {{:foo}}</h1>',
                engine: 'jsrender',
                recipe: 'phantom-pdf'
            },
            data: {
                foo: "world"
            }
        }).then(function (resp) {
            callback(/* error */ null, resp.content.toJSON().data);
        });
    }).catch(function (e) {
        callback(/* error */ e, null);
    });
};