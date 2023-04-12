---
uid: sample-enrich-exception
level: 200
---

# Example: Enriching exceptions with parameter values

[!metalama-project-buttons .]

Call stacks, while essential for diagnosing software issues, can be less helpful without parameter values because they lack context. By only showing the sequence of method calls, they do not reveal the data being processed during execution. Parameter values provide insights into the application's state at the time of the error, allowing developers to pinpoint the root cause more easily. Without parameter values, developers may need to spend additional time recreating the issue and tediously tracing through code paths, as they cannot directly assess the impact of input data on the error from the call stack alone.

In this example, we will show how to include parameter values into the call stack. The idea is to add an exception handler to all non-trivial methods, to append context information to the current <xref:System.Exception>, and to rethrow the exception to the next stack frame.

Here is an example of a method that has been enhanced with such an exception handler:

[!metalama-compare Calculator.cs]

In the last-chance exception handler, you can now include the context information in the crash report:

[!metalama-file Program.cs]

This program produces the following output:

```text
System.ArgumentOutOfRangeException: Specified argument was out of the range of valid values. (Parameter 'n')
   at Calculator.Fibonaci(Int32 n) in Calculator.cs:line 8
   at Calculator.Fibonaci(Int32 n) in Calculator.cs:line 23
   at Calculator.Fibonaci(Int32 n) in Calculator.cs:line 23
   at Calculator.Fibonaci(Int32 n) in Calculator.cs:line 23
   at Calculator.Fibonaci(Int32 n) in Calculator.cs:line 23
   at Calculator.Fibonaci(Int32 n) in Calculator.cs:line 23
   at Program.Main() in Program.cs:line 7
---with---
Calculator.Fibonaci(-1)
Calculator.Fibonaci(1)
Calculator.Fibonaci(2)
Calculator.Fibonaci(3)
Calculator.Fibonaci(4)
Calculator.Fibonaci(5)
----------
```

As you can see, parameter values are now included in the crash report.

## Infrastructure code

Let's see how it works.

The exception handler calls the helper class `EnrichExceptionHelper`:

[!metalama-file EnrichExceptionHelper.cs]


## Aspect code

The `EnrichExceptionAttribute` aspect is responsible for adding the exception handler to methods:

[!metalama-file EnrichExceptionAttribute.cs]

Most of the code in this aspect builds an interpolated string including the method name and its parameters. We have commented on this technique in detail in <xref:sample-log-3>.

## Fabric code

However, it would be highly cumbersome to add the aspect to all methods by hand, as a custom attribute. Instead, we are using a fabric, which adds the exception handler to all public methods of all public types. Note that we exclude the `ToString` method to avoid infinite recursions. For the same reason, we have excluded the aspect from the `EnrichExceptionHelper.AppendContextFrame` method.

[!metalama-file Fabric.cs]


> [!WARNING]
> Including sensitive information (e.g., user credentials, personal data, etc.) in logs can pose a security risk. Be cautious when adding parameter values to logs and avoid exposing sensitive data.
> To remove sensitive information from the logs, see <xref:sample-log-7>