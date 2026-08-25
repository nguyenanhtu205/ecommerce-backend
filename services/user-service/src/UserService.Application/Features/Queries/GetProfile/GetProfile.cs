namespace UserService.Application.Features.Queries.GetProfile;

public record ProfileDto(
    string DisplayName,
    string? AvatarUrl,
    Gender? Gender,
    DateOnly? DateOfBirth,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record GetProfileQuery : IRequest<ProfileDto>;

public class GetProfile(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<GetProfileQuery, ProfileDto>
{
    public async Task<ProfileDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        Profile? profile = await context.Profiles.FindAsync([currentUser.UserId.Value], cancellationToken);

        if (profile is null)
        {
            throw new NotFoundException("Profile not found");
        }

        return new ProfileDto(profile.DisplayName, profile.AvatarUrl, profile.Gender, profile.DateOfBirth,
            profile.CreatedAt, profile.UpdatedAt);
    }
}
