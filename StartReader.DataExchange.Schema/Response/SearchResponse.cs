using Newtonsoft.Json;
using StartReader.DataExchange.Model;
using StartReader.DataExchange.Request;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartReader.DataExchange.Response
{
    public sealed class SearchResponse : ResponseMessageBase,IResponseMessage<SearchRequest, SearchResponse>
    {
        public IList<BookDataBrief> Books { get; } = new List<BookDataBrief>();
    }
}
