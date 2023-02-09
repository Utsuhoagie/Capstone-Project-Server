﻿using Capstone.ExceptionHandling;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Capstone.ExceptionHandling
{
	public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
	{
		public int Order => int.MaxValue - 10;

		public void OnActionExecuting(ActionExecutingContext context) { }

		public void OnActionExecuted(ActionExecutedContext context)
		{
			if (context.Exception is HttpResponseException httpResponseException)
			{
				context.Result = new ObjectResult(httpResponseException.Value)
				{
					StatusCode = httpResponseException.StatusCode
				};
				context.ExceptionHandled = true;
			}
		}
	}
}
