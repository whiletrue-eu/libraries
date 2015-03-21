using WhileTrue.Classes.Components;

namespace WhileTrue.Facades.WebServer
{
    /// <summary>
    /// interface to implement the content handling for <see cref="ISimpleWebServer"/>
    /// </summary>
    [ComponentInterface]
    public interface ISimpleWebServerContentRetriever
    {
        /// <summary>
        /// Returns true if resource address can be handled by this retriever
        /// </summary>
        bool CanHandleResource(string requestedResource);

        /// <summary>
        /// Retrieves content with the given path. If content path is not known, <c>null</c> must be returned
        /// </summary>
        byte[] GetContent(string requestedResource);

        /// <summary>
        /// Retrieves content with the given path. If content path is not known, <c>null</c> must be returned
        /// </summary>
        byte[] PostContent(string requestedResource, byte[] data);
    }
}