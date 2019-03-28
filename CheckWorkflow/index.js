const df = require('durable-functions');
const moment = require('moment');

module.exports = async function (context, req) {
    let input = req.params.input;

    let client = df.getClient(context);

    let offset = moment.duration(20, 'minutes');
    let time = moment.utc();

    let instances = await client.getStatusBy(
        time.subtract(offset).toDate(),
        time.add(offset).toDate(),
        []
    );

    context.res = {
        body: {
            instances,
            instancesForInput: instances.filter(i => i.input === input && i.name === 'Workflow')
        }
    };
    context.done();
};