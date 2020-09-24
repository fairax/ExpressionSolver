using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using ExpressionParser;
using NUnit.Framework;

namespace ExpressionSolverService {

	using ExpressionParser = ExpressionParser.ExpressionParser;
	public class ExpressionsRepository {
		public ExpressionParser.LogicalExpressionContext     CaseMConditionExpression { get; private set; }
		public ExpressionParser.LogicalExpressionContext     CasePConditionExpression { get; private set; }
		public ExpressionParser.LogicalExpressionContext     CaseTConditionExpression { get; private set; }

		public ExpressionParser.CalculationExpressionContext CaseMActionExpression    { get; private set; }
		public ExpressionParser.CalculationExpressionContext CasePActionExpression    { get; private set; }
		public ExpressionParser.CalculationExpressionContext CaseTActionExpression    { get; private set; }

		public ExpressionsRepository() {
			this.CaseMConditionExpression = ParseCaseConditionExpression("A && B && !C");
			this.CasePConditionExpression = ParseCaseConditionExpression("A && B && C" );
			this.CaseTConditionExpression = ParseCaseConditionExpression("!A && B && C");

			this.CaseMActionExpression = ParseCaseActionExpression("D + (D * E / 10)"        );
			this.CasePActionExpression = ParseCaseActionExpression("D + (D * (E - F) / 25.6)");
			this.CaseTActionExpression = ParseCaseActionExpression("D - (D * F / 30)"        );
		}

		public (string, decimal) CalculateResult(bool? a, bool? b, bool? c, decimal? d, int? e, int? f) {
			var (caseAction, type) = ComputeLogicalExpression(this.CaseMConditionExpression, a, b, c) ? (this.CaseMActionExpression, "M")
			                       : ComputeLogicalExpression(this.CasePConditionExpression, a, b, c) ? (this.CasePActionExpression, "P")
						           : ComputeLogicalExpression(this.CaseTConditionExpression, a, b, c) ? (this.CaseTActionExpression, "T")
						           : throw new Exception("No suitable expression found");
			return (type, ComputeCalculationExpression(caseAction, d, e, f));
		}

		public bool SetExpression(string expression) {
			var input = ParseInputExpression(expression);
			var error = ValidateInputExpression(input).FirstOrDefault();
			if (error != null)
				throw error;
			var root = input.GetChild<ParserRuleContext>(0);
			if (root is ExpressionParser.CaseConditionExpressionContext caseCondition) {
				var caseExpression = caseCondition.caseExpression();
				if (caseExpression.M() != null)
					this.CaseMConditionExpression = caseCondition.logicalExpression();
				else if (caseExpression.P() != null)
					this.CasePConditionExpression = caseCondition.logicalExpression();
				else if (caseExpression.T() != null)
					this.CaseTConditionExpression = caseCondition.logicalExpression();
			} else if (root is ExpressionParser.CaseActionExpressionContext caseAction) {
				var caseExpression = caseAction.caseExpression();
				if (caseExpression.M() != null)
					this.CaseMActionExpression = caseAction.calculationExpression();
				else if (caseExpression.P() != null)
					this.CasePActionExpression = caseAction.calculationExpression();
				else if (caseExpression.T() != null)
					this.CaseTActionExpression = caseAction.calculationExpression();
			}
			return true;
		}

		private static bool ComputeLogicalExpression(ExpressionParser.LogicalExpressionContext context, bool? a, bool? b, bool? c) =>
			context.Accept(new ComputeConditionVisitor(a, b, c));

		private static decimal ComputeCalculationExpression(ExpressionParser.CalculationExpressionContext context, decimal? d, int? e, int? f) =>
			context.Accept(new ComputeCaseValueVisitor(d, e, f));

		private static IList<ExpressionValidationError> ValidateInputExpression(ExpressionParser.InputContext context) =>
			context.Accept(new ExpressionValidationVisitor());

