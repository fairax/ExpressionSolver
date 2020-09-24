using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using ExpressionParser;

namespace ExpressionSolverService {

	using ExpressionParser = ExpressionParser.ExpressionParser;
	public class ComputeConditionVisitor : ExpressionBaseVisitor<bool> {
		public bool? A { get; }
		public bool? B { get; }
		public bool? C { get; }
		public ComputeConditionVisitor(bool? a, bool? b, bool? c) {
			this.A = a;
			this.B = b;
			this.C = c;
		}

		public override bool VisitLogicalVariable([NotNull] ExpressionParser.LogicalVariableContext context) {
			if (context.A() != null)
				return this.A ?? throw new ArgumentNullException();
			else if (context.B() != null)
				return this.B ?? throw new ArgumentNullException();
			else if (context.C() != null)
				return this.C ?? throw new ArgumentNullException();
			throw new NotSupportedException();
		}

		public override bool VisitLogicalAndExpression([NotNull] ExpressionParser.LogicalAndExpressionContext context) =>
			 context.unaryLogicalExpression(0) .Accept(this) &&
			(context.unaryLogicalExpression(1)?.Accept(this) ?? context.logicalAndExpression().Accept(this));

		public override bool VisitLogicalOrExpression([NotNull] ExpressionParser.LogicalOrExpressionContext context) =>
			context.unaryLogicalExpression().Accept(this) || context.logicalExpression().Accept(this);

		public override bool VisitLogicalNotExpression([NotNull] ExpressionParser.LogicalNotExpressionContext context) =>
			!context.simpleLogicalExpression().Accept(this);

		public override bool VisitSimpleLogicalExpression([NotNull] ExpressionParser.SimpleLogicalExpressionContext context) =>
			context.logicalExpression()?.Accept(this) ?? context.logicalVariable().Accept(this);
	}
}
