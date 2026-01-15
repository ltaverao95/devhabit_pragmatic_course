using System.Dynamic;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.DTOs.HabitTags;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
using DevHabit.Api.Services.Sorting;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("habits")]
public sealed class HabitsController(ApplicationDbContext dbContext, LinkService linkService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHabits([FromQuery] HabitsQueryParameters query,
                                               SortMappingProvider sortMappingProvider,
                                               DataShapingService dataShapingService)
    {
        if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(query.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided sort parameter isn´t valid: '{query.Sort}'");
        }

        ArgumentNullException.ThrowIfNull(sortMappingProvider);

        if (!dataShapingService.Validate<HabitDto>(query.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shaping fields parameter isn´t valid: '{query.Fields}'");
        }

        query.Search ??= query.Search?.Trim().ToLower();

        SortMapping[] sortMappings = sortMappingProvider.GetMappings<HabitDto, Habit>();

        IQueryable<HabitDto> habitsQuery = dbContext
            .Habits
            .Where(h => query.Search == null ||
                h.Name.ToLower().Contains(query.Search) ||
                h.Description != null && h.Description.ToLower().Contains(query.Search))
            .Where(h => query.Type == null || h.Type == query.Type)
            .Where(h => query.Status == null || h.Status == query.Status)
            .ApplySort(query.Sort, sortMappings)
            .Select(HabitQueries.ProjectToDto());

        int totalCount = await habitsQuery.CountAsync();
        List<HabitDto> habits = await habitsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        PaginationResult<ExpandoObject> paginationResult = new()
        {
            Items = dataShapingService.ShapeCollectionData(
                habits,
                query.Fields,
                h => CreateLinksForHabit(h.Id, query.Fields)),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };

        paginationResult.Links = CreateLinksForHabits(query,
                                                      paginationResult.HasNextPage,
                                                      paginationResult.HasPreviousPage);

        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetHabit(string id,
                                              string? fields,
                                              DataShapingService dataShapingService)
    {
        if (!dataShapingService.Validate<HabitWithTagsDto>(fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shaping fields parameter isn´t valid: '{fields}'");
        }



        HabitTagDto? habit = await dbContext
            .Habits
            .Where(x => x.Id == id)
            .Select(HabitQueries.ProjectToDtoWithTags())
            .FirstOrDefaultAsync();

        if (habit == null)
        {
            return NotFound();
        }

        ExpandoObject shapedHabitDto = dataShapingService.ShapeData(habit, fields);

        List<LinkDto> links = CreateLinksForHabit(id, fields);

        shapedHabitDto.TryAdd("links", links);

        return Ok(shapedHabitDto);
    }

    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitDto createHabitDto, IValidator<CreateHabitDto> validator)
    {
        await validator.ValidateAndThrowAsync(createHabitDto);

        if (createHabitDto == null)
        {
            return BadRequest("Habit data is required.");
        }

        Habit habit = createHabitDto.ToEntity();

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        HabitDto habitDto = habit.ToDto();

        habitDto.Links = CreateLinksForHabit(habit.Id, null);

        return CreatedAtAction(nameof(GetHabit), new { id = habitDto.Id }, habitDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id, [FromBody] UpdateHabitDto updateHabitDto)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(x => x.Id == id);
        if (habit == null)
        {
            return NotFound();
        }

        habit.UpdateFromDto(updateHabitDto);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchHabit(string id, [FromBody] JsonPatchDocument<HabitDto> patchDocument)
    {
        if (patchDocument == null)
        {
            return BadRequest("Patch document is required.");
        }

        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(x => x.Id == id);
        if (habit == null)
        {
            return NotFound();
        }
        HabitDto habitDto = habit.ToDto();
        patchDocument.ApplyTo(habitDto, ModelState);

        if (!TryValidateModel(habitDto))
        {
            return ValidationProblem(ModelState);
        }

        habit.Name = habitDto.Name;
        habit.Description = habitDto.Description;
        habit.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit(string id)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(x => x.Id == id);
        if (habit == null)
        {
            return NotFound();
        }

        dbContext.Habits.Remove(habit);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private List<LinkDto> CreateLinksForHabits(HabitsQueryParameters habitsQueryParameters,
                                               bool hasNextPage,
                                               bool hasPreviousPage)
    {
        List<LinkDto> links = [
            linkService.Create(nameof(GetHabits),
                               "self",
                               HttpMethods.Get,
                               new
                               {
                                   page = habitsQueryParameters.Page,
                                   pageSize = habitsQueryParameters.PageSize,
                                   fields = habitsQueryParameters.Fields,
                                   q = habitsQueryParameters.Search,
                                   sort = habitsQueryParameters.Sort,
                                   type = habitsQueryParameters.Type,
                                   status = habitsQueryParameters.Status
                               }),
            linkService.Create(nameof(CreateHabit), "create", HttpMethods.Post)
        ];

        if (hasNextPage)
        {
            links.Add(linkService.Create(nameof(GetHabits),
                               "next-page",
                               HttpMethods.Get,
                               new
                               {
                                   page = habitsQueryParameters.Page + 1,
                                   pageSize = habitsQueryParameters.PageSize,
                                   fields = habitsQueryParameters.Fields,
                                   q = habitsQueryParameters.Search,
                                   sort = habitsQueryParameters.Sort,
                                   type = habitsQueryParameters.Type,
                                   status = habitsQueryParameters.Status
                               }));
        }

        if (hasPreviousPage)
        {
            links.Add(linkService.Create(nameof(GetHabits),
                               "previous-page",
                               HttpMethods.Get,
                               new
                               {
                                   page = habitsQueryParameters.Page - 1,
                                   pageSize = habitsQueryParameters.PageSize,
                                   fields = habitsQueryParameters.Fields,
                                   q = habitsQueryParameters.Search,
                                   sort = habitsQueryParameters.Sort,
                                   type = habitsQueryParameters.Type,
                                   status = habitsQueryParameters.Status
                               }));
        }

        return links;
    }

    private List<LinkDto> CreateLinksForHabit(string id, string? fields)
    {
        return [
            linkService.Create(nameof(GetHabit), "self", HttpMethods.Get, new { id, fields }),
            linkService.Create(nameof(GetHabit), "update", HttpMethods.Put, new { id }),
            linkService.Create(nameof(GetHabit), "partial-update", HttpMethods.Patch, new { id }),
            linkService.Create(nameof(GetHabit), "delete", HttpMethods.Delete, new { id }),
            linkService.Create(nameof(HabitTagsController.UpsertHabitTags), "upsert-tags", HttpMethods.Put, new { habitId = id }, HabitTagsController.Name)
        ];
    }
}
