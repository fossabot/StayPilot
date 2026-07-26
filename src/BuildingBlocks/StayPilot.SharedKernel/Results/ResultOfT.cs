namespace StayPilot.SharedKernel.Results;

/// <summary>
/// The outcome of an operation that returns a value on success.
/// Implicit conversions let handlers <c>return value;</c> or <c>return error;</c>.
/// </summary>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error) => _value = value;

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failed result cannot be accessed.");

    public static implicit operator Result<TValue>(TValue value) => Success(value);

    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);
}
