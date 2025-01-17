﻿using Polly;

namespace Metalama.Samples.Retry5;

public interface IPolicyFactory
{
    Policy GetPolicy( PolicyKind policyKind );
    AsyncPolicy GetAsyncPolicy( PolicyKind policyKind );
}