Synapse Technical Assesssment by Alexander Haynes
-------------------------------------------------
- This is a .NET 7 console app using xUnit for unit testing.
- Data mocking is performed via RichardSzalay.MockHttp.
- The application runs without error on my machine and produces all of the expected output.
- Six unit tests are included and all pass when ran.
- Many code changes were made to bring it to current best programming standards, including but not limited to:
    - replaced ".GetAwaiter().GetResult()" with "await()" because the former can cause deadlocks and is intended for the compiler.
    - added try-catches to each method.
    - implemented logging to the Log.txt file.
    - added summary and parameter info to each method, fixed grammar of existing comments.
    - converted several methods to be asynchronous.
    - changed name of SendAlertAndUpdateOrder() to PostUpdatedOrdersAsync() because the alert notifications are already performed before that method via the ProcessOrderAsync() method.
    - removed IncrementDeliveryNotification() method and simply moved its one line of body code to replace its function call within ProcessOrderAsync().
    - added `order["Items"] = items;` in ProcessOrderAsync() so the increment update saves past the method call.
    - changed ProcessOrderAsync() return type from "JObject" to "Task<JObject>."
    - Possibly only need one HttpClient instance for the class in production instead of one per method.

NuGet packages installed:
1. dotnet add package xunit --version 2.9.2
2. dotnet add package RichardSzalay.MockHttp --version 7.0.0
3. dotnet add package Newtonsoft.Json --version 13.0.3
