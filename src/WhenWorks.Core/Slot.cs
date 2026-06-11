using System;
using System.Collections.Generic;
using System.Text;

namespace WhenWorks.Core
{

    // This struct is immutable and represents a time slot with a start and end time. 
    public readonly record struct Slot(DateTimeOffset Start, DateTimeOffset End)
    {
        public bool IsValid => Start < End;
        public TimeSpan Duration => End - Start;

        public override string ToString()
        {
            return $"{Start:ddd dd MMM HH:mm} - {End:ddd dd MMM HH:mm}";
        }
    }
}
