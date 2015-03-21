using System;
using WhileTrue.Classes.Components;

namespace WhileTrue.DragNDrop.Modules.ImageLibraryViewer
{
    /// <summary/>
    [Component]
    partial class ImageLibraryViewerView : IImageLibraryViewerView
    {
        public ImageLibraryViewerView()
        {
            InitializeComponent();
        }

        public IImageLibraryViewerModel Model
        {
            set { this.DataContext = value; }
        }

        public void Open()
        {
            this.ShowDialog();
        }
    }
}
