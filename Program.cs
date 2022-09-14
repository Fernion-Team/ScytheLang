using Scythe;
using System.Linq;
using ClangSharp;
using ClangSharp.Interop;
using LLVMSharp;
using LLVMSharp.Interop;

using Yoakke.SynKit.Lexer;
partial class Program
{
    public unsafe static void ScytheCLI(string option, params string[] extraOptions)
    {
        if (option == "build")
        {
            bool JIT = false;
            bool Intrp = false;
            var SymbolTable = new Dictionary<string, Symbol>();

            List<string> linkByteCodeFiles = new List<string>();
            List<string> linkStaticLibraryFiles = new List<string>();

            var xz = DateTime.Now;
            Lexer lx = new Lexer(File.ReadAllText(extraOptions[0]));
            Parser ps = new Parser(lx);
            var program = ps.ParseProgram();

            if (program.IsOk)
            {
                unsafe
                {
                    string fileName = "";
                    if (extraOptions.Length > 1)
                    {
                        if (extraOptions.Contains("--modules"))
                        {
                            /*Console.WriteLine(extraOptions.ToList().IndexOf("--modules"));
                            for(int f = 0; f < extraOptions.ToList().IndexOf("--modules"); f++)
                            {
                                var x = extraOptions[f];
                                Console.WriteLine(x);
                                if (x.EndsWith(".bc"))
                                {
                                    linkByteCodeFiles.Add(x);
                                }
                            }*/

                            int sz = extraOptions.ToList().IndexOf("--modules");
                            int i = sz+1;

                            while (extraOptions.Length > i)
                            {
                                if (extraOptions[i].EndsWith(".bc"))
                                {
                                    linkByteCodeFiles.Add(extraOptions[i]);
                                }
                                i++;
                            }


                        }

                        if (extraOptions.Contains("--statlib"))
                        {

                            int sz = extraOptions.ToList().IndexOf("--statlib");
                            int i = sz + 1;

                            while (extraOptions.Length > i)
                            {
                                if (extraOptions[i].EndsWith(".a") || extraOptions[i].EndsWith(".lib"))
                                {
                                    linkStaticLibraryFiles.Add(extraOptions[i]);
                                }
                                else if (extraOptions[i].EndsWith(".dll") || extraOptions[i].EndsWith(".so"))
                                {
                                    Console.WriteLine("--statlib is for loading *static* libraries, if you want to load dynamic libraries, use --dylib.");
                                }
                                i++;
                            }


                        }

                        if (extraOptions.Contains("--jit"))
                        {
                            JIT = true;
                        }

                        if (extraOptions.Contains("--int"))
                        {
                            Intrp = true;
                        }

                        if (extraOptions.Contains("--wasm"))
                        {
                            LLVM.InitializeWebAssemblyTargetInfo();
                            LLVM.InitializeWebAssemblyTarget();
                            LLVM.InitializeWebAssemblyTargetMC();
                            LLVM.InitializeWebAssemblyAsmParser();
                            LLVM.InitializeWebAssemblyAsmPrinter();
                            fileName = "out/" + extraOptions[1] + ".wasm";
                        }

                        if (extraOptions.Contains("--nat"))
                        {
                            LLVM.InitializeAllTargetInfos();
                            LLVM.InitializeAllTargets();
                            LLVM.InitializeAllTargetMCs();
                            LLVM.InitializeAllAsmParsers();
                            LLVM.InitializeAllAsmPrinters();
                        }

                        if (extraOptions.Contains("--agpu"))
                        {
                            LLVM.InitializeAMDGPUTargetInfo();
                            LLVM.InitializeAMDGPUTarget();
                            LLVM.InitializeAMDGPUTargetMC();
                            LLVM.InitializeAMDGPUAsmParser();
                            LLVM.InitializeAMDGPUAsmPrinter();
                        }
                    }
                    

                    fileName = "out/" + Path.GetFileNameWithoutExtension(extraOptions[0]) + ".o";
                    Directory.CreateDirectory("out");

                    var triple = LLVM.GetDefaultTargetTriple();

                    sbyte* Error;

                    LLVMModuleRef module = LLVM.ModuleCreateWithName(Helpers.StrToSByte(extraOptions[1]));

                    if (linkByteCodeFiles.Count > 0)
                    {
                        foreach (var lbc in linkByteCodeFiles)
                        {
                            LLVMOpaqueModule* l_mod;
                            LLVMOpaqueMemoryBuffer* l_mbf;
                            sbyte* outErr;

                            LLVM.CreateMemoryBufferWithContentsOfFile(Helpers.StrToSByte(lbc), &l_mbf, &outErr);
                            //LLVM.GetBitcodeModule2(l_mbf, &l_mod);
                            sbyte* outErrBC;
                            LLVM.ParseBitcode(l_mbf, &l_mod, &outErrBC);
                            LLVM.LinkModules2(module, l_mod);
                        }
                    }

                    LLVMBuilderRef builder = LLVM.CreateBuilder();

                    //LLVM.LoadLibraryPermanently(Helpers.StrToSByte("raylib.dll"));

                    var CodeGenVisit = new Scythe.CodeGen.CodeGenVisitor(module, builder, SymbolTable);
                    foreach (var x in new Binder().Bind(program.Ok.Value.ToList()))
                    {
                        var y = CodeGenVisit.Visit(x);
                    }

                    LLVMTarget* target;

                    var _Target = LLVM.GetTargetFromTriple(triple, &target, &Error);

                    if (target == null)
                    {
                        Console.WriteLine("[NATGEN]: Error while creating target, target is null.\nError Code(Sbyte*):");
                        Console.WriteLine(*Error);
                        return;
                    }

                    var tm = LLVM.CreateTargetMachine(target, triple, Helpers.StrToSByte("generic"), Helpers.StrToSByte(""), LLVMCodeGenOptLevel.LLVMCodeGenLevelAggressive, LLVMRelocMode.LLVMRelocDefault, LLVMCodeModel.LLVMCodeModelDefault);

                    LLVM.SetDataLayout(module, LLVM.CopyStringRepOfTargetData(LLVM.CreateTargetDataLayout(tm)));

                    LLVM.SetTarget(module, triple);

                   

                    LLVMCodeGenFileType flType = LLVMCodeGenFileType.LLVMObjectFile;


                    if (JIT)
                    {
                        sbyte* err;
                        var execJIT = module.CreateMCJITCompiler();
                        var mainFn = execJIT.FindFunction("main");
                        LLVM.ExecutionEngineGetErrMsg(execJIT, &err);
                        if(err != null)
                            Console.WriteLine("[FATAL][JIT]: " + Helpers.SByteToStr(err));
                        execJIT.RunFunction(mainFn, new LLVMGenericValueRef[] { });
                    }

                    else if(Intrp)
                    {
                        sbyte* err;
                        var execINT = module.CreateInterpreter();
                        var mainFn = execINT.FindFunction("main");
                        LLVM.ExecutionEngineGetErrMsg(execINT, &err);
                        if (err != null)
                            Console.WriteLine("[FATAL][INTERPRETER]: " + Helpers.SByteToStr(err));
                        execINT.RunFunction(mainFn, new LLVMGenericValueRef[] { });
                    }

                    else
                    {
                        //module.Dump();

                        sbyte* error;

                        LLVM.TargetMachineEmitToFile(tm, module, Helpers.StrToSByte("out/" + extraOptions[1] + ".o"), flType, &error);

                        System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();

                        info.Arguments = "out/" + extraOptions[1] + ".o -o " + "out/" + extraOptions[1] + ".exe";
                        info.CreateNoWindow = true;
                        info.FileName = "clang";

                        System.Diagnostics.Process.Start(info);

                        //module.PrintToFile("out/" + extraOptions[1] + ".ll");
                    }

                    

                    Console.WriteLine("\nDone in " + (DateTime.Now - xz).TotalMilliseconds + " ms.");
                }
            }
            else
            {
                var err = program.Error;
                foreach (var element in err.Elements.Values)
                {
                    Console.WriteLine($"  expected {string.Join(" or ", element.Expected)} while parsing {element.Context}");
                }
                Console.WriteLine($"  but got {(err.Got == null ? "end of input" : err.Got)}");
            }
        }
        if(option == "new")
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Creating a new Scythe project...");
            Console.ResetColor();
            var project = new ScytheProject(extraOptions[0], "0.1", 1.0f, null, "x64");
            Directory.CreateDirectory(extraOptions[0]);
            project.GenerateProjectFile(extraOptions[0]);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Created Project "+extraOptions[0]+"!");
            Console.ResetColor();
        }
        if(option == "arch")
        {
            if(extraOptions.Length > 0)
            {
                var syproj = ScytheProject.GenerateProjectClassFromFile(Directory.GetFiles(Directory.GetCurrentDirectory()).First(e => e.EndsWith(".syproj")));

                Console.WriteLine($"NAME: {syproj.ProjectName}\nPROJECT_VERSION: {syproj.Version}\nLANGUAGE_VERSION: {syproj.LanguageVersion}\nARCH: {syproj.TargetArch}");

                syproj.TargetArch = extraOptions[0];

                syproj.GenerateProjectFile(Directory.GetCurrentDirectory());
            }
            else
            {
                Console.WriteLine("You need to choose an architecture to set for your project.\n\n\tOptions:\nx64\nx86\narm\naarch64\nwasm");
            }
        }

