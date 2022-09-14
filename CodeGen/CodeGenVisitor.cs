using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scythe.Symbols;
using Scythe.Nodes.Bound;
using LLVMSharp;
using LLVMSharp.Interop;

namespace Scythe.CodeGen
{
    public class CodeGenVisitor
    {
        private readonly LLVMValueRef nullValue = new LLVMValueRef(IntPtr.Zero);

        private readonly LLVMModuleRef module;

        private readonly LLVMBuilderRef builder;

        private readonly Dictionary<string, Pointer<LLVMOpaqueValue>> namedValues = new Dictionary<string, Pointer<LLVMOpaqueValue>>();

        private readonly Dictionary<string, Pointer<LLVMOpaqueValue>> varAllocaValues = new Dictionary<string, Pointer<LLVMOpaqueValue>>();

        private readonly Dictionary<string, Pointer<LLVMOpaqueType>> strObjTypes = new Dictionary<string, Pointer<LLVMOpaqueType>>();

        private readonly Dictionary<string, Pointer<LLVMOpaqueType>> structTypes = new Dictionary<string, Pointer<LLVMOpaqueType>>();

        private readonly Stack<Pointer<LLVMOpaqueValue>> valueStack = new Stack<Pointer<LLVMOpaqueValue>>();

        private readonly Dictionary<string, Symbol> symbolTable;

        private LLVMPassManagerRef FPM;

        public CodeGenVisitor(LLVMModuleRef module, LLVMBuilderRef builder, Dictionary<string, Symbol> symbolTable)
        {
            this.module = module;
            this.builder = builder;
            this.symbolTable = symbolTable;
            this.FPM = module.CreateFunctionPassManager();

            FPM.InitializeFunctionPassManager();
        }

        public Stack<Pointer<LLVMOpaqueValue>> ResultStack { get { return valueStack; } }

        public object Visit(BoundExpression expr) => expr switch
        {
            BoundIntLiteralExpr => VisitIntegerNumber(expr as BoundIntLiteralExpr),
            BoundFloatLiteralExpr => VisitFloatNumber(expr as BoundFloatLiteralExpr),
            BoundVariableExpr => VisitVariableExpr(expr as BoundVariableExpr),
            BoundCallFunctionExpr => VisitCallFExpr(expr as BoundCallFunctionExpr),
            BoundBinaryExpr => VisitBinaryExpr(expr as BoundBinaryExpr),
            BoundStringLiteralExpr => VisitStringLiteral(expr as BoundStringLiteralExpr),
            BoundStructMVExpr => VisitStructMVExpr(expr as BoundStructMVExpr),
            BoundNewObjectExpression => VisitNewObj(expr as BoundNewObjectExpression),
            BoundPointerExpr => VisitValuePointer(expr as BoundPointerExpr),
            BoundBoolLiteralExpr => VisitBoolLiteral(expr as BoundBoolLiteralExpr),
            _ => throw new Exception("[FATAL]: Expression that was attempted to be visited is invalid/unknown. '" + expr + "'."),
        };

        public object Visit(BoundStatement stmt) => stmt switch
        {
            BoundFunctionStatement => this.VisitFunction(stmt as BoundFunctionStatement),
            BoundExpressionStatement => this.VisitExprStmt(stmt as BoundExpressionStatement),
            BoundReturnStatement => this.VisitReturn(stmt as BoundReturnStatement),
            BoundInlineAsmStatement => this.VisitInlineAsm(stmt as BoundInlineAsmStatement),
            BoundVariableDeclStatement => this.VisitVariableDecl(stmt as BoundVariableDeclStatement),
            BoundUsePackageStatement => this.VisitImportStmt(stmt as BoundUsePackageStatement),
            BoundVariableSetStatement => this.VisitVarSet(stmt as BoundVariableSetStatement),
            BoundExternFunctionStatement => this.VisitFunctionExtern(stmt as BoundExternFunctionStatement),
            BoundCastStatement => this.VisitCast(stmt as BoundCastStatement),
            BoundIfStatement => this.VisitIfStmt(stmt as BoundIfStatement),
            BoundStructStatement => this.VisitStructStmt(stmt as BoundStructStatement),
            BoundStrMSetStatement => this.VisitStrM(stmt as BoundStrMSetStatement),
            BoundWhileStatement => this.VisitWhileStmt(stmt as BoundWhileStatement),
            _ => throw new Exception("[FATAL]: Statement that was attempted to be visited is invalid/unknown. '"+stmt+"'."),
        };

