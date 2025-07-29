using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Habits;

internal static class HabitMappings
{
    public static HabitDto ToDto(this Habit habit)
    {
        return new HabitDto
        {
            Id = habit.Id,
            Name = habit.Name,
            Description = habit.Description,
            Type = habit.Type,
            CreatedAtUtc = habit.CreatedAtUtc,
            Frequency = new FrequencyDto
            {
                Type = habit.Frequency.Type,
                TimesPerPeriod = habit.Frequency.TimesPerPeriod
            },
            IsArchived = habit.IsArchived,
            Status = habit.Status,
            Target = new TargetDto
            {
                Value = habit.Target.Value,
                Unit = habit.Target.Unit
            },
            EndDate = habit.EndDate,
            LastCompletedAtUtc = habit.LastCompletedAtUtc,
            Milestone = habit.Milestone == null ? null : new MilestoneDto
            {
                Target = habit.Milestone.Target,
                Current = habit.Milestone.Current
            },
            UpdatedAtUtc = habit.UpdatedAtUtc
        };
    }

    public static Habit ToEntity(this CreateHabitDto dto)
    {
        Habit habit = new()
        {
            Id = $"h_{Guid.CreateVersion7()}",
            Name = dto.Name,
            Description = dto.Description,
            Type = dto.Type,
            CreatedAtUtc = DateTime.UtcNow,
            Frequency = new Frequency
            {
                Type = dto.Frequency.Type,
                TimesPerPeriod = dto.Frequency.TimesPerPeriod
            },
            IsArchived = false,
            Status = HabitStatus.Ongoing,
            Target = new Target
            {
                Value = dto.Target.Value,
                Unit = dto.Target.Unit
            },
            EndDate = dto.EndDate,
            LastCompletedAtUtc = null,
            Milestone = dto.Milestone == null ? null : new Milestone
            {
                Target = dto.Milestone.Target,
                Current = dto.Milestone.Current
            }
        };

        return habit;
    }
}
