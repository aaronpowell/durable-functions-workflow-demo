const df = require('durable-functions');

module.exports = async function (context, req) {
    context.log('JavaScript HTTP trigger function processed a request.');

    let name = req.params.input;

    let client = df.getClient(context);

    await client.startNew('Workflow', undefined, name);

    context.res = {
        // status: 200, /* Defaults to 200 */
        body: 'workflow is running'
    };

    context.done();
};