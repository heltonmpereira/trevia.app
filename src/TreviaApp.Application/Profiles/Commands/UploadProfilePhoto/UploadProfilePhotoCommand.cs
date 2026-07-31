namespace TreviaApp.Application.Profiles.Commands.UploadProfilePhoto;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Profiles.Responses;

public sealed record UploadProfilePhotoCommand(
    byte[] FileBytes,
    string FileName,
    string ContentType,
    long SizeBytes) : ICommand<PhotoUploadResultResponse>;
