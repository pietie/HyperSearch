using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HyperSearch
{
    public class PerformanceTracker
    {
        public DateTime? CreateDate { get; private set; }
        public double? DurationInMilliseconds { get; private set; }
        public string Description { get; private set; }

        public List<PerformanceTrackerEntry> _entries = new List<PerformanceTrackerEntry>();

        private PerformanceTracker(string description)
        {
            this.CreateDate = DateTime.Now;
            this.Description = description;
        }

        public static PerformanceTracker Begin(string description)
        {
            PerformanceTracker tracker = new PerformanceTracker(description);

            return tracker;
        }

        public double End(bool outputToConsole = true)
        {
            if (this.DurationInMilliseconds.HasValue) throw new Exception("This performance tracking entry has already been ended.");

            this.DurationInMilliseconds = DateTime.Now.Subtract(this.CreateDate.Value).TotalMilliseconds;

            foreach (var e in _entries)
            {
                if (!e.DurationInMilliseconds.HasValue) e.End();
            }

            if (outputToConsole) OutputToConsole();

            return this.DurationInMilliseconds.Value;
        }

        public PerformanceTrackerEntry AddEntry(string description, params object[] args)
        {
            var entry = new PerformanceTrackerEntry(this, string.Format(description, args));

            _entries.Add(entry);

            return entry;
        }

        public void OutputToConsole()
        {
            if (this.DurationInMilliseconds.HasValue)
            {
                if (_entries.Count > 0) Console.WriteLine("\r\n----------------------------------------------------------");

                Console.WriteLine("{0}\t: {1:###,##0}ms", this.Description, this.DurationInMilliseconds);

                foreach (var e in _entries)
                {
                    Console.WriteLine("\t● {0}\t: {1:###,##0}ms", e.Description, e.DurationInMilliseconds);
                }

                if (_entries.Count > 0) Console.WriteLine("\r\n----------------------------------------------------------");
            }
            else
            {
                Console.WriteLine("{0}\t: (still busy)", this.Description);
            }
        }

        public class PerformanceTrackerEntry
        {
            public DateTime? CreateDate { get; private set; }
            public double? DurationInMilliseconds { get; private set; }
            public string Description { get; private set; }

            private PerformanceTracker ParentTracker { get; set; }

            internal PerformanceTrackerEntry(PerformanceTracker parentTracker, string description)
            {
                this.CreateDate = DateTime.Now;
                this.Description = description;
                this.ParentTracker = parentTracker;
            }

            public PerformanceTrackerEntry Next(string description, params object[] args)
            {
                this.End();
                return ParentTracker.AddEntry(description, args);
            }

            public double End()
            {
                if (this.DurationInMilliseconds.HasValue) throw new Exception("This performance tracking entry has already been ended.");

                this.DurationInMilliseconds = DateTime.Now.Subtract(this.CreateDate.Value).TotalMilliseconds;

                return this.DurationInMilliseconds.Value;
            }
        }
    }
}
