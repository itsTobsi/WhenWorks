using System;
using System.Collections.Generic;
using System.Text;

namespace WhenWorks.Core
{
    public static class WhenWorks
    {

        // Merges overlapping slots into a list of non-overlapping slots
        public static List<Slot> Merge(IEnumerable<Slot> slots)
        {
            var sortedSlots = slots.Where(s => s.IsValid).OrderBy(s => s.Start).ToList();
            var mergedSlots = new List<Slot>();

            foreach (var slot in sortedSlots)
            {
                // Check if the current slot overlaps with the last merged slot
                if (mergedSlots.Count > 0 && slot.Start <= mergedSlots[^1].End) // ^1 is the index from the end (https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/ranges-indexes)
                {
                    mergedSlots[^1] = mergedSlots[^1] with { End = slot.End > mergedSlots[^1].End ? slot.End : mergedSlots[^1].End };
                }
                else
                {
                    mergedSlots.Add(slot);
                }
            }

            return mergedSlots;
        }

        // Find windows where at least n people are available
        public static List<Slot> AtLeast(int n, IEnumerable<Person> people)
        {
            var events = people.SelectMany(p => Merge(p.Availability))
                .SelectMany(s => new[] { (time: s.Start, delta: 1), (time: s.End, delta: -1) })
                .OrderBy(e => e.time)
                .ThenBy(e => e.delta) // Ensure that end events are processed before start events at the same time
                .ToList();

            var res = new List<Slot>();
            int count = 0;

            DateTimeOffset open = default;
            foreach (var (time, delta) in events)
            {
                bool before = count >= n;
                count += delta;
                bool after = count >= n;

                if (!before && after)
                {
                    open = time;
                }
                else if (before && !after && open < time)
                {
                    res.Add(new Slot(open, time));
                }
            }

            return res;
        }

        public record CommonWindow(DateTimeOffset Start, DateTimeOffset End, IReadOnlyList<string> People)
        {
            public override string ToString() => $"{Start:ddd dd MMM HH:mm} – {End:HH:mm}  ({string.Join(", ", People)})";
        }

        // TODO: Windows app needs to use this, I'll fix it after the Blazor project is done.
        public static List<CommonWindow> FindCommonWindow(int count, IEnumerable<Person> people)
        {
            var events = people.SelectMany(p => Merge(p.Availability).Select(s => (p.Name, slot: s)))
                .SelectMany(s => new[]
                {
                    (time: s.slot.Start, delta: +1, person: s.Name),
                    (time: s.slot.End, delta: -1, person: s.Name)
                })
                .OrderBy(e => e.time)
                .ToList();

            var res = new List<CommonWindow>();
            var active = new HashSet<string>();

            int i = 0;
            DateTimeOffset SegmentStart = default;
            List<string>? SegmentPeople = null;

            while (i < events.Count)
            {
                var time = events[i].time;

                // Close previous window 
                if (SegmentPeople != null && SegmentStart < time)
                {
                    res.Add(new CommonWindow(SegmentStart, time, SegmentPeople));
                }

                // Apply all events at the current time (so set is updated before we check if we can open a new window)
                while (i < events.Count && events[i].time == time)
                {
                    if (events[i].delta == +1)
                    {
                        active.Add(events[i].person);
                    }
                    else
                    {
                        active.Remove(events[i].person);
                    }

                    i++;
                }

                // New window from time if enough people are available
                if (active.Count >= count)
                {
                    SegmentStart = time;
                    SegmentPeople = active.ToList();
                }
                else
                {
                    SegmentPeople = null;
                }
            }

            return res;
        }
    }
}
