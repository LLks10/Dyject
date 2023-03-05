using Dyject.Attributes;

namespace Dyject.Tests.TestData.Depender;

using Depender = Dyject.Depender;

public sealed class ShSiMain0 : Depender
{
    private readonly ShSiSi0 si0;
}

public sealed class ShSiMain1 : Depender
{
    private readonly ShSiSi0 si0;
    private readonly ShSiSi1 si1;
    private readonly ShSiSi2 si2;
}

[Injectable]
public class ShSiTr0 : Identifier
{

}


[Injectable(InjScope.Singleton)]
public class ShSiSi0 : Identifier
{
    private readonly ShSiTr0 tr0;
    private readonly ShSiSi2 si0;
}

[Injectable(InjScope.Singleton)]
public class ShSiSi1 : Identifier
{
    private readonly ShSiTr0 tr0;
    private readonly ShSiSi2 si0;
}

[Injectable(InjScope.Singleton)]
public class ShSiSi2 : Identifier
{
    private readonly ShSiTr0 tr0;
}