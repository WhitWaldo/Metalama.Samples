﻿// This is an open-source Metalama example. See https://github.com/postsharp/Metalama.Samples for more.

using Metalama.Framework.Aspects;
using System;

public class RetryAttribute : OverrideMethodAspect
{
    public int Attempts { get; set; } = 5;

    public override dynamic? OverrideMethod()
    {
        for ( var i = 0;; i++ )
        {
            try
            {
                return meta.Proceed();
            }
            catch ( Exception e ) when ( i < this.Attempts )
            {
                Console.WriteLine( e.Message + " Retrying." );
            }
        }
    }
}