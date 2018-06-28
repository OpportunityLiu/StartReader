using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using StartReader.DataExchange;
using Windows.ApplicationModel.AppService;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace StartReader.ExtensionProvider
{
    internal abstract class DataExchangeProviderEx : DataProvider, IDisposable
    {
        static DataExchangeProviderEx()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            GBEncoding = Encoding.GetEncoding(54936);
        }

        protected static Encoding GBEncoding { get; }

        private HttpClient client = new HttpClient();

        protected Uri BaseUri { get; }

        protected DataExchangeProviderEx(Uri baseUri)
        {
            this.BaseUri = baseUri;
        }

        public void Dispose()
        {
            this.client?.Dispose();
            this.client = null;
        }

        private static HtmlDocument loadDoc(IBuffer data)
        {
            using (var stream = data.AsStream())
            {
                var doc = new HtmlDocument();
                doc.Load(stream);
                var enc = doc.DeclaredEncoding;
                if (enc is null || enc == Encoding.UTF8)
                    return doc;
                if (enc.CodePage == 936 || enc.CodePage == 20936 || enc.CodePage == 54936)
                    enc = GBEncoding;
                stream.Position = 0;
                doc.Load(stream, enc);
                return doc;
            }
        }

        private void reformUri(ref Uri uri)
        {
            if (!uri.IsAbsoluteUri)
                uri = new Uri(BaseUri, uri);
        }

        protected async Task<HttpResponseMessage> Get(Uri uri)
        {
            reformUri(ref uri);
            return await this.client.GetAsync(uri);
        }

        protected async Task<IBuffer> GetBuffer(Uri uri)
        {
            reformUri(ref uri);
            return await this.client.GetBufferAsync(uri);
        }

        protected async Task<HtmlDocument> GetDoc(Uri uri)
        {
            var buf = await GetBuffer(uri);
            return loadDoc(buf);
        }

        protected Task<HtmlDocument> PostDoc(Uri uri, IEnumerable<KeyValuePair<string, string>> content)
            => PostDoc(uri, new HttpFormUrlEncodedContent(content));

        protected Task<HtmlDocument> PostDoc(Uri uri, params KeyValuePair<string, string>[] content)
            => PostDoc(uri, (IEnumerable<KeyValuePair<string, string>>)content);

        protected Task<HtmlDocument> PostDoc(Uri uri, string content)
            => PostDoc(uri, new HttpStringContent(content));

        protected async Task<HtmlDocument> PostDoc(Uri uri, IHttpContent content)
        {
            var res = await Post(uri, content);
            res.EnsureSuccessStatusCode();
            var buf = await res.Content.ReadAsBufferAsync();
            return loadDoc(buf);
        }

        protected async Task<HttpResponseMessage> Post(Uri uri, IHttpContent content)
        {
            reformUri(ref uri);
            return await this.client.PostAsync(uri, content);
        }
    }
}