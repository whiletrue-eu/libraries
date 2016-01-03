namespace WhileTrue.Components.WebServer
{
    ///<summary>
    /// OPtions for the <see cref="SimpleWebServer"/>
    ///</summary>
    public class SimpleWebServerOptions
    {
        /// <summary/>
        public string Host { get; set; }
        /// <summary/>
        public int Port { get; set; }
        /// <summary>
        /// If set to <c>true</c> and the port given is already used, the port number will be increased until a free port is found
        /// </summary>
        public bool AutoScanForFreePort { get; set; }
    }
}