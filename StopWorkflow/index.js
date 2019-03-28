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

    let instance = instances.filter(i => i.input === input && i.name === 'Workflow')[0];

    if (instance) {
        context.log(`Found the workflow with id ${instance.instanceId}. Time to kill it!`);
        await client.raiseEvent(instance.instanceId, 'workflow');
        context.res = {
            body: [instance]
        };
    } else {
        context.log(`Didn't find a matching workflow.`);
        context.res = {
            status: 404,
            body: instances
        };
    }

    context.done();
};