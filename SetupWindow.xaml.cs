using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using NintendoSpy.Readers;
using System.ComponentModel;

namespace NintendoSpy;

public partial class SetupWindow : Window
{
    private readonly SetupWindowViewModel _vm;

    public SetupWindow()
    {
        InitializeComponent();
        _vm = new SetupWindowViewModel();
        DataContext = _vm;

        _vm.ViewerServerAddress = Properties.Settings.Default.ViewerServerAddress;

        var portListUpdateTimer = new DispatcherTimer();
        portListUpdateTimer.Interval = TimeSpan.FromSeconds(1);
        portListUpdateTimer.Tick += (sender, e) => UpdatePortList();
        portListUpdateTimer.Start();

        UpdatePortList();
        _vm.Ports.SelectFirst();
    }

    private void UpdatePortList()
    {
        _vm.Ports.UpdateContents(SerialPort.GetPortNames());
    }

    private void GoButton_Click(object sender, RoutedEventArgs e)
    {
        this.Hide();
        Properties.Settings.Default.ViewerServerAddress = _vm.ViewerServerAddress;
        Properties.Settings.Default.Save();

#if !DEBUG
        try {
#endif
        IControllerReader reader;
        reader = new SerialControllerReader(_vm.Ports.SelectedItem, GameCube.ReadFromPacket);

        new ViewWindow(reader, _vm.ViewerServerAddress)
            .ShowDialog();
#if !DEBUG
        }
        catch (Exception ex) {
            MessageBox.Show (ex.Message, "NintendoSpy", MessageBoxButton.OK, MessageBoxImage.Error);
        }
#endif

        this.Show();
    }
}

public class SetupWindowViewModel : INotifyPropertyChanged
{
    public class ListView<T>
    {
        private readonly List<T> _items;

        public CollectionView Items { get; }
        public T SelectedItem { get; set; }

        public ListView()
        {
            _items = new List<T>();
            Items = new CollectionView(_items);
        }

        public void UpdateContents(IEnumerable<T> items)
        {
            _items.Clear();
            _items.AddRange(items);
            Items.Refresh();
        }

        public void SelectFirst()
        {
            if (_items.Count > 0) SelectedItem = _items[0];
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
            if (SelectedItem != null)
            {
                return _items.IndexOf(SelectedItem);
            }

            return -1;
        }
    }

    public ListView<string> Ports { get; set; }
    public string ViewerServerAddress { get; set; }

    public SetupWindowViewModel()
    {
        Ports = new ListView<string>();
        ViewerServerAddress = "localhost:4096";
    }

    public event PropertyChangedEventHandler PropertyChanged;

    void NotifyPropertyChanged(string prop)
    {
        if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(prop));
    }
}
