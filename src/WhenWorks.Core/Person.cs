using System;
using System.Collections.Generic;
using System.Text;

namespace WhenWorks.Core
{
    public record Person(string Name, List<Slot> Availability);
}
