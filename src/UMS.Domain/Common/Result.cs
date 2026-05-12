using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Domain.Common
{
    // they below two record and class
    // putted in  same folder because they work to gether
    // as common err handler for the global domain



    //this record work for void method domain validators

    public class Result
    {
        public Error Error { get; }
        public bool IsSuccess => Error == Error.None;
        protected Result(Error error)
        {
            Error = error;
        }
        public static Result Success() => new(Error.None);
        public static Result Failure(Error error) => new(error);

        public static implicit operator Result(Error error) => Failure(error);
    }



    //this work for method that return data
    public class Result<T>:Result
    {
        public T? Value { get; }
        private Result(T value) : base(Error.None)
        {
            Value = value;
        }

        private Result(Error error) : base(error)
        {
            Value = default;
        }


        public static Result<T> Success(T value) => new(value);
        public new static Result<T> Failure(Error error) => new(error);

        public static implicit operator Result<T>(Error error) => Failure(error);
    }
}
