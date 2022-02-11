using System.Collections.Generic;

namespace NintendoSpy.Readers;

public class ControllerState
{
    public IReadOnlyDictionary <string, bool> Buttons { get; }
    public IReadOnlyDictionary <string, float> Analogs { get; }

    public ControllerState (IReadOnlyDictionary <string, bool> buttons, IReadOnlyDictionary <string, float> analogs)
    {
        Buttons = buttons;
        Analogs = analogs;
    }
}
