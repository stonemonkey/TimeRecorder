using System;

namespace TimeRecorder.Main
{
    public class TimeRecord
    {
        public TimeRecord(int id, int number, DateTime time)
        {
            Id = id;
            Number = number;
            FullTime = time;
        }
        public int Id { get; set; }

        public int Number { get; set;  }

        public bool IsPossiblyWrong { get; set; }
        
        public DateTime FullTime { get; set; }
        
        public string Time
        {
            get { return FullTime.ToString(Configuration.TimeFormat); }
        }
    }
}