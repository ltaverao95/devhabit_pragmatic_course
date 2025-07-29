using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("habits")]
public sealed class HabitsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HabitsCollectionDto>> GetHabits()
    {
        List<HabitDto> habits = await dbContext
            .Habits
            .Select(x => new HabitDto
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
            })
            .ToListAsync();

        var habitsCollectionDto = new HabitsCollectionDto
        {
            Data = habits
        };

        return Ok(habitsCollectionDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HabitDto>> GetHabit(string id)
    {
        HabitDto? habit = await dbContext
            .Habits
            .Where(x => x.Id == id)
            .Select(x => new HabitDto
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
            })
            .FirstOrDefaultAsync();

        if (habit == null)
        {
            return NotFound();
        }

        return Ok(habit);
    }
}
