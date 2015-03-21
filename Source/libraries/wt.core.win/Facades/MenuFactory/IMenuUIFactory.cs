using Mz.Classes.Components;

namespace Mz.Facades.MenuFactory
{
    //TODO: Add Support for Toolbar Dropdown Buttons
    [ComponentInterface]
    public interface IMenuUIFactory
    {
        #region Menu/ContextMenu Specific

        object CreateMenu(IMenu menu);
        object CreateContextMenu(IContextMenu contextMenu);

        object CreateMenuItem(IMenuItem item);
        object CreateDynamicMenuItem(IDynamicMenuItem dynamicMenuItem);
        object CreateSubmenuItem(ISubmenuItem item);
        object CreateMenuSeparator(IMenuSeparator item);

        void AddItemToSubitem(object subitem, object item, object nextItem);
        void AddItemToMenu(object menu, object item, object nextItem);

        void ShowContextMenu(object contextMenu, int x, int y);

        #endregion

        #region Toolbar Specific

        object CreateToolbar(IToolbar toolbar);

        object CreateToolbarItem(IToolbarItem item);
        object CreateToolbarSeparator(IToolbarSeparator item);

        void AddItemToToolbar(object toolbar, object item, object nextItem);

        #endregion

        #region general

        void UpdateItem(object item, bool enabled);
        void UpdateDynamicItem(object item, bool visible, bool enabled, string caption, string icon);
        event MenuItemEventHandler MenuItemClicked;

        #endregion

        void UpdateToolbarSeparatorVisibility(object separator, bool visible);
        void UpdateMenuSeparatorVisibility(object separator, bool visible);
    }
}