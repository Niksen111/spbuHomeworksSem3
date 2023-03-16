# MyNUnit

C\# library for Unit testing of .Net projects.

## Usage

Mark all methods to be tested with the `[Test]` attribute.
If the test script is expected to throw an exception, use the `[Test(Type? excpetedException)]` constructor overloading.
If the test should not run at the moment, use overloading constructor `[Test(string? reasonForIgnoring)]`.
Methods marked with the `[Test]` attribute must be non-static, have no arguments, and have a void return value type.

Methods that must be executed before and after running tests in the class are marked with `[BeforeClass]` and `[AfterClass]` attributes. 
The methods placed by `[BeforeClass]` and `[AfterClass]` attributes must be static, take no arguments and have a void return value type.

The methods that must be executed before and after the test run in class must be marked with `[Before]` and `[After]` attributes. 
The methods marked with `[Before]` and `[After]` attributes must be non-static, take no arguments and have a void return value type.


## License

[Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0)
