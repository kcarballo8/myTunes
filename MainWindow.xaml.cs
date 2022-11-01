using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
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

        private Point startPoint;
        private string? playlistName;

        private readonly MusicRepo musicRepo;
        private readonly ObservableCollection<String> playlists; // Store playlist names and All Music string
        private readonly ObservableCollection<Song> songs;
        private readonly MediaPlayer mediaPlayer;
        bool selected = false;
        bool canStop = false;
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
            }
            ListBox1.ItemsSource = playlists;    // Bind ObservableCollection to Actual ListBox
           
            dataGrid1.ItemsSource = songs; //Bind songs Observable collection to the data grid
        }
   
        private void PlaySong_Click(object sender, RoutedEventArgs e)
        {
            Song? song = dataGrid1.SelectedItem as Song;
            if (song != null)
            {
                mediaPlayer.Open(new Uri(song.Filename));
            }
            mediaPlayer.Play();
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
                string? selectedPlaylist = ListBox1.SelectedItem?.ToString();
                if(selectedPlaylist != "All Music")
                {
                    ListBox1.SelectedItem = "All Music";
                }
                Song? s = musicRepo.AddSong(openFileDialog.FileName);
                //musicRepo.AddSong(s);
                songs.Add(s);
                mediaPlayer.Open(new Uri(s.Filename));
                mediaPlayer.Play();
                dataGrid1.SelectedItem = songs.Last();
                canStop = true;
            }
        }
        private void ListBox1_SelectionChanged(object sender, SelectionChangedEventArgs e) // If user selects another album or when the app originally opens
        {
            string? playlist = ListBox1.SelectedItem?.ToString();
            
            if (playlist != null && playlist != "All Music") // Prevent exception being thrown if playlist is null
            {
                RenamePlaylist.IsEnabled = true;
                DeletePlaylist.IsEnabled = true;
                RenamePlaylist.Header = "Rename Playlist";
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
                selected = false;
            }
            else if(playlist != null && playlist == "All Music")
            {
                RenamePlaylist.IsEnabled = false;
                DeletePlaylist.IsEnabled = false;

                foreach (Song s in songs.ToList())  // Remove all songs 
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
                selected = false;
            }
        }

        private void NewPlaylistButton_Click(object sender, RoutedEventArgs e)  // User clicks button to add new playlist
        {
            newPlaylistWindow playlistWindow = new newPlaylistWindow(); // Create new window for user to enter name of new playlist
            playlistWindow.ShowDialog();                                // Show user new window just created
            string playlist = playlistWindow.playlistTextBox.Text;
            if(playlistWindow.DialogResult == true && musicRepo.PlaylistExists(playlist) == false)  // If user selected Ok button and playlist doesn't already exist
            {
                musicRepo.AddPlaylist(playlist);                                // Add playlist to musicRepo   
                playlists.Add(playlist);                                        // Add playlist name to observable collection
            }
            else if(musicRepo.PlaylistExists(playlist))                         // If playlist name already exists
            {
                playlistWarning warn = new playlistWarning();                   // Create new playlist warning dialog box
                warn.ShowDialog();                                              // Show dialog box to user because playlist name already exists.
            }
            
        }
        private void DeleteSong_Click(object sender, RoutedEventArgs e)
        {
            string? playlist = ListBox1.SelectedItem?.ToString();
            Song? song = dataGrid1.SelectedItem as Song;
            if (playlist == "All Music")
            {
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure you want to remove?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (messageBoxResult == MessageBoxResult.Yes && song != null)
                {
                    musicRepo.DeleteSong(song.Id);  // Deletes song         
                   // musicRepo.Save();               // Saves removal of the song
                    songs.Remove(song);             // Remove from ObservableCollection (Removes it from datagrid
                }
            }
            else
            {
                if (song != null)
                {
                    musicRepo.DeleteSong(song.Id);  // Deletes song         
                    //musicRepo.Save();               // Saves removal of the song
                    songs.Remove(song);             // Remove from ObservableCollection (Removes it from datagrid
                }
            }
            selected = false;
        }
        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            Rename_Playlist playlistWindow = new Rename_Playlist(); // Create new window for user to enter name of new playlist
            playlistWindow.ShowDialog();

            string? oldPlaylist = ListBox1.SelectedItem?.ToString();
            string newPlaylistName = playlistWindow.playlistTextBox.Text;

            if (playlistWindow.DialogResult == true && musicRepo.PlaylistExists(newPlaylistName) == false)  // If user selected Ok button and playlist doesn't already exist
            {
                musicRepo.RenamePlaylist(oldPlaylist, newPlaylistName);   // rename playlist to musicRepo
                int i = 0;
                foreach (String playlist in playlists.ToList()) // For each playlist
                {
                    if (playlist == oldPlaylist)                // If current playlist name is the playlist that needs to be renamed
                    {
                        playlists.Insert(i, newPlaylistName);  // Insert new playlist into ObservableCollection
                        ListBox1.SelectedIndex = i;            // Changes selected playlist to playlist that was just inserted
                        playlists.Remove(oldPlaylist);         // Removes old playlist from ObservableCollection
                    }
                    i++;                                       // Increment i
                }
            }
            else if (musicRepo.PlaylistExists(newPlaylistName))                         // If playlist name already exists
            {
                playlistWarning warn = new playlistWarning();                   // Create new playlist warning dialog box
                warn.ShowDialog();                                              // Show dialog box to user because playlist name already exists.
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            string? playlistToDelete = ListBox1.SelectedItem.ToString();
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure you want to remove?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(messageBoxResult == MessageBoxResult.Yes)
            {
                if (playlistToDelete != null)
                {
                    musicRepo.DeletePlaylist(playlistToDelete); // Delete playlist
                    musicRepo.Save();               // Saves removal of the song
                    playlists.Remove(playlistToDelete);     // Remove playlist name from List box 
                    ListBox1.SelectedIndex = 0; // After deleting playlist return to "All Music" tab
                }
            }
           
        }

        private void PlayCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = selected;

        }

        private void PlayCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Song? song = dataGrid1.SelectedItem as Song;
            if (song != null)
            {
                mediaPlayer.Open(new Uri(song.Filename));
            }
            mediaPlayer.Play();
            canStop = true;
        }

        private void DataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string? playlist = ListBox1.SelectedItem?.ToString();

            if (playlist != "All Music")
            {
                RemoveSong.Header = "Remove from Playlist";
            }
            else
            {
                RemoveSong.Header = "Remove";
            }
           
            selected = true;
        }

        private void StopPlaying_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mediaPlayer.Stop();
            canStop = false;
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = canStop;
        }
      
        private void DataGrid1_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;
            Song? song = dataGrid1.SelectedItem as Song;
            string? songId = song?.Id.ToString();
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                if (songId != null)
                {
                    DragDrop.DoDragDrop(dataGrid1, songId, DragDropEffects.Copy);
                }
            }
        }
        private void DataGrid1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }
        private void ListBox1_DragOver(object sender, DragEventArgs e)
        {
            Label? playlist = sender as Label;

            if (playlist != null)
            {
                playlistName = playlist.Content.ToString();
            }

        }

        private void ListBox1_Drop(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                //string dataString = (string)e.Data.GetData();
                string id = (string)e.Data.GetData(DataFormats.StringFormat);
                int numId = Int32.Parse(id);
                if (id != null && playlistName != null && playlistName != "All Music")
                {
                    musicRepo.AddSongToPlaylist(numId, playlistName);

                }
            }
        }

        private void ListBox1_KeyDown_1(object sender, KeyEventArgs e)
        {
            //https://stackoverflow.com/questions/8072032/capturing-ctrl-x-with-the-keydown-event-of-a-textbox-in-wpf
            string? playlist;
            playlist = ListBox1.SelectedItem?.ToString();
            if (playlist != "All Music" && e.Key == Key.R && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                Rename_Click(sender, e);
            }
            else if (playlist != "All Music" && e.Key == Key.Delete)
            {
                Delete_Click(sender, e);
            }
        }

        private void myTunes_Closed(object sender, EventArgs e) // Executes right before the program is closed
        {
            musicRepo.Save();     // Save all changes
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            // link for the text box code in the xaml: https://stackoverflow.com/questions/7425618/how-can-i-add-a-hint-text-to-wpf-textbox
            //https://stackoverflow.com/questions/42821518/search-in-datagrid-by-textbox-wpf
           var filtered = songs.Where(Title => Title.Title.Contains(TextBox1.Text, System.StringComparison.CurrentCultureIgnoreCase) 
           || Title.Artist.Contains(TextBox1.Text, System.StringComparison.CurrentCultureIgnoreCase)
           || Title.Album.Contains(TextBox1.Text, System.StringComparison.CurrentCultureIgnoreCase) ||
           Title.Genre.Contains(TextBox1.Text, System.StringComparison.CurrentCultureIgnoreCase));
           dataGrid1.ItemsSource = filtered;
        }
    }
}
