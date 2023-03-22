#if DEBUG

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using Cocotte.Services;
using Cocotte.Utils;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Cocotte.Modules.Ping;

/// <summary>
/// Module containing different test and debug commands
/// </summary>
[RequireOwner]
[Group("ping", "Debug related commands")]
public class PingModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<PingModule> _logger;
    private readonly SharedCounter _sharedCounter;
    private readonly TransientCounter _transientCounter;
    private static readonly SemaphoreSlim CounterWait = new(0);

    public PingModule(ILogger<PingModule> logger, SharedCounter sharedCounter, TransientCounter transientCounter)
    {
        _logger = logger;
        _sharedCounter = sharedCounter;
        _transientCounter = transientCounter;
    }

    [SlashCommand("runtime-info", "Get runtime info")]
    public async Task RuntimeInfo()
    {
        var embed = new EmbedBuilder()
            .WithTitle("Cocotte runtime info")
            .WithColor(0x3196c8)
            .WithFields(
                new EmbedFieldBuilder()
                    .WithName(":computer: OS")
                    .WithValue(RuntimeInformation.OSDescription)
                    .WithIsInline(true),
                new EmbedFieldBuilder()
                    .WithName(":pencil: .Net info")
                    .WithValue(RuntimeInformation.FrameworkDescription)
                    .WithIsInline(true),
                new EmbedFieldBuilder()
                    .WithName(":mirror_ball: Discord.Net info")
                    .WithValue(Assembly.GetAssembly(typeof(Discord.Net.BucketId))!.ToString())
                    .WithIsInline(false)
            );

        await RespondAsync(embed: embed.Build());
    }

    [SlashCommand("ping", "Check if Coco is alive and get latency")]
    public async Task Ping()
    {
        _logger.LogTrace("[Ping/ping] Received ping command");

        await RespondAsync($":ping_pong: It took me {Context.Client.Latency}ms to respond to you!");
    }

    [SlashCommand("echo", "Repeat the input")]
    public async Task Echo(string echo, [Summary(description: "mention the user")] bool mention = false)
    {
        _logger.LogTrace("[Ping/echo] Received ping command with arg: {{ echo:'{Echo}', mention:{Mention} }}", echo, mention);

        await RespondAsync($"{echo} {(mention ? Context.User.Mention : string.Empty)}");
    }

    // This command will greet target user in the channel this was executed in.
    [UserCommand("Greet")]
    public async Task GreetUserAsync(IUser user)
    {
        await RespondAsync($":wave: {Context.User.Username} said hi to you, <@{user.Id}>!");
    }

    [SlashCommand("summon", "Summon Coco to test if she can see and write to this channel")]
    public async Task Summon()
    {
        _logger.LogTrace("{User} summoned Coco in {Channel}", Context.User.Username, Context.Channel.Name);

        await RespondAsync(":white_check_mark: Coco at your service!");
    }

    [SlashCommand("reply", "Reply to a message")]
    public async Task Reply(string channelId, string messageId)
    {
        var channel = Context.Client.GetChannel(ulong.Parse(channelId)) as ISocketMessageChannel;
        var message = await channel?.GetMessageAsync(ulong.Parse(messageId))!;

        if (message is IUserMessage)
        {
            await message.Channel.SendMessageAsync("What can I do for you?",
                messageReference: new MessageReference(message.Id));

            await RespondAsync("Reply successfully sent", ephemeral: true);
        }
        else
        {
            await RespondAsync("An error occured while sending message", ephemeral: true);
        }
    }

    // Pins a message in the channel it is in.
    [MessageCommand("Pin")]
    public async Task PinMessageAsync(IMessage message)
    {
        // make a safety cast to check if the message is ISystem- or IUserMessage
        if (message is not IUserMessage userMessage)
            await RespondAsync(":x: You cant pin system messages!");

        // if the pins in this channel are equal to or above 50, no more messages can be pinned.
        else if ((await Context.Channel.GetPinnedMessagesAsync()).Count >= 50)
            await RespondAsync(":x: You cant pin any more messages, the max has already been reached in this channel!");

        else
        {
            await userMessage.PinAsync();
            await RespondAsync(":white_check_mark: Successfully pinned message!");
        }
    }

    [SlashCommand("emote", "Test sending an emote")]
    public async Task SendEmote(string emoteText)
    {
        if (Emote.TryParse(emoteText, out var emote))
        {
            await RespondAsync($"{emote}/{emoteText}: `{emoteText}/{emote}`");
        }
        else
        {
            await RespondAsync(embed: EmbedUtils.ErrorEmbed("Couldn't parse the emote").Build());
        }
    }

    [SlashCommand("emoji", "Test sending an emoji")]
    public async Task SendEmoji(string emojiText)
    {
        if (Emoji.TryParse(emojiText, out var emoji))
        {
            await RespondAsync($"{emoji}/{emojiText}: `{emojiText}/{emoji}`");
        }
        else
        {
            await RespondAsync(embed: EmbedUtils.ErrorEmbed("Couldn't parse the emoji").Build());
        }
    }

    [SlashCommand("test-button", "Test buttons components")]
    public async Task TestButton()
    {
        var component = new ComponentBuilder()
            .WithButton("Button1", $"button:1:{Context.User.Username}")
            .WithButton("Button2", $"button:2:{Context.User.Username}")
            .AddRow(new ActionRowBuilder()
                .WithButton("Button3", $"button:3:{Context.User.Username}"));

        await RespondAsync(components: component.Build());
    }

    [ComponentInteraction("button:*:*")]
    public async Task TestButtonClick(int buttonId, string userName)
    {
        await RespondAsync($"{userName} clicked on button: {buttonId}");
    }

    [SlashCommand("counter-shared", "Spawn a shared counter")]
    public async Task CounterShared()
    {
        var component = new ComponentBuilder()
            .WithButton("Increment", "increment_shared");

        await RespondAsync($"Counter: {_sharedCounter.Count}", components: component.Build());
    }

    [ComponentInteraction("increment_shared")]
    public async Task SharedCounterIncrement()
    {
        _logger.LogTrace("Received increment on shared counter");
        _sharedCounter.Count++;

        try
        {
            await (Context.Interaction as IComponentInteraction)!.UpdateAsync(msg =>
            {
                msg.Content = $"Counter: {_sharedCounter.Count}";
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while updating original message:");
        }
        await RespondAsync();
    }

    [SlashCommand("counter-transient", "Spawn a transient counter")]
    public async Task CounterTransient()
    {
        var component = new ComponentBuilder()
            .WithButton("Increment", "increment_transient");

        await RespondAsync($"Counter: {_transientCounter.Count}", components: component.Build());
    }

    [ComponentInteraction("increment_transient")]
    public async Task TransientCounterIncrement()
    {
        _logger.LogTrace("Received increment on transient counter");
        _transientCounter.Count++;

        try
        {
            await (Context.Interaction as IComponentInteraction)!.UpdateAsync(msg =>
            {
                msg.Content = $"Counter: {_transientCounter.Count}";
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while updating original message:");
        }
        await RespondAsync();
    }

    [SlashCommand("counter-transient-wait", "Spawn a transient counter using wait")]
    public async Task CounterTransientWait()
    {
        var component = new ComponentBuilder()
            .WithButton("Increment", "increment_transient_wait");

        await RespondAsync($"Counter: {_transientCounter.Count}", components: component.Build());
        var response = await GetOriginalResponseAsync();

        while (true)
        {
            // Wait for the button to be clicked
            _logger.LogTrace("Waiting for semaphore release");
            await CounterWait.WaitAsync();

            _logger.LogTrace("Received increment on transient wait counter");
            _transientCounter.Count++;

            await ModifyOriginalResponseAsync(m => m.Content = $"Counter: {_transientCounter.Count}");
        }
    }

    [ComponentInteraction("increment_transient_wait")]
    public async Task WaitCounterIncrement()
    {
        CounterWait.Release();

        await RespondAsync();
    }

    [SlashCommand("select-test", "Test menu select")]
    public async Task SelectTest()
    {
        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Select an option")
            .WithCustomId("menu-1")
            .WithMinValues(1)
            .WithMaxValues(1)
            .AddOption("Option A", "opt-a", "Option B is lying!")
            .AddOption("Option B", "opt-b", "Option A is telling the truth!");

        var builder = new ComponentBuilder()
            .WithSelectMenu(menuBuilder);

        await RespondAsync("Whos really lying?", components: builder.Build());
    }

    [ComponentInteraction("menu-1")]
    public async Task TestMenuSelected(string[] selectedChoice)
    {
        await RespondAsync($"You have selected: {string.Join(", ", selectedChoice)}");
    }

    [SlashCommand("modal-test", "Test a modal")]
    public async Task ModalTest()
    {
        await RespondWithModalAsync<FoodModal>("food_menu");
    }

    [ModalInteraction("food_menu")]
    public async Task FoodMenuSubmit(FoodModal modal)
    {
        // Check if "Why??" field is populated
        string reason = string.IsNullOrWhiteSpace(modal.Reason)
            ? "."
            : $" because {modal.Reason}";

        // Build the message to send.
        string message = "hey @everyone, I just learned " +
            $"{Context.User.Mention}'s favorite food is " +
            $"{modal.Food}{reason}";

        // Specify the AllowedMentions so we don't actually ping everyone.
        AllowedMentions mentions = new()
        {
            AllowedTypes = AllowedMentionTypes.Users
        };

        // Respond to the modal.
        await RespondAsync(message, allowedMentions: mentions, ephemeral: true);
    }
}

// Defines the modal that will be sent.
public class FoodModal : IModal
{
    public string Title => "Fav Food";

    // Strings with the ModalTextInput attribute will automatically become components.
    [NotNull]
    [InputLabel("What??")]
    [ModalTextInput("food_name", placeholder: "Pizza", maxLength: 20)]
    public string? Food { get; set; }

    // Additional paremeters can be specified to further customize the input.
    // Parameters can be optional
    [RequiredInput(false)]
    [InputLabel("Why??")]
    [ModalTextInput("food_reason", TextInputStyle.Paragraph, "Kuz it's tasty", maxLength: 500)]
    public required string Reason { get; set; }
}


#endif