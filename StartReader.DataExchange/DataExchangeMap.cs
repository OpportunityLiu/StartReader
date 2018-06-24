using StartReader.DataExchange.Request;
using StartReader.DataExchange.Response;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StartReader.DataExchange
{
    public static class DataExchangeMap
    {
        static DataExchangeMap()
        {
            MethodToRequestMap = new ReadOnlyDictionary<string, Type>(new Dictionary<string, Type>
            {
                ["Search"] = typeof(SearchRequest),
                ["GetBook"] = typeof(GetBookRequest),
            });
            RequestToResponseMap = new ReadOnlyDictionary<Type, Type>(new Dictionary<Type, Type>
            {
                [typeof(SearchRequest)] = typeof(SearchResponse),
                [typeof(GetBookRequest)] = typeof(GetBookResponse),
            });
        }

        public static IReadOnlyDictionary<string, Type> MethodToRequestMap { get; }
        public static IReadOnlyDictionary<Type, Type> RequestToResponseMap { get; }
    }
}