		private ExpressionParser.InputContext ParseInputExpression(string expression) =>
			ParseExpression(expression).input();

		private ExpressionParser.LogicalExpressionContext ParseCaseConditionExpression(string expression) =>
			ParseExpression(expression).logicalExpression();

		private ExpressionParser.CalculationExpressionContext ParseCaseActionExpression(string expression) =>
			ParseExpression(expression).calculationExpression();

		private ExpressionParser ParseExpression(string expression) {
			var lexer  = new ExpressionLexer(new AntlrInputStream(expression));
			return new ExpressionParser(new CommonTokenStream(lexer));
		}

		#if DEBUG
		[TestFixture]
		private class Tests {
			[Test]
			[TestCase(" A &&  B && !C  => H = M", true, true, false, ExpectedResult = true )]
			[TestCase(" A &&  B &&  C  => H = P", true, true, false, ExpectedResult = false)]
			[TestCase("!A &&  B &&  C  => H = T", true, true, false, ExpectedResult = false)]
			[TestCase(" A &&  B && !C  => H = T", true, true, false, ExpectedResult = true )]
			[TestCase(" A && !B &&  C  => H = M", true, true, false, ExpectedResult = false)]
			[TestCase(" A && (B && !C) => H = M", true, true, false, ExpectedResult = true )]
			[TestCase(" A && (B ||  C) => H = M", true, true, false, ExpectedResult = true )]
			public bool CaseConditionExpressionTest(string expression, bool a, bool b, bool c) {
				var repository = new ExpressionsRepository();
				var input = repository.ParseInputExpression(expression);
				var caseCondition = input.caseConditionExpression();
				Assert.NotNull(caseCondition);
				Assert.IsNull(input.caseActionExpression());
				Assert.IsTrue(ValidateInputExpression(input).Count == 0);

				return ComputeLogicalExpression(caseCondition.logicalExpression(), a, b, c);
			}

			[Test]
			[TestCase("A &&   B && !C => bad  ")]
			[TestCase("A &&   B && !C => BAD  ")]
			[TestCase("A && (!B &&  C => H = M")]
			[TestCase("A &&   B && !C =>      ")]
			[TestCase("bad A && (B ||  C) => H = M")]
			[TestCase("A & B => H = M")]
			public void ParseCaseConditionExpressionFailTest(string expression) {
				var repository = new ExpressionsRepository();
				var input = repository.ParseInputExpression(expression);
				var errors = ValidateInputExpression(input);
				Assert.IsTrue(errors.Count > 0);
				foreach (var error in errors) {
					Assert.IsTrue(
						   error.StartOffset >= 0 && error.StartOffset < expression.Length && error.Length > 0
						|| error.StartOffset == expression.Length && error.Length == 0
					);
				}
			}

			private static TestCaseData[] CaseActionTestCases =  {
				 new TestCaseData("H = M => K = D + (D * E / 10)",         1m,   2, 3) { ExpectedResult = 1.2m       }
				,new TestCaseData("H = M => K = D + (D * E / 10)",         1.1m, 2, 3) { ExpectedResult = 1.32m      }
				,new TestCaseData("H = P => K = D + (D * (E - F) / 25.6)", 1m,   2, 3) { ExpectedResult = 0.9609375m }
				,new TestCaseData("H = T => K = D - (D * F / 30)",         1m,   2, 3) { ExpectedResult = 0.9m       }
				,new TestCaseData("H = P => K = 2 * D + (D * E / 100)",    1m,   2, 3) { ExpectedResult = 2.02m      }
				,new TestCaseData("H = M => K = F + D + (D * E / 100)",    1m,   2, 3) { ExpectedResult = 4.02m      }
				,new TestCaseData("H = M => K = D + E * F",                1m,   2, 3) { ExpectedResult = 7m         }
				,new TestCaseData("H = M => K = E * F + D",                1m,   2, 3) { ExpectedResult = 7m         }
				,new TestCaseData("H = M => K = + E * (F + D)",            1m,   2, 3) { ExpectedResult = 8m         }
			};
			
