# Expression Solver

Application solves set of expressions for a given inputs. It is implemented as web application. Web UI allows to enter inputs A, B, C, D, E, F to solve for outputs H and K. Also it allows to enter new custom expression to add it to the set of expressions.

## Implementation ##

I used C# to implement the application because I'm most familiar with the language.

I chose to use .NET Core as a framework because it is modern and cross-platform.

I wanted to create simple UI just to access web API so it is implemented as static HTML page and uses ajax requests to access web API.

Web API is implemented using ASP.NET Core MVC. This framework is very similar to ASP.NET MVC that I used in other projects also it is flexible and allows to implement web API with minimal boilerplate code.

To parse expressions I'm using ANTLR4. This tool allows to generate parser based on grammar. When parser is given expression it produces an AST and with AST there is a lot of flexibility. It can be used to get detailed syntax error messages, to generate objects to do computations or to generate code that can be compiled.

Code quality verification is done through unit testing with NUnit.

## Structure ##

Frontend is implemented in  ExpressionSolver/ExpressionSolverService/wwwroot/index.html.

Web API in  ExpressionSolver/ExpressionSolverService/Controllers/ExpressionSolverController.cs

Business logic in  ExpressionSolver/ExpressionSolverService/ExpressionsRepository/ExpressionsRepository.cs

I didn't implemented persistance for expressions so there is no separate layer for their storage and ExpressionsRepository.cs is responsible for storing expressions.

Also in .NET unit tests are usually implemented as separate project but in my implementation they are used to test ExpressionsRepository methods only and I decided to place them in ExpressionsRepository class directly for simplicity.

## How to run ##

Application could be built with Visual Studio or there are two builds in ExpressionSolver/builds/linux-x64/ and ExpressionSolver/builds/win10-x64/ folders. Those builds are self-contained so they are supposed to work without .NET Core installed. But I only tested win10 build and my OS had .NET Core installed.

After application started it attaches to port 5001. So web UI is accessible through browser at url https://localhost:5001/.