        public void ClearResultStack()
        {
            valueStack.Clear();
        }

        /*
            public unsafe BoundStatement VisitVariableDecl(BoundVariableDeclStatement stmt)
        {
            var alloca_v = LLVM.BuildAlloca(builder, DataTyToType(stmt.Type), StrToSByte(stmt.Name));
            
            this.Visit(stmt.Value);
            LLVM.BuildStore(builder, valueStack.Pop(), alloca_v);

            this.Visit(stmt.Value);
            namedValues.Add(stmt.Name, valueStack.Pop());

            return stmt;
        }
         */

        public unsafe BoundStatement VisitStrM(BoundStrMSetStatement stmt)
        {
            // visit the object name expression
            this.Visit(stmt.a);
            // get it
            var strobj = valueStack.Pop();
            //get the type of the value
            var strType = LLVM.TypeOf(strobj);

            // get the name of the struct type.
            var x = LLVM.PrintTypeToString(strType);

            var theName = Helpers.SByteToStr(x);
            theName = theName.Replace("%", "");
            theName = theName.Replace("*", "");

            // check if the symbol table has the struct symbol
            if(symbolTable.ContainsKey(theName))
            {
                //get structsymbol
                var sym = symbolTable[theName] as StructSymbol;

                // find the member variable
                var memberVar = sym.Members.Single(e => e.Name == stmt.b);
                // get it's GEP index
                var gepIndex = sym.Members.IndexOf(memberVar);
                Console.WriteLine(theName+" "+memberVar + " : " + gepIndex +" : "+strobj+" : "+stmt.a);
                // build the GEP
                try
                {


                    Console.WriteLine(module.PrintToString());
                    Console.WriteLine(strobj);
                    var GEP = LLVM.BuildStructGEP(builder, strobj, (uint)gepIndex, StrToSByte("struct_gep")); /*LLVM.BuildStructGEP(builder, strobj, (uint)gepIndex, StrToSByte("struct_gep_" + theName));*/
                        //visit the set value expression
                    this.Visit(stmt.c);
                
                        //make a store
                    LLVM.BuildStore(builder, valueStack.Pop(), GEP);
                }
                catch(AccessViolationException e)
                {
                        
                }
            }
            else
            {
                //LLVM.DumpModule(module);
                // *oh no*
                throw new Exception("Unknown Struct. "+theName);
            }

            return stmt;
        }

        public unsafe BoundStatement VisitVarSet(BoundVariableSetStatement stmt)
        {
            if(namedValues.ContainsKey(stmt.a))
            {
                this.Visit(stmt.b);
                var x = valueStack.Pop();
                namedValues[stmt.a] = x;

                LLVM.BuildStore(builder, x, varAllocaValues[stmt.a]);
            }
            else
            {
                throw new Exception("The variable name you are to set to, " + stmt.a + " was never declared.");
            }
            return stmt;
        }

        public BoundStatement VisitExprStmt(BoundExpressionStatement stmt)
        {
            this.Visit(stmt.expr);
            return stmt;
        }

        public unsafe BoundExpression VisitIntegerNumber(BoundIntLiteralExpr expr)
        {

            valueStack.Push(LLVM.ConstInt(LLVM.Int32Type(), (ulong)expr.Literal, 1));
            return expr;
        }

        public unsafe BoundExpression VisitBoolLiteral(BoundBoolLiteralExpr expr)
        {
            valueStack.Push(LLVM.ConstInt(LLVM.Int1Type(), (ulong)expr.Literal, 0));
            return expr;
        }

