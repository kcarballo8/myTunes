using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace myTunes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
  
    public partial class MainWindow : Window
    {
        private readonly MusicRepo musicRepo;
        private readonly ObservableCollection<String> listBoxItems; // Store playlist names and All Music string
        public MainWindow()
        {
            musicRepo = new MusicRepo();

            InitializeComponent();
            listBoxItems = new ObservableCollection<string>();  
            listBoxItems.Add("All Music");  // Add 'All Music' tab to list box
            foreach(String playlist in musicRepo.Playlists) // For each playlist
            {
                listBoxItems.Add(playlist); // Add playlist to listbox
            }
            ListBox1.ItemsSource = listBoxItems;    // Bind ObservableCollection to Actual ListBox
        }
    }
}
