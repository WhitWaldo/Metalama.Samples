﻿using System;
using System.Text;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Syntax;

public class LogAttribute : OverrideMethodAspect
{

    public override dynamic OverrideMethod()
    {
        // Build a formatting string.
        var methodName = BuildInterpolatedString();

        // Write entry message.
        var entryMessage = methodName.Clone();
        entryMessage.AddText(" started.");
        Console.WriteLine(entryMessage.ToInterpolatedString());

        try
        {
            // Invoke the method.
            dynamic result = meta.Proceed();

            // Display the success message.
            var successMessage = methodName.Clone();
            if (meta.Target.Method.ReturnType.Is(typeof(void)))
            {
                successMessage.AddText(" succeeded.");
            }
            else
            {
                successMessage.AddText(" returned ");
                successMessage.AddExpression(result);
                successMessage.AddText(".");
            }

            Console.WriteLine(successMessage.ToInterpolatedString());

            return result;
        }
        catch (Exception e)
        {
            // Display the failure message.
            var failureMessage = methodName.Clone();
            failureMessage.AddText(" failed: ");
            failureMessage.AddExpression(e.Message);
            Console.WriteLine(failureMessage.ToInterpolatedString());
            throw;
        }
    }

    private static InterpolatedStringBuilder BuildInterpolatedString()
    {
        var stringBuilder = InterpolatedStringBuilder.Create();
        stringBuilder.AddText(meta.Target.Type.ToDisplayString(CodeDisplayFormat.MinimallyQualified));
        stringBuilder.AddText(".");
        stringBuilder.AddText(meta.Target.Method.Name);
        stringBuilder.AddText("(");
        var i = meta.CompileTime(0);

        foreach (var p in meta.Target.Parameters)
        {
            var comma = i > 0 ? ", " : "";

            if (p.IsOut())
            {
                stringBuilder.AddText($"{comma}{p.Name} = <out> ");
            }
            else
            {
                stringBuilder.AddText($"{comma}{p.Name} = {{");
                stringBuilder.AddExpression(p.Value);
                stringBuilder.AddText("}");
            }

            i++;
        }
        stringBuilder.AddText(")");

        return stringBuilder;
    }
}