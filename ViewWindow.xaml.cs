using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Windows.Shapes;

using NintendoSpy.Readers;

namespace NintendoSpy
{
    public partial class ViewWindow : Window, INotifyPropertyChanged
    {
        IControllerReader _reader;
        UdpClient _client;

        public ViewWindow (IControllerReader reader, string viewerServerAddress)
        {
            InitializeComponent ();
            DataContext = this;

            _reader = reader;

            _reader.ControllerStateChanged += reader_ControllerStateChanged;
            _reader.ControllerDisconnected += reader_ControllerDisconnected;

            _client = new UdpClient();
            int lastColon = viewerServerAddress.LastIndexOf(':');
            string hostname = viewerServerAddress[..lastColon];
            int port = Int32.Parse(viewerServerAddress[(lastColon + 1)..]);
            _client.Connect(hostname, port);
        }

        void reader_ControllerDisconnected (object sender, EventArgs e)
        {
            Close ();
        }

        void Window_Closing (object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            _client.Close();
            _reader.Finish ();
        }

        void reader_ControllerStateChanged (IControllerReader reader, ControllerState newState)
        {
            byte[] packet = new byte[18];

            packet[0] = newState.Buttons["a"] ? (byte) 1 : (byte) 0;
            packet[1] = newState.Buttons["b"] ? (byte) 1 : (byte) 0;
            packet[2] = newState.Buttons["x"] ? (byte) 1 : (byte) 0;
            packet[3] = newState.Buttons["y"] ? (byte) 1 : (byte) 0;

            packet[4] = newState.Buttons["left"] ? (byte) 1 : (byte) 0;
            packet[5] = newState.Buttons["right"] ? (byte) 1 : (byte) 0;
            packet[6] = newState.Buttons["down"] ? (byte) 1 : (byte) 0;
            packet[7] = newState.Buttons["up"] ? (byte) 1 : (byte) 0;

            packet[8] = newState.Buttons["start"] ? (byte) 1 : (byte) 0;
            packet[9] = newState.Buttons["z"] ? (byte) 1 : (byte) 0;
            packet[10] = newState.Buttons["r"] ? (byte) 1 : (byte) 0;
            packet[11] = newState.Buttons["l"] ? (byte) 1 : (byte) 0;

            packet[12] = (byte) ((newState.Analogs["lstick_x"] * 128.0) + 128.0);
            packet[13] = (byte) ((newState.Analogs["lstick_y"] * 128.0) + 128.0);
            packet[14] = (byte) ((newState.Analogs["cstick_x"] * 128.0) + 128.0);
            packet[15] = (byte) ((newState.Analogs["cstick_y"] * 128.0) + 128.0);
            packet[16] = (byte) (newState.Analogs["trig_l"] * 256.0);
            packet[17] = (byte) (newState.Analogs["trig_r"] * 256.0);
            
            _client.Send(packet, packet.Length);
        }

        // INotifyPropertyChanged interface implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged (string propertyName) {
            if (PropertyChanged != null) PropertyChanged (this, new PropertyChangedEventArgs(propertyName));
        }

        
    }
}
