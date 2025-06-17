using ClearSight.Core.Enums;

namespace ClearSight.Core.Helpers
{
    public static class DaysOffHelper
    {
        public static (DaysOff, List<string>) ConvertToEnum(List<string> days)
        {
            DaysOff result = DaysOff.None;
            List<string> invalidDays = new List<string>();

            foreach (var day in days)
            {
                if (Enum.TryParse(day, true, out DaysOff dayEnum) && Enum.IsDefined(typeof(DaysOff), dayEnum))
                {
                    result |= dayEnum;
                }
                else
                {
                    invalidDays.Add(day);
                }
            }

            return (result, invalidDays);
        }

        public static List<string> ConvertToStringList(DaysOff daysOff)
        {
            return Enum.GetValues(typeof(DaysOff))
                       .Cast<DaysOff>()
                       .Where(d => d != DaysOff.None && daysOff.HasFlag(d))
                       .Select(d => d.ToString())
                       .ToList();
        }
    }
}
