using System;

namespace NintendoSpy.Readers;

public sealed class SerialControllerReader : IControllerReader
{
    public event StateEventHandler ControllerStateChanged;
    public event EventHandler ControllerDisconnected;

    private readonly Func<byte[], ControllerState> _packetParser;
    private SerialMonitor _serialMonitor;

    public SerialControllerReader(string portName, Func<byte[], ControllerState> packetParser)
    {
        _packetParser = packetParser;

        _serialMonitor = new SerialMonitor(portName);
        _serialMonitor.PacketReceived += serialMonitor_PacketReceived;
        _serialMonitor.Disconnected += serialMonitor_Disconnected;
        _serialMonitor.Start();
    }

    private void serialMonitor_Disconnected(object sender, EventArgs e)
    {
        Finish();
        if (ControllerDisconnected != null) ControllerDisconnected(this, EventArgs.Empty);
    }

    private void serialMonitor_PacketReceived(object sender, byte[] packet)
    {
        if (ControllerStateChanged != null)
        {
            var state = _packetParser(packet);
            if (state != null)
            {
                ControllerStateChanged(this, state);
            }
        }
    }

    public void Finish()
    {
        if (_serialMonitor != null)
        {
            _serialMonitor.Stop();
            _serialMonitor = null;
        }
    }
}
