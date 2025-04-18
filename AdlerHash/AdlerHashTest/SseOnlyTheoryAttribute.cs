using System;
using System.Runtime.Intrinsics.X86;
using Xunit;

namespace AdlerHashTest;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class SseOnlyTheoryAttribute : TheoryAttribute
{
    public SseOnlyTheoryAttribute(string reason = "Will not run on non-SSE hardware")
    {
        if (Ssse3.IsSupported == false)
        {
            Skip = reason;
        }
    }
}