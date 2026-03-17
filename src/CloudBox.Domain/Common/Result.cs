using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudBox.Domain.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        public static Result Success() => new(true, null);
        public static Result Failure(string error) => new(false, error);
        public static Result<T> Success<T>(T value) => new(value, true, null);
        public static Result<T> Failure<T>(string error) => new(default, false, error);

        protected Result(bool isSuccess, string? error)
        {
            if (isSuccess && error is not null)
            {
                throw new InvalidOperationException("Success result cannot have an error.");
            }

            if (!isSuccess && error is null)
            {
                throw new InvalidOperationException("Failure result must have an error.")
            }
        }
    }

    public class Result<T> : Result
    {
        private readonly T? _value;

        internal Result(T? value, bool isSuccess, string? error) : base(isSuccess, error) => _value = value;

        public T Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access value of a failed result.");
    }
}
