using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using StartReader.DataExchange.Model;
using StartReader.DataExchange.Request;
using StartReader.DataExchange.Response;
using Windows.ApplicationModel.AppService;
using Windows.Web.Http;
using Opportunity.ChakraBridge.WinRT;
using System.Text;

namespace StartReader.ExtensionProvider
{

    internal class TwoKXiaoShuo : DataExchangeProviderEx
    {
        public TwoKXiaoShuo()
            : base(new Uri("https://www.2kxs.com/"))
        {
        }

        private static readonly Regex chpIdReg = new Regex(@"(^.+/|^)(\d+)/?.+?$");
        private static readonly Regex bookIdReg = new Regex(@"(^.+/|^)(\d+/\d+)/?.+?$");

        protected override async Task<SearchResponse> SearchAsync(SearchRequest request)
        {
            var sb = new StringBuilder("https://www.2kxs.com/modules/article/search.php?searchtype=keywords&searchkey=");
            foreach (var item in GBEncoding.GetBytes(request.Keyword))
            {
                sb.Append('%');
                sb.Append(item.ToString("X2"));
            }
            var uri = new Uri(sb.ToString());
            var doc = await GetDoc(uri);
            var r = new SearchResponse();
            var content = doc.GetElementbyId("content");
            if (content is null)
            {
                var bf = new BookDataDetailed();
                parseBookPage(bf, doc);
                var readPage = await GetDoc(new Uri($"/xiaoshuo/{bf.Key}/", UriKind.Relative));
                parseBookMeta(bf, readPage);
                parseReadPage(bf, readPage);
                r.Books.Add(bf);
            }
            else
            {
                foreach (var item in content.SelectNodes("table/tr[position()>1]"))
                {
                    var title = item.SelectSingleNode("./td[1]/a");
                    var latestChp = item.SelectSingleNode("./td[2]/a");
                    var author = item.SelectSingleNode("./td[3]");
                    var wordCount = item.SelectSingleNode("./td[4]").GetInnerText();
                    var wc = int.Parse(wordCount.Substring(0, wordCount.Length - 1));
                    if (char.IsDigit(wordCount.Last()))
                        wc *= 10;
                    else if (char.ToLowerInvariant(wordCount.Last()) == 'k')
                        wc *= 1000;
                    else if (char.ToLowerInvariant(wordCount.Last()) == 'm')
                        wc *= 1000_000;
                    var lastUpdate = item.SelectSingleNode("./td[5]").GetInnerText();
                    var status = item.SelectSingleNode("./td[6]").GetInnerText();

                    var key = latestChp.GetAttributeValue("href", "");
                    key = bookIdReg.Match(key).Groups[2].Value;

                    r.Books.Add(new BookDataBrief
                    {
                        Title = title.GetInnerText(),
                        Author = author.GetInnerText(),
                        Key = key,
                        WordCount = wc,
                        IsFinished = status == "完成",
                        LatestChapter = new ChapterDataBrief
                        {
                            Key = "Latest",
                            UpdateTime = DateTime.Parse(lastUpdate),
                            Title = latestChp.GetInnerText(),
                        }
                    });
                }
            }
            return r;
        }

        protected override async Task<GetBookResponse> GetBookAsync(GetBookRequest request)
        {
            var d = new BookDataDetailed();
            var readPage = await GetDoc(new Uri($"/xiaoshuo/{request.BookKey}/", UriKind.Relative));
            parseBookMeta(d, readPage);
            parseReadPage(d, readPage);
            var bookPage = await GetDoc(new Uri($"/{request.BookKey.Split('/')[1]}/", UriKind.Relative));
            parseBookPage(d, bookPage);
            return new GetBookResponse { BookData = d };
        }

        protected override async Task<GetChaptersResponse> GetChaptersAsync(GetChaptersRequest request)
        {
            var r = new GetChaptersResponse();
            var keyList = request.ChapterKeys.ToArray();
            for (var i = 0; i < keyList.Length; i++)
            {
                var ck = keyList[i].Split(new[] { ' ' }, 2);
                var doc = await GetDoc(new Uri($"/xiaoshuo/{request.BookKey}/{ck[0]}.html", UriKind.Relative));
                var cc = new ChapterDataDetailed { Key = keyList[i], VolumeTitle = ck[1] };
                parseChapterPage(cc, doc);
                r.Chapters.Add(cc);
            }
            return r;
        }

