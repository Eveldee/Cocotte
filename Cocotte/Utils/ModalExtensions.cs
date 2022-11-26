using Discord;

namespace Cocotte.Utils;

public static class ModalExtensions
{
    public static ModalBuilder UpdateTextInput(this ModalBuilder modal, string customId, Action<TextInputBuilder> inputUpdater)
    {
        var components = modal.Components.ActionRows.SelectMany(r => r.Components).OfType<TextInputComponent>();
        var component = components.First(c => c.CustomId == customId);

        var builder = new TextInputBuilder
        {
            CustomId    = customId,
            Label       = component.Label,
            MaxLength   = component.MaxLength,
            MinLength   = component.MinLength,
            Placeholder = component.Placeholder,
            Required    = component.Required,
            Style       = component.Style,
            Value       = component.Value
        };

        inputUpdater(builder);

        foreach (var row in modal.Components.ActionRows.Where(row => row.Components.Any(c => c.CustomId == customId)))
        {
            row.Components.RemoveAll(c => c.CustomId == customId);
            row.AddComponent(builder.Build());
        }

        return modal;
    }
}