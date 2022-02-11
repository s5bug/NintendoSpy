using System;

namespace NintendoSpy.Readers;

public static class GameCube
{
    private const int PacketSize = 64;
    private const int NicohoodPacketSize = 8;

    private static readonly string[] Buttons =
    {
        null, null, null, "start", "y", "x", "b", "a", null, "l", "r", "z", "up", "down", "right", "left"
    };


    // Button order for the Nicohood Nintendo API
    // https://github.com/NicoHood/Nintendo
    // Each byte is reverse from the buttons above
    private static readonly string[] NicohoodButtons =
    {
        "a", "b", "x", "y", "start", null, null, null, "left", "right", "down", "up", "z", "r", "l", null
    };

    private static float ReadStick(byte input)
    {
        return (input - 128.0f) / 128.0f;
    }

    private static float ReadTrigger(byte input)
    {
        return input / 256.0f;
    }

    public static ControllerState ReadFromPacket(byte[] packet)
    {
        var state = new ControllerStateBuilder();
        switch (packet.Length)
        {
            // Standard 64 bit packet size
            case PacketSize:
                for (int i = 0; i < Buttons.Length; ++i)
                {
                    if (string.IsNullOrEmpty(Buttons[i])) continue;
                    state.SetButton(Buttons[i], packet[i] != 0x00);
                    Console.WriteLine(i + ": " + packet[i]);
                }

                state.SetAnalog("lstick_x", ReadStick(SignalTool.ReadByte(packet, Buttons.Length)));
                state.SetAnalog("lstick_y", ReadStick(SignalTool.ReadByte(packet, Buttons.Length + 8)));
                state.SetAnalog("cstick_x", ReadStick(SignalTool.ReadByte(packet, Buttons.Length + 16)));
                state.SetAnalog("cstick_y", ReadStick(SignalTool.ReadByte(packet, Buttons.Length + 24)));
                state.SetAnalog("trig_l", ReadTrigger(SignalTool.ReadByte(packet, Buttons.Length + 32)));
                state.SetAnalog("trig_r", ReadTrigger(SignalTool.ReadByte(packet, Buttons.Length + 40)));
                break;
            // Packets are written as bytes when writing from the NicoHood API, so we're looking for a packet size of 8 (interpreted as bytes)
            case NicohoodPacketSize:
                for (int i = 0; i < 16; i++)
                {
                    if (string.IsNullOrEmpty(NicohoodButtons[i])) continue;
                    int bitPacket = (packet[i / 8] >> (i % 8)) & 0x1;
                    state.SetButton(NicohoodButtons[i], bitPacket != 0x00);
                }

                state.SetAnalog("lstick_x", ReadStick(packet[2]));
                state.SetAnalog("lstick_y", ReadStick(packet[3]));
                state.SetAnalog("cstick_x", ReadStick(packet[4]));
                state.SetAnalog("cstick_y", ReadStick(packet[5]));
                state.SetAnalog("trig_l", ReadTrigger(packet[6]));
                state.SetAnalog("trig_r", ReadTrigger(packet[7]));
                break;
            default:
                return null;
        }

        return state.Build();
    }
}
