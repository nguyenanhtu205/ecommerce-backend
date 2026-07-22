using FluentValidation.Results;

namespace Common.Domain.Exceptions;

public class ValidationException(List<ValidationFailure> errors)
    : Exception("One or more validation failures have occurred")
{
    public List<ValidationFailure> Errors { get; } = errors;
}
