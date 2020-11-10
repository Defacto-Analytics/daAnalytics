# **daAnalytics** - a in-development linear rates pricing and risk library.
Developed by [Gustav Rasmussen](https://www.linkedin.com/in/gustav-rasmussen-b013135b/) & [Alexander Bech](https://www.linkedin.com/in/alexander-lillevang-bech-70186b120/) @DefactoAnalytics

## **Project Setup**
The project is written entirely in C# and the primary interface to price and risk interest rate derivatives is Excel. The interface is made possible by using the [ExcelDNA](https://github.com/Excel-DNA/ExcelDna) framework.
* **daLib:** Core functionality of the library.
* **daAnalyticsExcel:**  Excel Interface of daLib

## **Building and using daAnalytics**
The binaries for the project consists of a simple .dll file from daLib and a variety of .xll files from daAnalyticsExcel. It's recommended to use the packed version of xll files, and using 32/64-bit addins depending on your Excel version. 

It's recommended to build this project with Visual Studio, as it's built upon .NET Framework, in order to comply with ExcelDNA. Then it's also possible to debug the code via Excel. Simply load the project and press start in VS.

As an introduction to the Excel addin, it's recommended to start with the supplied Excel sheet: *daAnalytics/daAnalyticsExcel/prototype.xlsm*





