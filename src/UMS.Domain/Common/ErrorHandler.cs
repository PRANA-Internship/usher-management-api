using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Domain.Common
{
    // they below two record and class
    // putted in  same folder because they work to gether
    // as common err handler for the global domain



    //this record work for void method domain validators
    public sealed record Error(string Code, string? Description = null)
    {
        public static readonly Error None = new(string.Empty);
    }



    //this work for method that return data
    public class Result<T>
    {
        public T? Value { get; }
        public Error Error { get; }
        public bool IsSuccess => Error == Error.None;

        private Result(T value)
        {
            Value = value;
            Error = Error.None;
        }

        private Result(Error error)
        {
            Value = default;
            Error = error;
        }

        public static Result<T> Success(T value) => new(value);
        public static Result<T> Failure(Error error) => new(error);
    }
}
