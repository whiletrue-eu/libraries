// ReSharper disable UnusedMemberInSuper.Global
using WhileTrue.Classes.Components;

namespace WhileTrue.Facades.WebServer
{
    /// <summary>
    /// Simple Web Server implementation. Web Server is started on creation
    /// </summary>
    [ComponentInterface]
    public interface ISimpleWebServer
    {
        /// <summary>
        /// Stops simple web server
        /// </summary>
        void Stop();
        /// <summary>
        /// Waits until the web server is stopped
        /// </summary>
        void Join();
    }
}