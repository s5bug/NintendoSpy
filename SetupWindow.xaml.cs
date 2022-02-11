using System;
using System.Linq;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

using NintendoSpy.Readers;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace NintendoSpy
{
    public partial class SetupWindow : Window
    {
        SetupWindowViewModel _vm;
        DispatcherTimer _portListUpdateTimer;

        public SetupWindow ()
        {
            InitializeComponent ();
            _vm = new SetupWindowViewModel ();
            DataContext = _vm;

            if (! Directory.Exists ("skins")) {
                MessageBox.Show ("Could not find skins folder!", "NintendoSpy", MessageBoxButton.OK, MessageBoxImage.Error);
                Close ();
                return;
            }
            
            _vm.Sources.UpdateContents (InputSource.ALL);
            

            _vm.ViewerServerAddress = Properties.Settings.Default.ViewerServerAddress;

            _portListUpdateTimer = new DispatcherTimer ();
            _portListUpdateTimer.Interval = TimeSpan.FromSeconds (1);
            _portListUpdateTimer.Tick += (sender, e) => updatePortList ();
            _portListUpdateTimer.Start ();

            updatePortList ();
            _vm.Ports.SelectFirst ();
            _vm.Sources.SelectId(Properties.Settings.Default.Source);
        }

        void updatePortList () {
            _vm.Ports.UpdateContents (SerialPort.GetPortNames ());
        }

        void goButton_Click (object sender, RoutedEventArgs e) 
        {
            this.Hide ();
            Properties.Settings.Default.Source = _vm.Sources.GetSelectedId();
            Properties.Settings.Default.ViewerServerAddress = _vm.ViewerServerAddress;
            Properties.Settings.Default.Save();
            
#if !DEBUG
            try {
#endif
                IControllerReader reader; 
                reader = _vm.Sources.SelectedItem.BuildReader(_vm.Ports.SelectedItem);

                new ViewWindow (reader, _vm.ViewerServerAddress)
                    .ShowDialog ();
#if !DEBUG
            }
            catch (Exception ex) {
                MessageBox.Show (ex.Message, "NintendoSpy", MessageBoxButton.OK, MessageBoxImage.Error);
            }
#endif

            this.Show ();
        }

        private void SourceSelectComboBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            if (_vm.Sources.SelectedItem == null) return;
            updatePortList();
        }
    }

    public class SetupWindowViewModel : INotifyPropertyChanged
    {
        public class ListView <T>
        {
            List <T> _items;

            public CollectionView Items { get; private set; }
            public T SelectedItem { get; set; }

            public ListView () {
                _items = new List <T> ();
                Items = new CollectionView (_items);
            }

            public void UpdateContents (IEnumerable <T> items) {
                _items.Clear ();
                _items.AddRange (items);
                Items.Refresh ();
            }
            
            public void SelectFirst () {
                if (_items.Count > 0) SelectedItem = _items [0];
            }

            public void SelectId(int id)
            {
                if (_items.Count > 0 && id >= 0 && id < _items.Count)
                {
                    SelectedItem = _items[id];
                }
                else
                {
                    SelectFirst();
                }
            }

            public int GetSelectedId()
            {
                if( SelectedItem != null)
                {
                    return _items.IndexOf(SelectedItem);
                }
                return -1;
            }
        }

        public ListView <string> Ports { get; set; }
        public ListView <InputSource> Sources { get; set; }
        public string ViewerServerAddress { get; set; }

        public SetupWindowViewModel () {
            Ports   = new ListView <string> ();
            Sources = new ListView<InputSource>();
            ViewerServerAddress = "localhost:4096";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged (string prop) {
            if (PropertyChanged == null) return;
            PropertyChanged (this, new PropertyChangedEventArgs (prop));
        }
    }
}

