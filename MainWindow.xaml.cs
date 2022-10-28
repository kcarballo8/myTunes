using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
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
        private readonly ObservableCollection<String> playlists; // Store playlist names and All Music string
        private readonly ObservableCollection<Song> songs;
        private readonly MediaPlayer mediaPlayer;
        public MainWindow()
        {
            musicRepo = new MusicRepo();

            InitializeComponent();
            mediaPlayer = new MediaPlayer();
            playlists = new ObservableCollection<string>();
            songs = new ObservableCollection<Song>();

            playlists.Add("All Music");  // Add 'All Music' tab to list box
            
            foreach(String playlist in musicRepo.Playlists) // For each playlist
            {
                playlists.Add(playlist); // Add playlist to listbox
                //musicRepo.AddPlaylist(playlist); add to database
            }
            ListBox1.ItemsSource = playlists;    // Bind ObservableCollection to Actual ListBox
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
            dataGrid1.ItemsSource = songs; //Bind songs Observable collection to the data grid
        }
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
        }

        private void PlaySong_Click(object sender, RoutedEventArgs e)
        {
            PlayButton_Click(sender, e);
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "",
                DefaultExt = "*.wma;*.wav;*mp3;*.m4a",
                Filter = "Media files|*.mp3;*.m4a;*.wma;*.wav|MP3 (*.mp3)|*.mp3|M4A (*.m4a)|*.m4a|Windows Media Audio (*.wma)|*.wma|Wave files (*.wav)|*.wav|All files|*.*"
            };

            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                // Selected file is openFileDialog.FileName
                // Call the MusicRepo method AddSong() to read the song's metadata from the opened file.
                Song? s = musicRepo.AddSong(openFileDialog.FileName);

                // Call the MusicRepo method AddSong() to add the song to the DataSet.
                musicRepo.AddSong(s);

                // Call the MusicRepo method Save() to save the DataSet to the music.xml file.
                musicRepo.Save();
            }
        }

        private void DataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //when the user selects a song from the data grid
            Console.WriteLine(dataGrid1.SelectedItem);
            Song? song = dataGrid1.SelectedItem as Song;

            if (song != null)  // Prevent exception being thrown if song is not found
            {
                Song s = musicRepo.GetSong(song.Id);
                if(s != null)
                {
                    songTitle.Header = s.Title;
                    songArtist.Header = s.Artist;
                    songAlbum.Header = s.Album;
                    songGenre.Header = s.Genre;
                    mediaPlayer.Open(new Uri(s.Filename));
                }
               
            }
            
        }

        private void ListBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string? playlist = ListBox1.SelectedItem.ToString();
            if(playlist != null && playlist != "All Music") // Prevent exception being thrown if playlist is null
            {
                DataTable data = musicRepo.SongsForPlaylist(playlist);


                foreach (Song s in songs.ToList())  // Delete all songs from observable collection
                {
                    songs.Remove(s);
                }

                foreach(DataRow row in data.Rows)   
                {
                    songs.Add(new Song //adding to the observable collection 
                    {
                        Id = Int32.Parse(row[0].ToString()!),
                        Title = row[2].ToString()!,
                        Artist = row[3].ToString()!,
                        Album = row[4].ToString()!,
                        Genre = row[5].ToString()!,
                    });
                }


            }
            else if(playlist != null && playlist == "All Music")
            {
                foreach (Song s in songs.ToList())
                {
                    songs.Remove(s);
                }

                foreach(DataRow row in musicRepo.Songs.Rows)
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
            }
            
        }

        private void NewPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            newPlaylistWindow playlistWindow = new newPlaylistWindow();
            playlistWindow.ShowDialog();
            string playlist = playlistWindow.playlistTextBox.Text;
            if(playlistWindow.DialogResult == true && musicRepo.PlaylistExists(playlist) == false)
            {
                musicRepo.AddPlaylist(playlist);
                playlists.Add(playlist);
            }
            else if(musicRepo.PlaylistExists(playlist))
            {
                playlistWarning warn = new playlistWarning();
                warn.warningMessage.Content = "Playlist name already exists";
                warn.ShowDialog();
            }
            
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            deleteConfirmationWindow confirm = new deleteConfirmationWindow();
            confirm.ShowDialog();


        }

        public bool doesPlaylistExist(string name)
        {
            return musicRepo.PlaylistExists(name);
        }
    }
}
