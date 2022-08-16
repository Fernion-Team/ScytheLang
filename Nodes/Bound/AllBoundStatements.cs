﻿using Scythe.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scythe.Nodes.Bound
{
    public enum BoundStatementKind
    {
        PACKAGE_DECL,
        USE_PACKAGE,
        BLOCK,
        FUNCTION,
        RETURN,
        VARIABLE
    }

    public class BoundStatement
    {
        public BoundStatementKind Kind;
    }

    #region BoundStatement

    public class BoundPackageDeclStatement : BoundStatement
    {
        public string Name;

        public BoundPackageDeclStatement(string name)
        {
            Name = name;
        }
    }

    public class BoundUsePackageStatement : BoundStatement
    {
        public string Name;

        public BoundUsePackageStatement(string name)
        {
            Name = name;
        }
    }

    public class BoundBlockStatement : BoundStatement
    {
        public IReadOnlyList<BoundStatement> Body;

        public BoundBlockStatement(IReadOnlyList<BoundStatement> body)
        {
            Body = body;
        }
    }

    public class BoundExpressionStatement : BoundStatement
    {
        public BoundExpression expr;

        public BoundExpressionStatement(BoundExpression expr)
        {
            this.expr = expr;
        }
    }

    public class Parameter
    {
        public Scythe.Symbols.DataType DataType;
        public string _type;
        public string Name;

        public Parameter(DataType dataType, string name)
        {
            DataType = dataType;
            Name = name;
        }
    }

    public class MemberVariable
    {
        public Scythe.Symbols.DataType DataType;
        public string Name;
        public string _type;

        public MemberVariable(DataType dataType, string name)
        {
            DataType = dataType;
            Name = name;
        }
    }

    public class BoundFunctionStatement : BoundStatement
    {
        public List<Parameter> Parameters;
        public string Name;
        public Scythe.Symbols.DataType Type;
        public string _type;
        public BoundBlockStatement Body;
        public bool isVAARG;

        public BoundFunctionStatement(List<Parameter> parameters, string name, DataType type, BoundBlockStatement body, bool isvaarg)
        {
            Parameters = parameters;
            Name = name;
            Type = type;
            Body = body;
            isVAARG = isvaarg;
        }
    }

    public class BoundInlineAsmStatement : BoundStatement
    {
        public BoundExpression asm;

        public BoundInlineAsmStatement(BoundExpression asm)
        {
            this.asm = asm;
        }
    }

    public class BoundVariableSetStatement : BoundStatement
    {
        public string a;
        public BoundExpression b;

        public BoundVariableSetStatement(string a, BoundExpression b)
        {
            this.a = a;
            this.b = b;
        }
    }

    public class BoundStrMSetStatement : BoundStatement
    {
        public BoundExpression a;
        public string b;
        public BoundExpression c;

        public BoundStrMSetStatement(BoundExpression a, string b, BoundExpression c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }

    public class BoundVariableDeclStatement : BoundStatement
    {
        public string Name;
        public DataType Type;
        public string _type;
        public BoundExpression Value;

        public BoundVariableDeclStatement(string name, DataType type, BoundExpression value)
        {
            Name = name;
            Type = type;
            Value = value;
        }
    }

    public class BoundCastStatement : BoundStatement
    {
        public DataType Type;
        public BoundExpression Expr;

        public BoundCastStatement(DataType type, BoundExpression expr)
        {
            Type = type;
            Expr = expr;
        }
    }

    public class BoundIfStatement : BoundStatement
    {
        public BoundExpression cond;
        public BoundBlockStatement body;

        public BoundIfStatement(BoundExpression cond, BoundBlockStatement body)
        {
            this.cond = cond;
            this.body = body;
        }
    }

    public class BoundExternFunctionStatement : BoundStatement
    {
        public List<Parameter> Parameters;
        public string Name;
        public Scythe.Symbols.DataType Type;

        public BoundExternFunctionStatement(List<Parameter> parameters, string name, DataType type)
        {
            Parameters = parameters;
            Name = name;
            Type = type;
        }
    }

    public class BoundReturnStatement : BoundStatement
    {
        public BoundExpression Value;

        public BoundReturnStatement(BoundExpression value)
        {
            Value = value;
        }
    }

    public class BoundStructStatement : BoundStatement
    {
        public List<MemberVariable> Variables;
        public string Name;

        public BoundStructStatement(string name, List<MemberVariable> variables)
        {
            Variables = variables;
            Name = name;
        }
    }

    #endregion
}
