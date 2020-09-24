using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using ExpressionParser;

namespace ExpressionSolverService {

	using ExpressionParser = ExpressionParser.ExpressionParser;
	public class ComputeCaseValueVisitor : ExpressionBaseVisitor<decimal> {
		public decimal? D { get; }
		public int?     E { get; }
		public int?     F { get; }
		public ComputeCaseValueVisitor(decimal? d, int? e, int? f) {
			this.D = d;
			this.E = e;
			this.F = f;
		}

		public override decimal VisitCalculationVariable([NotNull] ExpressionParser.CalculationVariableContext context) {
			if (context.D() != null)
				return this.D ?? throw new ArgumentNullException();
			else if (context.E() != null)
				return this.E ?? throw new ArgumentNullException();
			else if (context.F() != null)
				return this.F ?? throw new ArgumentNullException();
			throw new NotSupportedException();
		}

		public override decimal VisitDecimalCalculationExpression([NotNull] ExpressionParser.DecimalCalculationExpressionContext context) =>
			Decimal.Parse(context.DECIMAL().GetText() ?? throw new NotSupportedException());

		public override decimal VisitUnaryMinusExpression([NotNull] ExpressionParser.UnaryMinusExpressionContext context) =>
			-context.simpleCalculationExpression().Accept(this);

		public override decimal VisitUnaryPlusExpression([NotNull] ExpressionParser.UnaryPlusExpressionContext context) =>
			context.simpleCalculationExpression().Accept(this);

		public override decimal VisitSummationExpression([NotNull] ExpressionParser.SummationExpressionContext context) =>
			context.priorityCalculationExpression().Accept(this) + context.calculationExpression().Accept(this);

		public override decimal VisitSubstractionExpression([NotNull] ExpressionParser.SubstractionExpressionContext context) =>
			context.priorityCalculationExpression().Accept(this) - context.calculationExpression().Accept(this);

		public override decimal VisitMultiplicationExpression([NotNull] ExpressionParser.MultiplicationExpressionContext context) =>
			context.unaryCalculationExpression().Accept(this) * context.priorityCalculationExpression().Accept(this);

		public override decimal VisitDivisionExpression([NotNull] ExpressionParser.DivisionExpressionContext context) =>
			context.unaryCalculationExpression().Accept(this) / context.priorityCalculationExpression().Accept(this);

		public override decimal VisitSimpleCalculationExpression([NotNull] ExpressionParser.SimpleCalculationExpressionContext context) =>
				context.calculationExpression()?.Accept(this)
			?? context.calculationVariable()?.Accept(this)
			?? context.decimalCalculationExpression().Accept(this);
	}
}
