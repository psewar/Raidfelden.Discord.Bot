using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raidfelden.Services
{
    public class ServiceResponse<T> : ServiceResponse
    {
        public ServiceResponse(bool isSuccess, string message, T result, Dictionary<string, Func<Task<ServiceResponse>>> interActiveCallbacks = null) : base(isSuccess, message, interActiveCallbacks)
        {
            Result = result;
        }

	    public ServiceResponse(ServiceResponse serviceResponse, T result) : base(serviceResponse.IsSuccess, serviceResponse.Message, serviceResponse.InterActiveCallbacks)
	    {
		    Result = result;
		}

		public T Result { get; }
    }

    public class ServiceResponse
    {
        public ServiceResponse(bool isSuccess, string message, Dictionary<string, Func<Task<ServiceResponse>>> interActiveCallbacks = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            InterActiveCallbacks = interActiveCallbacks;
        }

        public bool IsSuccess { get; }
        public string Message { get; }
        public Dictionary<string, Func<Task<ServiceResponse>>> InterActiveCallbacks { get; }
    }
}