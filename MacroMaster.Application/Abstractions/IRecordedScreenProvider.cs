using MacroMaster.Domain.Models;

namespace MacroMaster.Application.Abstractions;

public interface IRecordedScreenProvider
{
    RecordedScreenInfo? GetRecordedScreen();
}
