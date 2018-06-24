using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using StartReader.DataExchange;
using StartReader.DataExchange.Model;
using StartReader.DataExchange.Request;
using StartReader.DataExchange.Response;
using Windows.ApplicationModel.AppService;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace StartReader.ExtensionProvider
{
    internal abstract class DataExchangeProviderEx : DataExchangeProvider, IDisposable
    {
        static DataExchangeProviderEx()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            GBEncoding = Encoding.GetEncoding(54936);
        }

        protected static Encoding GBEncoding { get; }

        private HttpClient client = new HttpClient();

        protected Uri BaseUri { get; }

        public DataExchangeProviderEx(AppServiceConnection connection, Uri baseUri) : base(connection)
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
                if (enc == Encoding.UTF8)
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

    internal class MiaoBiGeProvider : DataExchangeProviderEx
    {
        public MiaoBiGeProvider(AppServiceConnection connection)
            : base(connection, new Uri("https://www.miaobige.com/"))
        {
        }

        private static readonly Regex idReg = new Regex(@"^.+/(\d+)/?.+?$");

        private static readonly Uri searchUri = new Uri("https://www.miaobige.com/search/");
        protected override async Task<SearchResponse> SearchAsync(SearchRequest request)
        {
            var content = new HttpBufferContent(GBEncoding.GetBytes($"s={request.Keyword}").AsBuffer())
            {
                Headers =
                {
                     ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/x-www-form-urlencoded")
                }
            };
            var doc = await PostDoc(searchUri, content);
            var r = new SearchResponse();
            var box = doc.GetElementbyId("sitembox");
            if (box is null)
            {
                var bf = new BookDataDetailed();
                await parseBookPageAsync(bf, doc);
                r.Books.Add(bf);
            }
            else
            {
                foreach (var item in box.Elements("dl"))
                {
                    var title = item.SelectSingleNode("./dd[1]/h3/a");
                    var key = title.GetAttributeValue("href", "");
                    key = idReg.Match(key).Groups[1].Value;
                    var author = item.SelectSingleNode("./dd[2]/span[1]");
                    var des = item.SelectSingleNode("./dd[3]");
                    var latestName = item.SelectSingleNode("./dd[4]/a");
                    var latestTime = item.SelectSingleNode("./dd[4]/span");
                    r.Books.Add(new BookDataBrief
                    {
                        Title = title.InnerText,
                        Author = author.InnerText,
                        Key = key,
                        Description = des.InnerText,
                        LastUpdate = DateTime.Parse(latestTime.InnerText),
                        LastestChapter = new ChapterDataBrief
                        {
                            BookKey = key,
                            Key = latestName.GetAttributeValue("href", ""),
                            Title = latestName.InnerText,
                        }
                    });
                }
            }
            return r;
        }

        private async Task parseBookPageAsync(BookDataDetailed book, HtmlDocument document)
        {
            var head = document.DocumentNode.SelectSingleNode("/html/head");
            var uri = head.SelectSingleNode("./meta[@property='og:novel:read_url']").GetAttributeValue("content", "");
            book.Key = idReg.Match(uri).Groups[1].Value;
            book.Title = head.SelectSingleNode("./meta[@property='og:novel:book_name']").GetAttributeValue("content", "");
            book.Author = head.SelectSingleNode("./meta[@property='og:novel:author']").GetAttributeValue("content", "");
            book.Description = string.Join("\n", document.GetElementbyId("intro_win").SelectNodes("./div[1]/p").Select(p => p.InnerText));
            book.LastUpdate = DateTime.Parse(head.SelectSingleNode("./meta[@property='og:novel:update_time']").GetAttributeValue("content", ""));
            book.Cover = (await GetBuffer(new Uri(head.SelectSingleNode("./meta[@property='og:image']").GetAttributeValue("content", "")))).ToArray();
            book.LastestChapter = new ChapterDataBrief
            {
                BookKey = book.Key,
                Key = head.SelectSingleNode("./meta[@property='og:novel:latest_chapter_url']").GetAttributeValue("content", ""),
                Title = head.SelectSingleNode("./meta[@property='og:novel:latest_chapter_name']").GetAttributeValue("content", ""),
            };
        }

        protected override async Task<GetBookResponse> GetBookAsync(GetBookRequest request)
        {
            var r = new GetBookResponse
            {
                BookData = new BookDataDetailed()
            };
            var bookPage = await GetDoc(new Uri($"/book/{request.BookKey}", UriKind.Relative));
            await parseBookPageAsync(r.BookData, bookPage);
            var chapsPage = await GetDoc(new Uri($"/read/{request.BookKey}", UriKind.Relative));
            return r;
        }

        protected override Task<GetChaptersResponse> GetChaptersAsync(GetChaptersRequest request) => throw new NotImplementedException();
    }
}