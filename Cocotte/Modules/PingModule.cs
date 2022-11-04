#if DEBUG

using Cocotte.Services;
using Discord;
using Discord.Interactions;

namespace Cocotte.Modules;

/// <summary>
/// Module containing different test and debug commands
/// </summary>
[RequireOwner]
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
}

#endif