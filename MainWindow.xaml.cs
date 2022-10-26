using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
using TagLib;
using TagLib.Jpeg;
using static System.Net.Mime.MediaTypeNames;

namespace myTunes
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
  
    public partial class MainWindow : Window
    {
        private readonly MusicRepo musicRepo;
        private readonly ObservableCollection<String> listBoxItems; // Store playlist names and All Music string
        private readonly ObservableCollection<Song> songs;
        private readonly MediaPlayer mediaPlayer;
        public MainWindow()
        {
            musicRepo = new MusicRepo();

            InitializeComponent();
            mediaPlayer = new MediaPlayer();
            listBoxItems = new ObservableCollection<string>();
            songs = new ObservableCollection<Song>();

            listBoxItems.Add("All Music");  // Add 'All Music' tab to list box
            
            foreach(String playlist in musicRepo.Playlists) // For each playlist
            {
                listBoxItems.Add(playlist); // Add playlist to listbox
                //musicRepo.AddPlaylist(playlist); add to database
            }
            ListBox1.ItemsSource = listBoxItems;    // Bind ObservableCollection to Actual ListBox

            foreach (DataRow row in musicRepo.Songs.Rows) //looping over rows of the data table
            {
                songs.Add(new Song //adding to the observable collection 
                {
                    Id = (int)row["id"],
                    Title = row["title"].ToString()!,
                    Artist = row["artist"].ToString()!,
                    Album = row["album"].ToString()!,
                    Filename = row["filename"].ToString()!,
                    Length = row["length"].ToString()!,
                    Genre = row["genre"].ToString()!,
                    AboutUrl = row["url"].ToString()!,
                    AlbumImageUrl = row["albumImage"].ToString()!
                });
            }
            dataGrid1.ItemsSource = songs;
        }
        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
        }

        private void PlaySong_Click(object sender, RoutedEventArgs e)
        {
            playButton_Click(sender, e);
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }
    }
}
