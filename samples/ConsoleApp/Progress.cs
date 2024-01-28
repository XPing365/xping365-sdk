using Microsoft.Extensions.Logging;
using XPing365.Sdk.Core.Components;

namespace ConsoleApp;

sealed class Progress(ILogger<Program> logger) : IProgress<TestStep>
{
    private readonly ILogger<Program> _logger = logger;

    public void Report(TestStep value)
    {
        switch (value.Result)
        {
            case TestStepResult.Succeeded:
                _logger.LogInformation("{Value}", value.ToString());
                break;
            case TestStepResult.Failed:
                _logger.LogError("{Value}", value.ToString());
                break;
        }
    }
}
