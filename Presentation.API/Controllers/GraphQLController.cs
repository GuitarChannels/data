using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using GraphQL.Validation.Complexity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Presentation.API.GraphQL;

namespace Presentation.API.Controllers
{
	[Route("api/graphql")]
	[ApiController]
	public class GraphQlController : Controller
	{
		private readonly ILogger<GraphQlController> _logger;
		private readonly GraphQlQuery _graphQlQuery;
		private readonly IDocumentExecuter _documentExecuter;
		private readonly ISchema _schema;

		public GraphQlController(
			ILogger<GraphQlController> logger,
			GraphQlQuery graphQlQuery,
			IDocumentExecuter documentExecuter,
			ISchema schema)
		{
			_graphQlQuery = graphQlQuery ?? throw new ArgumentNullException(nameof(graphQlQuery));
			_documentExecuter = documentExecuter;
			_schema = schema;
			_logger = logger;
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody] IList<GraphQlParameter> queries)
		{
			var results = new List<ExecutionResult>();

			foreach (var query in queries)
			{
				var executionOptions = new ExecutionOptions
				{
					Schema = _schema,
					Query = query.Query,
					UserContext = User.Claims.ToDictionary(t => t.Type.ToString(), t => (object)t.Value),
					Inputs = JsonSerializer.Serialize(query.Variables).ToInputs(),
					ComplexityConfiguration = new ComplexityConfiguration { MaxDepth = 15 }
				};

				// execute and resolve the graphql query
				var result = await _documentExecuter
					.ExecuteAsync(executionOptions)
					.ConfigureAwait(false);

				if (result.Errors?.Count > 0)
				{
					// prepare error messages for logging and returning
					var errors = result.Errors.Select(e => e.ToString()).ToList();
					_logger.LogError(JsonSerializer.Serialize(errors));

					return BadRequest(errors);
				}

				results.Add(result);
			}

			return Ok(results);
		}
	}
}