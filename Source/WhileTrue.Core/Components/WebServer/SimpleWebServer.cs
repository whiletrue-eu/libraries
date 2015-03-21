using System;
using System.Linq;
using System.Net;
using System.Threading;
using WhileTrue.Classes.Components;
using WhileTrue.Facades.WebServer;

namespace WhileTrue.Components.WebServer
{
    [Component]
    public class SimpleWebServer : ISimpleWebServer, IDisposable
    {
        private readonly ISimpleWebServerContentRetriever[] contentRetriever;
        private readonly SimpleWebServerOptions options;
        private HttpListener listener;
        private Thread webServerThread;

        /// <summary/>
        public SimpleWebServer(ISimpleWebServerContentRetriever[] contentRetriever, SimpleWebServerOptions options)
        {
            this.contentRetriever = contentRetriever;
            this.options = options;
            this.Start();
        }

        private void Start()
        {
            bool Started = false;
            string BaseUri=null;
            string Host = this.options.Host ?? "*";

            while (!Started)
            {
                this.listener = new HttpListener();
                BaseUri = string.Format("http://{0}:{1}/", Host, this.options.Port);
                this.listener.Prefixes.Add(BaseUri);
                try
                {
                    this.listener.Start();
                    Started = true;
                }
                catch(Exception Exception)
                {
                    if (this.options.AutoScanForFreePort)
                    {
                        this.options.Port++;
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("Cannot start web server: {0}", Exception.Message), Exception);
                    }
                }
            }

            this.webServerThread = new Thread(this.ServeRequests);
            this.webServerThread.Name = string.Format("SimpleWebServer@{0}", BaseUri);
            this.webServerThread.IsBackground = true;
            this.webServerThread.Start();
        }

        private void ServeRequests()
        {
            while (this.listener.IsListening)
            {
                try
                {
                    HttpListenerContext Context = this.listener.GetContext();
                    this.HandleRequest(Context);
                }
                catch (HttpListenerException)
                { }
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            HttpListenerRequest Request = context.Request;
            HttpListenerResponse Response = context.Response;

            try
            {


                string RequestedResource = Request.Url.AbsolutePath;

                ISimpleWebServerContentRetriever ContentRetriever = (from Retriever in this.contentRetriever
                                                                     where Retriever.CanHandleResource(RequestedResource)
                                                                     select Retriever
                                                                    ).FirstOrDefault();

                if (ContentRetriever != null)
                {
                    switch (Request.HttpMethod)
                    {
                        case "GET":
                            {
                                byte[] ResponseData = ContentRetriever.GetContent(RequestedResource);

                                if (ResponseData != null)
                                {
                                    Response.ContentLength64 = ResponseData.Length;
                                    Response.OutputStream.Write(ResponseData, 0, ResponseData.Length);
                                }
                                else
                                {
                                    Response.StatusCode = (int) HttpStatusCode.NotFound;
                                }
                                break;
                            }
                        case "POST":
                            {
                                byte[] RequestData = new byte[Request.ContentLength64];
                                Request.InputStream.Read(RequestData, 0, RequestData.Length);

                                byte[] ResponseData = ContentRetriever.PostContent(RequestedResource, RequestData);

                                if (ResponseData != null)
                                {
                                    Response.ContentLength64 = ResponseData.Length;
                                    Response.OutputStream.Write(ResponseData, 0, ResponseData.Length);
                                }
                                else
                                {
                                    Response.StatusCode = (int) HttpStatusCode.NotFound;
                                }
                                break;
                            }
                        default:
                            Response.StatusCode = (int) HttpStatusCode.MethodNotAllowed;
                            break;
                    }
                }
                else
                {
                    Response.StatusCode = (int) HttpStatusCode.NotFound;
                }
            }
            catch (Exception)
            {
                Response.StatusCode = (int) HttpStatusCode.BadRequest;
            }

            Response.OutputStream.Close();
        }

        public void Dispose()
        {
            this.Stop();
        }

        public void Stop()
        {
            if (this.listener.IsListening)
            {
                this.listener.Stop();
            }
        }

        public void Join()
        {
            this.webServerThread.Join();
        }
    }
}

