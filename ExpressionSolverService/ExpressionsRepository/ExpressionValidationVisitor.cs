using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using ExpressionParser;

namespace ExpressionSolverService {
	public class ExpressionValidationVisitor : ExpressionBaseVisitor<IList<ExpressionValidationError>> {
		protected override IList<ExpressionValidationError> DefaultResult => new List<ExpressionValidationError>();
		protected override IList<ExpressionValidationError> AggregateResult(IList<ExpressionValidationError> aggregate, IList<ExpressionValidationError> nextResult) {
			foreach (var item in nextResult)
				aggregate.Add(item);
			return aggregate;
		}
		public override IList<ExpressionValidationError> VisitChildren([NotNull] IRuleNode node) {
			if (node is ParserRuleContext parserRule && parserRule.exception is InputMismatchException ex)
				return new ExpressionValidationError[] {
					new ExpressionValidationError(ex.OffendingToken.StartIndex,  ex.OffendingToken.StopIndex - ex.OffendingToken.StartIndex + 1)
				};
			return base.VisitChildren(node);
		}

		public override IList<ExpressionValidationError> VisitErrorNode([NotNull] IErrorNode node) =>
			new ExpressionValidationError[] {
				new ExpressionValidationError(node.Symbol.StartIndex,  node.GetText().Length)
			};
	}

	public class ExpressionValidationError : Exception {
		public int StartOffset { get; }
		public int Length      { get; }
		public ExpressionValidationError(int startOffset, int length) {
			this.StartOffset = startOffset;
			this.Length      = length;
		}
	}
}
