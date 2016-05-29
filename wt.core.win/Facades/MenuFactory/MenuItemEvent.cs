namespace Mz.Facades.MenuFactory
{
    public delegate void MenuItemEventHandler(object sender, MenuItemEventArgs e);

    public class MenuItemEventArgs
    {
        private readonly object item;

        public MenuItemEventArgs(object item)
        {
            this.item = item;
        }

        public object Item
        {
            get { return this.item; }
        }
    }
}