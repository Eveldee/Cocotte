using Discord.Interactions;

namespace Cocotte.Utils;

public enum CocotteDayOfWeek
{
    [ChoiceDisplay("Lundi")]
    Monday = DayOfWeek.Monday,
    [ChoiceDisplay("Mardi")]
    Tuesday = DayOfWeek.Tuesday,
    [ChoiceDisplay("Mercredi")]
    Wednesday = DayOfWeek.Wednesday,
    [ChoiceDisplay("Jeudi")]
    Thursday = DayOfWeek.Thursday,
    [ChoiceDisplay("Vendredi")]
    Friday = DayOfWeek.Friday,
    [ChoiceDisplay("Samedi")]
    Saturday = DayOfWeek.Saturday,
    [ChoiceDisplay("Dimanche")]
    Sunday = DayOfWeek.Sunday
}