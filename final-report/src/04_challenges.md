# Challenges

## Intellisense

The most obvious problem is that of intellisense not working as expected. This
is more a symptom of other problems with dependencies and experience, however,
so should be resolved as those problems are resolved.

## Distribution

Due to the problems with dependency resolution, a major challenge is
distributing the library in a way where users can consume it in the simplest
way possible. In order to truly solve this problem, a 'one-touch install'
process should be achieved, whereby the user is required to only install one
package and have Intellisense in their editor of choice.

## Debugging

Debugging has been possibly the biggest issue that has made it difficult to
develop a type provider. Debugging is crucial to working with any new piece of
technology, and especially when the output is more-or-less invisible. Techniques
that may work to debug one issue could completely fail the next. Even when a
consistent solution is found, in general it is very slow work.

The general work-flow involves using F# Interactive and Forest for the most
part. When more in-depth debugging is required, Visual Studio under Windows is
capable of attaching to the executing F# Interactive process, and breakpoints
can be added to the provider. Any changes require stopping the debugger, closing
F# Interactive, recompiling, opening F# Interactive, re-attaching the debugger,
and running the same set of commands to get the provider back into the same
state it was in previously.

If a test of Intellisense is required, a similar process must be used. Except
instead of opening the relatively light-weight instance of F# Interactive, an
entire new Visual Studio instance must be opened and the debugger attached to
it. Often times when debugging in this manner, symbols would fail to load, so no
breakpoints would be hit. This required restarting the whole process. Each
incremental change required closing the IDE, running the compiler (it can't run
while the DLLs are being used to provide types), opening the IDE, attaching the
debugger, and finally opening a project that uses the provider.

## Inexperience

Whilst having worked with the language and runtime for the better part of a
year, there are many aspects that are not fully understood. As mentioned
numerous times, dependency resolution is essentially a black box. This makes it
difficult to work on platforms outside of Windows. Windows has had many of these
problems solved, and Visual Studio manages to abstract the need for a lot of
this knowledge from the user. This is advantageous until there are any problems,
at which point relying on the IDE to do everything is no longer viable.

A better understanding of exactly how the runtime works, and how the platform
interacts with the assemblies, would likely have been very helpful in the project.