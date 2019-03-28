const df = require('durable-functions');

module.exports = df.orchestrator(function *(context) {
    const input = context.df.getInput();

    context.log(`Here's the input: ${input}`);

    let evt = context.df.waitForExternalEvent('foo');

    yield context.df.Task.any([evt]);

    context.done();
});