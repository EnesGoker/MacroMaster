using MacroMaster.Application.Abstractions;
using MacroMaster.Domain.Models;

namespace MacroMaster.Application.Services;

internal sealed class NullRecordedScreenProvider : IRecordedScreenProvider
{
    public static NullRecordedScreenProvider Instance { get; } = new();

    private NullRecordedScreenProvider()
    {
    }

    public RecordedScreenInfo? GetRecordedScreen()
    {
        return null;
    }
}