        private void parseChapterPage(ChapterDataDetailed chapter, HtmlDocument document)
        {
            //var articlename='放开那个女巫';
            //var chaptername=' 第一千一百七十四章 一劳永逸的货币方案';
            //var bookid='97525';
            //var preview_page = "/xiaoshuo/97/97525/25492728.html";
            //var next_page = "/xiaoshuo/97/97525/";
            //var index_page = "/xiaoshuo/97/97525/";
            //var chapter_id = "25497283";
            var script = document.DocumentNode.SelectNodes("/html/head/script").Last(n => !n.GetInnerText().IsNullOrWhiteSpace());
            var prop = Helpers.ParseJsKvp(script.GetInnerText());
            chapter.Title = prop["chaptername"];

            if (chapter.Title.StartsWith(chapter.VolumeTitle))
                chapter.Title = chapter.Title.Substring(chapter.VolumeTitle.Length);
            chapter.Title = chapter.Title.Trim();

            var c = document.GetElementbyId("box").Element("p", "Text");
            var s = default(StringBuilder);
            chapter.WordCount = 0;
            foreach (var item in c.Elements("#text"))
            {
                var t = item.GetInnerText();
                if (t.EndsWith("2k小说阅读网"))
                    t = t.Substring(0, t.Length - "2k小说阅读网".Length);
                t = t.Trim();
                if (t.IsNullOrEmpty() && s is null)
                    continue;
                s = s ?? new StringBuilder();
                s.AppendLine(t);
                chapter.WordCount += t.Length;
            }
            chapter.Content = s.ToString();
        }

        private void parseBookMeta(BookDataBrief book, HtmlDocument document)
        {
            var head = document.DocumentNode.SelectSingleNode("/html/head");
            var uri = head.SelectSingleNode("./meta[@property='og:novel:read_url']").GetAttribute("content", "");
            book.Key = bookIdReg.Match(uri).Groups[2].Value;
            book.Title = head.SelectSingleNode("./meta[@property='og:novel:book_name']").GetAttribute("content", "");
            book.Author = head.SelectSingleNode("./meta[@property='og:novel:author']").GetAttribute("content", "");
            book.CoverUri = head.SelectSingleNode("./meta[@property='og:image']").GetAttribute("content", BaseUri, null);
            book.IsFinished = head.SelectSingleNode("./meta[@property='og:novel:status']").GetAttribute("content", "").Contains("完结");
            book.LatestChapter = new ChapterDataBrief
            {
                UpdateTime = DateTime.Parse(head.SelectSingleNode("./meta[@property='og:novel:update_time']").GetAttribute("content", "")),
                Key = chpIdReg.Match(head.SelectSingleNode("./meta[@property='og:novel:latest_chapter_url']").GetAttribute("content", "")).Groups[2].Value,
                Title = head.SelectSingleNode("./meta[@property='og:novel:latest_chapter_name']").GetAttribute("content", ""),
            };
        }

        private void parseBookPage(BookDataBrief book, HtmlDocument document)
        {
            var info = document.DocumentNode.SelectSingleNode("./html/body/div[@class='wrap']/div[@class='mainarea']/div[@class='bortable']/div[@class='work']/div[@class='wright']");
            book.Description = info.SelectSingleNode("./p[@class='Text']").GetInnerText();
            var booinfo = document.GetElementbyId("bookinfo");
            book.WordCount = int.Parse(info.SelectSingleNode("./div[@id='box4']/p/small[1]").GetInnerText());
            var bt1 = document.GetElementbyId("bt_1");
            book.Key = bookIdReg.Match(bt1.Element("a").GetAttributeValue("href", "")).Groups[2].Value;
        }

        private void parseReadPage(BookDataDetailed book, HtmlDocument document)
        {
            var readerlists = document.DocumentNode.SelectSingleNode("./html/body/dl[@class='book']");
            var currentVol = "";
            var inChps = false;
            foreach (var item in readerlists.ChildNodes)
            {
                switch (item.Name)
                {
                case "dt":
                    var vol = item.GetInnerText().Trim();
                    if (inChps)
                    {
                        if (vol.StartsWith(book.Title))
                            vol = vol.Substring(book.Title.Length).Trim();
                        currentVol = vol;
                    }
                    else
                    {
                        if (vol.EndsWith("全文阅读"))
                            inChps = true;
                    }
                    break;
                case "dd":
                    if (!inChps)
                        continue;
                    var href = item.Element("a")?.GetAttribute("href", null);
                    var content = item.Element("a").GetInnerText();
                    book.Chapters.Add(new ChapterDataBrief
                    {
                        Key = chpIdReg.Match(href).Groups[2].Value + " " + currentVol,
                        Title = content,
                        VolumeTitle = currentVol,
                    });
                    break;
                default:
                    continue;
                }
            }
        }
    }
}