namespace ShippingService.Application.Features.Queries.GetCarriers;

public record GetCarriesQueryResponse(string CarrierId, string Code, string Name);

public record GetCarriersQuery : IRequest<List<GetCarriesQueryResponse>>;

public class GetCarriers(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<GetCarriersQuery, List<GetCarriesQueryResponse>>
{
    public async Task<List<GetCarriesQueryResponse>> Handle(GetCarriersQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        if (!currentUser.IsSeller)
        {
            throw new ForbiddenAccessException();
        }

        List<Carrier> carries = await context.Carriers
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return [.. carries.Select(c => new GetCarriesQueryResponse(c.Id.ToString(), c.Code, c.Name))];
    }
}
