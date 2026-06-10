namespace WhenWorks.FileHandler
{
    public class SaveLoadFile
    {
        public record SlotDto(DateTimeOffset Start, DateTimeOffset End);
        public record PersonDto(string Name, List<SlotDto> Slots);
        public record SaveFile(List<PersonDto> People);
    }
}
