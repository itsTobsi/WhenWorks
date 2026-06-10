using WhenWorks.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace WhenWorks.ViewModels
{
    public class PersonViewModel
    {
        public string Name { get; }
        public ObservableCollection<Slot> Slots { get; } = new();
        public PersonViewModel(string name) { Name = name; }
        public override string ToString() { return Name; }
    }
}
