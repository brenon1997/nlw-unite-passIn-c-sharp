using Microsoft.EntityFrameworkCore;
using PassIn.Communication.Responses;
using PassIn.Exceptions;
using PassIn.Infrastructure;

namespace PassIn.Application.UseCases.Attendees.GetAllByEventId;

public class GetAllAttendeesByEventIdUseCase
{
    private readonly PassInDbContext _dbContext;

    public GetAllAttendeesByEventIdUseCase()
    {
        _dbContext = new PassInDbContext();
    }
    public ResponseAllAttendeesJson Execute(Guid eventId)
    {
        var entityEvent = _dbContext.Events.Include(ev => ev.Attendees).ThenInclude(att => att.CheckIn).FirstOrDefault(ev => ev.Id == eventId);

        return entityEvent is null ?
            throw new NotFoundException("An event with this id dont exist.") :
            new ResponseAllAttendeesJson
            {
               Attendees = entityEvent.Attendees.Select(att => new ResponseAttendeeJson
               {
                   Id = att.Id,
                   Name = att.Name,
                   Email = att.Email,
                   CreatedAt = att.Created_At,
                   CheckedInAt = att.CheckIn?.Created_at
               }).ToList()
            };

    }
}
