using Microsoft.EntityFrameworkCore;
using PassIn.Communication.Responses;
using PassIn.Exceptions;
using PassIn.Infrastructure;

namespace PassIn.Application.UseCases.Events.GetById;

public class GetEventByIdUseCase
{
    public ResponseEventJson Execute(Guid id)
    {
        var dbContext = new PassInDbContext();

        var entityEvent = dbContext.Events.Include(ev => ev.Attendees).FirstOrDefault(ev => ev.Id == id);

        return entityEvent is null ?
            throw new NotFoundException("An event with this id dont exist.") :
            new ResponseEventJson {
                Id = id,
                Title = entityEvent.Title,
                Details = entityEvent.Details,
                MaximumAttendees = entityEvent.Maximum_Attendees,
                AttendeesAmount = entityEvent.Attendees.Count(),
            };
    }
}
