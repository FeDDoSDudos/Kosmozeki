using Kosmozeki.Api.Contracts.Notes;
using Kosmozeki.Application.Common;
using Kosmozeki.Application.Notes.CreateNote;
using Kosmozeki.Application.Notes.DeleteNote;
using Kosmozeki.Application.Notes.GetRoomNotes;
using Kosmozeki.Application.Notes.UpdateNote;
using Kosmozeki.Contracts.Notes.Dtos;
using Kosmozeki.Domain.Notes;
using Microsoft.AspNetCore.Mvc;

namespace Kosmozeki.Server.Controllers;

[ApiController]
[Route("api/rooms/{roomId:guid}/notes")]
public sealed class NotesController : ControllerBase
{
    private readonly ICommandHandler<CreateNoteCommand, NoteDto> _createNoteHandler;
    private readonly ICommandHandler<UpdateNoteCommand> _updateNoteHandler;
    private readonly ICommandHandler<DeleteNoteCommand> _deleteNoteHandler;
    private readonly IQueryHandler<GetRoomNotesQuery, IReadOnlyList<NoteDto>> _getRoomNotesHandler;

    public NotesController(
        ICommandHandler<CreateNoteCommand, NoteDto> createNoteHandler,
        ICommandHandler<UpdateNoteCommand> updateNoteHandler,
        ICommandHandler<DeleteNoteCommand> deleteNoteHandler,
        IQueryHandler<GetRoomNotesQuery, IReadOnlyList<NoteDto>> getRoomNotesHandler)
    {
        _createNoteHandler = createNoteHandler;
        _updateNoteHandler = updateNoteHandler;
        _deleteNoteHandler = deleteNoteHandler;
        _getRoomNotesHandler = getRoomNotesHandler;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NoteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoomNotes(
        [FromRoute] Guid roomId,
        [FromQuery] bool @private,
        CancellationToken ct)
    {
        var result = await _getRoomNotesHandler.HandleAsync(
            new GetRoomNotesQuery(roomId, @private),
            ct);

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Create(
        [FromRoute] Guid roomId,
        [FromBody] CreateNoteRequest request,
        CancellationToken ct)
    {
        var visibility = Enum.Parse<NoteVisibility>(request.Visibility, ignoreCase: true);

        await _createNoteHandler.HandleAsync(
            new CreateNoteCommand(
                roomId,
                request.AuthorPlayerId,
                request.Content,
                visibility),
            ct);

        return NoContent();
    }

    [HttpPut("{noteId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid roomId,
        [FromRoute] Guid noteId,
        [FromBody] UpdateNoteRequest request,
        CancellationToken ct)
    {
        var visibility = Enum.Parse<NoteVisibility>(request.Visibility, ignoreCase: true);

        await _updateNoteHandler.HandleAsync(
            new UpdateNoteCommand(
                roomId,
                noteId,
                request.Content,
                visibility),
            ct);

        return NoContent();
    }

    [HttpDelete("{noteId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid roomId,
        [FromRoute] Guid noteId,
        CancellationToken ct)
    {
        await _deleteNoteHandler.HandleAsync(
            new DeleteNoteCommand(roomId, noteId),
            ct);

        return NoContent();
    }
}