using DevHabit.Api.Entities;
using DevHabit.Api.Services.Sorting;

namespace DevHabit.Api.DTOs.Habits;

internal static class HabitMappings
{
    public static readonly SortMappingDefinition<HabitDto, Habit> SortMapping = new()
    {
        Mappings =
        [
            new SortMapping(nameof(Habit.Name), nameof(Habit.Name)),
            new SortMapping(nameof(Habit.Description), nameof(Habit.Description)),
            new SortMapping(nameof(Habit.Type), nameof(Habit.Type)),
            new SortMapping(
                $"{nameof(HabitDto.Frequency)}.{nameof(FrequencyDto.Type)}",
                $"{nameof(Habit.Frequency)}.{nameof(Frequency.Type)}"),
            new SortMapping(
                $"{nameof(HabitDto.Frequency)}.{nameof(FrequencyDto.TimesPerPeriod)}",
                $"{nameof(Habit.Frequency)}.{nameof(Frequency.TimesPerPeriod)}"),
            new SortMapping(
                $"{nameof(HabitDto.Target)}.{nameof(TargetDto.Value)}",
                $"{nameof(Habit.Target)}.{nameof(Target.Value)}"),
            new SortMapping(
                $"{nameof(HabitDto.Target)}.{nameof(TargetDto.Unit)}",
                $"{nameof(Habit.Target)}.{nameof(Target.Unit)}"),
            new SortMapping(nameof(Habit.Status), nameof(Habit.Status)),
            new SortMapping(nameof(Habit.EndDate), nameof(Habit.EndDate)),
            new SortMapping(nameof(Habit.CreatedAtUtc), nameof(Habit.CreatedAtUtc)),
            new SortMapping(nameof(Habit.UpdatedAtUtc), nameof(Habit.UpdatedAtUtc)),
            new SortMapping(nameof(Habit.LastCompletedAtUtc), nameof(Habit.LastCompletedAtUtc))
        ]
    };

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

    public static void UpdateFromDto(this Habit habit, UpdateHabitDto updateHabitDto)
    {
        habit.Name = updateHabitDto.Name;
        habit.Description = updateHabitDto.Description;
        habit.Type = updateHabitDto.Type;
        habit.EndDate = updateHabitDto.EndDate;

        habit.Frequency = new Frequency
        {
            Type = updateHabitDto.Frequency.Type,
            TimesPerPeriod = updateHabitDto.Frequency.TimesPerPeriod
        };

        habit.Target = new Target
        {
            Value = updateHabitDto.Target.Value,
            Unit = updateHabitDto.Target.Unit
        };

        if (updateHabitDto.Milestone != null)
        {
            habit.Milestone ??= new Milestone();
            habit.Milestone.Target = updateHabitDto.Milestone.Target;
        }

        habit.UpdatedAtUtc = DateTime.UtcNow;
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
