﻿using System.Collections.Generic;

namespace dotnetCampus.TelescopeTask.CompileTime
{
    internal class CompileMethod : CompileMember, ICompileMethod
    {
        /// <inheritdoc />
        public CompileMethod(IEnumerable<ICompileAttribute> attributes, string name) : base(name, attributes)
        {
        }
    }
}