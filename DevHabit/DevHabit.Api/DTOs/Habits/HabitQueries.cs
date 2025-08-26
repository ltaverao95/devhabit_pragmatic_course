using System.Linq.Expressions;
using DevHabit.Api.DTOs.HabitTags;
using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Habits;

internal static class HabitQueries
{
    public static Expression<Func<Habit, HabitDto>> ProjectToDto()
    {
        return x => new HabitDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            Type = x.Type,
            CreatedAtUtc = x.CreatedAtUtc,
            Frequency = new FrequencyDto
            {
                Type = x.Frequency.Type,
                TimesPerPeriod = x.Frequency.TimesPerPeriod
            },
            IsArchived = x.IsArchived,
            Status = x.Status,
            Target = new TargetDto
            {
                Value = x.Target.Value,
                Unit = x.Target.Unit
            },
            EndDate = x.EndDate,
            LastCompletedAtUtc = x.LastCompletedAtUtc,
            Milestone = x.Milestone == null
                            ? null
                            : new MilestoneDto
                            {
                                Target = x.Milestone.Target,
                                Current = x.Milestone.Current
                            },
            UpdatedAtUtc = x.UpdatedAtUtc
        };
    }

    public static Expression<Func<Habit, HabitTagDto>> ProjectToDtoWithTags()
    {
        return x => new HabitTagDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            Type = x.Type,
            CreatedAtUtc = x.CreatedAtUtc,
            Frequency = new FrequencyDto
            {
                Type = x.Frequency.Type,
                TimesPerPeriod = x.Frequency.TimesPerPeriod
            },
            IsArchived = x.IsArchived,
            Status = x.Status,
            Target = new TargetDto
            {
                Value = x.Target.Value,
                Unit = x.Target.Unit
            },
            Tags = x.Tags.Select(t => t.Name).ToArray(),
            EndDate = x.EndDate,
            LastCompletedAtUtc = x.LastCompletedAtUtc,
            Milestone = x.Milestone == null
                            ? null
                            : new MilestoneDto
                            {
                                Target = x.Milestone.Target,
                                Current = x.Milestone.Current
                            },
            UpdatedAtUtc = x.UpdatedAtUtc
        };
    }
}
