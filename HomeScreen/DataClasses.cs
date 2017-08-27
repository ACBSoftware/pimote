using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HomeScreen
{
    public class WebDownloadInfo
    {
        public String DisplayString { get; set; }
        public DateTime LastRetrieved { get; set; }
        public Int32 IntervalSeconds { get; set; }
        public Boolean IsPrimary { get; set; }
        public Boolean GetNeeded()
        {
            return DateTime.Now > LastRetrieved.AddSeconds(IntervalSeconds);
        }
    }
    public class DataClasses : INotifyPropertyChanged
    {
        public String BIServer { get; set; }
        public String KodiServer { get; set; }
        public String StartFolder { get; set; }
        public WebDownloadInfo CurrentTemp { get; set; }
        public List<WebDownloadInfo> SecurityCams { get; set; }
        public FolderPosition MediaFolders { get; set; }
        public PermaCache FolderCache { get; set; }
        public ObservableCollection <MusicFolderEntry> PlaylistEntries { get; set; }
        public Int32 CurrentPlaylistId { get; set; }
        public Int32 CurrentPlayerId { get; set; }
        public DataClasses()
        {
            this.CurrentTemp = new WebDownloadInfo(){ DisplayString="", LastRetrieved=DateTime.Now.AddMinutes(-10), IntervalSeconds=600};
            this.SecurityCams = new List<WebDownloadInfo>();
            this.MediaFolders = new FolderPosition();
            this.FolderCache = new PermaCache();
            this.PlaylistEntries = new ObservableCollection<MusicFolderEntry>();
            this.CurrentPlaylistId = 0;
            this.CurrentPlayerId = 0; //TODO: Should I set the player id a get active player call? Have not researched KODI API enough to know yet...
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
    public class FolderInfo
    {
        public String DisplayString { get; set; }
        public String FullName { get; set; }
    }
    public class FolderPosition
    {

        Stack<FolderInfo> FolderStack { get; set; }
        public FolderInfo CurrentFolder {
            get
            {
                if (FolderStack.Count > 0)
                {
                    return FolderStack.Peek();
                }
                else
                {
                    return new FolderInfo(); 
                }
                
            }
        }
        public Boolean IsTop
        {
            get
            {
                return FolderStack.Count <= 1;
            }
        }
        public Int32 LevelsDeep
        {
            get
            {
                return FolderStack.Count;
            }
        }
        public Boolean ShouldDisplayWithFolderStructure
        {
            get
            {
                //Use folder structure at the top, then library structure as we get deeper
                if (CurrentFolder.FullName.ToLower().Contains("/artists") && LevelsDeep < 4)
                {
                    return true;
                }
                else
                {
                    return LevelsDeep < 3; 
                }
            }

        }
        public FolderPosition()
        {
            this.FolderStack = new Stack<FolderInfo>();
        }
        public void NextFolder(String FullName)
        {
            FolderInfo NewFolder = new FolderInfo() { FullName = FullName };
            String DisplayString = NewFolder.FullName.TrimEnd('/');
            if (DisplayString.LastIndexOf("/") >-1)
            {
                NewFolder.DisplayString = DisplayString.Substring(DisplayString.LastIndexOf("/"));
                NewFolder.DisplayString = NewFolder.DisplayString.TrimStart('/');
            }
            else
            {
                NewFolder.DisplayString = FullName;
            }
            FolderStack.Push(NewFolder);
        }
        public void GoBack()
        {
            FolderStack.Pop();
        }
        public void Clear()
        {
            FolderStack.Clear();
        }
        public String FormattedFolderString()
        {
            Int32 MaxDisplayLength = 25;
            String LongString = this.CurrentFolder.DisplayString;

            if (this.CurrentFolder.FullName.Contains("Billboard Hot 100 Singles") && FolderStack.Count > 2)
            {
                return this.CurrentFolder.DisplayString;
            }
            if (this.CurrentFolder.DisplayString.Length < MaxDisplayLength)
            {
                LongString = "";
                foreach (FolderInfo fi in FolderStack.Reverse())
                {
                    LongString += fi.DisplayString + " / ";
                }
                if (LongString.Length > MaxDisplayLength)
                {
                    LongString = "..." + LongString.Substring(LongString.Length - MaxDisplayLength);
                }
                if (LongString.EndsWith("/ "))
                {
                    LongString = LongString.Remove(LongString.Length - 3, 2);
                }
            }
            return LongString;
        }
    }

    public class MusicFolderEntry : INotifyPropertyChanged
    {
        private Boolean _isChecked;
        public string file { get; set; }
        public string filetype { get; set; }
        public String Line1Display { get; set; }
        public String Line2Display { get; set; }
        public String Track { get; set; }
        public Boolean HasInfo { get; set; }
        public Boolean IsChecked {
            get { return _isChecked; }
            set
            {
                if (value != _isChecked)
                {
                    _isChecked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }

    public class ItemImageURLConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            MusicFolderEntry E = (MusicFolderEntry)value;
            if (E.IsChecked)
            {
                return "/Assets/musiccheck.png";
            }
            if (!E.file.ToString().ToLower().EndsWith(".mp3"))
            {
                return "/Assets/music_folder.png";
            }
            else
            {
                return "/Assets/music.png";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class PermaCacheEntry
    {
        public String CompressedSerializedContents { get; set; }
        public DateTime LastUpdateDate { get; set; }

    }
    public class PermaCache
    {
        Dictionary<String, PermaCacheEntry> SerializedFolderListCache;  //Cache once we have all the info formatted  //TODO: Save this as binary or something slimmer than straight json but is fast enough to decompress on the Raspberry pi
        Dictionary<String, List<MusicFolderEntry>> UnserializedCache;   //Cache while we're working on the info

        public PermaCache()
        {
            SerializedFolderListCache = new Dictionary<string, PermaCacheEntry>();
            UnserializedCache = new Dictionary<string, List<MusicFolderEntry>>();
        }
         public Boolean Exists(String Folder)
        {
            //return SerializedFolderListCache.ContainsKey(Folder);
            return UnserializedCache.ContainsKey(Folder);
        }
        public void ToCache(String Folder, List<MusicFolderEntry> Entries)
        {
            /*
             PermaCacheEntry Entry = new PermaCacheEntry();
            Entry.LastUpdateDate = DateTime.Now;
            Entry.CompressedSerializedContents = HelperMethods.SerializeObject(Entries);
            SerializedFolderListCache[Folder] = Entry;
            */
            UnserializedCache[Folder] = Entries;
        }
        public List<MusicFolderEntry> FromCache(String Key)
        {
            List<MusicFolderEntry> Entries = new List<MusicFolderEntry>();
            /*
            if (SerializedFolderListCache.ContainsKey(Key))
            {
                Entries = HelperMethods.DeSerializeObject<List<MusicFolderEntry>>(SerializedFolderListCache[Key].CompressedSerializedContents);
                
            }
            */
            if (UnserializedCache.ContainsKey(Key))
            {
                Entries = UnserializedCache[Key];
            }
            return Entries;
        }

    }
}
