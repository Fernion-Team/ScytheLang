﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.SynKit.Parser;
using Scythe.Nodes.Bound;
using Scythe.Nodes;
using Scythe.Symbols;
using Token = Yoakke.SynKit.Lexer.IToken<Scythe.TokenType>;

namespace Scythe
{
    public static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
                action(item);
        }
        public static IEnumerable<T> PassForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
                action(item);
            return items;
        }
    }

    public class Binder
    {
        public List<Scope> Scopes = new List<Scope>();
        public Scope currentScope { get; set; }
        public Scope global = new Scope(ScopeKind.Global);

        public DataType? DecideType(string type)
        {
            if (type == null)
                 throw new NullReferenceException("The type is not specified or null, please try rerunning the compiler.");
            switch (type)
            {
                case "int":
                    return DataType.Int;
                case "float":
                    return DataType.Float;
                case "string":
                    return DataType.String;
                case "bool":
                    return DataType.Bool;
                case "uint":
                    return DataType.Uint;
                case "void":
                    return DataType.Void;
                default:
                    if(global.Lookup(type).GetType() == typeof(StructSymbol))
                    {
                        return DataType.Struct;
                    }
                    else
                    {
                        throw new Exception("This type " + type + " does not exist!");
                    }
            }
        }

        public Operator DecideOperator(string op)
        {
            switch (op)
            {
                case "+":
                    return Operator.PLUS;
                case "-":
                    return Operator.MINUS;
                case "*":
                    return Operator.MULTI;
                case "/":
                    return Operator.DIV;
                case ">":
                    return Operator.GT;
                case "<":
                    return Operator.LT;
                case ">=":
                    return Operator.GQ;
                case "<=":
                    return Operator.LQ;
                case "==":
                    return Operator.EQ;
                case "!=":
                    return Operator.NOT;
                default:
                    throw new InvalidDataException("Operator " + op + " does not exist, sorry!");
            }
        }

        public void Push(Scope scope)
        {
            Scopes.Add(scope);
        }

        public Scope Pop()
        {
            var x = Scopes.Last();
            Scopes.RemoveAt(Scopes.Count);
            return x;
        }

        public BoundExpression BindExpression(Expression expr)
        {
            switch(expr)
            {
                case VariableExpr:
                    return new BoundVariableExpr((expr as VariableExpr).name.Text);
                case BinaryExpr:
                    var bnex = expr as BinaryExpr;
                    return new BoundBinaryExpr(BindExpression(bnex.left), DecideOperator(bnex.op.Text), BindExpression(bnex.right));
                case CallFunctionExpr:
                    var cfunc = expr as CallFunctionExpr;
                    return new BoundCallFunctionExpr(cfunc.name.Text, BindExpressionList(cfunc.arguments));
                case IntLiteralExpr:
                    var intlit = expr as IntLiteralExpr;
                    return new BoundIntLiteralExpr(int.Parse(intlit.literal.Text));
                case FloatLiteralExpr:
                    var Floatlit = expr as FloatLiteralExpr;
                    return new BoundFloatLiteralExpr(float.Parse(Floatlit.literal.Text));
                case StringLiteralExpr:
                    var strlit = expr as StringLiteralExpr;
                    return new BoundStringLiteralExpr(strlit.literal.Text.Replace("\"",""));
                case StructMVExpr:
                    return new BoundStructMVExpr((expr as StructMVExpr).structName.Text, (expr as StructMVExpr).mvName.Text);
                case NewObjectExpression:
                    return new BoundNewObjectExpression((expr as NewObjectExpression).structName.Text);
            }
            throw new InvalidDataException($"Expression of type {expr.GetType().Name} could not be binded to! (It is invalid.)");
        }

        public List<BoundExpression> BindExpressionList(IReadOnlyList<Expression> exprs)
        {
            var x = new List<BoundExpression>();
            foreach (var expr in exprs)
                x.Add(BindExpression(expr));
            return x;
        }

        public List<Parameter> BindParams(Punctuated<(Token Ident, Token Colon, Token Type), Token> prms)
        {
            var parameters = new List<Parameter>();
            foreach(var p in prms)
            {
                var pr = new Parameter(DecideType(p.Value.Type.Text).Value, p.Value.Ident.Text);
                pr._type = p.Value.Type.Text;
                parameters.Add(pr);
            }
            return parameters;
        }

        public List<MemberVariable> BindMemberVars(Punctuated<(Token Ident, Token Colon, Token Type), Token> mv)
        {
            var mvs = new List<MemberVariable>();
            foreach (var m in mv)
            {
                var _m = new MemberVariable(DecideType(m.Value.Type.Text).Value, m.Value.Ident.Text);
                _m._type = m.Value.Type.Text;
                mvs.Add(_m);
            }
            return mvs;
        }

        public List<BoundStatement> Bind(List<Statement> statements)
        {
            var allStmts = new List<BoundStatement>();
            Scopes.Add(global); // Implement the global scope.
            currentScope = global;
            foreach(var x in statements)
            {
                switch(x)
                {
                    case PackageUseStatement:
                        allStmts.Add(new BoundUsePackageStatement((x as PackageUseStatement).name.Text));
                        break;
                    case PackageDeclStatement:
                        allStmts.Add(new BoundPackageDeclStatement((x as PackageDeclStatement).name.Text));
                        break;
                    case BlockStatement:
                        allStmts.Add(new BoundBlockStatement(Bind((x as BlockStatement).statements.ToList())));
                        break;
                    case ExpressionStatement:
                        allStmts.Add(new BoundExpressionStatement(BindExpression((x as ExpressionStatement).Expression)));
                        break;
                    case FunctionStatement:
                        var fncSt = new BoundFunctionStatement(BindParams((x as FunctionStatement).parameters), (x as FunctionStatement).name.Text, DecideType((x as FunctionStatement).type.Text).Value, new BoundBlockStatement(Bind((x as FunctionStatement).body.statements.ToList())), (x as FunctionStatement).isVAARG);
                        fncSt._type = (x as FunctionStatement).type.Text;
                        allStmts.Add(fncSt);
                        var fnsy = new FunctionSymbol(fncSt.Name, fncSt.Type);
                        fnsy._retType = (x as FunctionStatement).type.Text;
                        currentScope.Insert(fnsy);
                        break;
                    case ReturnStatement:
                        allStmts.Add(new BoundReturnStatement(BindExpression((x as ReturnStatement).value)));
                        break;
                    case VariableDeclStatement:
                        var vds = new BoundVariableDeclStatement((x as VariableDeclStatement).name.Text, DecideType((x as VariableDeclStatement).type.Text).Value, BindExpression((x as VariableDeclStatement).value));
                        vds._type = (x as VariableDeclStatement).type.Text;
                        allStmts.Add(vds);
                        break;
                    case InlineAsmStatement:
                        allStmts.Add(new BoundInlineAsmStatement(BindExpression((x as InlineAsmStatement).asm)));
                        break;
                    case VariableSetStatement:
                        allStmts.Add(new BoundVariableSetStatement((x as VariableSetStatement).a.Text, BindExpression((x as VariableSetStatement).b)));
                        break;
                    case ExternFunctionStatement:
                        allStmts.Add(new BoundExternFunctionStatement(BindParams((x as ExternFunctionStatement).parameters), (x as ExternFunctionStatement).name.Text, DecideType((x as ExternFunctionStatement).type.Text).Value));
                        break;
                    case CastStatement:
                        allStmts.Add(new BoundCastStatement(DecideType((x as CastStatement).dataType.Text).Value, BindExpression((x as CastStatement).expression)));
                        break;
                    case IfStatement:
                        allStmts.Add(new BoundIfStatement(BindExpression((x as IfStatement).condition), new BoundBlockStatement(Bind((x as IfStatement).conditionBlock.statements.ToList()))));
                        break;
                    case StructStatement:
                        var nstmt = new BoundStructStatement((x as StructStatement).name.Text, BindMemberVars((x as StructStatement).Variables));
                        allStmts.Add(nstmt);
                        if (global.Lookup(nstmt.Name) == null)
                        {
                            global.symbols.Add(nstmt.Name, new StructSymbol(nstmt.Name, nstmt.Variables));
                        }
                        break;
                    case StrMSetStatement:
                        allStmts.Add(new BoundStrMSetStatement(BindExpression((x as StrMSetStatement).a), (x as StrMSetStatement).b.Text, BindExpression((x as StrMSetStatement).c)));
                        break;
                }
            }
            return allStmts;
        }
    }
}
