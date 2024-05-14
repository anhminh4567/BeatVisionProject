using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Shared.Helper
{
    public class Result : Result<string>
    {
        public Result(string value) : base(value){ }

        public Result(Error error) : base(error){ }

        public static Result Success()
        {
            return new Result(string.Empty);
        }
        public static Result Fail(Error? error = null)
        {
            if(error == null)
                return new Result(new Error()
                {
                    ErrorMessage = "fail with no further detail",
                    StatusCode = (int) HttpStatusCode.BadRequest,
                });
            return new Result(error);
        }
    }
    public class Result<T> : Result<T, Error> //where T : object
    {
        public Result(T value) : base(value){ }

        public Result(Error error) : base(error){ }
        public static Result<T> Success(T value)
        {
            return new Result<T>(value);
        }
        public static Result<T> Fail(Error? error = null)
        {
            if( error == null)
            {
                return new Result<T>(new Error()
                {
                    ErrorMessage = "fail with no further detail",
                    StatusCode = (int) HttpStatusCode.BadRequest
                });
            }
            return new Result<T>(error);
        }
    }
    public class Result<TValue, TError>
    {
        public TValue? Value { get; set; }
        public TError? Error { get; set; }
        public bool isSuccess { get; set; }
        public Result(TValue value)
        {
            isSuccess = true;
            Value = value;
        }
        public Result(TError ErrorValue)
        {
            isSuccess = false;
            Error = ErrorValue;
        }
        public static Result<TValue, TError> Success(TValue value) => new Result<TValue, TError>(value);
        public static Result<TValue, TError> Fail(TError ErrorValue) => new Result<TValue, TError>(ErrorValue);
    }

}
