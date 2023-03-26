namespace Cocotte.Utils;

public static class DateTimeUtils
{
    // Source: https://stackoverflow.com/a/50033489
    public static int CalculateDayOfWeekOffset(DayOfWeek current, DayOfWeek desired) {
        // f( c, d ) = [7 - (c - d)] mod 7
        // f( c, d ) = [7 - c + d] mod 7
        // c is current day of week and 0 <= c < 7l
        // d is desired day of the week and 0 <= d < 7
        int c = (int)current;
        int d = (int)desired;
        int offset = (7 - c + d) % 7;
        return offset;
    }

    public static DateTime NextDateWithDayAndTime(DayOfWeek desired, TimeOnly time)
    {
        var date = DateTime.Today;
        date = date.AddDays(CalculateDayOfWeekOffset(date.DayOfWeek, desired)).WithTimeOnly(time);

        // If it's previous than current date, add 7 days
        return date < DateTime.Now ? date.AddDays(7) : date;
    }

    public static DateTime WithTimeOnly(this DateTime dateTime, TimeOnly timeOnly)
    {
        return new DateTime(
            dateTime.Year, dateTime.Month, dateTime.Day, timeOnly.Hour, timeOnly.Minute, timeOnly.Second
        );
    }

    public static DateTime NextDateWithTimeOfDay(TimeOnly timeOfDay) =>
        DateTime.Now.TimeOfDay.Ticks > timeOfDay.Ticks ? DateTime.Now.AddDays(1).WithTimeOnly(timeOfDay) : DateTime.Now.WithTimeOnly(timeOfDay);
}