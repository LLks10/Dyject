using Dyject.Attributes;

namespace Dyject.Tests.TestData.Depender;

using Depender = Dyject.Depender;

public sealed class Circ : Depender
{
    private readonly CircTrA trA;
}

[Injectable]
public class CircTrA
{
    private readonly CircTrB trB;
}

[Injectable]
public class CircTrB
{
    private readonly CircTrC trC;
}

[Injectable]
public class CircTrC
{
    private readonly CircTrA trA;
}