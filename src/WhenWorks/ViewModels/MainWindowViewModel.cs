using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WhenWorks.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using WhenWorks.Models;

namespace WhenWorks.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<PersonViewModel> People { get; } = new();
        public ObservableCollection<Slot> Res { get; } = new();

        // Person
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RemovePersonCommand))]
        private PersonViewModel? selectedPerson;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RemoveSlotCommand))]
        private Slot? selectedSlot;

        [ObservableProperty]
        private string newPersonName = "";

        // Date and time
        [ObservableProperty]
        private DateTimeOffset? startDate = DateTimeOffset.Now;

        [ObservableProperty]
        private TimeSpan? startTime;

        [ObservableProperty]
        private DateTimeOffset? endDate = DateTimeOffset.Now;

        [ObservableProperty]
        private TimeSpan? endTime;

        // User variables
        [ObservableProperty]
        private decimal requiredPeople = 1;

        [ObservableProperty]
        private string status = "";

        public MainWindowViewModel()
        {
            // Testing
            //People.Add(new PersonViewModel("Alex"));
            //People.Add(new PersonViewModel("Emma"));
            selectedPerson = People.FirstOrDefault();
            requiredPeople = People.Count();
        }

        [RelayCommand]
        private void AddPerson()
        {
            var name = NewPersonName.Trim();
            if (name.Length == 0)
            {
                return;
            }

            People.Add(new PersonViewModel(name));
            NewPersonName = "";
            RequiredPeople = People.Count();
        }

        [RelayCommand(CanExecute = nameof(CanRemovePerson))]
        private void RemovePerson()
        {
            if (SelectedPerson == null)
            {
                return;
            }

            var removed = SelectedPerson.Name;
            People.Remove(SelectedPerson);
            SelectedPerson = People.FirstOrDefault();

            if (RequiredPeople > People.Count)
            {
                RequiredPeople = Math.Max(1, People.Count);
            }

            Status = $"Removed {removed}.";
        }

        private bool CanRemovePerson() => SelectedPerson != null;

        [RelayCommand]
        private void AddSlot()
        {
            if (SelectedPerson == null)
            {
                Status = "Please select a person";
                return;
            }

            if (StartDate == null || StartTime == null || EndDate == null || EndTime == null )
            {
                Status = "Please fill in all date and time fields";
                return;
            }

            var start = new DateTimeOffset(StartDate.Value.Date + StartTime.Value, StartDate.Value.Offset);
            var end = new DateTimeOffset(EndDate.Value.Date + EndTime.Value, EndDate.Value.Offset);

            var slot = new Slot(start, end);
            if (!slot.IsValid)
            {
                Status = "Invalid slot: End must be after start";
                return;
            }

            SelectedPerson.Slots.Add(slot);
            Status = $"Added slot for {SelectedPerson.Name}: {slot}";
        }

        [RelayCommand(CanExecute = nameof(CanRemoveSlot))]
        private void RemoveSlot()
        {
            if (SelectedPerson == null || SelectedSlot == null)
            {
                Status = "Please select a person and a slot.";
                return;
            }

            SelectedPerson.Slots.Remove(SelectedSlot.Value);

            Status = $"Removed slot for {SelectedPerson.Name}.";
        }

        private bool CanRemoveSlot() => SelectedPerson != null && SelectedSlot != null;

        [RelayCommand]
        private void FindSlots()
        {
            var people = People.Select(p => new Person(p.Name, p.Slots.ToList())).ToList();
            var numb = (int)Math.Clamp(RequiredPeople, 1, Math.Max(1, people.Count));

            Res.Clear();
            foreach (var slot in WhenWorks.Core.WhenWorks.AtLeast(numb, people))
            {
                Res.Add(slot);
            }

            Status = Res.Count == 0 ? "No common slots found" : $"Found {Res.Count} window(s) where at least {numb} people are available.";
        }

        public SaveFile SaveToFile() => new(People.Select(p => new PersonDto(p.Name, p.Slots.Select(s => new SlotDto(s.Start, s.End)).ToList())).ToList());

        public void LoadFromFile(SaveFile data)
        {
            People.Clear();

            foreach (var person in data.People)
            {
                var pvm = new PersonViewModel(person.Name);

                foreach (var slot in person.Slots)
                {
                    pvm.Slots.Add(new Slot(slot.Start, slot.End));
                }

                People.Add(pvm);
            }

            SelectedPerson = People.FirstOrDefault();
            RequiredPeople = People.Count == 0 ? 1 : People.Count;
            Res.Clear();
            Status = $"Loaded {People.Count} people from file.";
        }
    }
}
