using System;
using WhileTrue.Classes.Commands;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Modules.ModelInspectorWindow.Model
{
    public class ZoomModel : ObservableObject
    {
        private double zoomFactor;
        private double minimumZoomFactor;
        private double maximumZoomFactor;
        private double zoom = 1;
        private double zoomStep = .5;


        public ZoomModel()
        {
            this.ResetZoomCommand = new DelegateCommand(this.ResetZoom);
            this.IncreaseZoomCommand = new DelegateCommand(this.IncreaseZoom, ()=>this.ZoomFactor < this.MaximumZoomFactor);
            this.DecreaseZoomCommand = new DelegateCommand(this.DecreaseZoom, ()=> this.ZoomFactor > this.MinimumZoomFactor);
        }

        private void DecreaseZoom()
        {
            this.ZoomFactor -= this.zoomStep;
        }

        private void IncreaseZoom()
        {
            this.ZoomFactor += this.zoomStep;
        }


        private void ResetZoom()
        {
            this.ZoomFactor = 0;
        }


        public double MinimumZoomFactor
        {
            get
            {
                return this.minimumZoomFactor;
            }
            set
            {
                this.SetAndInvoke(nameof(this.MinimumZoomFactor), ref this.minimumZoomFactor, value);
                this.ZoomFactor = Math.Max(this.ZoomFactor, this.MinimumZoomFactor);
            }
        }

        public double MaximumZoomFactor
        {
            get
            {
                return this.maximumZoomFactor;
            }
            set
            {
                this.SetAndInvoke(nameof(this.MaximumZoomFactor), ref this.maximumZoomFactor, value);
                this.ZoomFactor = Math.Min(this.ZoomFactor, this.MaximumZoomFactor);
            }
        }


        public double ZoomFactor
        {
            get
            {
                return this.zoomFactor;
            }
            set
            {
                this.SetAndInvoke(nameof(ZoomModel.ZoomFactor), ref this.zoomFactor, value);
                this.SetAndInvoke(nameof(ZoomModel.Zoom), ref this.zoom, Math.Pow(2, this.zoomFactor));
            }
        }

        public double Zoom => this.zoom;

        public DelegateCommand ResetZoomCommand { get; }

        public DelegateCommand IncreaseZoomCommand { get; }

        public DelegateCommand DecreaseZoomCommand { get; }
    }
}