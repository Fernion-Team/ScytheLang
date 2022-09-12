using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scythe.Nodes.Bound
{
    public enum Operator
    {
        PLUS,
        MINUS,
        MULTI,
        DIV,
        LT,
        GT,
        LQ,
        GQ,
        NOT,
        EQ
    }

    public class BoundExpression
    {
    }

    #region BoundExpressions

    public class BoundCallFunctionExpr : BoundExpression
    {
        public string Name;
        public List<BoundExpression> Arguments;

        public BoundCallFunctionExpr(string name, List<BoundExpression> arguments)
        {
            Name = name;
            Arguments = arguments;
        }
    }

    public class BoundBinaryExpr : BoundExpression
    {
        public BoundExpression a;
        public Operator op;
        public BoundExpression b;

        public BoundBinaryExpr(BoundExpression a, Operator op, BoundExpression b)
        {
            this.a = a;
            this.op = op;
            this.b = b;
        }
    }

    public class BoundVariableExpr : BoundExpression
    {
        public string Name;

        public BoundVariableExpr(string name)
        {
            Name = name;
        }
    }

    public class BoundNewObjectExpression : BoundExpression
    {
        public string structName;

        public BoundNewObjectExpression(string structName)
        {
            this.structName = structName;
        }
    }

    public class BoundIntLiteralExpr : BoundExpression
    {
        public int Literal;

        public BoundIntLiteralExpr(int literal)
        {
            Literal = literal;
        }
    }

    public class BoundFloatLiteralExpr : BoundExpression
    {
        public float Literal;

        public BoundFloatLiteralExpr(float literal)
        {
            Literal = literal;
        }
    }

    public class BoundStructMVExpr : BoundExpression
    {
        public string strName;
        public string mvName;

        public BoundStructMVExpr(string strName, string mvName)
        {
            this.strName = strName;
            this.mvName = mvName;
        }
    }

    public class BoundStringLiteralExpr : BoundExpression
    {
        public string Literal;

        public BoundStringLiteralExpr(string literal)
        {
            Literal = literal;
        }
    }

    public class BoundPointerExpr : BoundExpression
    {
        public BoundExpression expr;

        public BoundPointerExpr(BoundExpression expr)
        {
            this.expr = expr;
        }
    }

    #endregion      
}
