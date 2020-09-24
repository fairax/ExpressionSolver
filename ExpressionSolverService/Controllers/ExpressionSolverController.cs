using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExpressionSolverService.Controllers {
	[Route("api/expression_solver")]
	[ApiController]
	public class ExpressionSolverController : ControllerBase {
		[HttpGet]
		public object Get([FromServices] ExpressionsRepository repository, bool a, bool b, bool c, decimal? d, int? e, int? f) {
			try {
				var (type, result) = repository.CalculateResult(a, b , c, d, e, f);
				return new { IsError = false, Type = type, Result = result };
			} catch (ArgumentNullException) {
				return new { IsError = true, ErrorMessage = "Required parameter isn't set" };
			} catch (Exception ex) {
				return new { IsError = true, ErrorMessage = ex.Message };
			}
		}

		[HttpPost]
		public object Post([FromServices] ExpressionsRepository repository, [FromForm] string expression) {
			try {
				repository.SetExpression(expression);
				return new { IsError = false };
			} catch (ExpressionValidationError ex) {
				return new { IsError = true, ex.StartOffset, ex.Length };
			} catch (Exception ex) {
				return new { IsError = true, ErrorMessage = ex.Message };
			}
		}


	}
}
