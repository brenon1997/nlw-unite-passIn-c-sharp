using Microsoft.EntityFrameworkCore;
using PassIn.Communication.Requests;
using PassIn.Communication.Responses;
using PassIn.Exceptions;
using PassIn.Infrastructure;
using System.Net.Mail;
using System.Reflection.Metadata.Ecma335;

namespace PassIn.Application.UseCases.Events.RegisterAttendee;

public class RegisterAttedeeOnEventUseCase
{
    private readonly PassInDbContext _dbContext;

    public RegisterAttedeeOnEventUseCase()
    {
        _dbContext = new PassInDbContext();
    }
    public ResponseRegisteredJson Execute(Guid eventId, RequestRegisterEventJson request)
    {
        Validate(eventId, request);

        var entityAttendee = new Infrastructure.Entities.Attendee
        {
            Email = request.Email,
            Name = request.Name,
            Event_Id = eventId,
            Created_At = DateTime.UtcNow,
        };

        _dbContext.Attendees.Add(entityAttendee);
        _dbContext.SaveChanges();

        return new ResponseRegisteredJson
        {
            Id = entityAttendee.Id,
        };
    }

    private void Validate(Guid eventId, RequestRegisterEventJson request)
    {
        var eventEntity = _dbContext.Events.Find(eventId);
        if (eventEntity is null)
            throw new NotFoundException("An event with this id dont exist.");

        if(string.IsNullOrWhiteSpace(request.Name))
            throw new ErrorOnValidationException("The name is invalid.");

        if(!EmailIsValid(request.Email))
            throw new ErrorOnValidationException("The e-mail is invalid.");

        var attendeeAlreadyRegistered = _dbContext.Attendees.Any(att => att.Email.Equals(request.Email) && att.Event_Id == eventId);

        if(attendeeAlreadyRegistered)
            throw new ConflictException("You can not register twice on the same event.");

        var attendeesForEvent =  _dbContext.Attendees.Count(att => att.Event_Id == eventId);
        if(attendeesForEvent >= eventEntity.Maximum_Attendees)
            throw new ForbiddenException("There is no room for this event.");

    }

    private bool EmailIsValid(string email)
    {
        try
        {
            new MailAddress(email);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