        if (option == "migrate")
        {
            if (extraOptions.Length > 0)
            {
                var syproj = ScytheProject.GenerateProjectClassFromFile(Directory.GetFiles(Directory.GetCurrentDirectory()).First(e => e.EndsWith(".syproj")));

                syproj.LanguageVersion = float.Parse(extraOptions[0]);

                syproj.GenerateProjectFile(Directory.GetCurrentDirectory());
            }
            else
            {
                Console.WriteLine("Choose a language version to migrate to.");
            }
        }
    }

    public static ScytheProject CollectProjectFile()
    {
        var projPath = Directory.GetFiles(Directory.GetCurrentDirectory()).Single(e => e.EndsWith(".syproj"));

        return ScytheProject.GenerateProjectClassFromFile(projPath);
    }
    
    public unsafe static void ScytheCLI2(string option, params string[] extraOptions)
    {
        var now = DateTime.Now;
        switch(option)
        {
            case "build":
                var proj = CollectProjectFile();

                /* FLAGS */

                bool nonRecursive = false;
                bool leaveLL = false;

                if(extraOptions.Length > 0)
                {
                    if(extraOptions.Contains("-nr"))
                    {
                        nonRecursive = true;
                    }

                    if (extraOptions.Contains("-dbg"))
                    {
                        leaveLL = true;
                    }
                }

                string[] CompileFiles;

                if(nonRecursive == false)
                {
                    CompileFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sy", SearchOption.AllDirectories);
                }
                else
                {
                    CompileFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sy", SearchOption.TopDirectoryOnly);
                }

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Creating directories(if not created yet.)");

                Directory.CreateDirectory("bin");
                Directory.CreateDirectory("obj");

                Console.WriteLine("Setting up LLVM targets.");

                // TARGET INITIALIZATION:

                LLVM.InitializeAllTargetInfos();
                LLVM.InitializeAllTargets();
                LLVM.InitializeAllTargetMCs();
                LLVM.InitializeAllAsmParsers();
                LLVM.InitializeAllAsmPrinters();

                Console.WriteLine("Starting Build Process.");

                var bldr = LLVM.CreateBuilder();

                Console.ResetColor();

                foreach (var bldFile in CompileFiles)
                {
                    var lexer = new Lexer(File.ReadAllText(bldFile));
                    var parser = new Parser(lexer);

                    var result = parser.ParseProgram();

                    if (result.IsOk)
                    {
                        var module = LLVM.ModuleCreateWithName(Helpers.StrToSByte(Path.GetFileName(bldFile)));

                        var CGV = new Scythe.CodeGen.CodeGenVisitor(module, bldr, new Dictionary<string, Symbol>());

                        var Binder = new Scythe.Binder();

                        var listRes = result.Ok.Value.ToList();

                        foreach (var node in Binder.Bind(listRes))
                        {
                            try
                            {
                                CGV.Visit(node);
                            }
                            catch
                            {
                                if (leaveLL)
                                    LLVM.DumpModule(module);
                            }
                        }

                        sbyte* error;

                        LLVMTarget* target;

                        var triple = LLVM.GetDefaultTargetTriple();

                        var _Target = LLVM.GetTargetFromTriple(triple, &target, &error);

                        var tm = LLVM.CreateTargetMachine(target, triple, Helpers.StrToSByte("generic"), Helpers.StrToSByte(""), LLVMCodeGenOptLevel.LLVMCodeGenLevelAggressive, LLVMRelocMode.LLVMRelocDefault, LLVMCodeModel.LLVMCodeModelDefault);

                        LLVM.SetDataLayout(module, LLVM.CopyStringRepOfTargetData(LLVM.CreateTargetDataLayout(tm)));

                        LLVM.SetTarget(module, triple);

                        LLVMCodeGenFileType flType = LLVMCodeGenFileType.LLVMObjectFile;

                        sbyte* _err;

                        LLVM.TargetMachineEmitToFile(tm, module, Helpers.StrToSByte("obj/"+Path.GetFileNameWithoutExtension(bldFile) + ".o"), flType, &_err);

                        if (leaveLL)
                        {
                            Console.WriteLine("Putting out LLVM code.");

                            sbyte* err;

                            LLVM.PrintModuleToFile(module, Helpers.StrToSByte(Path.GetFileNameWithoutExtension(bldFile) + ".ll"), &err);

                            if (err != null)
                            {
                                Console.WriteLine("Error occured while printing out module to file: " + Helpers.SByteToStr(err));
                            }
                        }
                        Console.WriteLine(Path.GetFileName(bldFile) + " -> " + Path.GetFileNameWithoutExtension(bldFile) + ".o");
                    }
                    else
                    {
                        var err = result.Error;

                        Console.Clear();
                        Console.WriteLine("Encountered an error while parsing file " + Path.GetRelativePath(Directory.GetCurrentDirectory(), bldFile));
                        Console.ForegroundColor = ConsoleColor.Red;
                        foreach (var element in err.Elements.Values)
                        {
                            Console.WriteLine($"  expected {string.Join(" or ", element.Expected)} while parsing {element.Context}");
                        }
                        Console.WriteLine($"  but got {(err.Got == null ? "end of input" : err.Got)}");

                        Console.ResetColor();

                        Console.WriteLine("Compiling Failed.");

                        Environment.Exit(-45);
                    }
                }
                break;
            case "clear":
                string[] objectFiles = Directory.GetFiles(Directory.GetCurrentDirectory()+"/obj", "*.o", SearchOption.AllDirectories);
                string[] llvmFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.ll", SearchOption.AllDirectories);

                foreach (var obj in objectFiles)
                    File.Delete(obj);
                foreach (var llvm in llvmFiles)
                    File.Delete(llvm);

                break;
            case "new":

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Creating a new Scythe project...");
                Console.ResetColor();
                var project = new ScytheProject(extraOptions[0], "0.1", 1.0f, null, "x64");
                Directory.CreateDirectory(extraOptions[0]);
                project.GenerateProjectFile(extraOptions[0]);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Created Project " + extraOptions[0] + "!");
                Console.ResetColor();

                break;
        }

        var end = DateTime.Now;

        Console.WriteLine("Operations took " + (end - now).Milliseconds + " ms.");
    }

    public static void Main(string[] args)
    {
        if (args.Length >= 1)
        {
            /// ArgsList contains all of the args, then firstElement is set to the first element, and it is then removed from argsList so it's just the rest of the elements.
            List<string> argsList = args.ToList();
            string firstElement = argsList.First();
            argsList.Remove(firstElement);

            ScytheCLI2(firstElement, argsList.ToArray());
        }
    }
}
