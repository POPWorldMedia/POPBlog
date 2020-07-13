using System;

namespace PopBlog.Mvc.Services
{
	public interface ITimeAdjustService
	{
		DateTime GetAdjustedTime(DateTime timeInput);
		DateTime GetReverseAdjustedTime(DateTime timeInput);
		bool IsDaylightSaving(DateTime localTime, double utcOffset);
	}

	public class TimeAdjustService : ITimeAdjustService
	{
		private readonly IConfig _config;

		public TimeAdjustService(IConfig config)
		{
			_config = config;
		}

		public DateTime GetAdjustedTime(DateTime timeInput)
		{
			var utcOffset = _config.TimeZone;
			var isDst = IsDaylightSaving(timeInput, utcOffset);
			var timeOffSet = utcOffset;
			if (isDst) timeOffSet = timeOffSet + 1;
			return timeInput.AddHours(timeOffSet);
		}

		public DateTime GetReverseAdjustedTime(DateTime timeInput)
		{
			var utcOffset = _config.TimeZone;
			var isDst = IsDaylightSaving(timeInput, utcOffset);
			var timeOffSet = utcOffset;
			if (isDst) timeOffSet = timeOffSet + 1;
			return timeInput.AddHours(-timeOffSet);
		}

		public bool IsDaylightSaving(DateTime localTime, double utcOffset)
		{
			DateTime startDate;
			DateTime endDate;
			if ((utcOffset < -4) && (utcOffset > -11))
			{
				// us dst
				if (localTime.Year < 2007)
				{
					startDate = new DateTime(localTime.Year, 4, Convert.ToInt32((2 + 6 * localTime.Year - Math.Floor((double)localTime.Year / 4)) % 7) + 1, 2, 0, 0);
					endDate = new DateTime(localTime.Year, 10, Convert.ToInt32(31 - (Math.Floor((double)localTime.Year * 5 / 4) + 1) % 7), 2, 0, 0);
				}
				else
				{
					startDate = new DateTime(localTime.Year, 3, 14 - Convert.ToInt32(Math.Floor(1 + (double)localTime.Year * 5 / 4) % 7), 2, 0, 0);
					endDate = new DateTime(localTime.Year, 11, 7 - Convert.ToInt32(Math.Floor(1 + (double)localTime.Year * 5 / 4) % 7), 2, 0, 0);
				}
				if ((localTime > startDate) && (localTime < endDate)) return true;
			}
			if ((utcOffset > -1) && (utcOffset < 5))
			{
				// eu dst
				startDate = new DateTime(localTime.Year, 3, Convert.ToInt32(31 - (Math.Floor((double)localTime.Year * 5 / 4) + 1) % 7), 1, 0, 0);
				endDate = new DateTime(localTime.Year, 10, Convert.ToInt32(31 - (Math.Floor((double)localTime.Year * 5 / 4) + 1) % 7), 1, 0, 0);
				if ((localTime > startDate) && (localTime < endDate)) return true;
			}
			return false;
		}
	}
}