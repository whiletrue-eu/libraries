using System.Reflection;
using WhileTrue.Classes.Components;
using WhileTrue.Facades.ApplicationLoader;

namespace AtrEditor.About
{
    [Component]
    public class AboutWindow : IAboutWindow, IModule
    {
        private readonly ComponentRepository repository;

        public AboutWindow(ComponentRepository repository)
        {
            this.repository = repository;
        }

        public void ShowModal()
        {
            using(ComponentContainer ComponentContainer = new ComponentContainer(this.repository))
            {
                IAboutWindowView View = ComponentContainer.ResolveInstance<IAboutWindowView>();
                View.Model = this;
                View.ShowModal();
            }
        }

        public void AddSubcomponents(ComponentRepository componentRepository)
        {
            componentRepository.AddComponent<AboutWindowView>();
        }

        public void Initialize(ComponentContainer componentContainer)
        {
        }

        public string ApplicationVersion
        {
            get
            {
                Assembly EntryAssembly = Assembly.GetEntryAssembly();
                return EntryAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            }
        }
    }
}