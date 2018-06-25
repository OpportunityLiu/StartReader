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
                var bf = new BookDataBrief();
                parseBookMeta(bf, doc);
                parseBookPage(bf, doc);
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
                    var status = item.SelectSingleNode("./dd[2]/span[2]");
                    var wordCount = item.SelectSingleNode("./dd[2]/span[4]");
                    var tags = item.SelectNodes("./dd[2]/a").EmptyIfNull();
                    var des = item.SelectSingleNode("./dd[3]");
                    var latestName = item.SelectSingleNode("./dd[4]/a");
                    var latestTime = item.SelectSingleNode("./dd[4]/span");
                    var image = item.SelectSingleNode("./dt[1]/a/img");
                    r.Books.Add(new BookDataBrief
                    {
                        Title = title.GetInnerText(),
                        Author = author.GetInnerText(),
                        Key = key,
                        CoverUri = image?.GetAttribute("_src", BaseUri, null),
                        Description = des.GetInnerText(),
                        WordCount = int.Parse(wordCount.GetInnerText()),
                        Tags = tags.Select(n => n.GetInnerText()).ToArray(),
                        IsFinished = status.GetInnerText() == "已完结",
                        LastestChapter = new ChapterDataBrief
                        {
                            UpdateTime = DateTime.Parse(latestTime.GetInnerText()),
                            Key = latestName.GetAttribute("href", ""),
                            Title = latestName.GetInnerText(),
                        }
                    });
                }
            }
            return r;
        }

        protected override async Task<GetBookResponse> GetBookAsync(GetBookRequest request)
        {
            var r = new GetBookResponse
            {
                BookData = new BookDataDetailed()
            };
            var bookPage = await GetDoc(new Uri($"/book/{request.BookKey}", UriKind.Relative));
            parseBookMeta(r.BookData, bookPage);
            parseBookPage(r.BookData, bookPage);
            var chapsPage1 = await GetDoc(new Uri($"/read/{request.BookKey}", UriKind.Relative));
            var pc = parseReadPage(r.BookData, chapsPage1);
            for (var i = 1; i < pc; i++)
            {
                var chapsPage = await GetDoc(new Uri($"/read/{request.BookKey}/{i + 1}", UriKind.Relative));
                parseReadPage(r.BookData, chapsPage);
            }
            return r;
        }

        protected override async Task<GetChaptersResponse> GetChaptersAsync(GetChaptersRequest request)
        {
            var r = new GetChaptersResponse();
            var keyList = request.ChapterKeys.ToArray();
            for (var i = 0; i < keyList.Length; i++)
            {
                var ck = keyList[i];
                if (ck.StartsWith("n"))
                {
                    var pck = ck.Substring(1);
                    var pdoc = await GetDoc(new Uri($"read/{request.BookKey}/{pck}.html", UriKind.Relative));
                    ck = parseChapterPage(new ChapterDataDetailed(), pdoc);
                }
                var doc = await GetDoc(new Uri($"read/{request.BookKey}/{ck}.html", UriKind.Relative));
                var cc = new ChapterDataDetailed();
                var nk = parseChapterPage(cc, doc);
                if (i + 1 < keyList.Length)
                    keyList[i + 1] = nk;
                r.Chapters.Add(cc);
            }
            return r;
        }

        private static readonly Regex chpInfoRegex = new Regex(@"(^|,|;|\s)(?<key>\w+)\s*=\s*""(?<value>.+?)""", RegexOptions.ExplicitCapture);
        private string parseChapterPage(ChapterDataDetailed chapter, HtmlDocument document)
        {
            // var preview = "7966638.html",next = "7966640.html",bid = "11341",book="剑娘",zid = "7966639",chapter = "第893章 舰娘再现";document.onkeydown= jumpPage;
            var prop = chpInfoRegex.Matches(document.DocumentNode.SelectSingleNode("/html/head/script[last()]").GetInnerText())
                .OfType<Match>().ToDictionary(m => m.Groups["key"].Value, m => m.Groups["value"].Value);
            chapter.Key = prop["zid"];
            chapter.Title = prop["chapter"];
            var nk = prop["next"]; // 123567.html
            nk = nk.Substring(0, nk.IndexOf('.'));

            var center = document.GetElementbyId("center");
            chapter.UpdateTime = DateTime.Parse(center.SelectSingleNode("./div[@class='title']/span[last()]").GetInnerText());
            chapter.WordCount = int.Parse(center.SelectSingleNode("./div[@class='title']/span[last()-1]").GetInnerText());

            var content = document.GetElementbyId("content");
            chapter.Content = string.Join("\n", content.Elements("p").Select(p => p.GetInnerText()));

            return nk;
        }

        private void parseBookMeta(BookDataBrief book, HtmlDocument document)
        {
            var head = document.DocumentNode.SelectSingleNode("/html/head");
            var uri = head.SelectSingleNode("./meta[@property='og:novel:read_url']").GetAttribute("content", "");
            book.Key = idReg.Match(uri).Groups[1].Value;
            book.Title = head.SelectSingleNode("./meta[@property='og:novel:book_name']").GetAttribute("content", "");
            book.Author = head.SelectSingleNode("./meta[@property='og:novel:author']").GetAttribute("content", "");
            book.CoverUri = head.SelectSingleNode("./meta[@property='og:image']").GetAttribute("content", BaseUri, null);
            book.IsFinished = "已完结" == head.SelectSingleNode("./meta[@property='og:novel:status']").GetAttribute("content", "");
            book.LastestChapter = new ChapterDataBrief
            {
                UpdateTime = DateTime.Parse(head.SelectSingleNode("./meta[@property='og:novel:update_time']").GetAttribute("content", "")),
                Key = head.SelectSingleNode("./meta[@property='og:novel:latest_chapter_url']").GetAttribute("content", ""),
                Title = head.SelectSingleNode("./meta[@property='og:novel:latest_chapter_name']").GetAttribute("content", ""),
            };
        }

        private void parseBookPage(BookDataBrief book, HtmlDocument document)
        {
            book.Description = string.Join("\n", document.GetElementbyId("intro_win").SelectNodes("./div[1]/p").Select(p => p.GetInnerText()));
            var booinfo = document.GetElementbyId("bookinfo");
            book.WordCount = int.Parse(booinfo.SelectSingleNode("./div[2]/div[2]/ul[1]/li[11]/span[1]").GetInnerText());
            book.Tags = booinfo.SelectNodes("./div[2]/div[4]/span[1]/a").EmptyIfNull().Select(n => n.GetInnerText()).ToArray();
        }

        private int parseReadPage(BookDataDetailed book, HtmlDocument document)
        {
            var readerlists = document.GetElementbyId("readerlists");
            var page = readerlists.SelectNodes("./div[@class='pages']/*")?.Count ?? 1;
            var chps = readerlists.SelectNodes("./ul/li").EmptyIfNull();
            foreach (var item in chps)
            {
                var href = item.Element("a")?.GetAttribute("href", null);
                if (href.IsNullOrWhiteSpace())
                {
                    var content = item.GetInnerText();
                    var ep = content.LastIndexOf(' ');
                    if (ep > 0)
                        content = content.Substring(0, ep);
                    book.Chapters.Add(new ChapterDataBrief
                    {
                        Key = "n" + book.Chapters.Last().Key,
                        Title = content,
                    });
                }
                else
                {
                    var content = item.Element("a").GetInnerText();
                    book.Chapters.Add(new ChapterDataBrief
                    {
                        Key = idReg.Match(href).Groups[1].Value,
                        Title = content,
                    });
                }
            }
            return page;
        }
    }
}