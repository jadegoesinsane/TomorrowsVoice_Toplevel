namespace TomorrowsVoice_Toplevel.Utilities
{
    public static class DateUtilities
    {
        public static DateTime GetNextWeekday(DateTime startDate, int daysInFuture)
        {
            DateTime targetDate = startDate.AddDays(daysInFuture).Date.AddHours(9); // Set time to 9:00 AM
            if (targetDate.DayOfWeek == DayOfWeek.Saturday)
            {
                targetDate = targetDate.AddDays(2);
            }
            else if (targetDate.DayOfWeek == DayOfWeek.Sunday)
            {
                targetDate = targetDate.AddDays(1);
            }
            return targetDate;
        }
    }
}
