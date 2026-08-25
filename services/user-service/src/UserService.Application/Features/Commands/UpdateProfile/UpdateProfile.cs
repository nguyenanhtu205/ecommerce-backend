namespace UserService.Application.Features.Commands.UpdateProfile;

public record UpdateProfileCommand(
    string? DisplayName,
    string? AvatarUrl,
    Gender? Gender,
    DateOnly? DateOfBirth) : IRequest;

public class UpdateProfile(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    ITopicProducer<AvatarMediaAttached> producer)
    : IRequestHandler<UpdateProfileCommand>
{
    public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
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

        profile.DisplayName = request.DisplayName ?? profile.DisplayName;
        profile.AvatarUrl = request.AvatarUrl ?? profile.AvatarUrl;
        profile.Gender = request.Gender ?? profile.Gender;
        profile.DateOfBirth = request.DateOfBirth ?? request.DateOfBirth;
        profile.UpdatedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        if (request.AvatarUrl != null)
        {
            await producer.Produce(new AvatarMediaAttached(currentUser.UserId.Value.ToString(),
                [new MediaAttachmentItem(request.AvatarUrl, "avatar", 0)],
                DateTimeOffset.UtcNow), cancellationToken);
        }
    }
}
