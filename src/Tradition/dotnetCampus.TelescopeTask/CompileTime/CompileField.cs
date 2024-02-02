using System.Collections.Generic;

namespace dotnetCampus.TelescopeTask.CompileTime
{
    internal class CompileField : CompileMember, ICompileField
    {
        /// <inheritdoc />
        public CompileField(string name, IEnumerable<ICompileAttribute> attributes) : base(name, attributes)
        {
        }
    }
}