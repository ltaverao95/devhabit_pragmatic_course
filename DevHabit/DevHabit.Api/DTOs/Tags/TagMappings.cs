using DevHabit.Api.DTOs.Tags;
using DevHabit.Api.Entities;
using DevTag.Api.DTOs.Tags;

namespace DevTag.Api.DTOs.Tags;

internal static class TagMappings
{
    public static TagDto ToDto(this Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description,
            CreatedAtUtc = tag.CreatedAtUtc,
            UpdatedAtUtc = tag.UpdatedAtUtc
        };
    }

    public static void UpdateFromDto(this Tag tag, UpdateTagDto updateTagDto)
    {
        tag.Name = updateTagDto.Name;
        tag.Description = updateTagDto.Description;
        tag.UpdatedAtUtc = DateTime.UtcNow;
    }

    public static Tag ToEntity(this CreateTagDto dto)
    {
        Tag tag = new()
        {
            Id = $"h_{Guid.CreateVersion7()}",
            Name = dto.Name,
            Description = dto.Description,
            CreatedAtUtc = DateTime.UtcNow
        };

        return tag;
    }
}
