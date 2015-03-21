using System;
using WhileTrue.Classes.Commanding;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Classes.Models
{
    public class ZoomModel : ObservableObject
    {
        private readonly DelegateCommand resetZoomCommand;
        private readonly DelegateCommand increaseZoomCommand;
        private readonly DelegateCommand decreaseZoomCommand;

        private double zoomFactor;
        private double minimumZoomFactor;
        private double maximumZoomFactor;
        private double zoom = 1;
        private double zoomStep = .5;


        public ZoomModel()
        {
            this.resetZoomCommand = new DelegateCommand(this.ResetZoom);
            this.increaseZoomCommand = new DelegateCommand(this.IncreaseZoom, ()=>this.ZoomFactor < this.MaximumZoomFactor);
            this.decreaseZoomCommand = new DelegateCommand(this.DecreaseZoom, ()=> this.ZoomFactor > this.MinimumZoomFactor);
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
                this.SetAndInvoke(() => MinimumZoomFactor, ref this.minimumZoomFactor, value);
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
                this.SetAndInvoke(() => MaximumZoomFactor, ref this.maximumZoomFactor, value);
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
                this.SetAndInvoke(() => ZoomFactor, ref this.zoomFactor, value);
                this.SetAndInvoke(() => Zoom, ref this.zoom, Math.Pow(2, this.zoomFactor));
            }
        }

        public double Zoom
        {
            get
            {
                return this.zoom;
            }
        }

        public DelegateCommand ResetZoomCommand
        {
            get { return this.resetZoomCommand; }
        }

        public DelegateCommand IncreaseZoomCommand
        {
            get { return this.increaseZoomCommand; }
        }

        public DelegateCommand DecreaseZoomCommand
        {
            get { return this.decreaseZoomCommand; }
        }
    }
}