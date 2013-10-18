﻿using System;
using System.Collections.Generic;

namespace Fixie
{
    public class FailResult
    {
        public FailResult(Case @case, IReadOnlyList<Exception> exceptions)
        {
            Case = @case;
            Exceptions = exceptions;
        }

        public Case Case { get; private set; }
        public IReadOnlyList<Exception> Exceptions { get; private set; }
    }
}