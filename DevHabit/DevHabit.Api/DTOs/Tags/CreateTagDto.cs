using System.ComponentModel.DataAnnotations;
using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Tags;

public sealed record CreateTagDto
{
    public string Name { get; set; }
    public string? Description { get; set; }
}
