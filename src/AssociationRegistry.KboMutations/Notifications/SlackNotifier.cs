using Amazon.Lambda.Core;
using Slack.Webhooks;

namespace AssociationRegistry.KboMutations.Notifications;

public class SlackNotifier : INotifier
{
    private readonly ILambdaLogger _logger;
    private SlackClient _slackClient;

    public SlackNotifier(ILambdaLogger logger, string webhookUrl)
    {
        if (webhookUrl == null) throw new ArgumentNullException(nameof(webhookUrl));
        _logger = logger;

        _slackClient = new SlackClient(webhookUrl);
    }

    public async Task NotifyLambdaTriggered()
        => await PostSlackAsync(Emoji.Bulb, $"KBO sync mutation lambda has started.");

    public async Task NotifyLambdaFinished()
        => await PostSlackAsync(Emoji.Bulb, $"KBO sync mutation lambda has started.");

    public async Task NotifyLambdaFailed(string exceptionMessage)
        => await PostSlackAsync(Emoji.Anger, $"KBO sync mutation lambda has encountered an exception! '{exceptionMessage}'");

    public async Task NotifyDownloadFileSuccess(int numberOfFiles)
        => await PostSlackAsync(Emoji.Up, $"Kbo Mutaties Lambda heeft {numberOfFiles} bestanden opgehaald van Magda");

    public async Task NotifyFailure(string reason)
        => await PostSlackAsync(Emoji.X, $"Er zijn fouten opgetreden tijdens bij het ophalen van mutaties in de Kbo Mutaties Lambda: `{reason}`");

    private async Task PostSlackAsync(string emoji, string text)
    {
        var postAsync = await _slackClient.PostAsync(new SlackMessage
        {
            Channel = string.Empty,
            Markdown = true,
            Text = text,
            IconEmoji = emoji,
            Username = "Kbo Sync"
        });
        
        if(!postAsync)
            _logger.LogWarning("Could not notify slack");
        
    }
}