        public unsafe BoundExpression VisitFloatNumber(BoundFloatLiteralExpr expr)
        {

            valueStack.Push(LLVM.ConstReal(LLVM.FloatType(), expr.Literal));
            return expr;
        }

        public unsafe BoundExpression VisitValuePointer(BoundPointerExpr expr)
        {
            this.Visit(expr.expr);

            var val = valueStack.Pop();
    
            valueStack.Push(LLVM.BuildLoad2(builder, LLVM.PointerType(LLVM.TypeOf(val), 0), val, StrToSByte("ix_drp")));

            return expr;
        }

        public unsafe BoundExpression VisitStringLiteral(BoundStringLiteralExpr expr)
        {

            int ln = expr.Literal.Length;

            //valueStack.Push(LLVM.BuildGEP2(builder, LLVM.PointerType(LLVM.Int8Type(), 0), LLVM.ConstString(StrToSByte(expr.Literal), (uint)ln, 0), pptr, (uint)ln, StrToSByte("GEPStr")));
            valueStack.Push(LLVM.BuildGlobalStringPtr(builder, StrToSByte(expr.Literal), StrToSByte("strtmp")));
            
            return expr;
        }

        public unsafe BoundExpression VisitVariableExpr(BoundVariableExpr expr)
        {
            Pointer<LLVMOpaqueValue> value;

            if (namedValues.TryGetValue(expr.Name, out value))
            {
                if (varAllocaValues.ContainsKey(expr.Name))
                    valueStack.Push(LLVM.BuildLoad(builder, varAllocaValues[expr.Name], StrToSByte("load_var_" + expr.Name)));
                else
                    valueStack.Push(value);
            }
            else
            {
                throw new Exception("Invalid/Unknown Variable Name.");
            }

            return expr;
        }

        public unsafe BoundExpression VisitNewObj(BoundNewObjectExpression expr)
        {
            Pointer<LLVMOpaqueType> value;
            if (structTypes.TryGetValue(expr.structName, out value))
            {
                valueStack.Push(LLVM.BuildAlloca(builder, value, StrToSByte("object_" + expr.structName)));
            }
            else
            {
                throw new Exception("Invalid/Unknown Struct Name.");
            }

            return expr;
        }

        public unsafe BoundExpression VisitStructMVExpr(BoundStructMVExpr expr)
        {
            // get it
            var strobj = namedValues[expr.strName];

            //get the type of the value
            var strType = LLVM.TypeOf(strobj);

            // get the name of the struct type.
            var x = LLVM.PrintTypeToString(strType);

            Console.WriteLine(expr.strName);

            var theName = Helpers.SByteToStr(x);
            theName = theName.Replace("%", "");
            theName = theName.Replace("*", "");

            // check if the symbol table has the struct symbol
            if (symbolTable.ContainsKey(theName))
            {
                //get structsymbol
                var sym = symbolTable[theName] as StructSymbol;

                // find the member variable
                var memberVar = sym.Members.Single(e => e.Name == expr.mvName);
                // get it's GEP index
                var gepIndex = sym.Members.IndexOf(memberVar);

                Console.WriteLine(memberVar+ " : " + gepIndex);

                var GEP = LLVM.BuildStructGEP(builder, strobj, (uint)gepIndex, StrToSByte("struct_member_gep_" + theName));

                valueStack.Push(LLVM.BuildLoad(builder, GEP, StrToSByte("struct_member_" + theName)));
            }
            else
            {
                LLVM.DumpModule(module);
                // *oh no*
                throw new Exception("Unknown Struct. " + theName);
            }

            return expr;
        }

        public unsafe sbyte* StrToSByte(string str)
        {
            IntPtr ptr = Marshal.StringToHGlobalAnsi(str);
            sbyte* sby = (sbyte*)ptr;
            return sby;
        }

