namespace Dyject.Exceptions;

internal sealed class InvalidDependencyException : Exception
{
	public InvalidDependencyException() { }
	public InvalidDependencyException(string message) : base(message) { }
	public InvalidDependencyException(string message, Exception inner) : base(message, inner) { }
}