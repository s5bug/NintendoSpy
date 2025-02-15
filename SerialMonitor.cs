﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Windows.Threading;

namespace NintendoSpy;

public delegate void PacketEventHandler(object sender, byte[] packet);

public class SerialMonitor
{
    private const int BaudRate = 115200;
    private const int TimerMs = 30;

    public event PacketEventHandler PacketReceived;
    public event EventHandler Disconnected;

    private SerialPort _datPort;
    private readonly List<byte> _localBuffer;

    private DispatcherTimer _timer;

    public SerialMonitor(string portName)
    {
        _localBuffer = new List<byte>();
        _datPort = new SerialPort(portName, BaudRate);
        _datPort.Handshake = Handshake.RequestToSend;
        _datPort.DtrEnable = true;
    }

    public void Start()
    {
        if (_timer != null) return;

        _localBuffer.Clear();
        _datPort.Open();

        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(TimerMs);
        _timer.Tick += Tick;
        _timer.Start();
    }

    public void Stop()
    {
        if (_datPort != null)
        {
            try
            {
                // If the device has been unplugged, Close will throw an IOException.  This is fine, we'll just keep cleaning up.
                _datPort.Close();
            }
            catch (IOException)
            {
            }

            _datPort = null;
        }

        if (_timer != null)
        {
            _timer.Stop();
            _timer = null;
        }
    }

    private void Tick(object sender, EventArgs e)
    {
        if (_datPort == null || !_datPort.IsOpen || PacketReceived == null) return;

        // Try to read some data from the COM port and append it to our localBuffer.
        // If there's an IOException then the device has been disconnected.
        try
        {
            int readCount = _datPort.BytesToRead;
            if (readCount < 1) return;
            byte[] readBuffer = new byte [readCount];
            _datPort.Read(readBuffer, 0, readCount);
            _datPort.DiscardInBuffer();
            _localBuffer.AddRange(readBuffer);
        }
        catch (IOException)
        {
            Stop();
            if (Disconnected != null) Disconnected(this, EventArgs.Empty);
            return;
        }

        // Try and find 2 splitting characters in our buffer.
        int lastSplitIndex = _localBuffer.LastIndexOf(0x0A);
        if (lastSplitIndex <= 1) return;
        int sndLastSplitIndex = _localBuffer.LastIndexOf(0x0A, lastSplitIndex - 1);
        if (sndLastSplitIndex == -1) return;

        // Grab the latest packet out of the buffer and fire it off to the receive event listeners.
        int packetStart = sndLastSplitIndex + 1;
        int packetSize = lastSplitIndex - packetStart;
        PacketReceived(this, _localBuffer.GetRange(packetStart, packetSize).ToArray());

        // Clear our buffer up until the last split character.
        _localBuffer.RemoveRange(0, lastSplitIndex);
    }
}
