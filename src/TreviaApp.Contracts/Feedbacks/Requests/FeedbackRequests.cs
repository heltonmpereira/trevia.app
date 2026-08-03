using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Feedbacks.Requests;

public sealed record CreateWorkoutFeedbackRequest
{
    public CreateWorkoutFeedbackRequest() { }

    public CreateWorkoutFeedbackRequest(
        string text,
        FeedbackTone tone = FeedbackTone.Neutral,
        bool isPublic = true)
    {
        Text = text;
        Tone = tone;
        IsPublic = isPublic;
    }

    public string Text { get; init; } = string.Empty;
    public FeedbackTone Tone { get; init; } = FeedbackTone.Neutral;
    public bool IsPublic { get; init; } = true;
}

public sealed record CreateExerciseFeedbackRequest
{
    public CreateExerciseFeedbackRequest() { }

    public CreateExerciseFeedbackRequest(
        string text,
        FeedbackTone tone = FeedbackTone.Neutral,
        bool isPublic = true)
    {
        Text = text;
        Tone = tone;
        IsPublic = isPublic;
    }

    public string Text { get; init; } = string.Empty;
    public FeedbackTone Tone { get; init; } = FeedbackTone.Neutral;
    public bool IsPublic { get; init; } = true;
}

public sealed record CreateSetFeedbackRequest
{
    public CreateSetFeedbackRequest() { }

    public CreateSetFeedbackRequest(
        string text,
        FeedbackTone tone = FeedbackTone.Neutral,
        bool isPublic = true,
        string? mediaReferenceUrl = null)
    {
        Text = text;
        Tone = tone;
        IsPublic = isPublic;
        MediaReferenceUrl = mediaReferenceUrl;
    }

    public string Text { get; init; } = string.Empty;
    public FeedbackTone Tone { get; init; } = FeedbackTone.Neutral;
    public bool IsPublic { get; init; } = true;
    public string? MediaReferenceUrl { get; init; }
}

public sealed record UpdateFeedbackRequest
{
    public UpdateFeedbackRequest() { }

    public UpdateFeedbackRequest(
        string text,
        FeedbackTone tone,
        bool? isPublic = null,
        string? mediaReferenceUrl = null)
    {
        Text = text;
        Tone = tone;
        IsPublic = isPublic;
        MediaReferenceUrl = mediaReferenceUrl;
    }

    public string Text { get; init; } = string.Empty;
    public FeedbackTone Tone { get; init; }
    public bool? IsPublic { get; init; }
    public string? MediaReferenceUrl { get; init; }
}

public sealed record RespondToExerciseFeedbackRequest
{
    public RespondToExerciseFeedbackRequest() { }

    public RespondToExerciseFeedbackRequest(string responseText)
    {
        ResponseText = responseText;
    }

    public string ResponseText { get; init; } = string.Empty;
}
