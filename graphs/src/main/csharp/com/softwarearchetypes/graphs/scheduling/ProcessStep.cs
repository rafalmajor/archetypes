using System;

namespace com.softwarearchetypes.graphs.scheduling;

public sealed record class ProcessStep
{
    private readonly string nameValue;

    public ProcessStep(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Process step name cannot be null or blank");
        }

        nameValue = name;
    }

    public string name() => nameValue;
}
