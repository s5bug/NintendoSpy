using System.Collections.Generic;

namespace NintendoSpy.Readers;

public sealed class ControllerStateBuilder
{
    private readonly Dictionary<string, bool> _buttons = new();
    private readonly Dictionary<string, float> _analogs = new();

    public void SetButton(string name, bool value)
    {
        _buttons[name] = value;
    }

    public void SetAnalog(string name, float value)
    {
        _analogs[name] = value;
    }

    public ControllerState Build()
    {
        return new ControllerState(_buttons, _analogs);
    }
}
