/*
Reactive Services
 
Copyright (c) Rafael Romão 2015
 
All rights reserved.
 
MIT License
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
associated documentation files (the ""Software""), to deal in the Software without restriction,
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS
OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("ReactiveServices.ComputationalUnit.Dispatching")]

[assembly: Guid("a62e0a1d-8ecb-45a7-bd74-fa2af476cab4")]

[assembly: AssemblyProduct("Reactive Services")]
[assembly: AssemblyCompany("Reactive Services")]
[assembly: AssemblyCopyright("Copyright © Rafael Romão 2015")]

[assembly: ComVisible(false)]

[assembly: AssemblyVersion("0.2.*")]
[assembly: AssemblyFileVersion("0.2.0.0")]


[assembly: InternalsVisibleTo("ComputationalUnit")]
[assembly: InternalsVisibleTo("ReactiveServices.Application")]
[assembly: InternalsVisibleTo("ReactiveServices.Application.Launching.Tests")]
[assembly: InternalsVisibleTo("ReactiveServices.Application.Monitoring.Tests")]
[assembly: InternalsVisibleTo("ReactiveServices.Application.Termination.Tests")]
[assembly: InternalsVisibleTo("ReactiveServices.Application.Restoration.Tests")]
[assembly: InternalsVisibleTo("ReactiveServices.ComputationalUnit.Dispatching.Tests")]
[assembly: InternalsVisibleTo("ReactiveServices.ComputationalUnit.Dispatching.LoadBalancing.Tests")]
[assembly: InternalsVisibleTo("ReactiveServices.ComputationalUnit.Dispatching.Work.Tests")]