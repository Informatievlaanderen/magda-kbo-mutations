using Amazon.Lambda.Core;
using AssocationRegistry.KboMutations.Models;
using Slack.Webhooks;

namespace AssociationRegistry.KboMutations.MutationLambdaContainer.Notifications;

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

    public async Task NotifySuccess(int numberOfFiles)
    {
        var postAsync = await _slackClient.PostAsync(new SlackMessage
        {
            Channel = string.Empty,
            Text = $"Kbo Mutaties Lambda heeft {numberOfFiles} bestanden opgehaald van Magda",
            IconEmoji = Emoji.Up,
            Username = "Kbo Sync"
        });
        
        if(!postAsync)
            _logger.LogWarning("Could not notify slack");
    }
    
    public async Task NotifyFailure(string reason)
    {
        var postAsync = await _slackClient.PostAsync(new SlackMessage
        {
            Channel = string.Empty,
            Markdown = true,
            Text = $"Er zijn fouten opgetreden tijdens bij het ophalen van mutaties in de Kbo Mutaties Lambda: `{reason}`",
            IconEmoji = Emoji.X,
            Username = "Kbo Sync"
        });
        
        if(!postAsync)
            _logger.LogWarning("Could not notify slack");
    }
}