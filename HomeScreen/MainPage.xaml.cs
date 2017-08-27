using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Popups;
using Windows.Devices.I2c;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml.Media.Animation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using System.Collections.ObjectModel;
using Windows.System;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HomeScreen
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public List<Image> SecurityCamImages = new List<Image>();
        public DataClasses MyInfo = new DataClasses();
        public Boolean IsDimmed { get; set; }
        static Windows.UI.Xaml.DispatcherTimer tmrPollFast;
        static Windows.UI.Xaml.DispatcherTimer tmrPollSlow;
        public Boolean FirstRun { get; set; }
        private Int32 ShortListHeight = 190;
        private Int32 TallListHeight = 350;
        private Int32 FastPollSeconds = 2;
        private Int32 SlowPollSeconds = 5;
        private Int32 QuickPagerIndex = 0;
        public List<MusicFolderEntry> CurrentEntries;

        public MainPage()
        {
            this.InitializeComponent();
            this.IsDimmed = false;
            this.FirstRun = true;

            tmrPollFast = new Windows.UI.Xaml.DispatcherTimer();
            tmrPollFast.Interval = new TimeSpan(0, 0, FastPollSeconds);
            tmrPollFast.Tick += tmrPollFast_TickAsync;
            tmrPollFast.Start();

            tmrPollSlow = new Windows.UI.Xaml.DispatcherTimer();
            tmrPollSlow.Interval = new TimeSpan(0, 0, SlowPollSeconds);
            tmrPollSlow.Tick += tmrPollSlow_TickAsync;
            tmrPollSlow.Start();

            this.PointerPressed += MyMouseEvent_PointerPressed;

        }
        private void MyMouseEvent_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (this.IsDimmed)
            {
                DisplayFadeMessage("Normal screen");
                SetRPIBrightnessAsync(255);
            }
        }
        private async void btnExpand_Click(object sender, RoutedEventArgs e)
        {
            DisplayFadeMessage("Resize Music Area");
            Windows.UI.Xaml.Controls.Button TheButton = ((Windows.UI.Xaml.Controls.Button)sender);
            Windows.UI.Xaml.Controls.Image ButtonImage = ((Windows.UI.Xaml.Controls.Image)TheButton.Content);

            if (flipView1.Visibility == Visibility.Visible)
            {
                //Expand the music list to take up more space
                flipView1.Visibility = Visibility.Collapsed;
                lvFiles.Height = this.TallListHeight;
                lvPlaylist.Height = this.TallListHeight;
                wvBrowser.Height = this.TallListHeight;
                ButtonImage.Source = new BitmapImage(new Uri(this.BaseUri, "/Assets/collapse_menu.png"));
            }
            else
            {
                //Shrink the music list to take up less space
                flipView1.Visibility = Visibility.Visible;
                lvFiles.Height = this.ShortListHeight;
                lvPlaylist.Height = this.ShortListHeight;
                wvBrowser.Height = this.ShortListHeight;
                ButtonImage.Source = new BitmapImage(new Uri(this.BaseUri, "/Assets/expand_menu.png"));
            }
        }

        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            ShutdownManager.BeginShutdown(Windows.System.ShutdownKind.Restart, TimeSpan.FromSeconds(4));
            DisplayFadeMessage("Re-boot in 3 2 1...");
        }
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit(); 
        }
        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //TODO: Validate format of these
                MyInfo.KodiServer = txtKodiServer.Text;
                MyInfo.BIServer = txtBIServerName.Text;
                MyInfo.StartFolder = txtHomeFolder.Text;
                //MyInfo.StartFolder = "smb://OfficePC/Music/";

                //Add security image codes to config
                MyInfo.SecurityCams.Clear();
                this.SecurityCamImages.Clear(); 
                String[] CamImageCodes = txtCamNames.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                Boolean FirstCam = true;
                for (int i=0;i<CamImageCodes.Length; i++)
                {
                    this.MyInfo.SecurityCams.Add(new WebDownloadInfo() { DisplayString = CamImageCodes[i], IsPrimary = FirstCam, IntervalSeconds = (i+4) });
                    FirstCam = false; 
                }
                foreach (WebDownloadInfo i in MyInfo.SecurityCams)
                {
                    Image img = new Image() { Tag = i.DisplayString };
                    this.SecurityCamImages.Add(img);
                }
                this.flipView1.ItemsSource = this.SecurityCamImages;

                flipView1.SelectionChanged += FlipView1_SelectionChanged;
                btnMusicViewSwitch.Visibility = Visibility.Visible;
                DisplayFadeMessage("APP STARTED...");

                LoadHomeFolder();
                SetUpScreen("folders");
                LoadKodiPlaylistsAsync();
            }
            catch (Exception ex)
            {
                ShowErrorMessageAsync(ex);
            }

        }
        private void btnSet1_Click(object sender, RoutedEventArgs e)
        {
            txtKodiServer.Text = "http://192.168.1.141:8080";
            txtBIServerName.Text = "http://192.168.1.115:81";
            txtTempQuery.Text = "Germantown TN";
            txtCamNames.Text = "Yard,Back,Front,Gate1,Gate2,Shed";
            txtHomeFolder.Text = "smb://OFFICEPC/Music/";
        }
        private void btnSet2_Click(object sender, RoutedEventArgs e)
        {
            txtKodiServer.Text = "http://192.168.1.195:8080";
            txtBIServerName.Text = "http://192.168.1.115:81";
            txtTempQuery.Text = "Germantown TN";
            txtCamNames.Text = "Yard,Back,Front,Gate1,Gate2,Shed";
            txtHomeFolder.Text = "smb://OFFICEPC/Music/";
        }
        private void DisplayFadeMessage(String Message)
        {
            txtFadeMessage.Visibility = Visibility.Visible;
            txtFadeMessage.Text = Message;
            DoubleAnimationUsingKeyFrames _OpacityAnimation = new DoubleAnimationUsingKeyFrames();
            DoubleKeyFrameCollection keyFrames = _OpacityAnimation.KeyFrames;
            LinearDoubleKeyFrame fr = new LinearDoubleKeyFrame();
            fr.Value = 1;
            fr.KeyTime = TimeSpan.FromSeconds(0);
            keyFrames.Add(fr);

            LinearDoubleKeyFrame frEnd = new LinearDoubleKeyFrame();
            frEnd.Value = 0.0;
            frEnd.KeyTime = TimeSpan.FromSeconds(3);
            keyFrames.Add(frEnd);

            ObjectAnimationUsingKeyFrames _VisibilityAnimation = new ObjectAnimationUsingKeyFrames();
            DiscreteObjectKeyFrame frHide = new DiscreteObjectKeyFrame();
            frHide.KeyTime = TimeSpan.FromSeconds(4);
            frHide.Value = Visibility.Collapsed;
            _VisibilityAnimation.KeyFrames.Add(frHide);

            Storyboard _Storyboard = new Storyboard();
            _Storyboard.Children.Add(_OpacityAnimation);
            Storyboard.SetTargetProperty(_OpacityAnimation, "TextBlock.Opacity");
            _Storyboard.Children.Add(_VisibilityAnimation);
            Storyboard.SetTargetProperty(_VisibilityAnimation, "TextBlock.Visibility");
            Storyboard.SetTarget(_Storyboard, txtFadeMessage);
            _Storyboard.Begin();

        }
        private void StartLoadingAnimation()
        {
            this.imgLoading.Visibility = Visibility.Visible;
        }
        private void StopLoadingAnimation()
        {
            this.imgLoading.Visibility = Visibility.Collapsed;
        }

        private void FlipView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                String NewPrimaryTag = ((Windows.UI.Xaml.Controls.Image)flipView1.SelectedItem).Tag.ToString();
                foreach (WebDownloadInfo I in MyInfo.SecurityCams)
                {
                    I.IsPrimary = (I.DisplayString == NewPrimaryTag);
                }

            }
            catch (Exception ex)
            {
                LogMessage("Error switching images " + ex.ToString());
            }
        }
        private void LoadHomeFolder()
        {
            MyInfo.MediaFolders.Clear();
            MyInfo.MediaFolders.NextFolder(MyInfo.StartFolder);
            LoadFileEntries();
            //  lvPlaylist.ItemsSource = this.MyInfo.PlaylistEntries;
            BindingOperations.SetBinding(lvPlaylist, ListView.ItemsSourceProperty, new Binding() { Source = MyInfo.PlaylistEntries, Mode = BindingMode.OneWay });
        }
        private async void LoadKodiPlaylistsAsync()
        {
            try
            {
                //TODO
                 //               String Result = await KodiCommand.GetPlaylists(MyInfo.KodiServer);
                //Parse and display results "{\"id\":0,\"jsonrpc\":\"2.0\",\"result\":[{\"playlistid\":0,\"type\":\"audio\"},{\"playlistid\":1,\"type\":\"video\"},{\"playlistid\":2,\"type\":\"picture\"}]}"
            }
            catch (Exception ex)
            {

            }

        }
        private async void btnHomeDir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadHomeFolder();
            }
            catch (Exception ex)
            {
                ShowErrorMessageAsync(ex);
            }
            
        }
        private async void LoadFileEntries()
        {
            try
            {
                StartLoadingAnimation();
                CurrentEntries = await GetFolderEntries(MyInfo.MediaFolders.CurrentFolder.FullName);
                this.lvFiles.ItemsSource = CurrentEntries;
                btnBackDir.IsEnabled = !MyInfo.MediaFolders.IsTop;
                txtCurrentFolder.Text = MyInfo.MediaFolders.FormattedFolderString();
            }
            catch (Exception ex)
            {
                ShowErrorMessageAsync(ex);
            }
            finally
            {
                StopLoadingAnimation();
            }
        }
        private void btnBackDir_Click(object sender, RoutedEventArgs e)
        {
            /*
            MyInfo.MediaFolders.GoBack();
            LoadFileEntries();
            */
            //Remember what folder we're on, i.e. "/Music/Artists/Beatles"
            String SubFolder = MyInfo.MediaFolders.CurrentFolder.FullName;
            
            //Go up one, i.e. "/Music/Artists"
            MyInfo.MediaFolders.GoBack();

            //If we've been to the parent folder before, we cached it
            Boolean CanScrollDown = MyInfo.FolderCache.Exists(MyInfo.MediaFolders.CurrentFolder.FullName);
            
            //Load entries from new folder
            LoadFileEntries();

            //If we can let's position them where they were, i.e. on the "Beatles" 
            if (CanScrollDown)
            {
                foreach (MusicFolderEntry E in CurrentEntries)
                {
                    if (E.file == SubFolder)
                    {
                        try
                        {
                            lvFiles.ScrollIntoView(E);
                            //int index = CurrentEntries.IndexOf(E);
                            //lvFiles.ScrollIntoView(lvFiles.Items[index], ScrollIntoViewAlignment.Default);
                            lvFiles.UpdateLayout();
                        }
                        catch (Exception ex)
                        {
                            LogMessage(ex.ToString());
                        }
                        break;
                    }
                }

            }

        }

        private async void btnAddAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnAddAll.IsEnabled = false;
                Int32 Counter = 0;
                List<String> Commands = new List<String>();
                foreach (MusicFolderEntry E in (List<MusicFolderEntry>)this.lvFiles.ItemsSource)
                {
                    if (E.filetype == "file" && E.file.ToLower().EndsWith("mp3"))
                    {
                        if (FirstRun)
                        {
                            await ProcessFirstRunAsync(E);
                        }
                        else
                        {
                            Commands.Add(HelperMethods.SerializeObject(new KodiAddFileToPlayList(MyInfo.CurrentPlaylistId, E.file)));
                        }
                        this.MyInfo.PlaylistEntries.Add(E);
                        Counter++;
                    }
                }
                if (Counter > 0)
                {
                    DisplayFadeMessage("Added " + Counter.ToString());
                    if (Commands.Count > 0)
                    {
                        await Task.Factory.StartNew(() => KodiCommand.SendKodiCommandsInOrderAsync(MyInfo.KodiServer, Commands));
                    }
                }
                else
                {
                    DisplayFadeMessage("Nothing added!");
                }
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.ToString());
                await dialog.ShowAsync();
            }
            finally
            {
                btnAddAll.IsEnabled = true;
            }
        }
        private async void btnListViewEntry_ClickAsync(object sender, RoutedEventArgs e)
        {
            MusicFolderEntry Entry = ((HomeScreen.MusicFolderEntry)((Windows.UI.Xaml.FrameworkElement)sender).DataContext);
            if (Entry.filetype.ToLower() == "directory")
            {
                MyInfo.MediaFolders.NextFolder(Entry.file);
                LoadFileEntries();
            }

            if (Entry.filetype.ToLower() == "file")
            {
                //Update the screen - This provides visual feedback but is not kept in sync during folder browsing
                Windows.UI.Xaml.Controls.Button TheButton = ((Windows.UI.Xaml.Controls.Button)sender);
                Windows.UI.Xaml.Controls.Image ButtonImage = ((Windows.UI.Xaml.Controls.Image)TheButton.Content);
                ButtonImage.Source = new BitmapImage(new Uri(this.BaseUri, "/Assets/musiccheck.png")); 
                if (this.FirstRun)
                {
                    //First song you clicked on since the app started
                    ProcessFirstRunAsync(Entry);
                }
                else
                {
                    //Is something playing?
                    KodiActivePlayersResponse Response = await KodiCommand.GetActivePlayers(MyInfo.KodiServer);
                    if (Response.result.Length == 0)
                    {
                        ProcessFirstRunAsync(Entry);
                    }
                    else
                    {
                        //Add file to end of playing list... 
                        KodiAddFileToPlayList AddRequest = new KodiAddFileToPlayList(MyInfo.CurrentPlaylistId, Entry.file);
                        Task.Factory.StartNew(() => KodiCommand.SendKodiCommandAsync(MyInfo.KodiServer, HelperMethods.SerializeObject(AddRequest)));
                        this.MyInfo.PlaylistEntries.Add(Entry);
                    }
                    
                }
            }
        }
        private async Task ProcessFirstRunAsync(MusicFolderEntry Entry)
        {
            try
            {
                List<String> Commands;
                
                Boolean PlaylistExists = await SyncPlaylistWithKodi(MyInfo.CurrentPlaylistId); //Does a playlist with entries exist in Kodi?
                KodiAddFileToPlayList AddRequest = new KodiAddFileToPlayList(MyInfo.CurrentPlaylistId, Entry.file);
                if ((!PlaylistExists || MyInfo.PlaylistEntries.Count == 0))
                {
                    //If playlist id does not exist or has no entries, then create it.
                    Commands = new List<string>();
                    Commands.Add(HelperMethods.SerializeObject(new KodiClearPlayList(MyInfo.CurrentPlaylistId)));
                    Commands.Add(HelperMethods.SerializeObject(AddRequest));
                    Commands.Add(HelperMethods.SerializeObject(new KodiOpenPlayList(MyInfo.CurrentPlaylistId, 0)));
                    await KodiCommand.SendKodiCommandsInOrderAsync(MyInfo.KodiServer, Commands);
                    this.MyInfo.PlaylistEntries.Add(Entry);
                }
                else
                {
                    //Playlist exists and may have entries, add file to end and resync
                    await KodiCommand.SendKodiCommandAsync(MyInfo.KodiServer, HelperMethods.SerializeObject(AddRequest));
                    await SyncPlaylistWithKodi(MyInfo.CurrentPlaylistId);

                    //If something is not already playing then start playlist at currently added file
                    KodiActivePlayersResponse Resp = await KodiCommand.GetActivePlayers(MyInfo.KodiServer);
                    if (Resp.result == null || Resp.result.Length == 0)
                    { 
                        await KodiCommand.SendKodiCommandAsync(MyInfo.KodiServer, HelperMethods.SerializeObject(new KodiOpenPlayList(MyInfo.CurrentPlaylistId, this.MyInfo.PlaylistEntries.Count())));
                    }
                    
                }
                
                //Set repeat mode to true
                await Task.Factory.StartNew(() => KodiCommand.SendKodiCommandAsync(MyInfo.KodiServer, HelperMethods.SerializeObject(new KodiSetRepeat(MyInfo.CurrentPlayerId,"all"))));
            }
            catch (Exception ex)
            {
                ShowErrorMessageAsync(ex);
            }
            this.FirstRun = false;
        }
        private async Task<Boolean> SyncPlaylistWithKodi(Int32 PlaylistId)
        {
            KodiPlayListItemsRequest ItemsInList = new KodiPlayListItemsRequest(PlaylistId);
            KodiPlayListItemsResponse Response = await KodiCommand.GetPlaylistItems(MyInfo.KodiServer, ItemsInList);
            if (Response.result.items != null && Response.result.items.Count() > 0)
            {
                MyInfo.PlaylistEntries.Clear(); 
                //Pull in existing items
                foreach (KodiPlayListItem I in Response.result.items)
                {
                    MusicFolderEntry E = new MusicFolderEntry() { Line1Display = I.title, file = I.file, Line2Display = I.file, Track = I.track };
                    if (I.artist != null && I.artist.Count() > 0)
                    {
                        E.Line2Display = I.artist[0];
                    }
                    this.MyInfo.PlaylistEntries.Add(E);
                }
                return true;
            }
            else
            {
                return false; 
            }
        }
        private void btnPlaylistViewEntry_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MusicFolderEntry item = (MusicFolderEntry)(sender as FrameworkElement).DataContext;
                int index = lvPlaylist.Items.IndexOf(item);
                KodiCommand.SendKodiCommandAsync(MyInfo.KodiServer, HelperMethods.SerializeObject(new KodiOpenPlayList(MyInfo.CurrentPlaylistId, index)));
                item.IsChecked = true;
                foreach (MusicFolderEntry listitem in MyInfo.PlaylistEntries)
                {
                    if (listitem.IsChecked && listitem.file != item.file)
                    {
                        listitem.IsChecked = false;
                    }
                    else
                    {
                        listitem.IsChecked = (listitem.file == item.file);
                    }
                }

                lvPlaylist.ItemsSource = null;
                BindingOperations.SetBinding(lvPlaylist, ListView.ItemsSourceProperty, new Binding() { Source = MyInfo.PlaylistEntries, Mode = BindingMode.OneWay });
                lvPlaylist.ScrollIntoView(item);
            }
            catch (Exception ex)
            {
                LogMessage("Error " + ex.ToString());
            }
        }
        private void btnPlaylistDeleteEntry_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MusicFolderEntry item = (MusicFolderEntry)(sender as FrameworkElement).DataContext;
                int index = lvPlaylist.Items.IndexOf(item);
                KodiCommand.SendKodiCommandAsync(MyInfo.KodiServer, HelperMethods.SerializeObject(new KodiRemoveItemFromPlaylist(MyInfo.CurrentPlaylistId, index)));
                foreach (MusicFolderEntry listitem in MyInfo.PlaylistEntries)
                {
                    if (listitem.file == item.file)
                    {
                        MyInfo.PlaylistEntries.Remove(listitem);
                        break;
                    }
                }
                lvPlaylist.ItemsSource = null;
                BindingOperations.SetBinding(lvPlaylist, ListView.ItemsSourceProperty, new Binding() { Source = MyInfo.PlaylistEntries, Mode = BindingMode.OneWay });
                lvPlaylist.ScrollIntoView(item);
            }
            catch (Exception ex)
            {
                LogMessage("Error " + ex.ToString());
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (wvBrowser.Visibility == Visibility.Visible)
            {
                SetUpScreen("settings");
            }
            else
            {
                SetUpScreen("browser");
            }
        }
        private void btnMusicViewSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (lvPlaylist.Visibility == Visibility.Visible)
            {
                SetUpScreen("folders");
            }
            else
            {
                SetUpScreen("playlist");
            }
        }
        private void SetUpScreen(String Mode)
        {
            pnlSettings.Visibility = Visibility.Collapsed;
            pnlFolderControls.Visibility = Visibility.Collapsed;
            lvFiles.Visibility = Visibility.Collapsed;
            pnlPlaylistControls.Visibility = Visibility.Collapsed;
            lvPlaylist.Visibility = Visibility.Collapsed;
            wvBrowser.Visibility = Visibility.Collapsed;

            switch (Mode)
            {
                case "playlist":
                    pnlPlaylistControls.Visibility = Visibility.Visible;
                    lvPlaylist.Visibility = Visibility.Visible;
                    DisplayFadeMessage("Playlist");
                    break;
                case "folders":
                    pnlFolderControls.Visibility = Visibility.Visible;
                    lvFiles.Visibility = Visibility.Visible;
                    DisplayFadeMessage("Browse Folders");
                    break;
                case "browser":
                    wvBrowser.Visibility = Visibility.Visible;
                    this.wvBrowser.Navigate(new Uri("http://brians:8080/#browser/music"));
                    DisplayFadeMessage("Kodi Browser");
                    break;
                case "settings":
                    pnlSettings.Visibility = Visibility.Visible;
                    DisplayFadeMessage("Settings");
                    break;
            }
        }
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            //TODO: When these buttons are clicked update playlist to show what's playing
            KodiMoveRequest R = new KodiMoveRequest(MyInfo.CurrentPlayerId, "left");
            Task.Factory.StartNew(() => KodiCommand.SendKodiCommandAsync(MyInfo.KodiServer, HelperMethods.SerializeObject(R)));
        }
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            KodiMoveRequest R = new KodiMoveRequest(MyInfo.CurrentPlayerId, "right");
            Task.Factory.StartNew(() => KodiCommand.SendKodiCommandAsync(MyInfo.KodiServer, HelperMethods.SerializeObject(R)));
        }
        private void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            KodiPlayPauseRequest R = new KodiPlayPauseRequest(MyInfo.CurrentPlayerId);
            Task.Factory.StartNew(() => KodiCommand.SendKodiCommandAsync(MyInfo.KodiServer, HelperMethods.SerializeObject(R)));
        }
        private async void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            KodiSetShuffle R = new KodiSetShuffle(MyInfo.CurrentPlayerId);
            await Task.Factory.StartNew(() => KodiCommand.SendKodiCommandAsync(MyInfo.KodiServer, HelperMethods.SerializeObject(R)));
            await SyncPlaylistWithKodi(MyInfo.CurrentPlaylistId);
            DisplayFadeMessage("Shuffle Toggled");
        }
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            List<string> Commands = new List<string>();
            Commands.Add(HelperMethods.SerializeObject(new KodiPlayerStop(MyInfo.CurrentPlayerId)));
            Commands.Add(HelperMethods.SerializeObject(new KodiClearPlayList(MyInfo.CurrentPlaylistId)));
            Task.Factory.StartNew(() => KodiCommand.SendKodiCommandsInOrderAsync(MyInfo.KodiServer, Commands));
            MyInfo.PlaylistEntries.Clear(); 
            DisplayFadeMessage("Playlist Cleared");
        }
        async void tmrPollFast_TickAsync(object sender, object e)
        {
            //This is the "Fast" Timer updates the main image
            if (!String.IsNullOrWhiteSpace(MyInfo.BIServer) && SecurityCamImages.Count > 0)
            {
                try
                {
                    String PrimaryTag = this.MyInfo.SecurityCams.Where(x => x.IsPrimary == true).ToList()[0].DisplayString;
                    String BIBaseURL = MyInfo.BIServer;
                    if (!BIBaseURL.EndsWith("/")) { BIBaseURL += "/"; }
                    BIBaseURL += "image/";
                    BitmapImage image = await HelperMethods.GetBlueIrisImage(BIBaseURL, PrimaryTag);
                    this.imgMain.Source = image as ImageSource;
                    for (int count = 0; count < SecurityCamImages.Count; count++)
                    {
                        if (SecurityCamImages[count].Tag.ToString() == PrimaryTag)
                        {
                            SecurityCamImages[count].Source = image as ImageSource;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage("Error primary cam: " + ex.ToString());
                }
            }
            this.Time.Text = DateTime.Now.ToString("h:mm tt");
        }
        async void tmrPollSlow_TickAsync(object sender, object e)
        {
            //This is the "Slow" Timer that updates the temperature, small thumbnails
            tmrPollSlow.Stop();
            if (this.MyInfo.CurrentTemp.GetNeeded() && !this.IsDimmed)
            {
                try
                {
                    txtCurrentTemp.Text = await HelperMethods.GetCurrentTempAsync(txtTempQuery.Text);
                    MyInfo.CurrentTemp.DisplayString = txtCurrentTemp.Text;
                    MyInfo.CurrentTemp.LastRetrieved = DateTime.Now;
                    LogMessage("Got current temp");
                }
                catch (Exception ex)
                {
                    LogMessage("Error getting weather: " + ex.ToString());
                }
                
            }
            if (!String.IsNullOrWhiteSpace(MyInfo.BIServer) && !this.IsDimmed)
            {
                try
                {
                    String BIBaseURL = MyInfo.BIServer;
                    if (!BIBaseURL.EndsWith("/")) { BIBaseURL += "/"; }
                    BIBaseURL += "image/";
                    Int32 ThumbCounter = 1;
                    foreach (WebDownloadInfo CamInfo in MyInfo.SecurityCams)
                    {
                        if (!CamInfo.IsPrimary && CamInfo.GetNeeded())
                        {
                            BitmapImage image = await HelperMethods.GetBlueIrisImage(BIBaseURL, CamInfo.DisplayString);
                            for (int count = 0; count < SecurityCamImages.Count; count++)
                            {
                                if (SecurityCamImages[count].Tag.ToString() == CamInfo.DisplayString)
                                {
                                    SecurityCamImages[count].Source = image as ImageSource;
                                    CamInfo.LastRetrieved = DateTime.Now;
                                    ThumbnailHackTemp(ThumbCounter, SecurityCamImages[count].Source);
                                    ThumbCounter += 1;
                                    if (ThumbCounter > 3) { ThumbCounter = 1; }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage("Error getting cam image: " + ex.ToString());
                }
            }
            tmrPollSlow.Start();
        }
        private void ThumbnailHackTemp(Int32 Counter, ImageSource src)
        {
            if (Counter == 1) { imgThumb1.Source = src; }
            if (Counter == 2) { imgThumb2.Source = src; }
            if (Counter == 3) { imgThumb3.Source = src; }
        }


        private void BrightnessAdjustment_Click(object sender, RoutedEventArgs e)
        {
            this.IsDimmed = !this.IsDimmed;
            if (this.IsDimmed)
            {
                DisplayFadeMessage("Dimming screen");
                SetRPIBrightnessAsync(20);
            }
            else
            {
                DisplayFadeMessage("Normal screen");
                SetRPIBrightnessAsync(255);
            }
            
        }

        private async void SetRPIBrightnessAsync(Int16 brightness)
        {
            try
            {
                I2cDevice screen;
                I2cConnectionSettings settings = new I2cConnectionSettings(0x45);
                var dis = await DeviceInformation.FindAllAsync(I2cDevice.GetDeviceSelector("I2C1"));
                screen = await I2cDevice.FromIdAsync(dis[0].Id, settings);
                Byte[] writeBuff = new Byte[] { 0x86, Convert.ToByte(brightness) }; //backlight address, brightness 0-255 
                screen.Write(writeBuff);
                

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error " + ex.ToString());
            }
            
        }
        public Boolean FormatFolderEntry(String Folder, KodiFileEntry KodiVersion, MusicFolderEntry MyVersion)
        {
            if (KodiVersion.file.EndsWith(".jpg") || KodiVersion.file.EndsWith(".txt"))
            {
                return false;
            }
            if (Folder.ToLower().EndsWith("music/") && KodiVersion.file.ToLower().Contains("excluded"))
            {
                return false;
            }

            switch (KodiVersion.filetype.ToLower())
            {
                case "directory":
                    MyVersion.file = KodiVersion.file;
                    MyVersion.filetype = KodiVersion.filetype;
                    MyVersion.Line1Display = KodiVersion.label;
                    MyVersion.Line2Display = String.Empty;
                    MyVersion.HasInfo = true;
                    break;
                case "file":
                    MyVersion.file = KodiVersion.file;
                    MyVersion.filetype = KodiVersion.filetype;
                    MyVersion.Line2Display = KodiVersion.label;
                    
                    if (!String.IsNullOrEmpty(KodiVersion.title))
                    {
                        //If the title property is populated from Kodi, "metadata exists"
                        MyVersion.Line1Display = KodiVersion.title;
                        MyVersion.HasInfo = true;
                    }
                    else
                    {
                        //If the title property is not populated from Kodi, hack it out from the filename
                        try
                        {
                            MyVersion.Line1Display = KodiVersion.label;
                            MyVersion.HasInfo = false;
                            if (Folder.ToLower().Contains("/artists"))
                            {
                                MP3DisplayFromArtistPath(Folder, KodiVersion.label, MyVersion);
                            }
                            else
                            {
                                MP3DisplayFromCollectionPath(Folder, KodiVersion.label, MyVersion);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogMessage("Error formatting " + ex.ToString());
                        }
                    }

                    //If we have a track use it...
                    if (!String.IsNullOrWhiteSpace(KodiVersion.track))
                    {
                        MyVersion.Track = KodiVersion.track; 
                    }

                    if (KodiVersion.artist != null && KodiVersion.artist.Count() > 0)
                    {
                        MyVersion.Line2Display = KodiVersion.artist[0]; 
                    }

                    break;
               
                default:
                    return false;
            }

            return true; 
        }
        private void MP3DisplayFromArtistPath(String Folder, String Label, MusicFolderEntry E)
        {
            String[] Parts;
            Int32 TrackNumber = 0;
            Boolean HasTrackNumber;
            Label = Label.Replace(".mp3", "").Replace("_","");
            if (Label.Contains("-"))
            {
                Parts = Label.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                HasTrackNumber = Int32.TryParse(Parts[0].Trim().Replace(".", ""), out TrackNumber);
                if (HasTrackNumber)
                {
                    E.Track = TrackNumber.ToString();
                    if (Parts[1].Trim().Length < 4)
                    {
                        for (int i = 1; i < Parts.Length; i++)
                        {
                            E.Line1Display += " " + Parts[i];
                        }
                    }
                    else
                    {
                        E.Line1Display = Parts[1];
                    }
                }
                else
                {
                    E.Line1Display = Parts[0];
                }
            }
            else
            {
                HasTrackNumber = Int32.TryParse(Label.Substring(0,2),out TrackNumber);
                if (HasTrackNumber)
                {
                    E.Track = TrackNumber.ToString();
                    E.Line1Display = Label.Substring(2).Replace(".", "");
                }
                else
                {
                    E.Line1Display = Label;
                }
            }
        }
        private void MP3DisplayFromCollectionPath(String Folder, String Label, MusicFolderEntry E)
        {
            String[] Parts;
            Int32 TrackNumber = 0;
            Label = Label.Replace(".mp3", "");
            if (Folder.ToLower().Contains("billboard")) //Billboard top hits folder is all years..
            {
                if (Label.Substring(2, 1) == "_" && Int32.TryParse(Label.Substring(3, 3), out TrackNumber))
                {
                    E.Track = TrackNumber.ToString(); 
                    Parts = Label.Substring(6).Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    Parts = Label.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            else
            {
                Parts = Label.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            }
            if (Parts.Length > 0)
            {
                E.Line1Display = Parts[0];
                if (Parts.Length > 1)
                {
                    E.Line2Display = Parts[1] + " ";
                    for (int i = 2; i < Parts.Length; i++)
                    {
                        E.Line2Display += " " + Parts[i];
                    }
                }

            }
            else
            {
                E.Line1Display = Label; 
            }
        }
        public async Task<List<MusicFolderEntry>> GetFolderEntries(String Folder)
        {
            List<MusicFolderEntry> Entries = new List<MusicFolderEntry>();
            try
            {
                if (MyInfo.FolderCache.Exists(Folder))
                {
                    Entries = MyInfo.FolderCache.FromCache(Folder);
                }
                else
                {
                    KodiDirectoryRequest Req = new KodiDirectoryRequest() { id = 1 };
                    Req._params.directory = Folder;
                    //Use folder structure at the top, then library structure as we get deeper
                    if (!MyInfo.MediaFolders.ShouldDisplayWithFolderStructure)
                    {
                        Req._params.media = "music";
                    }
                    KodiFileResponse FileList = await KodiCommand.GetDirectoryContents(MyInfo.KodiServer, Req);
                    foreach (KodiFileEntry E in FileList.result.files)
                    {
                        MusicFolderEntry M = new MusicFolderEntry();
                        if (FormatFolderEntry(Folder, E, M))
                        {
                            Entries.Add(M);
                        }
                    }
                    MyInfo.FolderCache.ToCache(Folder, Entries);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not get folders from Kodi", ex);
            }
            return Entries;
        }
        private async void ShowErrorMessageAsync(Exception ex)
        {
            try
            {
                var dialog = new MessageDialog(ex.ToString());
                await dialog.ShowAsync();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }
        private void LogMessage (String Message)
        {
            //TODO: Log to rolling listview
            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString() + " - " + Message);
        }

        private void lvFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (lvFiles.Items.Count > 1000 && flipView1.Visibility == Visibility.Collapsed)
            {
                QuickPagerIndex = lvFiles.SelectedIndex;
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
            
        }

        private void btnPgUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lvFiles.ScrollIntoView(lvFiles.Items[QuickPagerIndex - 100]);
                QuickPagerIndex -= 100;
            }
            catch (Exception ex)
            {
                LogMessage("Error " + ex.ToString());
            }
        }

        private void btnPgDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lvFiles.ScrollIntoView(lvFiles.Items[QuickPagerIndex + 100]);
                QuickPagerIndex += 100;
            }
            catch (Exception ex)
            {
                LogMessage("Error " + ex.ToString());
            }
        }
    }
}
