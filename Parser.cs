using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scythe.Nodes;
using Yoakke;
using Yoakke.Collections.Values;
using Yoakke.SynKit.Parser;
using Yoakke.SynKit.Parser.Attributes;
using Token = Yoakke.SynKit.Lexer.IToken<Scythe.TokenType>;

namespace Scythe
{
    [Parser(typeof(TokenType))]
    public partial class Parser
    {
        [Rule("program: stmt* tok_end")]
        private static IReadOnlyList<Statement> ProgramParse(IReadOnlyList<Statement> parsed, Token end)
        {
            return parsed;
        }

       

        [Rule("stmt: kw_Package bas_rightlook lit_String")]
        private static Statement DeclarePackage(Token notneeded, Token notneeded2, Token name)
        {
            return new PackageDeclStatement(name);
        }

        [Rule("stmt: kw_Use kw_Package bas_rightlook lit_String")]
        private static Statement UsePackage(Token notneededtoo, Token notneeded, Token notneeded2, Token name)
        {
            return new PackageUseStatement(name);
        }

        [Rule("expr: identifier'('(expr(',' expr) *) ? ')'")]
        public static Expression CallFunctionEx(Token name, Token _2, Punctuated<Expression, Token> arguments, Token _4)
        {
            return new CallFunctionExpr(name, arguments.Values.Select(t => t).ToList().ToValue());
        }

        [Rule("block_stmt: '{' stmt* '}'")]
        private static BlockStatement Block(Token notneeded, IReadOnlyList<Statement> stmts, Token notneeded2)
        {
            return new BlockStatement(stmts);
        }

        [Rule("type_identifier: (kw_Int|kw_String|kw_Float|kw_Bool|kw_Char|kw_Uint|kw_Void)")]
        private static Token typeident(Token ident)
        {
            return ident;
        }

        [Rule("stmt: expr")]
        public static Statement ExpressionStatementF(Expression expr)
        {
            return new ExpressionStatement(expr);
        }

        [Rule("stmt: kw_Function identifier '(' ((identifier ':' type_identifier) (',' (identifier ':' type_identifier))*)? (',' elp_vaa)? ')' bas_rightlook type_identifier block_stmt")]
        private static Statement FunctionDeclaration(Token _1, Token name, Token _3, Punctuated<(Token Ident, Token Colon, Token Type), Token> parameters, (Token, Token)? vaarg, Token _4, Token _5, Token type, BlockStatement body)
        {
            bool isvarg = false;

            if (vaarg.HasValue)
                isvarg = true;

            return new FunctionStatement(parameters, body, name, type, isvarg);
        }

        [Rule("stmt: identifier bas_rightlook kw_Struct '{' ((identifier ':' type_identifier) (',' (identifier ':' type_identifier))*)? '}'")]
        private static Statement StructDecl(Token name, Token _1, Token _2, Token _3, Punctuated<(Token Ident, Token Colon, Token Type), Token> values, Token _4)
        {
            return new StructStatement(name, values);
        }

        [Rule("stmt: kw_Var identifier bas_leftlook expr ':' identifier")]
        private static Statement VarDeclaration(Token _1, Token name, Token _2, Expression value, Token _3, Token type)
        {
            return new VariableDeclStatement(name, value, type);
        }

        [Rule("stmt: kw_Asm '(' expr ')'")]
        private static Statement InlAsmD(Token _1, Token _2, Expression expr, Token _3)
        {
            return new InlineAsmStatement(expr);
        }

        [Rule("stmt: kw_Return expr")]
        public static Statement ReturnStmt(Token _1, Expression expr)
        {
            return new ReturnStatement(expr);
        }

        [Rule("stmt: identifier bas_leftlook expr")]
        private static Statement SetVarStmt(Token a, Token _1, Expression b)
        {
            return new VariableSetStatement(a, b);
        }

        [Rule("stmt: expr bas_dot identifier bas_leftlook expr")]
        private static Statement SetStructElement(Expression a, Token _1, Token b, Token _2, Expression c)
        {
            return new StrMSetStatement(a, b, c);
        }

        [Rule("stmt: kw_Extern kw_Function identifier '(' ((identifier ':' type_identifier) (',' (identifier ':' type_identifier))*)? ')' bas_rightlook type_identifier")]
        private static Statement ExtFunctionDeclaration(Token _0, Token _1, Token name, Token _3, Punctuated<(Token Ident, Token Colon, Token Type), Token> parameters, Token _4, Token _5, Token type)
        {
            return new ExternFunctionStatement(parameters, name, type);
        }

        [Rule("stmt: kw_If '(' expr ')' block_stmt")]
        private static Statement IfStmtt(Token _0, Token _1, Expression expr, Token _2, BlockStatement stmt)
        { 
            return new IfStatement(expr, stmt);
        }

        [Rule("stmt: '(' type_identifier ')' expr")]
        private static Statement CastStmt(Token _1, Token type, Token _2, Expression expr)
        {
            return new CastStatement(type, expr);
        }

        [Right("^")]
        [Left("*", "/", "%")]
        [Left("+", "-")]
        [Left(">=", "<=", ">", "<")]
        [Left("==")]
        [Rule("expr")]
        public static Expression BinOp(Expression a, Token op, Expression b)
        {
            return new BinaryExpr(a, b, op);
        }

        [Rule("expr: '(' expr ')'")]
        public static Expression Grouping(Token _1, Expression n, Token _2) => n;

        [Rule("expr: lit_Int")]
        public static Expression IntLiteral(Token literal)
        {
            return new IntLiteralExpr(literal);
        }

        [Rule("expr: lit_String")]
        public static Expression StrLiteral(Token literal)
        {
            return new StringLiteralExpr(literal);
        }

        [Rule("expr: identifier")]
        public static Expression VariableExpr(Token varf)
        {
            return new VariableExpr(varf);
        }

        [Rule("expr: identifier bas_rightlook identifier")]
        public static Expression StrMVExpr(Token obj, Token _1, Token mvName)
        {
            return new StructMVExpr(obj, mvName);
        }

        [Rule("expr: lit_Float")]
        public static Expression FloatLiteral(Token literal)
        {
            return new FloatLiteralExpr(literal);
        }

        [Rule("expr: identifier bas_dot kw_New")]
        public static Expression CreateStructObject(Token strName, Token _1, Token _2)
        {
            return new NewObjectExpression(strName);
        }

        
    }
}
