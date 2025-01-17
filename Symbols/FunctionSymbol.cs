﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scythe.Symbols
{
    public enum DataType
    {
        Int,
        Float,
        Uint,
        Bool,
        String,
        Void,
        Struct
    }

    public class FunctionSymbol : Symbol
    {

        //public Nodes.BlockStatement Body;

        public DataType returnType;
        public string _retType;

        public unsafe FunctionSymbol(string name, DataType returnType)
        {
            this.name = name;
            this.returnType = returnType;
        }
    }
}
