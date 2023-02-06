using System.Text;

namespace LibFileOperation
{
    public static class CommonUtils
    {
        public static string TimeSpan2String(TimeSpan timeSpan)
        {
            StringBuilder sb = new StringBuilder();

            bool hasDays = false;
            if (timeSpan.Days > 0)
            {
                sb.Append(timeSpan.Days);
                sb.Append('d');
                hasDays = true;
            }

            bool hasHours = false;
            if (timeSpan.Hours > 0 || hasDays)
            {
                sb.Append(timeSpan.Hours);
                sb.Append('h');
                hasHours = true;
            }

            bool hasMinutes = false;
            if (timeSpan.Minutes > 0 || hasHours)
            {
                sb.Append(timeSpan.Minutes);
                sb.Append('m');
                hasMinutes = true;
            }

            if (timeSpan.Seconds > 0 || hasMinutes)
            {
                sb.Append(timeSpan.Seconds);
                sb.Append('s');
            }

            sb.Append(timeSpan.Milliseconds);
            sb.Append("ms");

            return sb.ToString();
        }
    }
}