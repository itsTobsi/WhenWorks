namespace WhenWorks.Web.Models
{
    public record SlotDto(DateTimeOffset Start, DateTimeOffset End);
    public record PersonDto(string Name, List<SlotDto> Slots);
    public record SaveFile(List<PersonDto> People);
}
