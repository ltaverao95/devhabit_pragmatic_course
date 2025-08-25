namespace DevHabit.Api.DTOs.Tags;

public sealed record TagsCollectionDto
{
    public List<TagDto> Data { get; init; } = new();
}

public sealed class TagDto
{
    public string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
