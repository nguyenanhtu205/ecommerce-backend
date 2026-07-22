using FluentValidation;
using FluentValidation.Results;
using ValidationException = Common.Domain.Exceptions.ValidationException;

namespace Common.Application.Behaviours;

public class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        ValidationResult[] validationResults = await Task.WhenAll(
            validators.Select(v =>
                v.ValidateAsync(new ValidationContext<TRequest>(request), cancellationToken)));

        List<ValidationFailure> errors = validationResults
            .Where(r => r.Errors.Count > 0)
            .SelectMany(r => r.Errors)
            .ToList();

        if (errors.Count != 0)
        {
            throw new ValidationException(errors);
        }

        return await next(cancellationToken);
    }
}
