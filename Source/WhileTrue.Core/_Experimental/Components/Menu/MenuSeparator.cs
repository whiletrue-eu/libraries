using System.Xml;
using Mz.Facades.MenuFactory;

namespace Mz.Components.Menu
{
    internal class MenuSeparator : MenuItemBase, IMenuSeparator
    {
        private bool visible;

        public MenuSeparator(XmlElement data)
            : base(data)
        {
        }

        public override bool Visible
        {
            get { return this.visible; }
        }

        protected override void EnsureNextSeparatorCreated()
        {
            this.EnsureUIItemCreated();
        }

        protected override void EnsurePreviousSeparatorCreated()
        {
            this.EnsureUIItemCreated();
        }

        protected override void UpdateNextSeparatorVisibility()
        {
            this.UpdateVisibility();
        }

        protected override void UpdatePreviousSeparatorVisibility()
        {
            this.UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            MenuItemBase NextItem = this.GetNextItemVisible();
            MenuItemBase PreviousItem = this.GetPreviousItemVisible();

            if (NextItem == null || NextItem is MenuSeparator || PreviousItem == null || PreviousItem is MenuSeparator)
            {
                this.visible = false;
            }
            else
            {
                this.visible = true;
            }
            this.UIFactory.UpdateMenuSeparatorVisibility(this.UIItem, this.visible);
        }

        private void EnsureUIItemCreated()
        {
            if (this.UIItem == null)
            {
                this.UIItem = this.UIFactory.CreateMenuSeparator(this);
                this.Parent.AddItem(this.UIItem, this.GetNextUIItemCreated());
                this.UpdateVisibility();
            }
        }
    }
}