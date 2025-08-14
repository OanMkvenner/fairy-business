using System;
using System.Collections.Generic;
using System.IO;
using Bus;
using Mono.Cecil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace SmoothOperator.Editor
{
    public class OperatorWeaver : WeaverBase
    {
        private static bool IL_DEBUG_ENABLED = false;
        protected override Result Process(AssemblyDefinition assembly)
        {
            var patcher = UnityObjectChecker.GetPatcher();
            var result = patcher.Process(assembly.MainModule, assembly.Name.Name, IL_DEBUG_ENABLED);
            return result ? Result.Success : Result.NoChanges;
        }
    }
    
    public class MyILPostProcessor : ILPostProcessor
    {
        private HashSet<string> userAssemblies;
        public override ILPostProcessor GetInstance() => this;

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            // call WillProcess because unity doesn't check it before calling Process
            if (!WillProcess(compiledAssembly))
            {
                return null;
            }
            var weaver = new OperatorWeaver();
            return weaver.Process(compiledAssembly);
        }

        public override bool WillProcess(ICompiledAssembly compiledAssembly) 
        { 
            if (userAssemblies == null)
            {
                CreateUserAssemblySet();
            }
            return userAssemblies.Contains(compiledAssembly.Name);
        }

        private void CreateUserAssemblySet ()
        {
            userAssemblies = new HashSet<string>();
            try
            {
                var userAssemblyPaths = Directory.GetFiles("Assets", "*.asmdef", SearchOption.AllDirectories);
                foreach (var userAssemblyPath in userAssemblyPaths)
                {
                    userAssemblies.Add(Path.GetFileNameWithoutExtension(userAssemblyPath));
                    var text = File.ReadAllText(userAssemblyPath);
                    var nameIndex = text.IndexOf("name", StringComparison.Ordinal);
                    if (nameIndex > 0)
                    {
                        nameIndex += 8;
                    }
                    var endQuote = text.IndexOf('"', nameIndex);
                    var name = text.Substring(nameIndex, endQuote - nameIndex);
                    userAssemblies.Add(name);
                }
            } 
            catch (Exception)
            {
                // ignored
            }
            userAssemblies.Add("Assembly-CSharp");
            userAssemblies.Add("Assembly-CSharp-Editor");
            userAssemblies.Add("Assembly-CSharp-firstpass");
            userAssemblies.Add("Assembly-CSharp-Editor-firstpass");
        }
    }
}