        public unsafe LLVMIntPredicate CDTOperator(Operator op) => op switch
        {
            Operator.GT => LLVMIntPredicate.LLVMIntSGT,
            Operator.LT => LLVMIntPredicate.LLVMIntSLT,
            Operator.GQ => LLVMIntPredicate.LLVMIntSGE,
            Operator.LQ => LLVMIntPredicate.LLVMIntSLT,
            Operator.NOT => LLVMIntPredicate.LLVMIntNE,
            Operator.EQ => LLVMIntPredicate.LLVMIntEQ,
            _ => throw new Exception("Unknown Operator.")
        };       

        public unsafe BoundStatement VisitIfStmt(BoundIfStatement stmt)
        {
            BoundBinaryExpr cond = stmt.cond as BoundBinaryExpr;

            this.Visit(cond.a); this.Visit(cond.b);

            LLVMOpaqueValue* r = valueStack.Pop();
            LLVMOpaqueValue* l = valueStack.Pop();

            //var val = LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealPredicateTrue, l, r, Helpers.StrToSByte("ifcond"));

            var val = LLVM.BuildICmp(builder, CDTOperator(cond.op), l, r, Helpers.StrToSByte("ifcond"));

            var func = LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder));

            LLVMOpaqueBasicBlock* thenBB = LLVM.AppendBasicBlock(func, Helpers.StrToSByte("then"));
            LLVMOpaqueBasicBlock* mergeBB = LLVM.AppendBasicBlock(func, Helpers.StrToSByte("ifcont"));

            LLVM.BuildCondBr(builder, val, thenBB, mergeBB);

            LLVM.PositionBuilderAtEnd(builder, thenBB);

            try
            {
                foreach (var x in stmt.body.Body)
                    this.Visit(x);
            }
            catch
            {
                Console.WriteLine("Error Writing Then Label.");
            }

            LLVM.BuildBr(builder, mergeBB);

            LLVM.PositionBuilderAtEnd(this.builder, mergeBB);

