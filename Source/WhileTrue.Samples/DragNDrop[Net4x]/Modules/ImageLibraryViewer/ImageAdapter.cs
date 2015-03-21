using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;
using WhileTrue.DragNDrop.Facades.ImageLibraryModel;

namespace WhileTrue.DragNDrop.Modules.ImageLibraryViewer
{
    internal class ImageAdapter : ItemAdapterBase
    {
        private static BitmapImage defaultThumbnail;
        private static TaskFactory loadTaskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(1));

        private readonly ReadOnlyPropertyAdapter<string> pathAdapter;
        private readonly ReadOnlyPropertyAdapter<string> nameAdapter;
        private BitmapImage thumbnail;

        internal ImageAdapter(IImage image)
        {
            this.pathAdapter = this.CreatePropertyAdapter(
                () => Path,
                () => image.Path
                );
            this.nameAdapter = this.CreatePropertyAdapter(
                () => Name,
                () => image.Name
                );

            this.thumbnail = GetDefaultThumbnail();
            loadTaskFactory.StartNew(this.LoadImage);
        }

        private void LoadImage()
        {
            BitmapImage BitmapImage = new BitmapImage();
            BitmapImage.BeginInit();
            BitmapImage.DecodePixelHeight = 48;
            BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            BitmapImage.UriSource = new Uri(this.Path);
            BitmapImage.EndInit();
            BitmapImage.Freeze();

            this.Thumbnail = BitmapImage;
        }

        public string Path   
        {
            get { return this.pathAdapter.GetValue(); }
        }

        public string Name   
        {
            get { return this.nameAdapter.GetValue(); }
        }

        public BitmapImage Thumbnail
        {
            get
            {
                return this.thumbnail;
            }
            private set
            {
                this.SetAndInvoke(()=>Thumbnail, ref this.thumbnail, value);
            }
        }

        private static BitmapImage GetDefaultThumbnail()
        {
            if( defaultThumbnail != null )
            {
                return defaultThumbnail;
            }
            else
            {
                using (Stream DefaultThumbnail = App.GetResourceStream(new Uri(@"pack://application:,,,/WhileTrue.Samples;component/DragNDrop/Modules/ImageLibraryViewer/generic_picture.png")).Stream)
                {
                    defaultThumbnail = new BitmapImage();
                    defaultThumbnail.BeginInit();
                    defaultThumbnail.DecodePixelHeight = 48;
                    defaultThumbnail.StreamSource = DefaultThumbnail;
                    defaultThumbnail.EndInit();
                }
                return defaultThumbnail;
            }
        }
    }
}