﻿using StartReader.App.Extensiton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartReader.DataExchange.Response
{
    public abstract class ResponseMessageBase
    {
        internal DataSource Source { get; set; }
    }
}