            return stmt;
        }

        public unsafe BoundStatement VisitWhileStmt(BoundWhileStatement stmt)
        {
                BoundBinaryExpr cond = stmt.cond as BoundBinaryExpr;

   

            var func = LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder));

            LLVMOpaqueBasicBlock* whileBB = LLVM.AppendBasicBlock(func, Helpers.StrToSByte("whilecondt"));
            LLVMOpaqueBasicBlock* trueBB = LLVM.AppendBasicBlock(func, Helpers.StrToSByte("condtrue"));
            LLVMOpaqueBasicBlock* mergeBB = LLVM.AppendBasicBlock(func, Helpers.StrToSByte("whilecont"));

            LLVM.BuildBr(builder, whileBB);

            LLVM.PositionBuilderAtEnd(builder, whileBB);

            this.Visit(cond.a); this.Visit(cond.b);
            
            LLVMOpaqueValue* r = valueStack.Pop();
            LLVMOpaqueValue* l = valueStack.Pop();

            var _val = LLVM.BuildICmp(builder, CDTOperator(cond.op), l, r, Helpers.StrToSByte("whilecond"));
            LLVM.BuildCondBr(builder, _val, trueBB, mergeBB);

            LLVM.PositionBuilderAtEnd(builder, trueBB);

            try
            {
                foreach (var x in stmt.body.Body)
                    this.Visit(x);
            }
            catch
            {
                Console.WriteLine("Error Writing True(while) Label.");
            }

            LLVM.BuildBr(builder, whileBB);

            LLVM.PositionBuilderAtEnd(this.builder, mergeBB);

            return stmt;
        }

        public unsafe BoundStatement VisitImportStmt(BoundUsePackageStatement stmt)
        {
            var importedFile = File.ReadAllText(stmt.Name.Replace("\"", ""));

            var Lexer = new Lexer(importedFile);
            var Parser = new Parser(Lexer);

            var AST = Parser.ParseProgram();

            if(AST.IsOk)
            {
                var CGV = new CodeGenVisitor(module, builder, new Dictionary<string, Symbol>());

                foreach (var statement in new Binder().Bind(AST.Ok.Value.ToList()))
                {
                    Console.WriteLine(statement);
                    CGV.Visit(statement);
                }
            }
            else
            {
                Console.WriteLine("Failed to Parse Scythe Package File " + stmt.Name+".");
            }

            return stmt;
        }

        public unsafe LLVMOpaqueType* ParameterType(Parameter mtr)
        {
            return UnwrapType(mtr.DataType);
        }

        public unsafe LLVMOpaqueType* UnwrapType(BoundType type)
        {
            if (type is BoundIdentifierType)
            {
                var ty = type as BoundIdentifierType;
                switch (ty.Type)
                {
                    case DataType.Int:
                        return LLVM.Int32Type();
                    case DataType.Float:
                        return LLVM.FloatType();
                    case DataType.Uint:
                        return LLVM.Int8Type();
                    case DataType.Bool:
                        return LLVM.Int1Type();
                    case DataType.String:
                        return LLVM.PointerType(LLVM.Int8Type(), 0);
                    case DataType.Void:
                        return LLVM.VoidType();
                    case DataType.Struct:
                        if (symbolTable.ContainsKey(ty.Name))
                        {
                            return structTypes[ty.Name];
                        }
                        else
                            throw new Exception("Unknown/Invalid Struct Typename.");
                }
            }
            else if (type is BoundPointerType)
            {
                return LLVM.PointerType(UnwrapType((type as BoundPointerType).Type), 0);
            }

            return LLVM.Int32Type();
        }
        
        public unsafe LLVMOpaqueType* MVarType(MemberVariable mtr)
        {
            return UnwrapType(mtr.DataType);
        }

        public unsafe LLVMOpaqueType*[] ParameterTypes(List<Parameter> mtrs)
        {
            var paramTypes = new LLVMOpaqueType*[mtrs.Count];

            for (int i = 0; i < mtrs.Count; i++)
                paramTypes[i] = ParameterType(mtrs[i]);

            return paramTypes;
        }

        public unsafe LLVMOpaqueType*[] MVarTypes(List<MemberVariable> mtrs)
        {
            var paramTypes = new LLVMOpaqueType*[mtrs.Count];

            for (int i = 0; i < mtrs.Count; i++)
                paramTypes[i] = MVarType(mtrs[i]);

            return paramTypes;
        }

        public unsafe BoundExpression VisitBinaryExpr(BoundBinaryExpr expr)
        {
            this.Visit(expr.a);
            this.Visit(expr.b);

            LLVMOpaqueValue* r = valueStack.Pop();
            LLVMOpaqueValue* l = valueStack.Pop();

            LLVMOpaqueValue* n;
            sbyte* s = null;
            switch (expr.op)
            {
                case Operator.PLUS:
                    if(expr.a.GetType() == typeof(BoundStringLiteralExpr) && expr.b.GetType() == typeof(BoundStringLiteralExpr))
                    {
                        n = LLVM.BuildGlobalStringPtr(builder, StrToSByte((expr.a as BoundStringLiteralExpr).Literal+(expr.b as BoundStringLiteralExpr).Literal), StrToSByte("strtmp"));
                        break;
                    }

                    n = LLVM.BuildAdd(builder, l, r, s = StrToSByte("AddTMP"));
                    break;
                case Operator.MINUS:
                    n = LLVM.BuildSub(builder, l, r, s = StrToSByte("SubTMP"));
                    break;
                case Operator.MULTI:
                    n = LLVM.BuildMul(builder, l, r, s = StrToSByte("MulTMP"));
                    break;
                case Operator.DIV:
                    n = LLVM.BuildSDiv(builder, l, r, s = StrToSByte("DivTMP"));
                    break;
                default:
                    throw new Exception("Invalid Operator in Binary Expression.");
            }
            Marshal.FreeHGlobal((IntPtr)s);
            valueStack.Push(n);
            return expr;
        }

        public unsafe BoundStatement VisitInlineAsm(BoundInlineAsmStatement stmt)
        {
            LLVM.AppendModuleInlineAsm(module, StrToSByte((stmt.asm as BoundStringLiteralExpr).Literal), (UIntPtr)(stmt.asm as BoundStringLiteralExpr).Literal.Length);
            //LLVM.ConstInlineAsm(LLVM.FunctionType(LLVM.VoidType(), null, 0, 0), StrToSByte((stmt.asm as BoundStringLiteralExpr).Literal), StrToSByte(""), 0, 1);
            return stmt;
        }

        /// <summary>
        /// Deprecated.
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns></returns>
        public unsafe BoundStatement VisitFunctionExtern(BoundExternFunctionStatement stmt)
        {
            fixed (LLVMOpaqueType** prTypes = ParameterTypes(stmt.Parameters))
            {
                LLVM.AddFunction(this.module, StrToSByte(stmt.Name), LLVM.FunctionType(UnwrapType(stmt.Type), prTypes, (uint)stmt.Parameters.Count, 0));
            }

            return stmt;
        }

        public unsafe BoundExpression VisitCallFExpr(BoundCallFunctionExpr expr)
        {
            var callee = LLVM.GetNamedFunction(module, StrToSByte(expr.Name));
            var mod = builder.InsertBlock.Parent.GlobalParent;

            if ((IntPtr)callee == IntPtr.Zero)
            {
                // namedValues.Add(expr.Name, LLVM.AddFunction(module, StrToSByte(expr.Name), DataTyToType((symbolTable[expr.Name] as FunctionSymbol).returnType)));
                //valueStack.Push(LLVM.ConstInt(LLVM.Int32Type(), 0, 1));
                if (expr.Name == "print")
                {
                    var namedF = mod.GetNamedFunction("printf");
                    if (namedF != null)
                    {
                        callee = namedF;
                    }
                    else
                    {

                        fixed (LLVMOpaqueType** pptr = new LLVMOpaqueType*[] { LLVM.PointerType(LLVM.Int8Type(), 0) })
                            callee = LLVM.AddFunction(mod, StrToSByte("printf"), LLVM.FunctionType(LLVM.Int32Type(), pptr, 1, 1));
                    }
                }
                else
                {
                    throw new Exception("Function to be called, does not exist.");
                }
            }

            //Console.WriteLine("Calling: " + expr.Name);
            //module.Dump();
            if (LLVM.IsFunctionVarArg(LLVM.TypeOf(callee)) == 1 && LLVM.CountParams(callee) != expr.Arguments.Count)
            {
                throw new Exception("Incorrect number of arguments passed.");
            }

            var argCount = (uint)expr.Arguments.Count;
            var argsV = new LLVMOpaqueValue*[Math.Max(argCount, 1)];

            for(int i = 0; i < argCount; ++i)
            {
                this.Visit(expr.Arguments[i]);
                argsV[i] = valueStack.Pop();
            }

            fixed (LLVMOpaqueValue** pptr = argsV)
            {
                valueStack.Push(LLVM.BuildCall(builder, callee, pptr, argCount, StrToSByte("CallTMP")));
            }

            return expr;
        }

        public unsafe BoundStatement VisitStructStmt(BoundStructStatement stmt)
        { 

            var ctx = LLVM.GetModuleContext(module);

            var strType = LLVM.StructCreateNamed(ctx, Helpers.StrToSByte(stmt.Name));

          
            

            symbolTable.Add(stmt.Name, new StructSymbol(stmt.Name, stmt.Variables));

            

            structTypes.Add(stmt.Name, strType);

            var fx = MVarTypes(stmt.Variables);

            fixed (LLVMOpaqueType** pptr = fx)
            {
                LLVM.StructSetBody(strType, pptr, (uint)fx.Length, 0);
            }
            var x = LLVM.PrintTypeToString(strType);

            var theName = Helpers.SByteToStr(x);
            theName = theName.Replace("%", "");
            theName = theName.Replace("*", "");


            Console.WriteLine(theName);
            symbolTable.Add(theName, new StructSymbol(stmt.Name, stmt.Variables));
            structTypes.Add(theName, strType);
            return stmt;
        }

        public unsafe BoundStatement VisitCast(BoundCastStatement stmt)
        {
            this.Visit(stmt.Expr);
            var x = valueStack.Pop();

            /*if (LLVM.TypeOf(x.Value) == LLVM.Int32Type() && stmt.Type == DataType.Float)
                LLVM.BuildCast(builder, LLVMOpcode.LLVMSIToFP, x.Value, DataTyToType(stmt.Type), StrToSByte("CastSIFP"));*/

            // add integer casting(only).
            
            return stmt;
        }

        public unsafe BoundStatement VisitFunction(BoundFunctionStatement node)
        {
            this.namedValues.Clear();



            
            var argumentCount = (uint)node.Parameters.Count;
            var arguments = new LLVMTypeRef[Math.Max(argumentCount, 1)];

            LLVMOpaqueValue* function = LLVM.GetNamedFunction(this.module, StrToSByte(node.Name));
            

            if (function != null)
            {
      
                if (LLVM.CountBasicBlocks(function) != 0)
                {
                    throw new Exception("redefinition of function.");
                }

    
                if (LLVM.CountParams(function) != argumentCount && !node.isVAARG)
                {
                    throw new Exception("redefinition of function with different # args");
                }
            }
            else
            {
                for (int i = 0; i < argumentCount; ++i)
                {
                    arguments[i] = LLVM.DoubleType();
                }
                fixed (LLVMOpaqueType** pptr = ParameterTypes(node.Parameters))
                {
                    if(node.isVAARG != true)
                        function = LLVM.AddFunction(this.module, StrToSByte(node.Name), LLVM.FunctionType(UnwrapType(node.Type), pptr, argumentCount, 0));
                    else
                        function = LLVM.AddFunction(this.module, StrToSByte(node.Name), LLVM.FunctionType(UnwrapType(node.Type), pptr, argumentCount, 1));
                }
                LLVM.SetLinkage(function, LLVMLinkage.LLVMExternalLinkage);
            }

            for (int i = 0; i < argumentCount; ++i)
            {
                string argumentName = node.Parameters[i].Name;

                LLVMOpaqueValue* param = LLVM.GetParam(function, (uint)i);
                LLVM.SetValueName(param, StrToSByte(argumentName));

                this.namedValues[argumentName] = param;
            }

            this.valueStack.Push(function);

            
            LLVMOpaqueValue* function2 = this.valueStack.Pop();

            LLVM.PositionBuilderAtEnd(this.builder, LLVM.AppendBasicBlock(function2, StrToSByte("entry")));

            try
            {
                foreach (var x in node.Body.Body)
                    this.Visit(x);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                LLVM.DeleteFunction(function2);
                throw;
            }

            if(UnwrapType(node.Type) == LLVM.VoidType())
            {
                LLVM.BuildRetVoid(builder);
            }

            LLVM.VerifyFunction(function2, LLVMVerifierFailureAction.LLVMPrintMessageAction);

            FPM.RunFunctionPassManager(function2);

            //this.valueStack.Push(function2);

            return node;
        }

        public unsafe BoundStatement VisitVariableDecl(BoundVariableDeclStatement stmt)
        {

            this.Visit(stmt.Value);
            var val = valueStack.Pop();

            var ptr = LLVM.BuildAlloca(builder, UnwrapType(stmt.Type), Helpers.StrToSByte("var_" + stmt.Name));

            LLVM.BuildStore(builder, val, ptr);

            varAllocaValues.Add(stmt.Name, ptr);
            namedValues.Add(stmt.Name, val);     

            return stmt;
        }

        public unsafe BoundStatement VisitReturn(BoundReturnStatement stmt)
        {
            this.Visit(stmt.Value);
            Pointer<LLVMOpaqueValue> popped;
            if(this.valueStack.TryPop(out popped))
                LLVM.BuildRet(this.builder, popped);
            else
            {
                LLVM.BuildRet(this.builder, LLVM.ConstInt(LLVM.Int32Type(), 1, 1));
            }
            return stmt;
        }
    }
}
