namespace Woo.AICopilot.SharedKernel.Result;

public interface IResult
{
    IEnumerable<object>? Errors { get; }

    bool IsSuccess { get; }

    ResultStatus Status { get; }

    object? GetValue();
}