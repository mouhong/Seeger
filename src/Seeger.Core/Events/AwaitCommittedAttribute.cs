﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seeger.Events
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class AwaitCommittedAttribute : Attribute
    {
    }
}
