using Kosmozeki.Application.Common;
using Kosmozeki.Application.Notes.CreateNote;
using Kosmozeki.Application.Notes.DeleteNote;
using Kosmozeki.Application.Notes.GetRoomNotes;
using Kosmozeki.Application.Notes.UpdateNote;
using Kosmozeki.Contracts.Notes.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace Kosmozeki.Application.DependencyInjection;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateNoteCommand, NoteDto>, CreateNoteCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateNoteCommand>, UpdateNoteCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteNoteCommand>, DeleteNoteCommandHandler>();

        services.AddScoped<IQueryHandler<GetRoomNotesQuery, IReadOnlyList<NoteDto>>, GetRoomNotesQueryHandler>();

        return services;
    }
}
