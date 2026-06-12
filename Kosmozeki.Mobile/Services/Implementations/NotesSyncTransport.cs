using Kosmozeki.Api.Contracts.Notes;
using Kosmozeki.Contracts.Notes.Dtos;
using Kosmozeki.Mobile.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Kosmozeki.Mobile.Services;

public sealed class NotesSyncTransport : INotesSyncTransport
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public NotesSyncTransport(HttpClient httpClient, IOptions<ServerOptions> options)
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/')
            ?? throw new InvalidOperationException("ServerOptions.BaseUrl is not configured.");

        httpClient.BaseAddress = new Uri($"{baseUrl}/");
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<NoteDto>> PullRoomNotesAsync(
        Guid roomId,
        bool includePrivate,
        CancellationToken ct = default)
    {
        var url = $"api/rooms/{roomId:D}/notes?private={includePrivate.ToString().ToLowerInvariant()}";

        var result = await _httpClient.GetFromJsonAsync<IReadOnlyList<NoteDto>>(url, JsonOptions, ct);
        return result ?? Array.Empty<NoteDto>();
    }

    public async Task PushUpsertAsync(NoteDto note, CancellationToken ct = default)
    {
        if (note.IsDeleted)
        {
            await PushDeleteAsync(note.RoomId, note.Id, ct);
            return;
        }

        var request = new UpsertNoteRequest(
            note.Id,
            note.AuthorPlayerId,
            note.Content,
            note.Visibility,
            note.UpdatedAt);

        var response = await _httpClient.PutAsJsonAsync(
            $"api/rooms/{note.RoomId:D}/notes/{note.Id:D}",
            request,
            JsonOptions,
            ct);

        await EnsureSuccessAsync(response, ct);
    }

    public async Task PushDeleteAsync(
        Guid roomId,
        Guid noteId,
        CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/rooms/{roomId:D}/notes/{noteId:D}",
            ct);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return;

        await EnsureSuccessAsync(response, ct);
    }

    private async Task<bool> ExistsAsync(
        Guid roomId,
        Guid noteId,
        CancellationToken ct)
    {
        var notes = await PullRoomNotesAsync(roomId, includePrivate: true, ct);
        return notes.Any(x => x.Id == noteId);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = response.Content is null
            ? null
            : await response.Content.ReadAsStringAsync(ct);

        throw new HttpRequestException(
            $"Notes sync request failed: {(int)response.StatusCode} {response.ReasonPhrase}. {body}");
    }
}