			//Can't use TestCase attribute due to limitation on using decimals in attribute parameters
			[Test, TestCaseSource("CaseActionTestCases")]
			public decimal CaseActionExpressionTest(string expression, decimal d, int e, int f) {
				var repository = new ExpressionsRepository();
				var input = repository.ParseInputExpression(expression);
				var caseAction = input.caseActionExpression();
				Assert.NotNull(caseAction);
				Assert.IsNull(input.caseConditionExpression());
				Assert.IsTrue(ValidateInputExpression(input).Count == 0);

				return ComputeCalculationExpression(caseAction.calculationExpression(), d, e ,f);
			}

			[Test]
			[TestCase("      => K = D + (D * E / 10)")]
			[TestCase("KED   => K = D + (D * E / 10)")]
			[TestCase("H = M =>     D + (D * E / 10)")]
			[TestCase("H = M => K = D + (D * E / 10")]
			[TestCase("H = M => K = * D + D * E / 10")]
			[TestCase("H = M => K = D + D * E / 10 + ")]
			[TestCase("H = M => K = D + D * E / 10 + ")]
			[TestCase("H = M => K = ")]
			[TestCase("H = M =>")]
			[TestCase("H = M => K = D bad")]
			[TestCase("H = M => K = D + A")]
			[TestCase("H = M => K = D H = M => K = E")]
			public void ParseCaseActionExpressionFailTest(string expression) {
				var repository = new ExpressionsRepository();
				var input = repository.ParseInputExpression(expression);
				var errors = ValidateInputExpression(input);
				Assert.IsTrue(errors.Count > 0);
				foreach (var error in errors) {
					Assert.IsTrue(
						   error.StartOffset >= 0 && error.StartOffset < expression.Length && error.Length > 0
						|| error.StartOffset == expression.Length && error.Length == 0
					);
				}
			}

			[Test]
			public void ExpressionSolvingTest() {
				var repository = new ExpressionsRepository();
				Assert.AreEqual(("M", 1.2m), repository.CalculateResult(true, true, false, 1, 2, 3));

				repository.SetExpression("A &&  B &&  !C  => H = T");
				Assert.AreEqual(("M", 1.2m), repository.CalculateResult(true, true, false, 1, 2, 3));

				repository.SetExpression(" A &&  B &&  C  => H = M");
				Assert.AreEqual(("T", 0.9m), repository.CalculateResult(true, true, false, 1, 2, 3));

				repository.SetExpression("A &&  B &&  !C  => H = P");
				Assert.AreEqual(("P", 0.9609375m), repository.CalculateResult(true, true, false, 1, 2, 3));

				repository.SetExpression("H = M => K = D + E * F");
				Assert.AreEqual(("P", 0.9609375m), repository.CalculateResult(true, true, false, 1, 2, 3));

				repository.SetExpression("H = T => K = E * F + D");
				Assert.AreEqual(("P", 0.9609375m), repository.CalculateResult(true, true, false, 1, 2, 3));

				repository.SetExpression("H = P => K = F + D + (D * E / 100)");
				Assert.AreEqual(("P", 4.02m), repository.CalculateResult(true, true, false, 1, 2, 3));

				repository.SetExpression("A &&  B &&  !C  => H = T");
				Assert.AreEqual(("P", 4.02m), repository.CalculateResult(true, true, false, 1, 2, 3));

				repository.SetExpression("A &&  B &&   C  => H = P");
				Assert.AreEqual(("T", 7m), repository.CalculateResult(true, true, false, 1, 2, 3));

				repository.SetExpression("A &&  B &&  !C  => H = M");
				Assert.AreEqual(("M", 7m), repository.CalculateResult(true, true, false, 1, 2, 3));
			}
		}
		#endif
	}
}
